using System;
using UnityEngine;

namespace Hanseithon.Gameplay
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class RisingLava : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float riseSpeed = 0.35f;
        [SerializeField] private LayerMask playerLayer = 1 << 7;
        [SerializeField] private bool hideDeadPlayer = true;

        private BoxCollider2D lavaCollider;

        public bool IsRising { get; private set; } = true;
        public GameObject DeadPlayer { get; private set; }

        public event Action<GameObject> PlayerDied;

        private void Awake()
        {
            lavaCollider = GetComponent<BoxCollider2D>();
            lavaCollider.isTrigger = true;
        }

        private void Update()
        {
            if (!IsRising)
            {
                return;
            }

            transform.position += Vector3.up * (riseSpeed * Time.deltaTime);
            CheckForSubmergedPlayer();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryKillPlayer(FindPlayerRoot(other));
        }

        private void CheckForSubmergedPlayer()
        {
            GameObject bunny = FindActivePlayer("NetworkBunny", "Bunny");
            GameObject turtle = FindActivePlayer("NetworkTurtle", "Turtle");

            GameObject victim = null;
            float lowestPoint = float.PositiveInfinity;
            ConsiderAsVictim(bunny, ref victim, ref lowestPoint);
            ConsiderAsVictim(turtle, ref victim, ref lowestPoint);

            if (victim != null)
            {
                TryKillPlayer(victim);
            }
        }

        private void ConsiderAsVictim(GameObject player, ref GameObject victim, ref float lowestPoint)
        {
            if (!IsValidPlayer(player) || !IsInsideLava(player, out float playerBottom))
            {
                return;
            }

            if (playerBottom < lowestPoint)
            {
                victim = player;
                lowestPoint = playerBottom;
            }
        }

        private bool IsInsideLava(GameObject player, out float playerBottom)
        {
            Bounds lavaBounds = lavaCollider.bounds;
            SpriteRenderer playerRenderer = player.GetComponent<SpriteRenderer>();
            Collider2D playerCollider = player.GetComponent<Collider2D>();

            Bounds playerBounds;
            if (playerRenderer != null && playerRenderer.enabled && playerRenderer.sprite != null)
            {
                playerBounds = playerRenderer.bounds;
            }
            else if (playerCollider != null && playerCollider.enabled)
            {
                playerBounds = playerCollider.bounds;
            }
            else
            {
                playerBounds = new Bounds(player.transform.position, Vector3.zero);
            }

            playerBottom = playerBounds.min.y;
            bool overlapsHorizontally = playerBounds.max.x >= lavaBounds.min.x &&
                                        playerBounds.min.x <= lavaBounds.max.x;
            return overlapsHorizontally && playerBottom <= lavaBounds.max.y;
        }

        private void TryKillPlayer(GameObject player)
        {
            if (!IsRising || !IsValidPlayer(player))
            {
                return;
            }

            IsRising = false;
            DeadPlayer = player;

            BunnyController bunnyController = player.GetComponent<BunnyController>();
            if (bunnyController != null)
            {
                bunnyController.enabled = false;
            }

            TurtleController turtleController = player.GetComponent<TurtleController>();
            if (turtleController != null)
            {
                turtleController.enabled = false;
            }

            TurtleCarrySkill carrySkill = player.GetComponent<TurtleCarrySkill>();
            if (carrySkill != null)
            {
                carrySkill.enabled = false;
            }

            Rigidbody2D body = player.GetComponent<Rigidbody2D>();
            if (body != null)
            {
                body.linearVelocity = Vector2.zero;
                body.angularVelocity = 0f;
                body.simulated = false;
            }

            Collider2D[] colliders = player.GetComponentsInChildren<Collider2D>();
            for (int i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = false;
            }

            if (hideDeadPlayer)
            {
                SpriteRenderer[] renderers = player.GetComponentsInChildren<SpriteRenderer>();
                for (int i = 0; i < renderers.Length; i++)
                {
                    renderers[i].enabled = false;
                }
            }

            Animator playerAnimator = player.GetComponent<Animator>();
            if (playerAnimator != null)
            {
                playerAnimator.enabled = false;
            }

            PlayerDied?.Invoke(player);
            Time.timeScale = 0f;
            Debug.Log($"{player.name} fell into the lava. Rising lava stopped.", this);
        }

        private bool IsValidPlayer(GameObject candidate)
        {
            if (candidate == null || !candidate.activeInHierarchy ||
                (playerLayer.value & (1 << candidate.layer)) == 0)
            {
                return false;
            }

            return candidate.GetComponent<BunnyController>() != null ||
                   candidate.GetComponent<TurtleController>() != null;
        }

        private static GameObject FindPlayerRoot(Collider2D other)
        {
            if (other == null)
            {
                return null;
            }

            BunnyController bunnyController = other.GetComponentInParent<BunnyController>();
            if (bunnyController != null)
            {
                return bunnyController.gameObject;
            }

            TurtleController turtleController = other.GetComponentInParent<TurtleController>();
            return turtleController != null ? turtleController.gameObject : null;
        }

        private static GameObject FindActivePlayer(string networkName, string fallbackName)
        {
            GameObject player = GameObject.Find(networkName);
            return player != null ? player : GameObject.Find(fallbackName);
        }
    }
}
