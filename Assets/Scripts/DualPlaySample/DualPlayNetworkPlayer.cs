using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Hanseithon.DualPlaySample
{
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

        private readonly NetworkVariable<PlayerRole> role = new NetworkVariable<PlayerRole>(
            PlayerRole.Turtle,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        private static bool hasLocalRole;
        private static PlayerRole localRole;

        private Rigidbody2D body;
        private BoxCollider2D bodyCollider;
        private BunnyController bunnyController;
        private TurtleController turtleController;
        private bool placedInGameplay;

        private static readonly Color TurtleColor = new Color(0.28f, 0.72f, 0.4f, 1f);
        private static readonly Color BunnyColor = new Color(0.95f, 0.62f, 0.3f, 1f);

        public static string LocalRoleName => hasLocalRole ? localRole.ToString() : "Connecting";
        public PlayerRole Role => role.Value;

        private void Awake()
        {
            if (playerRenderer == null)
            {
                playerRenderer = GetComponent<SpriteRenderer>();
            }
            if (playerRenderer.sprite == null)
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
            SceneManager.activeSceneChanged += HandleActiveSceneChanged;

            if (IsServer)
            {
                role.Value = OwnerClientId == NetworkManager.ServerClientId
                    ? PlayerRole.Turtle
                    : PlayerRole.Bunny;
            }

            if (IsOwner)
            {
                hasLocalRole = true;
                localRole = role.Value;
            }

            ConfigureRoleAndScene();
        }

        public override void OnNetworkDespawn()
        {
            role.OnValueChanged -= HandleRoleChanged;
            SceneManager.activeSceneChanged -= HandleActiveSceneChanged;

            if (IsOwner)
            {
                hasLocalRole = false;
            }

            bunnyController.enabled = false;
            turtleController.enabled = false;
            base.OnNetworkDespawn();
        }

        private void HandleRoleChanged(PlayerRole previousRole, PlayerRole newRole)
        {
            if (IsOwner)
            {
                localRole = newRole;
            }

            ConfigureRoleAndScene();
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
            bool controlsEnabled = IsSpawned && IsOwner && isGameplay;

            playerRenderer.color = currentRole == PlayerRole.Turtle ? TurtleColor : BunnyColor;
            transform.localScale = currentRole == PlayerRole.Turtle
                ? new Vector3(1.15f, 0.8f, 1f)
                : new Vector3(0.8f, 1.1f, 1f);
            gameObject.name = currentRole == PlayerRole.Turtle ? "NetworkTurtle" : "NetworkBunny";

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
    }
}
