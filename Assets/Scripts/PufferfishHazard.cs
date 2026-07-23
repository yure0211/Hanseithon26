using System;
using UnityEngine;

namespace Hanseithon.Gameplay
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class PufferfishHazard : MonoBehaviour
    {
        [SerializeField] private bool randomizeStartingDirection = true;
        [SerializeField] private bool hideTurtleOnHit = true;

        private BoxCollider2D hazardCollider;

        public bool HasTriggeredGameOver { get; private set; }

        public event Action<GameObject> TurtleDied;

        private void Awake()
        {
            hazardCollider = GetComponent<BoxCollider2D>();
            hazardCollider.isTrigger = true;
        }

        private void Start()
        {
            if (!randomizeStartingDirection)
            {
                return;
            }

            int directionIndex = UnityEngine.Random.Range(0, 4);
            transform.rotation = Quaternion.Euler(0f, 0f, directionIndex * 90f);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (HasTriggeredGameOver)
            {
                return;
            }

            TurtleController turtleController = other.GetComponentInParent<TurtleController>();
            if (turtleController == null || !turtleController.isActiveAndEnabled)
            {
                return;
            }

            TriggerGameOver(turtleController.gameObject);
        }

        private void TriggerGameOver(GameObject turtle)
        {
            HasTriggeredGameOver = true;

            TurtleController turtleController = turtle.GetComponent<TurtleController>();
            if (turtleController != null)
            {
                turtleController.enabled = false;
            }

            TurtleCarrySkill carrySkill = turtle.GetComponent<TurtleCarrySkill>();
            if (carrySkill != null)
            {
                carrySkill.enabled = false;
            }

            Rigidbody2D body = turtle.GetComponent<Rigidbody2D>();
            if (body != null)
            {
                body.linearVelocity = Vector2.zero;
                body.angularVelocity = 0f;
                body.simulated = false;
            }

            Collider2D[] colliders = turtle.GetComponentsInChildren<Collider2D>();
            for (int i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = false;
            }

            if (hideTurtleOnHit)
            {
                SpriteRenderer[] renderers = turtle.GetComponentsInChildren<SpriteRenderer>();
                for (int i = 0; i < renderers.Length; i++)
                {
                    renderers[i].enabled = false;
                }
            }

            Animator turtleAnimator = turtle.GetComponent<Animator>();
            if (turtleAnimator != null)
            {
                turtleAnimator.enabled = false;
            }

            TurtleDied?.Invoke(turtle);
            Time.timeScale = 0f;
            Debug.Log($"{turtle.name} touched a pufferfish. Game over.", this);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
            if (boxCollider != null)
            {
                boxCollider.isTrigger = true;
            }
        }
#endif
    }
}
