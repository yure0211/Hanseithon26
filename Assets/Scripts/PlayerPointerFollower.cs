using UnityEngine;

namespace Hanseithon.Gameplay
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class PlayerPointerFollower : MonoBehaviour
    {
        [SerializeField] private bool isRabbit;
        [SerializeField, Min(0f)] private float verticalGap = 0.05f;
        [SerializeField] private Vector3 fallbackOffset = new Vector3(0f, 0.75f, 0f);
        [SerializeField] private int sortingOrder = 20;

        private static readonly int IsRabbitParameter = Animator.StringToHash("IsRabbit");

        private Animator pointerAnimator;
        private SpriteRenderer pointerRenderer;
        private Transform target;

        private void Awake()
        {
            pointerAnimator = GetComponent<Animator>();
            pointerRenderer = GetComponent<SpriteRenderer>();

            pointerAnimator.SetBool(IsRabbitParameter, isRabbit);
            pointerRenderer.sortingOrder = sortingOrder;
            ResolveTarget();
        }

        private void LateUpdate()
        {
            if (!IsCurrentTargetValid())
            {
                ResolveTarget();
            }

            bool hasTarget = target != null && target.gameObject.activeInHierarchy;
            pointerRenderer.enabled = hasTarget;
            if (!hasTarget)
            {
                return;
            }

            transform.rotation = Quaternion.identity;
            transform.position = CalculatePointerPosition();
        }

        private Vector3 CalculatePointerPosition()
        {
            Vector3 pointerPosition = target.position + fallbackOffset;
            SpriteRenderer targetRenderer = target.GetComponent<SpriteRenderer>();
            if (targetRenderer == null || !targetRenderer.enabled || targetRenderer.sprite == null ||
                pointerRenderer.sprite == null)
            {
                return pointerPosition;
            }

            Bounds targetBounds = targetRenderer.bounds;
            pointerPosition.x = targetBounds.center.x;
            pointerPosition.y = targetBounds.max.y + verticalGap + pointerRenderer.bounds.extents.y;
            pointerPosition.z = target.position.z;
            return pointerPosition;
        }

        private bool IsCurrentTargetValid()
        {
            if (target == null || !target.gameObject.activeInHierarchy)
            {
                return false;
            }

            string expectedName = isRabbit ? "NetworkBunny" : "NetworkTurtle";
            string fallbackName = isRabbit ? "Bunny" : "Turtle";
            return target.name == expectedName || target.name == fallbackName;
        }

        private void ResolveTarget()
        {
            string networkTargetName = isRabbit ? "NetworkBunny" : "NetworkTurtle";
            GameObject targetObject = GameObject.Find(networkTargetName);
            if (targetObject == null)
            {
                targetObject = GameObject.Find(isRabbit ? "Bunny" : "Turtle");
            }

            target = targetObject != null ? targetObject.transform : null;
        }
    }
}
