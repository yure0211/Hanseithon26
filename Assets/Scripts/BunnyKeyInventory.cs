using Hanseithon.DualPlaySample;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Hanseithon.Gameplay
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(DualPlayNetworkPlayer))]
    public sealed class BunnyKeyInventory : NetworkBehaviour
    {
        private readonly NetworkVariable<int> keyCount = new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        private DualPlayNetworkPlayer networkPlayer;

        public int KeyCount => keyCount.Value;

        private void Awake()
        {
            networkPlayer = GetComponent<DualPlayNetworkPlayer>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            SceneManager.activeSceneChanged += HandleActiveSceneChanged;
        }

        public override void OnNetworkDespawn()
        {
            SceneManager.activeSceneChanged -= HandleActiveSceneChanged;
            base.OnNetworkDespawn();
        }

        public void RequestCollectKey(
            string interactionId,
            Vector2 worldCenter,
            string scenePath)
        {
            if (!CanLocalBunnyRequest() || string.IsNullOrEmpty(scenePath))
            {
                return;
            }

            CollectKeyServerRpc(interactionId, worldCenter, scenePath);
        }

        public void RequestUnlockKeyStone(
            string interactionId,
            Vector2 worldCenter,
            string scenePath)
        {
            if (!CanLocalBunnyRequest() || keyCount.Value <= 0 || string.IsNullOrEmpty(scenePath))
            {
                return;
            }

            UnlockKeyStoneServerRpc(interactionId, worldCenter, scenePath);
        }

        [ServerRpc]
        private void CollectKeyServerRpc(
            string interactionId,
            Vector2 worldCenter,
            string scenePath,
            ServerRpcParams rpcParams = default)
        {
            if (!CanServerProcess(rpcParams.Receive.SenderClientId))
            {
                return;
            }

            bool foundPickup = BunnyKeyPickup.TryGetActive(
                interactionId,
                out BunnyKeyPickup pickup);
            if (!foundPickup)
            {
                foundPickup = BunnyKeyPickup.TryGetNearestActive(
                    worldCenter,
                    scenePath,
                    0.75f,
                    out pickup);
            }

            if (!foundPickup || !pickup.IsWithinServerRange(transform))
            {
                return;
            }

            Vector2 authoritativeCenter = pickup.WorldCenter;
            string authoritativeScenePath = pickup.ScenePath;
            pickup.ApplyCollected();
            keyCount.Value++;
            ApplyKeyCollectedClientRpc(authoritativeCenter, authoritativeScenePath);
        }

        [ServerRpc]
        private void UnlockKeyStoneServerRpc(
            string interactionId,
            Vector2 worldCenter,
            string scenePath,
            ServerRpcParams rpcParams = default)
        {
            if (!CanServerProcess(rpcParams.Receive.SenderClientId) || keyCount.Value <= 0)
            {
                return;
            }

            bool foundKeyStone = BunnyKeyStoneLock.TryGetActive(
                interactionId,
                out BunnyKeyStoneLock keyStone);
            if (!foundKeyStone)
            {
                foundKeyStone = BunnyKeyStoneLock.TryGetNearestActive(
                    worldCenter,
                    scenePath,
                    0.75f,
                    out keyStone);
            }

            if (!foundKeyStone || !keyStone.IsWithinServerRange(transform))
            {
                return;
            }

            Vector2 center = keyStone.WorldCenter;
            float radius = keyStone.NearbyBreakRadius;
            string authoritativeScenePath = keyStone.ScenePath;

            keyCount.Value--;
            BunnyKeyStoneLock.ApplyUnlockedInRadius(center, radius, authoritativeScenePath);
            ApplyKeyStonesUnlockedClientRpc(center, radius, authoritativeScenePath);
        }

        [ClientRpc]
        private void ApplyKeyCollectedClientRpc(Vector2 center, string scenePath)
        {
            BunnyKeyPickup.ApplyCollectedNear(center, scenePath);
        }

        [ClientRpc]
        private void ApplyKeyStonesUnlockedClientRpc(
            Vector2 center,
            float radius,
            string scenePath)
        {
            BunnyKeyStoneLock.ApplyUnlockedInRadius(center, radius, scenePath);
        }

        private bool CanLocalBunnyRequest()
        {
            return IsSpawned &&
                   IsOwner &&
                   networkPlayer != null &&
                   networkPlayer.HasSelectedRole &&
                   networkPlayer.Role == DualPlayNetworkPlayer.PlayerRole.Bunny;
        }

        private bool CanServerProcess(ulong senderClientId)
        {
            return IsServer &&
                   IsSpawned &&
                   OwnerClientId == senderClientId &&
                   networkPlayer != null &&
                   networkPlayer.HasSelectedRole &&
                   networkPlayer.Role == DualPlayNetworkPlayer.PlayerRole.Bunny;
        }

        private void HandleActiveSceneChanged(Scene previousScene, Scene newScene)
        {
            if (IsServer)
            {
                keyCount.Value = 0;
            }
        }
    }
}
