using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Hanseithon.DualPlaySample
{
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class DualPlayNetworkPlayer : NetworkBehaviour
    {
        [SerializeField] private float moveSpeed = 4.5f;
        [SerializeField] private Vector2 minimumBounds = new Vector2(-5f, -3f);
        [SerializeField] private Vector2 maximumBounds = new Vector2(5f, 3.2f);
        [SerializeField] private SpriteRenderer playerRenderer;

        private static readonly Color HostColor = new Color(1f, 0.45f, 0.23f, 1f);
        private static readonly Color ClientColor = new Color(0.2f, 0.85f, 0.72f, 1f);

        private void Awake()
        {
            if (playerRenderer == null)
            {
                playerRenderer = GetComponent<SpriteRenderer>();
            }

            playerRenderer.sprite = DualPlayRuntimeSprite.Get();
            playerRenderer.color = Color.gray;
            playerRenderer.sortingOrder = 10;
            transform.localScale = Vector3.one * 0.8f;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            playerRenderer.color = OwnerClientId == NetworkManager.ServerClientId ? HostColor : ClientColor;
            gameObject.name = $"NetworkPlayer_{OwnerClientId}";
        }

        private void Update()
        {
            if (!IsSpawned || !IsOwner || Keyboard.current == null)
            {
                return;
            }

            Vector2 input = ReadMovementInput();
            if (input.sqrMagnitude > 1f)
            {
                input.Normalize();
            }

            Vector3 nextPosition = transform.position + (Vector3)(input * moveSpeed * Time.deltaTime);
            nextPosition.x = Mathf.Clamp(nextPosition.x, minimumBounds.x, maximumBounds.x);
            nextPosition.y = Mathf.Clamp(nextPosition.y, minimumBounds.y, maximumBounds.y);
            nextPosition.z = 0f;
            transform.position = nextPosition;
        }

        private static Vector2 ReadMovementInput()
        {
            Keyboard keyboard = Keyboard.current;
            float horizontal = 0f;
            float vertical = 0f;

            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            {
                horizontal -= 1f;
            }
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            {
                horizontal += 1f;
            }
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
            {
                vertical -= 1f;
            }
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
            {
                vertical += 1f;
            }

            return new Vector2(horizontal, vertical);
        }
    }
}