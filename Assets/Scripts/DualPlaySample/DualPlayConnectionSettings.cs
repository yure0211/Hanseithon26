using UnityEngine;

namespace Hanseithon.DualPlaySample
{
    [CreateAssetMenu(
        fileName = "DualPlayConnectionSettings",
        menuName = "Hanseithon/Networking/Dual Play Connection Settings")]
    public sealed class DualPlayConnectionSettings : ScriptableObject
    {
        private const string LastAddressKey = "Hanseithon.DualPlay.LastAddress";
        private const string LastModeKey = "Hanseithon.DualPlay.LastMode";

        [Header("Connection")]
        [SerializeField] private string defaultAddress = "127.0.0.1";
        [SerializeField, Min(1)] private int port = 7777;
        [SerializeField, Min(2)] private int maximumPlayers = 2;

        [Header("Shared runtime")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private bool persistAcrossScenes = true;
        [SerializeField] private bool rememberLastAddress = true;

        [Header("Scene flow")]
        [SerializeField] private string mainMenuSceneName = "MainMenuScene";
        [SerializeField] private string connectionSceneName = "DualPlayConnectionTestScene";
        [SerializeField] private string characterSelectSceneName = "CharacterSelectScene";
        [SerializeField] private string levelSceneName = "Level";
        [SerializeField] private string gameplaySceneName = "InGame";
        [SerializeField] private bool autoStartGameWhenFull = true;
        [SerializeField, Min(0f)] private float autoStartDelay = 1f;

        public string DefaultAddress => NormalizeAddress(defaultAddress);
        public ushort Port => (ushort)Mathf.Clamp(port, 1, ushort.MaxValue);
        public int MaximumPlayers => Mathf.Max(2, maximumPlayers);
        public GameObject PlayerPrefab => playerPrefab;
        public bool PersistAcrossScenes => persistAcrossScenes;
        public bool RememberLastAddress => rememberLastAddress;
        public string MainMenuSceneName => NormalizeSceneName(mainMenuSceneName, "MainMenuScene");
        public string ConnectionSceneName => NormalizeSceneName(connectionSceneName, "DualPlayConnectionTestScene");
        public string CharacterSelectSceneName => NormalizeSceneName(characterSelectSceneName, "CharacterSelectScene");
        public string LevelSceneName => NormalizeSceneName(levelSceneName, "Level");
        public string GameplaySceneName => NormalizeSceneName(gameplaySceneName, "InGame");
        public bool AutoStartGameWhenFull => autoStartGameWhenFull;
        public float AutoStartDelay => Mathf.Max(0f, autoStartDelay);
        public string LastSelectedMode => PlayerPrefs.GetString(LastModeKey, "None");

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

        public void SaveSessionSelection(string value, bool isHost)
        {
            SaveAddress(value);
            PlayerPrefs.SetString(LastModeKey, isHost ? "Host" : "Client");
            PlayerPrefs.Save();
        }

        private static string NormalizeAddress(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "127.0.0.1" : value.Trim();
        }

        private static string NormalizeSceneName(string value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        }

        private void OnValidate()
        {
            defaultAddress = NormalizeAddress(defaultAddress);
            port = Mathf.Clamp(port, 1, ushort.MaxValue);
            maximumPlayers = Mathf.Max(2, maximumPlayers);
            mainMenuSceneName = NormalizeSceneName(mainMenuSceneName, "MainMenuScene");
            connectionSceneName = NormalizeSceneName(connectionSceneName, "DualPlayConnectionTestScene");
            characterSelectSceneName = NormalizeSceneName(characterSelectSceneName, "CharacterSelectScene");
            levelSceneName = NormalizeSceneName(levelSceneName, "Level");
            gameplaySceneName = NormalizeSceneName(gameplaySceneName, "InGame");
            autoStartDelay = Mathf.Max(0f, autoStartDelay);
        }
    }
}
