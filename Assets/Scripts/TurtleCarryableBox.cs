using System.Collections.Generic;
using UnityEngine;

namespace Hanseithon.Gameplay
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class TurtleCarryableBox : MonoBehaviour
    {
        private const string HeldLayerName = "Player";
        private static readonly Dictionary<string, TurtleCarryableBox> ActiveBoxes =
            new Dictionary<string, TurtleCarryableBox>();

        [SerializeField, Min(1f)] private float heldScaleMultiplier = 1.15f;

        private Rigidbody2D body;
        private BoxCollider2D boxCollider;
        private Collider2D ignoredCarrierCollider;
        private RigidbodyType2D originalBodyType;
        private RigidbodyConstraints2D originalConstraints;
        private float originalGravityScale;
        private float originalLinearDamping;
        private int originalLayer;
        private Vector3 originalScale;
        private string networkSyncId;

        public bool IsHeld { get; private set; }
        public string NetworkSyncId => networkSyncId;

        private void Awake()
        {
            CacheComponents();
        }

        private void OnEnable()
        {
            networkSyncId = BuildNetworkSyncId(transform);
            ActiveBoxes[networkSyncId] = this;
        }

        private void Reset()
        {
            CacheComponents();
            body.gravityScale = 0f;
            body.linearDamping = 3f;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            boxCollider.size = Vector2.one;
        }

        public bool TryPickUp(Collider2D carrierCollider)
        {
            if (IsHeld)
            {
                return false;
            }

            CacheComponents();
            originalBodyType = body.bodyType;
            originalConstraints = body.constraints;
            originalGravityScale = body.gravityScale;
            originalLinearDamping = body.linearDamping;
            originalLayer = gameObject.layer;
            originalScale = transform.localScale;
            ignoredCarrierCollider = carrierCollider;

            IsHeld = true;
            gameObject.layer = LayerMask.NameToLayer(HeldLayerName);
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.linearDamping = 0f;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            transform.localScale = originalScale * heldScaleMultiplier;

            if (ignoredCarrierCollider != null)
            {
                Physics2D.IgnoreCollision(boxCollider, ignoredCarrierCollider, true);
            }

            return true;
        }

        public void MoveWhileHeld(Vector2 worldPosition)
        {
            if (!IsHeld)
            {
                return;
            }

            body.MovePosition(worldPosition);
        }

        public void SetNetworkPosition(Vector2 worldPosition)
        {
            CacheComponents();
            body.position = worldPosition;
        }

        public void Drop(Vector2 releaseVelocity)
        {
            if (!IsHeld)
            {
                return;
            }

            IsHeld = false;

            if (ignoredCarrierCollider != null)
            {
                Physics2D.IgnoreCollision(boxCollider, ignoredCarrierCollider, false);
            }

            ignoredCarrierCollider = null;
            gameObject.layer = originalLayer;
            body.bodyType = originalBodyType;
            body.gravityScale = originalGravityScale;
            body.linearDamping = originalLinearDamping;
            body.constraints = originalConstraints;
            body.linearVelocity = releaseVelocity;
            transform.localScale = originalScale;
        }

        private void OnDisable()
        {
            if (!string.IsNullOrEmpty(networkSyncId) &&
                ActiveBoxes.TryGetValue(networkSyncId, out TurtleCarryableBox registered) &&
                registered == this)
            {
                ActiveBoxes.Remove(networkSyncId);
            }

            if (IsHeld)
            {
                Drop(Vector2.zero);
            }
        }

        public static bool TryGetActive(string syncId, out TurtleCarryableBox carryableBox)
        {
            if (!string.IsNullOrEmpty(syncId) &&
                ActiveBoxes.TryGetValue(syncId, out carryableBox) &&
                carryableBox != null &&
                carryableBox.gameObject.activeInHierarchy)
            {
                return true;
            }

            carryableBox = null;
            return false;
        }

        private static string BuildNetworkSyncId(Transform target)
        {
            // Runtime-spawned network objects can change root sibling indices in a
            // different order on each peer. Names from the serialized hierarchy do
            // not, so use them to make every client resolve the same scene box.
            List<string> hierarchyPath = new List<string>();
            Transform current = target;
            while (current != null)
            {
                hierarchyPath.Add(current.name);
                current = current.parent;
            }

            hierarchyPath.Reverse();
            return $"{target.gameObject.scene.path}:{string.Join("/", hierarchyPath)}";
        }

        private void CacheComponents()
        {
            if (body == null)
            {
                body = GetComponent<Rigidbody2D>();
            }

            if (boxCollider == null)
            {
                boxCollider = GetComponent<BoxCollider2D>();
            }

        }
    }
}
