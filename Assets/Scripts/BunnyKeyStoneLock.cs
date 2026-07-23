using System.Collections.Generic;
using Hanseithon.DualPlaySample;
using UnityEngine;

namespace Hanseithon.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class BunnyKeyStoneLock : MonoBehaviour
    {
        private static readonly Dictionary<string, BunnyKeyStoneLock> ActiveLocks =
            new Dictionary<string, BunnyKeyStoneLock>();

        [SerializeField, Min(0.05f)] private float unlockDistance = 0.55f;
        [SerializeField, Min(0f)] private float nearbyBreakRadius = 10f;
        [SerializeField, Min(0f)] private float serverValidationSlack = 1f;
        [SerializeField, Min(0.05f)] private float requestRetryDelay = 0.25f;

        private string interactionId;
        private bool isUnlocked;
        private float nextRequestTime;

        internal string InteractionId => interactionId;
        internal float NearbyBreakRadius => nearbyBreakRadius;
        internal Vector2 WorldCenter => BunnyKeyInteractionUtility.GetWorldCenter(this);
        internal string ScenePath => gameObject.scene.path;

        private void OnEnable()
        {
            if (isUnlocked)
            {
                return;
            }

            interactionId = BunnyKeyInteractionUtility.BuildStableSceneId(transform);
            ActiveLocks[interactionId] = this;
        }

        private void OnDisable()
        {
            Unregister();
        }

        private void Update()
        {
            if (isUnlocked || Time.unscaledTime < nextRequestTime)
            {
                return;
            }

            if (!BunnyKeyInteractionUtility.IsNetworkSessionActive)
            {
                UnlockForLocalSceneBunny();
                return;
            }

            DualPlayNetworkPlayer localPlayer = DualPlayNetworkPlayer.LocalPlayer;
            if (localPlayer == null ||
                !localPlayer.IsSpawned ||
                !localPlayer.IsOwner ||
                !localPlayer.HasSelectedRole ||
                localPlayer.Role != DualPlayNetworkPlayer.PlayerRole.Bunny)
            {
                return;
            }

            BunnyKeyInventory inventory = localPlayer.GetComponent<BunnyKeyInventory>();
            if (inventory == null ||
                inventory.KeyCount <= 0 ||
                !BunnyKeyInteractionUtility.IsWithinDistance(localPlayer.transform, this, unlockDistance))
            {
                return;
            }

            nextRequestTime = Time.unscaledTime + requestRetryDelay;
            inventory.RequestUnlockKeyStone(interactionId, WorldCenter, ScenePath);
        }

        private void UnlockForLocalSceneBunny()
        {
            if (!BunnyKeyInteractionUtility.TryGetLocalSceneBunny(
                    out Transform bunny,
                    out BunnyLocalKeyInventory inventory) ||
                inventory.KeyCount <= 0 ||
                !BunnyKeyInteractionUtility.IsWithinDistance(bunny, this, unlockDistance) ||
                !inventory.TryConsumeKey())
            {
                return;
            }

            ApplyUnlockedInRadius(WorldCenter, nearbyBreakRadius, ScenePath);
        }

        internal bool IsWithinServerRange(Transform bunny)
        {
            return BunnyKeyInteractionUtility.IsWithinDistance(
                bunny,
                this,
                unlockDistance + serverValidationSlack);
        }

        internal void ApplyUnlocked()
        {
            if (isUnlocked)
            {
                return;
            }

            isUnlocked = true;
            Unregister();
            gameObject.SetActive(false);
        }

        internal static void ApplyUnlockedInRadius(
            Vector2 center,
            float radius,
            string scenePath)
        {
            float radiusSquared = radius * radius;
            List<BunnyKeyStoneLock> targets = new List<BunnyKeyStoneLock>();

            foreach (BunnyKeyStoneLock keyStone in ActiveLocks.Values)
            {
                if (keyStone == null ||
                    !keyStone.gameObject.activeInHierarchy ||
                    keyStone.ScenePath != scenePath ||
                    (keyStone.WorldCenter - center).sqrMagnitude > radiusSquared)
                {
                    continue;
                }

                targets.Add(keyStone);
            }

            for (int i = 0; i < targets.Count; i++)
            {
                targets[i].ApplyUnlocked();
            }
        }

        internal static bool TryGetActive(string id, out BunnyKeyStoneLock keyStone)
        {
            if (ActiveLocks.TryGetValue(id, out keyStone) &&
                keyStone != null &&
                keyStone.gameObject.activeInHierarchy)
            {
                return true;
            }

            ActiveLocks.Remove(id);
            keyStone = null;
            return false;
        }

        internal static bool TryGetNearestActive(
            Vector2 center,
            string scenePath,
            float maxDistance,
            out BunnyKeyStoneLock keyStone)
        {
            keyStone = null;
            float nearestDistanceSquared = maxDistance * maxDistance;

            foreach (BunnyKeyStoneLock candidate in ActiveLocks.Values)
            {
                if (candidate == null ||
                    !candidate.gameObject.activeInHierarchy ||
                    candidate.ScenePath != scenePath)
                {
                    continue;
                }

                float distanceSquared = (candidate.WorldCenter - center).sqrMagnitude;
                if (distanceSquared <= nearestDistanceSquared)
                {
                    keyStone = candidate;
                    nearestDistanceSquared = distanceSquared;
                }
            }

            return keyStone != null;
        }

        private void Unregister()
        {
            if (!string.IsNullOrEmpty(interactionId) &&
                ActiveLocks.TryGetValue(interactionId, out BunnyKeyStoneLock registered) &&
                registered == this)
            {
                ActiveLocks.Remove(interactionId);
            }
        }
    }
}
