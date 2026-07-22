using System.Collections.Generic;
using Hanseithon.DualPlaySample;
using UnityEngine;

namespace Hanseithon.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class BunnyKeyPickup : MonoBehaviour
    {
        private static readonly Dictionary<string, BunnyKeyPickup> ActivePickups =
            new Dictionary<string, BunnyKeyPickup>();

        [SerializeField, Min(0.05f)] private float pickupDistance = 0.55f;
        [SerializeField, Min(0f)] private float serverValidationSlack = 1f;
        [SerializeField, Min(0.05f)] private float requestRetryDelay = 0.25f;

        private string interactionId;
        private bool isCollected;
        private float nextRequestTime;

        internal string InteractionId => interactionId;

        private void OnEnable()
        {
            if (isCollected)
            {
                return;
            }

            interactionId = BunnyKeyInteractionUtility.BuildStableSceneId(transform);
            ActivePickups[interactionId] = this;
        }

        private void OnDisable()
        {
            Unregister();
        }

        private void Update()
        {
            if (isCollected || Time.unscaledTime < nextRequestTime)
            {
                return;
            }

            if (!BunnyKeyInteractionUtility.IsNetworkSessionActive)
            {
                CollectForLocalSceneBunny();
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
                !BunnyKeyInteractionUtility.IsWithinDistance(localPlayer.transform, this, pickupDistance))
            {
                return;
            }

            nextRequestTime = Time.unscaledTime + requestRetryDelay;
            inventory.RequestCollectKey(interactionId);
        }

        private void CollectForLocalSceneBunny()
        {
            if (!BunnyKeyInteractionUtility.TryGetLocalSceneBunny(
                    out Transform bunny,
                    out BunnyLocalKeyInventory inventory) ||
                !BunnyKeyInteractionUtility.IsWithinDistance(bunny, this, pickupDistance))
            {
                return;
            }

            inventory.AddKey();
            ApplyCollected();
        }

        internal bool IsWithinServerRange(Transform bunny)
        {
            return BunnyKeyInteractionUtility.IsWithinDistance(
                bunny,
                this,
                pickupDistance + serverValidationSlack);
        }

        internal void ApplyCollected()
        {
            if (isCollected)
            {
                return;
            }

            isCollected = true;
            Unregister();
            gameObject.SetActive(false);
        }

        internal static bool TryGetActive(string id, out BunnyKeyPickup pickup)
        {
            if (ActivePickups.TryGetValue(id, out pickup) &&
                pickup != null &&
                pickup.gameObject.activeInHierarchy)
            {
                return true;
            }

            ActivePickups.Remove(id);
            pickup = null;
            return false;
        }

        private void Unregister()
        {
            if (!string.IsNullOrEmpty(interactionId) &&
                ActivePickups.TryGetValue(interactionId, out BunnyKeyPickup registered) &&
                registered == this)
            {
                ActivePickups.Remove(interactionId);
            }
        }
    }
}
