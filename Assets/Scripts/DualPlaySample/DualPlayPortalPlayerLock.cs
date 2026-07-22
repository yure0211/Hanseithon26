using System.Collections;
using Hanseithon.Gameplay;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Hanseithon.DualPlaySample
{
    [DefaultExecutionOrder(32000)]
    [DisallowMultipleComponent]
    public sealed class DualPlayPortalPlayerLock : MonoBehaviour
    {
        private Rigidbody2D body;
        private Collider2D[] colliders;
        private bool[] colliderEnabledStates;
        private BunnyController bunnyController;
        private TurtleController turtleController;
        private TurtleCarrySkill turtleCarrySkill;
        private SpriteRenderer playerRenderer;

        private bool bodyWasSimulated;
        private bool bunnyWasEnabled;
        private bool turtleWasEnabled;
        private bool carryWasEnabled;
        private bool rendererWasEnabled;
        private bool restoreGameplayState;
        private bool isLocked;
        private Vector2 holdPosition;

        public void Lock(Vector2 position, bool restoreStateOnSceneChange)
        {
            holdPosition = position;
            restoreGameplayState = restoreStateOnSceneChange;

            if (!isLocked)
            {
                CacheAndDisablePlayerState();
                SceneManager.activeSceneChanged += HandleActiveSceneChanged;
                isLocked = true;
            }

            HoldPlayerInsidePortal();
        }

        private void FixedUpdate()
        {
            if (isLocked)
            {
                HoldPlayerInsidePortal();
            }
        }

        private void LateUpdate()
        {
            if (isLocked)
            {
                transform.position = new Vector3(
                    holdPosition.x,
                    holdPosition.y,
                    transform.position.z);
            }
        }

        private void CacheAndDisablePlayerState()
        {
            body = GetComponent<Rigidbody2D>();
            colliders = GetComponents<Collider2D>();
            colliderEnabledStates = new bool[colliders.Length];
            for (int i = 0; i < colliders.Length; i++)
            {
                colliderEnabledStates[i] = colliders[i].enabled;
                colliders[i].enabled = false;
            }

            bunnyController = GetComponent<BunnyController>();
            turtleController = GetComponent<TurtleController>();
            turtleCarrySkill = GetComponent<TurtleCarrySkill>();
            playerRenderer = GetComponent<SpriteRenderer>();

            if (bunnyController != null)
            {
                bunnyWasEnabled = bunnyController.enabled;
                bunnyController.enabled = false;
            }
            if (turtleController != null)
            {
                turtleWasEnabled = turtleController.enabled;
                turtleController.enabled = false;
            }
            if (turtleCarrySkill != null)
            {
                carryWasEnabled = turtleCarrySkill.enabled;
                turtleCarrySkill.enabled = false;
            }
            if (playerRenderer != null)
            {
                rendererWasEnabled = playerRenderer.enabled;
            }
            if (body != null)
            {
                bodyWasSimulated = body.simulated;
                body.linearVelocity = Vector2.zero;
                body.angularVelocity = 0f;
                body.simulated = false;
            }
        }

        private void HoldPlayerInsidePortal()
        {
            if (body != null)
            {
                body.linearVelocity = Vector2.zero;
                body.angularVelocity = 0f;
                body.position = holdPosition;
            }

            transform.position = new Vector3(
                holdPosition.x,
                holdPosition.y,
                transform.position.z);
        }

        private void HandleActiveSceneChanged(Scene previousScene, Scene newScene)
        {
            StartCoroutine(ReleaseAfterSceneSetup());
        }

        private IEnumerator ReleaseAfterSceneSetup()
        {
            yield return null;

            if (restoreGameplayState)
            {
                if (bunnyController != null) bunnyController.enabled = bunnyWasEnabled;
                if (turtleController != null) turtleController.enabled = turtleWasEnabled;
                if (turtleCarrySkill != null) turtleCarrySkill.enabled = carryWasEnabled;
                if (playerRenderer != null) playerRenderer.enabled = rendererWasEnabled;

                if (colliders != null && colliderEnabledStates != null)
                {
                    for (int i = 0; i < colliders.Length; i++)
                    {
                        if (colliders[i] != null)
                        {
                            colliders[i].enabled = colliderEnabledStates[i];
                        }
                    }
                }

                if (body != null)
                {
                    body.simulated = bodyWasSimulated;
                    body.linearVelocity = Vector2.zero;
                }
            }

            isLocked = false;
            SceneManager.activeSceneChanged -= HandleActiveSceneChanged;
            Destroy(this);
        }

        private void OnDestroy()
        {
            SceneManager.activeSceneChanged -= HandleActiveSceneChanged;
        }
    }
}
