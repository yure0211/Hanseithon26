using System;
using Hanseithon.Gameplay;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Hanseithon.DualPlaySample
{
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(NetworkTransform))]
    [RequireComponent(typeof(NetworkRigidbody2D))]
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
        private NetworkRigidbody2D networkBody;
        private BoxCollider2D bodyCollider;
        private Animator playerAnimator;
        private BunnyController bunnyController;
        private TurtleController turtleController;
        private TurtleCarrySkill turtleCarrySkill;
        private bool placedInGameplay;
        private string carriedBoxSyncId = string.Empty;
        private Vector2 previousBunnyPhysicsPosition;
        private bool hasPreviousBunnyPhysicsPosition;

        private static readonly Color TurtleColor = new Color(0.28f, 0.72f, 0.4f, 1f);
        private static readonly Color BunnyColor = new Color(0.95f, 0.62f, 0.3f, 1f);
        private static readonly int IsRunParameter = Animator.StringToHash("IsRun");
        private static readonly int IsGroundParameter = Animator.StringToHash("IsGround");
        private static readonly int YVelocityParameter = Animator.StringToHash("YVelocity");

        public static DualPlayNetworkPlayer LocalPlayer { get; private set; }
        public static string LocalRoleName => hasLocalRole ? localRole.ToString() : "Not selected";
        public PlayerRole Role => role.Value;
        public bool HasSelectedRole => hasSelectedRole.Value;
        public bool IsNetworkCarryAvailable =>
            IsSpawned && NetworkManager != null && NetworkManager.IsListening;
        public bool IsCarryingNetworkBox => !string.IsNullOrEmpty(carriedBoxSyncId);

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

            networkBody = GetComponent<NetworkRigidbody2D>();
            networkBody.UseRigidBodyForMotion = true;
            networkBody.AutoUpdateKinematicState = true;
            networkBody.AutoSetKinematicOnDespawn = true;

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

            turtleCarrySkill = GetComponent<TurtleCarrySkill>();
            if (turtleCarrySkill == null)
            {
                turtleCarrySkill = gameObject.AddComponent<TurtleCarrySkill>();
            }

            bunnyController.enabled = false;
            turtleController.enabled = false;
            turtleCarrySkill.enabled = false;
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
            carriedBoxSyncId = string.Empty;
            hasPreviousBunnyPhysicsPosition = false;
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
            turtleCarrySkill.enabled = false;
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
                !IsGameplayScene(SceneManager.GetActiveScene().name) ||
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

        public void RequestToggleNetworkCarry()
        {
            if (!IsNetworkCarryAvailable || !IsOwner ||
                !hasSelectedRole.Value || role.Value != PlayerRole.Turtle)
            {
                return;
            }

            ToggleNetworkCarryServerRpc();
        }

        [ServerRpc]
        private void ToggleNetworkCarryServerRpc()
        {
            if (!IsServer || !hasSelectedRole.Value || role.Value != PlayerRole.Turtle ||
                !IsGameplayScene(SceneManager.GetActiveScene().name))
            {
                return;
            }

            if (!string.IsNullOrEmpty(carriedBoxSyncId))
            {
                DropNetworkCarry();
                return;
            }

            TurtleCarryableBox nearestBox = FindNearestNetworkBox();
            if (nearestBox == null || !nearestBox.TryPickUp(bodyCollider))
            {
                return;
            }

            carriedBoxSyncId = nearestBox.NetworkSyncId;
            Vector2 carryPosition = (Vector2)transform.position +
                                    (Vector2)transform.right * turtleCarrySkill.CarryDistance;
            nearestBox.SetNetworkPosition(carryPosition);
            SetNetworkCarryStateClientRpc(carriedBoxSyncId, true, carryPosition);
        }

        private TurtleCarryableBox FindNearestNetworkBox()
        {
            Collider2D[] overlaps = Physics2D.OverlapCircleAll(
                transform.position,
                turtleCarrySkill.InteractionRadius);
            TurtleCarryableBox nearestBox = null;
            float nearestDistance = float.PositiveInfinity;
            Scene activeScene = SceneManager.GetActiveScene();

            for (int i = 0; i < overlaps.Length; i++)
            {
                TurtleCarryableBox candidate =
                    overlaps[i].GetComponentInParent<TurtleCarryableBox>();
                if (candidate == null || candidate.IsHeld ||
                    candidate.gameObject.scene != activeScene)
                {
                    continue;
                }

                float distance = ((Vector2)candidate.transform.position -
                                  (Vector2)transform.position).sqrMagnitude;
                if (distance < nearestDistance)
                {
                    nearestBox = candidate;
                    nearestDistance = distance;
                }
            }

            return nearestBox;
        }

        private void DropNetworkCarry()
        {
            string syncId = carriedBoxSyncId;
            carriedBoxSyncId = string.Empty;
            Vector2 dropPosition = transform.position;

            if (TurtleCarryableBox.TryGetActive(syncId, out TurtleCarryableBox carryableBox))
            {
                dropPosition = carryableBox.transform.position;
                if (carryableBox.IsHeld)
                {
                    carryableBox.Drop(Vector2.zero);
                }
            }

            SetNetworkCarryStateClientRpc(syncId, false, dropPosition);
        }

        [ClientRpc]
        private void SetNetworkCarryStateClientRpc(
            string syncId,
            bool isHeld,
            Vector2 worldPosition)
        {
            if (isHeld)
            {
                carriedBoxSyncId = syncId;
            }
            else if (carriedBoxSyncId == syncId)
            {
                carriedBoxSyncId = string.Empty;
            }

            if (!TurtleCarryableBox.TryGetActive(syncId, out TurtleCarryableBox carryableBox))
            {
                return;
            }

            carryableBox.SetNetworkPosition(worldPosition);
            if (isHeld)
            {
                if (!carryableBox.IsHeld)
                {
                    carryableBox.TryPickUp(bodyCollider);
                }
            }
            else if (carryableBox.IsHeld)
            {
                carryableBox.Drop(Vector2.zero);
            }
        }

        [ClientRpc(Delivery = RpcDelivery.Unreliable)]
        private void SyncNetworkCarryPositionClientRpc(Vector2 worldPosition)
        {
            if (string.IsNullOrEmpty(carriedBoxSyncId) ||
                !TurtleCarryableBox.TryGetActive(
                    carriedBoxSyncId,
                    out TurtleCarryableBox carryableBox))
            {
                return;
            }

            if (!carryableBox.IsHeld)
            {
                carryableBox.TryPickUp(bodyCollider);
            }

            carryableBox.SetNetworkPosition(worldPosition);
        }

        [ClientRpc]
        private void ClearNetworkCarryClientRpc()
        {
            string syncId = carriedBoxSyncId;
            carriedBoxSyncId = string.Empty;
            if (TurtleCarryableBox.TryGetActive(syncId, out TurtleCarryableBox carryableBox) &&
                carryableBox.IsHeld)
            {
                carryableBox.Drop(Vector2.zero);
            }
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

        private void HandleActiveSceneChanged(Scene previousScene, Scene newScene)
        {
            carriedBoxSyncId = string.Empty;
            hasPreviousBunnyPhysicsPosition = false;
            placedInGameplay = false;
            ConfigureRoleAndScene();
        }

        private void ConfigureRoleAndScene()
        {
            PlayerRole currentRole = role.Value;
            bool isGameplay = IsGameplayScene(SceneManager.GetActiveScene().name);
            bool controlsEnabled = IsSpawned && IsOwner && isGameplay && hasSelectedRole.Value;
            Vector2 fallbackSpawnPosition = currentRole == PlayerRole.Turtle
                ? turtleSpawnPosition
                : bunnySpawnPosition;
            Vector2 sceneSpawnPosition = isGameplay
                ? FindSceneCharacterPosition(currentRole, fallbackSpawnPosition)
                : fallbackSpawnPosition;

            ConfigureVisual(currentRole);
            playerRenderer.enabled = isGameplay && hasSelectedRole.Value;
            gameObject.name = currentRole == PlayerRole.Turtle ? "NetworkTurtle" : "NetworkBunny";

            if (isGameplay)
            {
                DisableLocalSceneCharacters();
            }

            bunnyController.enabled = controlsEnabled && currentRole == PlayerRole.Bunny;
            turtleController.enabled = controlsEnabled && currentRole == PlayerRole.Turtle;
            turtleCarrySkill.enabled = controlsEnabled && currentRole == PlayerRole.Turtle;
            // Keep remote kinematic bodies simulated so NetworkRigidbody2D can apply
            // synchronized positions through the 2D physics engine. Only the owner
            // receives input and owns an enabled collision shape.
            body.simulated = isGameplay && hasSelectedRole.Value;
            bodyCollider.enabled = controlsEnabled;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;

            if (controlsEnabled)
            {
                ConfigureGameplayCollision();
            }

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
                body.position = sceneSpawnPosition;
                body.linearVelocity = Vector2.zero;
                placedInGameplay = true;
            }
        }

        private void FixedUpdate()
        {
            MaintainOwnedGameplayPhysics();
            RecoverMissedBunnyGroundCollision();

            if (!IsSpawned || !IsServer || string.IsNullOrEmpty(carriedBoxSyncId))
            {
                return;
            }

            if (!IsGameplayScene(SceneManager.GetActiveScene().name) ||
                !TurtleCarryableBox.TryGetActive(
                    carriedBoxSyncId,
                    out TurtleCarryableBox carryableBox))
            {
                carriedBoxSyncId = string.Empty;
                ClearNetworkCarryClientRpc();
                return;
            }

            Vector2 carryPosition = (Vector2)transform.position +
                                    (Vector2)transform.right * turtleCarrySkill.CarryDistance;
            if (!carryableBox.IsHeld)
            {
                carryableBox.TryPickUp(bodyCollider);
            }

            carryableBox.SetNetworkPosition(carryPosition);
            SyncNetworkCarryPositionClientRpc(carryPosition);
        }

        private void MaintainOwnedGameplayPhysics()
        {
            bool ownsGameplayPhysics = IsSpawned && IsOwner && hasSelectedRole.Value &&
                                       IsGameplayScene(SceneManager.GetActiveScene().name);
            if (!ownsGameplayPhysics)
            {
                hasPreviousBunnyPhysicsPosition = false;
                return;
            }

            body.simulated = true;
            body.bodyType = RigidbodyType2D.Dynamic;
            bodyCollider.enabled = true;
            bodyCollider.isTrigger = false;

            int playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer >= 0)
            {
                gameObject.layer = playerLayer;
            }
        }

        private void RecoverMissedBunnyGroundCollision()
        {
            bool isOwnedBunny = IsSpawned && IsOwner && hasSelectedRole.Value &&
                                role.Value == PlayerRole.Bunny &&
                                IsGameplayScene(SceneManager.GetActiveScene().name) &&
                                body.simulated && bodyCollider.enabled;
            if (!isOwnedBunny)
            {
                hasPreviousBunnyPhysicsPosition = false;
                return;
            }

            Vector2 currentPosition = body.position;
            if (hasPreviousBunnyPhysicsPosition &&
                currentPosition.y < previousBunnyPhysicsPosition.y)
            {
                int groundLayer = LayerMask.NameToLayer("Ground");
                if (groundLayer >= 0)
                {
                    Vector2 scale = transform.lossyScale;
                    Vector2 fullColliderSize = new Vector2(
                        Mathf.Abs(bodyCollider.size.x * scale.x),
                        Mathf.Abs(bodyCollider.size.y * scale.y));
                    Vector2 castSize = fullColliderSize * 0.9f;
                    Vector2 worldOffset = new Vector2(
                        bodyCollider.offset.x * scale.x,
                        bodyCollider.offset.y * scale.y);
                    Vector2 castOrigin = new Vector2(
                        currentPosition.x + worldOffset.x,
                        previousBunnyPhysicsPosition.y + worldOffset.y);
                    float fallDistance = previousBunnyPhysicsPosition.y -
                                         currentPosition.y + 0.1f;
                    RaycastHit2D[] hits = Physics2D.BoxCastAll(
                        castOrigin,
                        castSize,
                        0f,
                        Vector2.down,
                        fallDistance,
                        1 << groundLayer);

                    for (int i = 0; i < hits.Length; i++)
                    {
                        RaycastHit2D hit = hits[i];
                        if (hit.collider == null || hit.collider.isTrigger)
                        {
                            continue;
                        }

                        float sizeCorrection = (fullColliderSize.y - castSize.y) * 0.5f;
                        float correctedY = hit.centroid.y - worldOffset.y +
                                           sizeCorrection + Physics2D.defaultContactOffset;
                        if (correctedY >= currentPosition.y)
                        {
                            body.position = new Vector2(currentPosition.x, correctedY);
                            body.linearVelocityY = 0f;
                            currentPosition = body.position;
                        }

                        break;
                    }
                }
            }

            previousBunnyPhysicsPosition = currentPosition;
            hasPreviousBunnyPhysicsPosition = true;
        }

        private void ConfigureGameplayCollision()
        {
            int playerLayer = LayerMask.NameToLayer("Player");
            int groundLayer = LayerMask.NameToLayer("Ground");

            if (playerLayer >= 0)
            {
                gameObject.layer = playerLayer;
            }

            body.bodyType = RigidbodyType2D.Dynamic;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            bodyCollider.isTrigger = false;

            if (playerLayer >= 0 && groundLayer >= 0)
            {
                Physics2D.IgnoreLayerCollision(playerLayer, groundLayer, false);
            }
        }

        private static Vector2 FindSceneCharacterPosition(
            PlayerRole currentRole,
            Vector2 fallbackPosition)
        {
            string characterName = currentRole == PlayerRole.Turtle ? "Turtle" : "Bunny";
            Scene activeScene = SceneManager.GetActiveScene();
            Transform[] sceneTransforms = FindObjectsByType<Transform>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            for (int i = 0; i < sceneTransforms.Length; i++)
            {
                Transform sceneTransform = sceneTransforms[i];
                if (sceneTransform.name == characterName &&
                    sceneTransform.gameObject.scene == activeScene &&
                    sceneTransform.GetComponent<NetworkObject>() == null)
                {
                    return sceneTransform.position;
                }
            }

            return fallbackPosition;
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

        private bool IsGameplayScene(string sceneName)
        {
            return string.Equals(sceneName, gameplaySceneName, StringComparison.OrdinalIgnoreCase) ||
                   (!string.IsNullOrWhiteSpace(sceneName) &&
                    sceneName.StartsWith("InGame", StringComparison.OrdinalIgnoreCase));
        }
    }
}
