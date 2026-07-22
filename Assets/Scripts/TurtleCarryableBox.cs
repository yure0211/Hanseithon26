using UnityEngine;

namespace Hanseithon.Gameplay
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class TurtleCarryableBox : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float heldScaleMultiplier = 1.15f;

        private Rigidbody2D body;
        private BoxCollider2D boxCollider;
        private Collider2D ignoredCarrierCollider;
        private RigidbodyType2D originalBodyType;
        private RigidbodyConstraints2D originalConstraints;
        private float originalGravityScale;
        private float originalLinearDamping;
        private Vector3 originalScale;

        public bool IsHeld { get; private set; }

        private void Awake()
        {
            CacheComponents();
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
            originalScale = transform.localScale;
            ignoredCarrierCollider = carrierCollider;

            IsHeld = true;
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
            body.bodyType = originalBodyType;
            body.gravityScale = originalGravityScale;
            body.linearDamping = originalLinearDamping;
            body.constraints = originalConstraints;
            body.linearVelocity = releaseVelocity;
            transform.localScale = originalScale;
        }

        private void OnDisable()
        {
            if (IsHeld)
            {
                Drop(Vector2.zero);
            }
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
