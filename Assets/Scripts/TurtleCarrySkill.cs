using UnityEngine;
using UnityEngine.InputSystem;
using Hanseithon.DualPlaySample;
using Hanseithon.UI;

namespace Hanseithon.Gameplay
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public sealed class TurtleCarrySkill : MonoBehaviour
    {
        [SerializeField] private Key interactionKey = Key.E;
        [SerializeField, Min(0.1f)] private float interactionRadius = 1.75f;
        [SerializeField, Min(0.1f)] private float carryDistance = 1.15f;
        [SerializeField] private bool showControlHint = true;

        private Rigidbody2D body;
        private Collider2D bodyCollider;
        private TurtleCarryableBox heldBox;
        private DualPlayNetworkPlayer networkPlayer;

        public bool IsCarrying => UsesNetworkCarry
            ? networkPlayer.IsCarryingNetworkBox
            : heldBox != null;
        public float InteractionRadius => interactionRadius;
        public float CarryDistance => carryDistance;

        private bool UsesNetworkCarry =>
            networkPlayer != null && networkPlayer.IsNetworkCarryAvailable;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            bodyCollider = GetComponent<Collider2D>();
            networkPlayer = GetComponent<DualPlayNetworkPlayer>();
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            if (keyboard[interactionKey].wasPressedThisFrame)
            {
                if (UsesNetworkCarry)
                {
                    networkPlayer.RequestToggleNetworkCarry();
                }
                else
                {
                    ToggleCarry();
                }
            }
        }

        private void FixedUpdate()
        {
            if (UsesNetworkCarry || heldBox == null)
            {
                return;
            }

            Vector2 headDirection = transform.right;
            Vector2 carryPosition = body.position + headDirection * carryDistance;
            heldBox.MoveWhileHeld(carryPosition);
        }

        private void OnDisable()
        {
            if (!UsesNetworkCarry)
            {
                DropHeldBox();
            }
        }

        private void ToggleCarry()
        {
            if (heldBox != null)
            {
                DropHeldBox();
                return;
            }

            TurtleCarryableBox nearestBox = FindNearestBox();
            if (nearestBox != null && nearestBox.TryPickUp(bodyCollider))
            {
                heldBox = nearestBox;
            }
        }

        private TurtleCarryableBox FindNearestBox()
        {
            Collider2D[] overlaps = Physics2D.OverlapCircleAll(transform.position, interactionRadius);
            TurtleCarryableBox nearestBox = null;
            float nearestDistance = float.PositiveInfinity;

            foreach (Collider2D overlap in overlaps)
            {
                TurtleCarryableBox candidate = overlap.GetComponentInParent<TurtleCarryableBox>();
                if (candidate == null || candidate.IsHeld)
                {
                    continue;
                }

                float distance = ((Vector2)candidate.transform.position - (Vector2)transform.position).sqrMagnitude;
                if (distance < nearestDistance)
                {
                    nearestBox = candidate;
                    nearestDistance = distance;
                }
            }

            return nearestBox;
        }

        private void DropHeldBox()
        {
            if (heldBox == null)
            {
                return;
            }

            TurtleCarryableBox boxToDrop = heldBox;
            heldBox = null;
            boxToDrop.Drop(Vector2.zero);
        }

        private void OnGUI()
        {
            if (!showControlHint)
            {
                return;
            }

            string action = IsCarrying ? "상자 내려놓기" : "가까운 상자 들기";
            Matrix4x4 previousMatrix = DualPlayUiTheme.BeginCanvas(false);
            GUI.Label(
                new Rect(20f, DualPlayUiTheme.VirtualHeight - 70f, 340f, 50f),
                $"거북이 스킬  ·  {interactionKey}  ·  {action}",
                DualPlayUiTheme.HintStyle);
            DualPlayUiTheme.EndCanvas(previousMatrix);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 0.9f, 0.45f, 0.8f);
            Gizmos.DrawWireSphere(transform.position, interactionRadius);
        }
    }
}
