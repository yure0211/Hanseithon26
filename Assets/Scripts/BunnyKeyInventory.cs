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

        public void RequestCollectKey(string interactionId)
        {
            if (!CanLocalBunnyRequest() || string.IsNullOrEmpty(interactionId))
            {
                return;
            }

            CollectKeyServerRpc(interactionId);
        }

        public void RequestUnlockKeyStone(string interactionId)
        {
            if (!CanLocalBunnyRequest() || keyCount.Value <= 0 || string.IsNullOrEmpty(interactionId))
            {
                return;
            }

            UnlockKeyStoneServerRpc(interactionId);
        }

        [ServerRpc]
        private void CollectKeyServerRpc(string interactionId, ServerRpcParams rpcParams = default)
        {
            if (!CanServerProcess(rpcParams.Receive.SenderClientId) ||
                !BunnyKeyPickup.TryGetActive(interactionId, out BunnyKeyPickup pickup) ||
                !pickup.IsWithinServerRange(transform))
            {
                return;
            }

            pickup.ApplyCollected();
            keyCount.Value++;
            ApplyKeyCollectedClientRpc(interactionId);
        }

        [ServerRpc]
        private void UnlockKeyStoneServerRpc(string interactionId, ServerRpcParams rpcParams = default)
        {
            if (!CanServerProcess(rpcParams.Receive.SenderClientId) ||
                keyCount.Value <= 0 ||
                !BunnyKeyStoneLock.TryGetActive(interactionId, out BunnyKeyStoneLock keyStone) ||
                !keyStone.IsWithinServerRange(transform))
            {
                return;
            }

            Vector2 center = keyStone.WorldCenter;
            float radius = keyStone.NearbyBreakRadius;
            string scenePath = keyStone.ScenePath;

            keyCount.Value--;
            BunnyKeyStoneLock.ApplyUnlockedInRadius(center, radius, scenePath);
            ApplyKeyStonesUnlockedClientRpc(center, radius, scenePath);
        }

        [ClientRpc]
        private void ApplyKeyCollectedClientRpc(string interactionId)
        {
            if (BunnyKeyPickup.TryGetActive(interactionId, out BunnyKeyPickup pickup))
            {
                pickup.ApplyCollected();
            }
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
