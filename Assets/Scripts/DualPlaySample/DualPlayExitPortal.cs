using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Hanseithon.DualPlaySample
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(NetworkObject))]
    public sealed class DualPlayExitPortal : NetworkBehaviour
    {
#if UNITY_EDITOR
        [Header("Scene transition")]
        [SerializeField] private UnityEditor.SceneAsset targetScene;
#endif
        [SerializeField, HideInInspector] private string targetSceneName = "InGame_2";
        [SerializeField] private CanvasGroup fadeCanvasGroup;
        [SerializeField, Min(0.05f)] private float fadeDuration = 0.8f;

        [Header("Portal lock")]
        [SerializeField, Min(2)] private int requiredPlayers = 2;
        [SerializeField, Min(0.5f)] private float serverValidationRadius = 3f;
        [SerializeField] private Vector2 turtleHoldOffset = new Vector2(-0.25f, 0f);
        [SerializeField] private Vector2 bunnyHoldOffset = new Vector2(0.25f, 0f);

        private readonly HashSet<ulong> trappedClientIds = new HashSet<ulong>();
        private readonly HashSet<int> trappedLocalPlayerIds = new HashSet<int>();
        private BoxCollider2D portalCollider;
        private Coroutine fadeRoutine;
        private bool transitionStarted;

        public string TargetSceneName => targetSceneName;

        private void Awake()
        {
            portalCollider = GetComponent<BoxCollider2D>();
            portalCollider.isTrigger = true;
            ResetFade();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsServer)
            {
                NetworkManager.OnClientDisconnectCallback += HandleClientDisconnected;
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer && NetworkManager != null)
            {
                NetworkManager.OnClientDisconnectCallback -= HandleClientDisconnected;
            }

            base.OnNetworkDespawn();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (transitionStarted)
            {
                return;
            }

            DualPlayNetworkPlayer player = other.GetComponentInParent<DualPlayNetworkPlayer>();
            if (player != null)
            {
                if (!IsSpawned)
                {
                    Debug.LogError(
                        "Portal NetworkObject is not spawned. Load this scene through the network scene flow.",
                        this);
                    return;
                }

                if (!player.IsSpawned || !player.IsOwner)
                {
                    return;
                }

                EnterPortalRpc(player.NetworkObjectId);
                return;
            }

            if (!IsNetworkSessionRunning())
            {
                TryEnterLocalPlayer(other);
            }
        }

        private void TryEnterLocalPlayer(Collider2D other)
        {
            TurtleController turtle = other.GetComponentInParent<TurtleController>();
            BunnyController bunny = other.GetComponentInParent<BunnyController>();
            if (turtle == null && bunny == null)
            {
                return;
            }

            GameObject playerObject = turtle != null ? turtle.gameObject : bunny.gameObject;
            if (!trappedLocalPlayerIds.Add(playerObject.GetInstanceID()))
            {
                return;
            }

            DualPlayNetworkPlayer.PlayerRole playerRole = turtle != null
                ? DualPlayNetworkPlayer.PlayerRole.Turtle
                : DualPlayNetworkPlayer.PlayerRole.Bunny;
            DualPlayPortalPlayerLock playerLock =
                playerObject.GetComponent<DualPlayPortalPlayerLock>();
            if (playerLock == null)
            {
                playerLock = playerObject.AddComponent<DualPlayPortalPlayerLock>();
            }

            playerLock.Lock(GetHoldPosition(playerRole), false);
            if (trappedLocalPlayerIds.Count >= requiredPlayers)
            {
                BeginLocalTransition();
            }
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void EnterPortalRpc(ulong playerNetworkObjectId)
        {
            if (!IsServer || transitionStarted || NetworkManager == null ||
                !NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(
                    playerNetworkObjectId,
                    out NetworkObject playerObject))
            {
                return;
            }

            DualPlayNetworkPlayer player = playerObject.GetComponent<DualPlayNetworkPlayer>();
            if (player == null || !player.HasSelectedRole)
            {
                return;
            }

            float distance = Vector2.Distance(player.transform.position, transform.position);
            if (distance > serverValidationRadius ||
                !trappedClientIds.Add(playerObject.OwnerClientId))
            {
                return;
            }

            Vector2 holdPosition = GetHoldPosition(player.Role);
            LockPlayerClientRpc(
                playerNetworkObjectId,
                holdPosition,
                IsGameplaySceneName(targetSceneName));

            if (trappedClientIds.Count >= requiredPlayers)
            {
                BeginTransition();
            }
        }

        private void BeginTransition()
        {
            if (!IsServer || transitionStarted)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(targetSceneName) ||
                !Application.CanStreamedLevelBeLoaded(targetSceneName))
            {
                Debug.LogError(
                    $"Portal target scene '{targetSceneName}' is missing from Build Settings.",
                    this);
                return;
            }

            transitionStarted = true;
            BeginFadeClientRpc(fadeDuration);
            StartCoroutine(LoadTargetSceneAfterFade());
        }

        private void BeginLocalTransition()
        {
            if (transitionStarted)
            {
                return;
            }

            if (!CanLoadTargetScene())
            {
                return;
            }

            transitionStarted = true;
            if (fadeRoutine != null)
            {
                StopCoroutine(fadeRoutine);
            }

            fadeRoutine = StartCoroutine(FadeToBlack(fadeDuration));
            StartCoroutine(LoadTargetSceneLocallyAfterFade());
        }

        [ClientRpc]
        private void LockPlayerClientRpc(
            ulong playerNetworkObjectId,
            Vector2 holdPosition,
            bool restoreGameplayStateOnSceneChange)
        {
            if (NetworkManager == null ||
                !NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(
                    playerNetworkObjectId,
                    out NetworkObject playerObject))
            {
                return;
            }

            DualPlayPortalPlayerLock playerLock =
                playerObject.GetComponent<DualPlayPortalPlayerLock>();
            if (playerLock == null)
            {
                playerLock = playerObject.gameObject.AddComponent<DualPlayPortalPlayerLock>();
            }

            playerLock.Lock(holdPosition, restoreGameplayStateOnSceneChange);
        }

        [ClientRpc]
        private void BeginFadeClientRpc(float duration)
        {
            if (fadeRoutine != null)
            {
                StopCoroutine(fadeRoutine);
            }

            fadeRoutine = StartCoroutine(FadeToBlack(duration));
        }

        private IEnumerator FadeToBlack(float duration)
        {
            if (fadeCanvasGroup == null)
            {
                yield break;
            }

            fadeCanvasGroup.blocksRaycasts = true;
            fadeCanvasGroup.interactable = true;
            float elapsed = 0f;
            float safeDuration = Mathf.Max(0.05f, duration);

            while (elapsed < safeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                fadeCanvasGroup.alpha = Mathf.Clamp01(elapsed / safeDuration);
                yield return null;
            }

            fadeCanvasGroup.alpha = 1f;
        }

        private IEnumerator LoadTargetSceneAfterFade()
        {
            yield return new WaitForSecondsRealtime(Mathf.Max(0.05f, fadeDuration));

            if (!IsServer || NetworkManager == null || NetworkManager.SceneManager == null)
            {
                yield break;
            }

            SceneEventProgressStatus status = NetworkManager.SceneManager.LoadScene(
                targetSceneName,
                LoadSceneMode.Single);
            if (status != SceneEventProgressStatus.Started)
            {
                transitionStarted = false;
                Debug.LogError(
                    $"Portal failed to load scene '{targetSceneName}': {status}",
                    this);
            }
        }

        private IEnumerator LoadTargetSceneLocallyAfterFade()
        {
            yield return new WaitForSecondsRealtime(Mathf.Max(0.05f, fadeDuration));
            SceneManager.LoadScene(targetSceneName, LoadSceneMode.Single);
        }

        private Vector2 GetHoldPosition(DualPlayNetworkPlayer.PlayerRole role)
        {
            Vector2 offset = role == DualPlayNetworkPlayer.PlayerRole.Turtle
                ? turtleHoldOffset
                : bunnyHoldOffset;
            return (Vector2)transform.position + offset;
        }

        private void HandleClientDisconnected(ulong clientId)
        {
            trappedClientIds.Remove(clientId);
        }

        private void ResetFade()
        {
            if (fadeCanvasGroup == null)
            {
                return;
            }

            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
            fadeCanvasGroup.interactable = false;
        }

        private bool CanLoadTargetScene()
        {
            if (!string.IsNullOrWhiteSpace(targetSceneName) &&
                Application.CanStreamedLevelBeLoaded(targetSceneName))
            {
                return true;
            }

            Debug.LogError(
                $"Portal target scene '{targetSceneName}' is missing from Build Settings.",
                this);
            return false;
        }

        private static bool IsNetworkSessionRunning()
        {
            return NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;
        }

        private static bool IsGameplaySceneName(string sceneName)
        {
            return !string.IsNullOrWhiteSpace(sceneName) &&
                   sceneName.StartsWith("InGame", StringComparison.OrdinalIgnoreCase);
        }

#if UNITY_EDITOR
        public void ConfigureInEditor(
            UnityEditor.SceneAsset sceneAsset,
            CanvasGroup transitionCanvasGroup)
        {
            targetScene = sceneAsset;
            targetSceneName = sceneAsset != null ? sceneAsset.name : string.Empty;
            fadeCanvasGroup = transitionCanvasGroup;
        }

        private void OnValidate()
        {
            if (targetScene != null)
            {
                targetSceneName = targetScene.name;
            }

            fadeDuration = Mathf.Max(0.05f, fadeDuration);
            requiredPlayers = Mathf.Max(2, requiredPlayers);
            serverValidationRadius = Mathf.Max(0.5f, serverValidationRadius);

            BoxCollider2D collider = GetComponent<BoxCollider2D>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }
        }
#endif
    }
}
