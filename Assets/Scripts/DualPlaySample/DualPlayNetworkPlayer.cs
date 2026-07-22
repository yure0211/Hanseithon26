using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Hanseithon.DualPlaySample
{
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class DualPlayNetworkPlayer : NetworkBehaviour
    {
        public enum PlayerRole : byte
        {
            Turtle,
            Bunny
        }

        [SerializeField] private string gameplaySceneName = "InGame";
        [SerializeField] private Vector2 turtleSpawnPosition = new Vector2(-2.5f, -0.8f);
        [SerializeField] private Vector2 bunnySpawnPosition = new Vector2(2.5f, -1.8f);
        [SerializeField] private SpriteRenderer playerRenderer;
        [SerializeField] private Sprite turtleSprite;
        [SerializeField] private Sprite bunnySprite;
        [SerializeField] private RuntimeAnimatorController turtleAnimatorController;
        [SerializeField] private RuntimeAnimatorController bunnyAnimatorController;

        private readonly NetworkVariable<PlayerRole> role = new NetworkVariable<PlayerRole>(
            PlayerRole.Turtle,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<bool> hasSelectedRole = new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<bool> animationIsRunning = new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);

        private readonly NetworkVariable<bool> animationIsGrounded = new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);

        private readonly NetworkVariable<float> animationYVelocity = new NetworkVariable<float>(
            0f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);

        private readonly NetworkVariable<int> animationStateHash = new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);

        private readonly NetworkVariable<bool> facingLeft = new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);

        private static bool hasLocalRole;
        private static PlayerRole localRole;

        private Rigidbody2D body;
        private BoxCollider2D bodyCollider;
        private Animator playerAnimator;
        private BunnyController bunnyController;
        private TurtleController turtleController;
        private bool placedInGameplay;

        private static readonly Color TurtleColor = new Color(0.28f, 0.72f, 0.4f, 1f);
        private static readonly Color BunnyColor = new Color(0.95f, 0.62f, 0.3f, 1f);
        private static readonly int IsRunParameter = Animator.StringToHash("IsRun");
        private static readonly int IsGroundParameter = Animator.StringToHash("IsGround");
        private static readonly int YVelocityParameter = Animator.StringToHash("YVelocity");

        public static DualPlayNetworkPlayer LocalPlayer { get; private set; }
        public static string LocalRoleName => hasLocalRole ? localRole.ToString() : "Not selected";
        public PlayerRole Role => role.Value;
        public bool HasSelectedRole => hasSelectedRole.Value;

        private void Awake()
        {
            if (playerRenderer == null)
            {
                playerRenderer = GetComponent<SpriteRenderer>();
            }

            playerAnimator = GetComponent<Animator>();
            if (playerAnimator == null)
            {
                playerAnimator = gameObject.AddComponent<Animator>();
            }
            playerAnimator.applyRootMotion = false;

            if (playerRenderer.sprite == null && turtleSprite == null && bunnySprite == null)
            {
                playerRenderer.sprite = DualPlayRuntimeSprite.Get();
            }

            playerRenderer.sortingOrder = 10;

            body = GetComponent<Rigidbody2D>();
            if (body == null)
            {
                body = gameObject.AddComponent<Rigidbody2D>();
            }

            bodyCollider = GetComponent<BoxCollider2D>();
            if (bodyCollider == null)
            {
                bodyCollider = gameObject.AddComponent<BoxCollider2D>();
            }
            bodyCollider.size = Vector2.one;

            bunnyController = GetComponent<BunnyController>();
            if (bunnyController == null)
            {
                bunnyController = gameObject.AddComponent<BunnyController>();
            }

            turtleController = GetComponent<TurtleController>();
            if (turtleController == null)
            {
                turtleController = gameObject.AddComponent<TurtleController>();
            }

            bunnyController.enabled = false;
            turtleController.enabled = false;
            body.simulated = false;
            bodyCollider.enabled = false;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            role.OnValueChanged += HandleRoleChanged;
            hasSelectedRole.OnValueChanged += HandleSelectionChanged;
            animationIsRunning.OnValueChanged += HandleAnimationIsRunningChanged;
            animationIsGrounded.OnValueChanged += HandleAnimationIsGroundedChanged;
            animationYVelocity.OnValueChanged += HandleAnimationYVelocityChanged;
            animationStateHash.OnValueChanged += HandleAnimationStateChanged;
            facingLeft.OnValueChanged += HandleFacingChanged;
            SceneManager.activeSceneChanged += HandleActiveSceneChanged;

            if (IsOwner)
            {
                LocalPlayer = this;
                RefreshLocalRole();
            }

            ConfigureRoleAndScene();
            ApplyAnimationState(animationStateHash.Value);
            playerRenderer.flipX = facingLeft.Value;
        }

        public override void OnNetworkDespawn()
        {
            role.OnValueChanged -= HandleRoleChanged;
            hasSelectedRole.OnValueChanged -= HandleSelectionChanged;
            animationIsRunning.OnValueChanged -= HandleAnimationIsRunningChanged;
            animationIsGrounded.OnValueChanged -= HandleAnimationIsGroundedChanged;
            animationYVelocity.OnValueChanged -= HandleAnimationYVelocityChanged;
            animationStateHash.OnValueChanged -= HandleAnimationStateChanged;
            facingLeft.OnValueChanged -= HandleFacingChanged;
            SceneManager.activeSceneChanged -= HandleActiveSceneChanged;

            if (IsOwner)
            {
                hasLocalRole = false;
                if (LocalPlayer == this)
                {
                    LocalPlayer = null;
                }
            }

            bunnyController.enabled = false;
            turtleController.enabled = false;
            base.OnNetworkDespawn();
        }

        private void HandleRoleChanged(PlayerRole previousRole, PlayerRole newRole)
        {
            if (IsOwner && hasSelectedRole.Value)
            {
                localRole = newRole;
            }

            ConfigureRoleAndScene();
            ApplyAnimationState(animationStateHash.Value);
            playerRenderer.flipX = facingLeft.Value;
        }

        private void LateUpdate()
        {
            if (!IsSpawned || !IsOwner || !hasSelectedRole.Value ||
                SceneManager.GetActiveScene().name != gameplaySceneName ||
                playerAnimator == null || playerAnimator.runtimeAnimatorController == null)
            {
                return;
            }

            if (facingLeft.Value != playerRenderer.flipX)
            {
                facingLeft.Value = playerRenderer.flipX;
            }

            bool isRunning = playerAnimator.GetBool(IsRunParameter);
            if (animationIsRunning.Value != isRunning)
            {
                animationIsRunning.Value = isRunning;
            }

            if (role.Value == PlayerRole.Bunny)
            {
                bool isGrounded = playerAnimator.GetBool(IsGroundParameter);
                if (animationIsGrounded.Value != isGrounded)
                {
                    animationIsGrounded.Value = isGrounded;
                }

                float yVelocity = playerAnimator.GetFloat(YVelocityParameter);
                if (Mathf.Abs(animationYVelocity.Value - yVelocity) > 0.01f)
                {
                    animationYVelocity.Value = yVelocity;
                }
            }

            if (playerAnimator.layerCount == 0 || playerAnimator.IsInTransition(0))
            {
                return;
            }

            int currentStateHash = playerAnimator.GetCurrentAnimatorStateInfo(0).fullPathHash;
            if (currentStateHash != 0 && animationStateHash.Value != currentStateHash)
            {
                animationStateHash.Value = currentStateHash;
            }
        }

        private void HandleSelectionChanged(bool previousValue, bool newValue)
        {
            if (IsOwner)
            {
                RefreshLocalRole();
            }

            ConfigureRoleAndScene();
        }

        private void HandleAnimationIsRunningChanged(bool previousValue, bool newValue)
        {
            ApplyAnimatorParameters();
        }

        private void HandleAnimationIsGroundedChanged(bool previousValue, bool newValue)
        {
            ApplyAnimatorParameters();
        }

        private void HandleAnimationYVelocityChanged(float previousValue, float newValue)
        {
            ApplyAnimatorParameters();
        }

        private void HandleAnimationStateChanged(int previousStateHash, int newStateHash)
        {
            if (!IsOwner)
            {
                ApplyAnimatorParameters();
                ApplyAnimationState(newStateHash);
            }
        }

        private void HandleFacingChanged(bool previousFacingLeft, bool newFacingLeft)
        {
            if (!IsOwner)
            {
                playerRenderer.flipX = newFacingLeft;
            }
        }

        public void RequestRoleSelection(PlayerRole requestedRole)
        {
            if (!IsOwner || !IsSpawned)
            {
                return;
            }

            SelectRoleServerRpc(requestedRole);
        }

        public static bool IsRoleTakenByOther(PlayerRole requestedRole)
        {
            DualPlayNetworkPlayer[] players = FindObjectsByType<DualPlayNetworkPlayer>(FindObjectsSortMode.None);
            for (int i = 0; i < players.Length; i++)
            {
                DualPlayNetworkPlayer player = players[i];
                if (player == LocalPlayer || !player.IsSpawned || !player.HasSelectedRole)
                {
                    continue;
                }

                if (player.Role == requestedRole)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool AreAllPlayersReady(int expectedPlayerCount)
        {
            DualPlayNetworkPlayer[] players = FindObjectsByType<DualPlayNetworkPlayer>(FindObjectsSortMode.None);
            int readyPlayers = 0;
            bool turtleSelected = false;
            bool bunnySelected = false;

            for (int i = 0; i < players.Length; i++)
            {
                DualPlayNetworkPlayer player = players[i];
                if (!player.IsSpawned)
                {
                    continue;
                }

                if (!player.HasSelectedRole)
                {
                    return false;
                }

                if (player.Role == PlayerRole.Turtle)
                {
                    if (turtleSelected)
                    {
                        return false;
                    }
                    turtleSelected = true;
                }
                else
                {
                    if (bunnySelected)
                    {
                        return false;
                    }
                    bunnySelected = true;
                }

                readyPlayers++;
            }

            return readyPlayers == expectedPlayerCount && turtleSelected && bunnySelected;
        }

        [ServerRpc]
        private void SelectRoleServerRpc(PlayerRole requestedRole)
        {
            if (requestedRole != PlayerRole.Turtle && requestedRole != PlayerRole.Bunny)
            {
                return;
            }

            DualPlayNetworkPlayer[] players = FindObjectsByType<DualPlayNetworkPlayer>(FindObjectsSortMode.None);
            for (int i = 0; i < players.Length; i++)
            {
                DualPlayNetworkPlayer player = players[i];
                if (player == this || !player.IsSpawned || !player.HasSelectedRole)
                {
                    continue;
                }

                if (player.Role == requestedRole)
                {
                    return;
                }
            }

            role.Value = requestedRole;
            hasSelectedRole.Value = true;
        }

        private void RefreshLocalRole()
        {
            hasLocalRole = hasSelectedRole.Value;
            if (hasLocalRole)
            {
                localRole = role.Value;
            }
        }

        private void HandleAnimationStateChanged(int previousStateHash, int newStateHash)
        {
            if (!IsOwner)
            {
                ApplyAnimationState(newStateHash);
            }
        }

        private void HandleFacingChanged(bool previousFacingLeft, bool newFacingLeft)
        {
            if (!IsOwner)
            {
                playerRenderer.flipX = newFacingLeft;
            }
        }

        private void HandleActiveSceneChanged(Scene previousScene, Scene newScene)
        {
            placedInGameplay = false;
            ConfigureRoleAndScene();
        }

        private void ConfigureRoleAndScene()
        {
            PlayerRole currentRole = role.Value;
            bool isGameplay = SceneManager.GetActiveScene().name == gameplaySceneName;
            bool controlsEnabled = IsSpawned && IsOwner && isGameplay && hasSelectedRole.Value;

            ConfigureVisual(currentRole);
            playerRenderer.enabled = isGameplay && hasSelectedRole.Value;
            gameObject.name = currentRole == PlayerRole.Turtle ? "NetworkTurtle" : "NetworkBunny";

            if (isGameplay)
            {
                DisableLocalSceneCharacters();
            }

            bunnyController.enabled = controlsEnabled && currentRole == PlayerRole.Bunny;
            turtleController.enabled = controlsEnabled && currentRole == PlayerRole.Turtle;
            body.simulated = controlsEnabled;
            bodyCollider.enabled = controlsEnabled;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;

            if (!isGameplay)
            {
                body.linearVelocity = Vector2.zero;
                return;
            }

            if (currentRole == PlayerRole.Turtle)
            {
                body.gravityScale = 0f;
                body.linearDamping = 0.5f;
            }
            else
            {
                body.gravityScale = 1f;
                body.linearDamping = 0f;
            }

            if (controlsEnabled && !placedInGameplay)
            {
                Vector2 spawnPosition = currentRole == PlayerRole.Turtle
                    ? turtleSpawnPosition
                    : bunnySpawnPosition;
                body.position = spawnPosition;
                body.linearVelocity = Vector2.zero;
                placedInGameplay = true;
            }
        }

        private void ConfigureVisual(PlayerRole currentRole)
        {
            Sprite targetSprite = currentRole == PlayerRole.Turtle ? turtleSprite : bunnySprite;
            RuntimeAnimatorController targetAnimatorController = currentRole == PlayerRole.Turtle
                ? turtleAnimatorController
                : bunnyAnimatorController;

            if (targetSprite != null)
            {
                playerRenderer.sprite = targetSprite;
                playerRenderer.color = Color.white;
                transform.localScale = Vector3.one;
            }
            else
            {
                playerRenderer.sprite = DualPlayRuntimeSprite.Get();
                playerRenderer.color = currentRole == PlayerRole.Turtle ? TurtleColor : BunnyColor;
                transform.localScale = currentRole == PlayerRole.Turtle
                    ? new Vector3(1.15f, 0.8f, 1f)
                    : new Vector3(0.8f, 1.1f, 1f);
            }

            if (playerAnimator.runtimeAnimatorController != targetAnimatorController)
            {
                playerAnimator.runtimeAnimatorController = targetAnimatorController;
                playerAnimator.Rebind();
                playerAnimator.Update(0f);
            }

            ApplyAnimatorParameters();
            ApplyAnimationState(animationStateHash.Value);
        }

        private void ApplyAnimatorParameters()
        {
            if (IsOwner || playerAnimator == null || playerAnimator.runtimeAnimatorController == null)
            {
                return;
            }

            playerAnimator.SetBool(IsRunParameter, animationIsRunning.Value);
            if (role.Value == PlayerRole.Bunny)
            {
                playerAnimator.SetBool(IsGroundParameter, animationIsGrounded.Value);
                playerAnimator.SetFloat(YVelocityParameter, animationYVelocity.Value);
            }
        }

        private void ApplyAnimationState(int stateHash)
        {
            if (stateHash == 0 || playerAnimator == null ||
                playerAnimator.runtimeAnimatorController == null ||
                !playerAnimator.HasState(0, stateHash))
            {
                return;
            }

            playerAnimator.Play(stateHash, 0, 0f);
        }

        private static void DisableLocalSceneCharacters()
        {
            DisableLocalSceneCharacter("Bunny");
            DisableLocalSceneCharacter("Turtle");
        }

        private static void DisableLocalSceneCharacter(string characterName)
        {
            GameObject localCharacter = GameObject.Find(characterName);
            if (localCharacter != null && localCharacter.GetComponent<NetworkObject>() == null)
            {
                localCharacter.SetActive(false);
            }
        }
    }
}
