using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

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
        }

        private void OnDisable()
        {
            if (!isPrimaryInstance || networkManager == null)
            {
                return;
            }

            networkManager.OnClientConnectedCallback -= HandleClientConnected;
            networkManager.OnClientDisconnectCallback -= HandleClientDisconnected;
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

            EnsureGuiStyles();

            GUILayout.BeginArea(new Rect(20f, 20f, 390f, 310f), GUI.skin.box);
            GUILayout.Label("Dual Play Network Sample", headerStyle);
            GUILayout.Space(6f);
            GUILayout.Label("Host address", wrappedLabelStyle);

            GUI.enabled = !networkManager.IsListening;
            address = GUILayout.TextField(address, 64);
            GUILayout.Label($"UDP port: {connectionSettings.Port}", wrappedLabelStyle);
            GUILayout.Space(4f);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Start Host", GUILayout.Height(36f)))
            {
                StartHost();
            }
            if (GUILayout.Button("Start Client", GUILayout.Height(36f)))
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
            GUILayout.Label("Move your own square with WASD or arrow keys. Run one build as Host and another as Client.", wrappedLabelStyle);
            GUILayout.EndArea();
        }

        private void StartHost()
        {
            connectionSettings.SaveAddress(address);

            if (!PrepareNetwork(true))
            {
                return;
            }

            statusMessage = networkManager.StartHost()
                ? "Host started. Waiting for one client..."
                : "Host could not start. Check the Console.";
        }

        private void StartClient()
        {
            connectionSettings.SaveAddress(address);

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
            return true;
        }

        private void ApproveConnection(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            int maximumPlayers = connectionSettings.MaximumPlayers;
            bool hasRoom = networkManager.ConnectedClientsIds.Count < maximumPlayers;
            response.Approved = hasRoom;
            response.CreatePlayerObject = hasRoom;
            response.Pending = false;
            response.Reason = hasRoom ? string.Empty : "This sample supports exactly two players.";
            response.Position = request.ClientNetworkId == NetworkManager.ServerClientId
                ? new Vector3(-2.5f, 0f, 0f)
                : new Vector3(2.5f, 0f, 0f);
            response.Rotation = Quaternion.identity;
        }

        private void Disconnect()
        {
            if (networkManager.IsListening)
            {
                networkManager.Shutdown();
            }

            statusMessage = "Disconnected.";
        }

        private void HandleClientConnected(ulong clientId)
        {
            if (networkManager.IsServer)
            {
                statusMessage = $"Player connected ({networkManager.ConnectedClientsIds.Count}/{connectionSettings.MaximumPlayers}).";
            }
            else if (clientId == networkManager.LocalClientId)
            {
                statusMessage = $"Connected as client {clientId}.";
            }
        }

        private void HandleClientDisconnected(ulong clientId)
        {
            if (networkManager.IsServer)
            {
                statusMessage = $"Player disconnected ({networkManager.ConnectedClientsIds.Count}/{connectionSettings.MaximumPlayers}).";
            }
            else if (clientId == networkManager.LocalClientId)
            {
                statusMessage = string.IsNullOrWhiteSpace(networkManager.DisconnectReason)
                    ? "Disconnected from host."
                    : networkManager.DisconnectReason;
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
                return $"Mode: Host | Players: {networkManager.ConnectedClientsIds.Count}/{connectionSettings.MaximumPlayers}";
            }
            if (networkManager.IsClient)
            {
                return $"Mode: Client | Local ID: {networkManager.LocalClientId}";
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
