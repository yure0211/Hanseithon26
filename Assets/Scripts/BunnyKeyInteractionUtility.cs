using System.Collections.Generic;
using Hanseithon.DualPlaySample;
using Unity.Netcode;
using UnityEngine;

namespace Hanseithon.Gameplay
{
    internal static class BunnyKeyInteractionUtility
    {
        private static BunnyLocalKeyInventory cachedLocalInventory;

        public static bool IsNetworkSessionActive =>
            NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;

        public static string BuildStableSceneId(Transform target)
        {
            List<string> hierarchyPath = new List<string>();
            Transform current = target;

            while (current != null)
            {
                Vector3 localPosition = current.localPosition;
                hierarchyPath.Add(
                    $"{current.name}[" +
                    $"{Mathf.RoundToInt(localPosition.x * 1000f)}," +
                    $"{Mathf.RoundToInt(localPosition.y * 1000f)}," +
                    $"{Mathf.RoundToInt(localPosition.z * 1000f)}]");
                current = current.parent;
            }

            hierarchyPath.Reverse();
            return $"{target.gameObject.scene.path}:{string.Join("/", hierarchyPath)}";
        }

        public static bool IsWithinDistance(Transform actor, Component target, float distance)
        {
            Vector2 actorPosition = actor.position;
            Vector2 closestPoint;

            Collider2D targetCollider = target.GetComponent<Collider2D>();
            if (targetCollider != null && targetCollider.enabled)
            {
                closestPoint = targetCollider.ClosestPoint(actorPosition);
            }
            else
            {
                Renderer targetRenderer = target.GetComponent<Renderer>();
                closestPoint = targetRenderer != null
                    ? (Vector2)targetRenderer.bounds.ClosestPoint(actorPosition)
                    : (Vector2)target.transform.position;
            }

            return (actorPosition - closestPoint).sqrMagnitude <= distance * distance;
        }

        public static Vector2 GetWorldCenter(Component target)
        {
            Collider2D targetCollider = target.GetComponent<Collider2D>();
            if (targetCollider != null && targetCollider.enabled)
            {
                return targetCollider.bounds.center;
            }

            Renderer targetRenderer = target.GetComponent<Renderer>();
            return targetRenderer != null
                ? (Vector2)targetRenderer.bounds.center
                : (Vector2)target.transform.position;
        }

        public static bool TryGetLocalSceneBunny(
            out Transform bunny,
            out BunnyLocalKeyInventory inventory)
        {
            bunny = null;
            inventory = null;

            if (IsNetworkSessionActive)
            {
                return false;
            }

            if (cachedLocalInventory != null && cachedLocalInventory.gameObject.activeInHierarchy)
            {
                bunny = cachedLocalInventory.transform;
                inventory = cachedLocalInventory;
                return true;
            }

            BunnyController[] bunnyControllers =
                Object.FindObjectsByType<BunnyController>(FindObjectsSortMode.None);

            for (int i = 0; i < bunnyControllers.Length; i++)
            {
                BunnyController controller = bunnyControllers[i];
                if (!controller.isActiveAndEnabled ||
                    controller.GetComponent<DualPlayNetworkPlayer>() != null)
                {
                    continue;
                }

                cachedLocalInventory = controller.GetComponent<BunnyLocalKeyInventory>();
                if (cachedLocalInventory == null)
                {
                    cachedLocalInventory = controller.gameObject.AddComponent<BunnyLocalKeyInventory>();
                }

                bunny = controller.transform;
                inventory = cachedLocalInventory;
                return true;
            }

            return false;
        }
    }
}
