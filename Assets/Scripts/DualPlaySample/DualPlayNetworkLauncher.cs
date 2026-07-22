using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Hanseithon.DualPlaySample
{
    [DefaultExecutionOrder(-1000)]
    [RequireComponent(typeof(NetworkManager))]
    [RequireComponent(typeof(UnityTransport))]
    public sealed class DualPlayNetworkLauncher : MonoBehaviour
    {
        [SerializeField] private NetworkManager networkManager;
        [SerializeField] private UnityTransport transport;
        [SerializeField] private DualPlayConnectionSettings connectionSettings;

        private static DualPlayNetworkLauncher persistentInstance;

        private string address;
        private string statusMessage = "Choose Host or Client.";
        private bool isPrimaryInstance = true;
        private bool isStartingGame;
        private GUIStyle headerStyle;
        private GUIStyle wrappedLabelStyle;

        private void Awake()
        {
            Application.runInBackground = true;

            if (networkManager == null)
            {
                networkManager = GetComponent<NetworkManager>();
            }
            if (transport == null)
            {
                transport = GetComponent<UnityTransport>();
            }

            if (connectionSettings == null)
            {
                isPrimaryInstance = false;
                enabled = false;
                Debug.LogError("DualPlayConnectionSettings is not assigned.", this);
                return;
            }

            address = connectionSettings.LoadAddress();

            if (!connectionSettings.PersistAcrossScenes)
            {
                return;
            }

            if (persistentInstance != null && persistentInstance != this)
            {
                isPrimaryInstance = false;
                gameObject.SetActive(false);
                Destroy(gameObject);
                return;
            }

            persistentInstance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            if (!isPrimaryInstance || networkManager == null)
            {
                return;
            }

            networkManager.OnClientConnectedCallback += HandleClientConnected;
            networkManager.OnClientDisconnectCallback += HandleClientDisconnected;
            SceneManager.activeSceneChanged += HandleActiveSceneChanged;
        }

        private void Start()
        {
            string[] arguments = System.Environment.GetCommandLineArgs();
            bool startAsHost = false;
            bool startAsClient = false;

            for (int i = 0; i < arguments.Length; i++)
            {
                string argument = arguments[i];
                if (argument == "-dualPlayHost")
                {
                    startAsHost = true;
                }
                else if (argument == "-dualPlayClient")
                {
                    startAsClient = true;
                }
                else if (argument.StartsWith("-dualPlayAddress="))
                {
                    address = argument.Substring("-dualPlayAddress=".Length);
                }
            }

            if (startAsHost)
            {
                StartHost();
            }
            else if (startAsClient)
            {
                StartClient();
            }
        }

        private void Update()
        {
            if (!isPrimaryInstance ||
                connectionSettings == null ||
                networkManager == null ||
                !networkManager.IsHost ||
                !networkManager.IsListening ||
                isStartingGame ||
                SceneManager.GetActiveScene().name != connectionSettings.CharacterSelectSceneName)
            {
                return;
            }

            if (DualPlayNetworkPlayer.AreAllPlayersReady(connectionSettings.MaximumPlayers))
            {
                StartCoroutine(StartLevelAfterCharacterSelection());
            }
        }

        private void OnDisable()
        {
            if (!isPrimaryInstance || networkManager == null)
            {
                return;
            }

            networkManager.OnClientConnectedCallback -= HandleClientConnected;
            networkManager.OnClientDisconnectCallback -= HandleClientDisconnected;
            SceneManager.activeSceneChanged -= HandleActiveSceneChanged;
        }

        private void OnDestroy()
        {
            if (persistentInstance == this)
            {
                persistentInstance = null;
            }
        }

        private void OnApplicationQuit()
        {
            if (isPrimaryInstance && connectionSettings != null)
            {
                connectionSettings.SaveAddress(address);
            }
        }

        private void OnGUI()
        {
            if (!isPrimaryInstance || connectionSettings == null)
            {
                return;
            }

            string activeScene = SceneManager.GetActiveScene().name;
            if (activeScene == connectionSettings.ConnectionSceneName)
            {
                DrawLobbyGui();
            }
            else if (activeScene == connectionSettings.CharacterSelectSceneName &&
                     networkManager.IsListening)
            {
                DrawCharacterSelectGui();
            }
            else if ((activeScene == connectionSettings.LevelSceneName ||
                      activeScene == connectionSettings.GameplaySceneName) &&
                     networkManager.IsListening)
            {
                DrawGameplayHud();
            }
        }

        private void DrawLobbyGui()
        {
            EnsureGuiStyles();

            float width = Mathf.Min(430f, Screen.width - 40f);
            GUILayout.BeginArea(new Rect(20f, 20f, width, 360f), GUI.skin.box);
            GUILayout.Label("Dual Play Connection Lobby", headerStyle);
            GUILayout.Space(6f);
            GUILayout.Label("Connect both players, then choose Turtle or Bunny.", wrappedLabelStyle);
            GUILayout.Label("Host address", wrappedLabelStyle);

            GUI.enabled = !networkManager.IsListening;
            address = GUILayout.TextField(address, 64);
            GUILayout.Label($"UDP port: {connectionSettings.Port}", wrappedLabelStyle);
            GUILayout.Space(4f);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Start Host", GUILayout.Height(40f)))
            {
                StartHost();
            }
            if (GUILayout.Button("Start Client", GUILayout.Height(40f)))
            {
                StartClient();
            }
            GUILayout.EndHorizontal();

            GUI.enabled = networkManager.IsListening;
            if (GUILayout.Button("Disconnect", GUILayout.Height(30f)))
            {
                Disconnect();
            }
            GUI.enabled = true;

            GUILayout.Space(8f);
            GUILayout.Label(GetSessionSummary(), wrappedLabelStyle);
            GUILayout.Label(statusMessage, wrappedLabelStyle);
            GUILayout.Space(8f);
            GUILayout.Label($"The Host opens {connectionSettings.CharacterSelectSceneName} after both players connect.", wrappedLabelStyle);
            GUILayout.Label($"Last saved selection: {connectionSettings.LastSelectedMode}", wrappedLabelStyle);
            GUILayout.EndArea();
        }

        private void DrawCharacterSelectGui()
        {
            EnsureGuiStyles();

            float width = Mathf.Min(520f, Screen.width - 40f);
            float height = 330f;
            Rect panel = new Rect(
                (Screen.width - width) * 0.5f,
                (Screen.height - height) * 0.5f,
                width,
                height);

            DualPlayNetworkPlayer localPlayer = DualPlayNetworkPlayer.LocalPlayer;
            bool canChoose = localPlayer != null && localPlayer.IsSpawned && !isStartingGame;
            bool turtleTaken = DualPlayNetworkPlayer.IsRoleTakenByOther(DualPlayNetworkPlayer.PlayerRole.Turtle);
            bool bunnyTaken = DualPlayNetworkPlayer.IsRoleTakenByOther(DualPlayNetworkPlayer.PlayerRole.Bunny);

            GUILayout.BeginArea(panel, GUI.skin.box);
            GUILayout.Space(20f);
            GUILayout.Label("CHOOSE YOUR CHARACTER", headerStyle);
            GUILayout.Space(12f);
            GUILayout.Label("Each character can be selected by only one player.", wrappedLabelStyle);
            GUILayout.Space(16f);

            GUILayout.BeginHorizontal();
            GUI.enabled = canChoose && !turtleTaken;
            if (GUILayout.Button(GetRoleButtonText(
                    localPlayer,
                    DualPlayNetworkPlayer.PlayerRole.Turtle,
                    turtleTaken),
                GUILayout.Height(70f)))
            {
                localPlayer.RequestRoleSelection(DualPlayNetworkPlayer.PlayerRole.Turtle);
            }

            GUI.enabled = canChoose && !bunnyTaken;
            if (GUILayout.Button(GetRoleButtonText(
                    localPlayer,
                    DualPlayNetworkPlayer.PlayerRole.Bunny,
                    bunnyTaken),
                GUILayout.Height(70f)))
            {
                localPlayer.RequestRoleSelection(DualPlayNetworkPlayer.PlayerRole.Bunny);
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();

            GUILayout.Space(16f);
            if (localPlayer == null)
            {
                GUILayout.Label("Waiting for the local network player...", wrappedLabelStyle);
            }
            else if (localPlayer.HasSelectedRole)
            {
                GUILayout.Label($"Your character: {localPlayer.Role}", wrappedLabelStyle);
                GUILayout.Label("Waiting for both players to finish choosing...", wrappedLabelStyle);
            }
            else
            {
                GUILayout.Label("Select Turtle or Bunny.", wrappedLabelStyle);
            }

            if (isStartingGame)
            {
                GUILayout.Label($"Both players are ready. Loading {connectionSettings.LevelSceneName}...", wrappedLabelStyle);
            }

            GUILayout.Space(8f);
            if (GUILayout.Button("Disconnect & Main Menu", GUILayout.Height(30f)))
            {
                DisconnectAndReturnToMainMenu();
            }
            GUILayout.EndArea();
        }

        private static string GetRoleButtonText(
            DualPlayNetworkPlayer localPlayer,
            DualPlayNetworkPlayer.PlayerRole targetRole,
            bool isTaken)
        {
            if (isTaken)
            {
                return $"{targetRole}\nSelected by other player";
            }

            if (localPlayer != null && localPlayer.HasSelectedRole && localPlayer.Role == targetRole)
            {
                return $"{targetRole}\nSelected";
            }

            return targetRole.ToString();
        }

        private void DrawGameplayHud()
        {
            EnsureGuiStyles();

            GUILayout.BeginArea(new Rect(16f, 16f, 280f, 130f), GUI.skin.box);
            GUILayout.Label($"Role: {DualPlayNetworkPlayer.LocalRoleName}", headerStyle);
            GUILayout.Label(GetSessionSummary(), wrappedLabelStyle);
            if (GUILayout.Button("Disconnect & Main Menu", GUILayout.Height(28f)))
            {
                DisconnectAndReturnToMainMenu();
            }
            GUILayout.EndArea();
        }

        private void StartHost()
        {
            connectionSettings.SaveSessionSelection(address, true);

            if (!PrepareNetwork(true))
            {
                return;
            }

            statusMessage = networkManager.StartHost()
                ? "Host started. Waiting for the other player..."
                : "Host could not start. Check the Console.";
        }

        private void StartClient()
        {
            connectionSettings.SaveSessionSelection(address, false);

            if (!PrepareNetwork(false))
            {
                return;
            }

            statusMessage = networkManager.StartClient()
                ? $"Connecting to {address}:{connectionSettings.Port}..."
                : "Client could not start. Check the address and Console.";
        }

        private bool PrepareNetwork(bool isHost)
        {
            if (networkManager.IsListening)
            {
                return false;
            }

            GameObject playerPrefab = connectionSettings.PlayerPrefab;
            if (playerPrefab == null)
            {
                statusMessage = "Player prefab is not assigned in the shared settings.";
                return false;
            }

            networkManager.NetworkConfig.NetworkTransport = transport;
            networkManager.NetworkConfig.ConnectionApproval = true;
            networkManager.NetworkConfig.PlayerPrefab = playerPrefab;
            networkManager.ConnectionApprovalCallback = ApproveConnection;
            transport.SetConnectionData(address, connectionSettings.Port, isHost ? "0.0.0.0" : null);
            isStartingGame = false;
            return true;
        }

        private void ApproveConnection(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            int maximumPlayers = connectionSettings.MaximumPlayers;
            bool hasRoom = networkManager.ConnectedClientsIds.Count < maximumPlayers;
            response.Approved = hasRoom;
            response.CreatePlayerObject = hasRoom;
            response.Pending = false;
            response.Reason = hasRoom ? string.Empty : "This game supports exactly two players.";
            response.Position = request.ClientNetworkId == NetworkManager.ServerClientId
                ? new Vector3(-2.5f, 0f, 0f)
                : new Vector3(2.5f, 0f, 0f);
            response.Rotation = Quaternion.identity;
        }

        private void Disconnect()
        {
            StopAllCoroutines();
            isStartingGame = false;
            if (networkManager.IsListening)
            {
                networkManager.Shutdown();
            }

            statusMessage = "Disconnected.";
        }

        private void DisconnectAndReturnToMainMenu()
        {
            Disconnect();
            string menuScene = connectionSettings.MainMenuSceneName;
            if (persistentInstance == this)
            {
                persistentInstance = null;
            }
            Destroy(gameObject);
            SceneManager.LoadScene(menuScene, LoadSceneMode.Single);
        }

        private void HandleClientConnected(ulong clientId)
        {
            if (networkManager.IsServer)
            {
                int playerCount = networkManager.ConnectedClientsIds.Count;
                statusMessage = $"Player connected ({playerCount}/{connectionSettings.MaximumPlayers}).";
                if (connectionSettings.AutoStartGameWhenFull &&
                    playerCount >= connectionSettings.MaximumPlayers &&
                    !isStartingGame)
                {
                    StartCoroutine(StartCharacterSelectAfterDelay());
                }
            }
            else if (clientId == networkManager.LocalClientId)
            {
                statusMessage = "Connected. Waiting for the Host...";
            }
        }

        private void HandleClientDisconnected(ulong clientId)
        {
            isStartingGame = false;
            if (networkManager.IsServer)
            {
                statusMessage = $"Player disconnected ({networkManager.ConnectedClientsIds.Count}/{connectionSettings.MaximumPlayers}).";
            }
            else if (clientId == networkManager.LocalClientId)
            {
                statusMessage = string.IsNullOrWhiteSpace(networkManager.DisconnectReason)
                    ? "Disconnected from host."
                    : networkManager.DisconnectReason;

                string activeScene = SceneManager.GetActiveScene().name;
                if (activeScene == connectionSettings.CharacterSelectSceneName ||
                    activeScene == connectionSettings.LevelSceneName ||
                    activeScene == connectionSettings.GameplaySceneName)
                {
                    StartCoroutine(ReturnToMainMenuAfterDisconnect());
                }
            }
        }

        private IEnumerator ReturnToMainMenuAfterDisconnect()
        {
            yield return null;

            string menuScene = connectionSettings.MainMenuSceneName;
            if (persistentInstance == this)
            {
                persistentInstance = null;
            }
            Destroy(gameObject);
            SceneManager.LoadScene(menuScene, LoadSceneMode.Single);
        }

        private void HandleActiveSceneChanged(Scene previousScene, Scene newScene)
        {
            isStartingGame = false;

            if (newScene.name == connectionSettings.CharacterSelectSceneName)
            {
                statusMessage = "Choose Turtle or Bunny.";
            }
        }

        private IEnumerator StartCharacterSelectAfterDelay()
        {
            isStartingGame = true;
            statusMessage = $"Both players connected. Loading {connectionSettings.CharacterSelectSceneName}...";
            yield return new WaitForSecondsRealtime(connectionSettings.AutoStartDelay);

            if (!networkManager.IsHost ||
                networkManager.ConnectedClientsIds.Count < connectionSettings.MaximumPlayers)
            {
                isStartingGame = false;
                yield break;
            }

            SceneEventProgressStatus result = networkManager.SceneManager.LoadScene(
                connectionSettings.CharacterSelectSceneName,
                LoadSceneMode.Single);

            if (result != SceneEventProgressStatus.Started)
            {
                isStartingGame = false;
                statusMessage = $"{connectionSettings.CharacterSelectSceneName} scene load failed: {result}";
                Debug.LogError(statusMessage, this);
            }
        }

        private IEnumerator StartLevelAfterCharacterSelection()
        {
            isStartingGame = true;
            yield return new WaitForSecondsRealtime(connectionSettings.AutoStartDelay);

            if (!networkManager.IsHost ||
                SceneManager.GetActiveScene().name != connectionSettings.CharacterSelectSceneName ||
                !DualPlayNetworkPlayer.AreAllPlayersReady(connectionSettings.MaximumPlayers))
            {
                isStartingGame = false;
                yield break;
            }

            SceneEventProgressStatus result = networkManager.SceneManager.LoadScene(
                connectionSettings.LevelSceneName,
                LoadSceneMode.Single);

            if (result != SceneEventProgressStatus.Started)
            {
                isStartingGame = false;
                statusMessage = $"{connectionSettings.LevelSceneName} scene load failed: {result}";
                Debug.LogError(statusMessage, this);
            }
        }

        private string GetSessionSummary()
        {
            if (!networkManager.IsListening)
            {
                return "Mode: Offline";
            }
            if (networkManager.IsHost)
            {
                return $"Mode: Host | Character: {DualPlayNetworkPlayer.LocalRoleName} | Players: {networkManager.ConnectedClientsIds.Count}/{connectionSettings.MaximumPlayers}";
            }
            if (networkManager.IsClient)
            {
                return $"Mode: Client | Character: {DualPlayNetworkPlayer.LocalRoleName} | Local ID: {networkManager.LocalClientId}";
            }

            return "Mode: Server";
        }

        private void EnsureGuiStyles()
        {
            if (headerStyle != null)
            {
                return;
            }

            headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold
            };
            wrappedLabelStyle = new GUIStyle(GUI.skin.label)
            {
                wordWrap = true
            };
        }
    }
}
