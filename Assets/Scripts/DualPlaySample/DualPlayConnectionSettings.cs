using UnityEngine;

namespace Hanseithon.DualPlaySample
{
    [CreateAssetMenu(
        fileName = "DualPlayConnectionSettings",
        menuName = "Hanseithon/Networking/Dual Play Connection Settings")]
    public sealed class DualPlayConnectionSettings : ScriptableObject
    {
        private const string LastAddressKey = "Hanseithon.DualPlay.LastAddress";

        [Header("Connection")]
        [SerializeField] private string defaultAddress = "127.0.0.1";
        [SerializeField, Min(1)] private int port = 7777;
        [SerializeField, Min(2)] private int maximumPlayers = 2;

        [Header("Shared runtime")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private bool persistAcrossScenes = true;
        [SerializeField] private bool rememberLastAddress = true;

        public string DefaultAddress => NormalizeAddress(defaultAddress);
        public ushort Port => (ushort)Mathf.Clamp(port, 1, ushort.MaxValue);
        public int MaximumPlayers => Mathf.Max(2, maximumPlayers);
        public GameObject PlayerPrefab => playerPrefab;
        public bool PersistAcrossScenes => persistAcrossScenes;
        public bool RememberLastAddress => rememberLastAddress;

        public string LoadAddress()
        {
            if (!rememberLastAddress)
            {
                return DefaultAddress;
            }

            return NormalizeAddress(PlayerPrefs.GetString(LastAddressKey, DefaultAddress));
        }

        public void SaveAddress(string value)
        {
            if (!rememberLastAddress)
            {
                return;
            }

            PlayerPrefs.SetString(LastAddressKey, NormalizeAddress(value));
            PlayerPrefs.Save();
        }

        private static string NormalizeAddress(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "127.0.0.1" : value.Trim();
        }

        private void OnValidate()
        {
            defaultAddress = NormalizeAddress(defaultAddress);
            port = Mathf.Clamp(port, 1, ushort.MaxValue);
            maximumPlayers = Mathf.Max(2, maximumPlayers);
        }
    }
}