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
        private string statusMessage = "호스트 또는 클라이언트를 선택하세요.";
        private bool isPrimaryInstance = true;
        private bool isStartingGame;
        private DualPlaySceneUi sceneUi;

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
            BindSceneUi();

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
            if (sceneUi == null)
            {
                BindSceneUi();
            }
            RefreshSceneUi();

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
            UnbindSceneUi();
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

        private void BindSceneUi()
        {
            UnbindSceneUi();
            sceneUi = Object.FindFirstObjectByType<DualPlaySceneUi>(FindObjectsInactive.Include);
            if (sceneUi == null)
            {
                return;
            }

            sceneUi.HostRequested += HandleHostRequested;
            sceneUi.ClientRequested += HandleClientRequested;
            sceneUi.DisconnectRequested += HandleDisconnectRequested;
            sceneUi.TurtleRequested += HandleTurtleRequested;
            sceneUi.BunnyRequested += HandleBunnyRequested;
            RefreshSceneUi();
        }

        private void UnbindSceneUi()
        {
            if (sceneUi == null)
            {
                return;
            }

            sceneUi.HostRequested -= HandleHostRequested;
            sceneUi.ClientRequested -= HandleClientRequested;
            sceneUi.DisconnectRequested -= HandleDisconnectRequested;
            sceneUi.TurtleRequested -= HandleTurtleRequested;
            sceneUi.BunnyRequested -= HandleBunnyRequested;
            sceneUi = null;
        }

        private void RefreshSceneUi()
        {
            if (!isPrimaryInstance || sceneUi == null || connectionSettings == null)
            {
                return;
            }

            switch (sceneUi.Mode)
            {
                case DualPlaySceneUi.SceneUiMode.Connection:
                    sceneUi.SetConnectionState(
                        address,
                        connectionSettings.Port,
                        networkManager.IsListening,
                        GetConnectionSummary(),
                        statusMessage);
                    break;

                case DualPlaySceneUi.SceneUiMode.CharacterSelect:
                    RefreshCharacterSelectUi();
                    break;

                case DualPlaySceneUi.SceneUiMode.GameplayHud:
                    sceneUi.SetGameplayState(
                        $"내 역할  ·  {GetLocalRoleName()}",
                        GetSessionSummary());
                    break;
            }
        }

        private void RefreshCharacterSelectUi()
        {
            DualPlayNetworkPlayer localPlayer = DualPlayNetworkPlayer.LocalPlayer;
            bool canChoose = localPlayer != null && localPlayer.IsSpawned && !isStartingGame;
            bool turtleTaken = DualPlayNetworkPlayer.IsRoleTakenByOther(
                DualPlayNetworkPlayer.PlayerRole.Turtle);
            bool bunnyTaken = DualPlayNetworkPlayer.IsRoleTakenByOther(
                DualPlayNetworkPlayer.PlayerRole.Bunny);

            string characterStatus;
            if (isStartingGame)
            {
                characterStatus = "선택 완료 · 레벨 화면으로 이동합니다...";
            }
            else if (localPlayer == null)
            {
                characterStatus = "로컬 플레이어를 준비하는 중입니다...";
            }
            else if (localPlayer.HasSelectedRole)
            {
                characterStatus =
                    $"내 캐릭터  ·  {GetRoleName(localPlayer.Role)}\n" +
                    "상대 플레이어의 선택을 기다리고 있습니다.";
            }
            else
            {
                characterStatus = "토끼 또는 거북이를 선택하세요.";
            }

            sceneUi.SetCharacterState(
                canChoose,
                turtleTaken,
                bunnyTaken,
                GetRoleButtonText(
                    localPlayer,
                    DualPlayNetworkPlayer.PlayerRole.Turtle,
                    turtleTaken),
                GetRoleButtonText(
                    localPlayer,
                    DualPlayNetworkPlayer.PlayerRole.Bunny,
                    bunnyTaken),
                characterStatus);
        }

        private void HandleHostRequested()
        {
            ReadAddressFromSceneUi();
            StartHost();
        }

        private void HandleClientRequested()
        {
            ReadAddressFromSceneUi();
            StartClient();
        }

        private void HandleDisconnectRequested()
        {
            if (SceneManager.GetActiveScene().name == connectionSettings.ConnectionSceneName)
            {
                Disconnect();
            }
            else
            {
                DisconnectAndReturnToMainMenu();
            }
        }

        private static void HandleTurtleRequested()
        {
            DualPlayNetworkPlayer.LocalPlayer?.RequestRoleSelection(
                DualPlayNetworkPlayer.PlayerRole.Turtle);
        }

        private static void HandleBunnyRequested()
        {
            DualPlayNetworkPlayer.LocalPlayer?.RequestRoleSelection(
                DualPlayNetworkPlayer.PlayerRole.Bunny);
        }

        private void ReadAddressFromSceneUi()
        {
            if (sceneUi != null && !string.IsNullOrWhiteSpace(sceneUi.Address))
            {
                address = sceneUi.Address.Trim();
            }
        }

        private static string GetRoleButtonText(
            DualPlayNetworkPlayer localPlayer,
            DualPlayNetworkPlayer.PlayerRole targetRole,
            bool isTaken)
        {
            if (isTaken)
            {
                return $"{GetRoleName(targetRole)}\n상대가 선택함";
            }

            if (localPlayer != null &&
                localPlayer.HasSelectedRole &&
                localPlayer.Role == targetRole)
            {
                return $"{GetRoleName(targetRole)}\n선택 완료";
            }

            return targetRole == DualPlayNetworkPlayer.PlayerRole.Turtle
                ? "거북이\n바다 · 오브젝트 상호작용"
                : "토끼\n육지 · 빠른 이동과 점프";
        }

        private static string GetRoleName(DualPlayNetworkPlayer.PlayerRole role)
        {
            return role == DualPlayNetworkPlayer.PlayerRole.Turtle ? "거북이" : "토끼";
        }

        private void StartHost()
        {
            connectionSettings.SaveSessionSelection(address, true);

            if (!PrepareNetwork(true))
            {
                return;
            }

            statusMessage = networkManager.StartHost()
                ? "호스트를 열었습니다. 다른 플레이어를 기다립니다."
                : "호스트를 시작하지 못했습니다. Console을 확인하세요.";
        }

        private void StartClient()
        {
            connectionSettings.SaveSessionSelection(address, false);

            if (!PrepareNetwork(false))
            {
                return;
            }

            statusMessage = networkManager.StartClient()
                ? $"{address}:{connectionSettings.Port}에 연결하는 중입니다..."
                : "접속하지 못했습니다. 주소와 Console을 확인하세요.";
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
                statusMessage = "공용 설정에 플레이어 프리팹이 지정되지 않았습니다.";
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

            statusMessage = "연결을 끊었습니다.";
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
                statusMessage = $"플레이어가 연결되었습니다. ({playerCount}/{connectionSettings.MaximumPlayers})";
                if (connectionSettings.AutoStartGameWhenFull &&
                    playerCount >= connectionSettings.MaximumPlayers &&
                    !isStartingGame)
                {
                    StartCoroutine(StartCharacterSelectAfterDelay());
                }
            }
            else if (clientId == networkManager.LocalClientId)
            {
                statusMessage = "호스트에 연결되었습니다. 다음 화면을 기다립니다.";
            }
        }

        private void HandleClientDisconnected(ulong clientId)
        {
            isStartingGame = false;
            if (networkManager.IsServer)
            {
                statusMessage = $"플레이어 연결이 끊어졌습니다. ({networkManager.ConnectedClientsIds.Count}/{connectionSettings.MaximumPlayers})";
            }
            else if (clientId == networkManager.LocalClientId)
            {
                statusMessage = string.IsNullOrWhiteSpace(networkManager.DisconnectReason)
                    ? "호스트와 연결이 끊어졌습니다."
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
            BindSceneUi();

            if (newScene.name == connectionSettings.CharacterSelectSceneName)
            {
                statusMessage = "토끼 또는 거북이를 선택하세요.";
            }
        }

        private IEnumerator StartCharacterSelectAfterDelay()
        {
            isStartingGame = true;
            statusMessage = "두 플레이어가 연결되었습니다. 캐릭터 선택으로 이동합니다...";
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
                return "오프라인";
            }
            if (networkManager.IsHost)
            {
                return $"호스트 · 접속 {networkManager.ConnectedClientsIds.Count}/{connectionSettings.MaximumPlayers}";
            }
            if (networkManager.IsClient)
            {
                return $"클라이언트 · ID {networkManager.LocalClientId}";
            }

            return "서버";
        }

        private string GetConnectionSummary()
        {
            if (!networkManager.IsListening)
            {
                return "현재 상태  ·  오프라인";
            }

            if (networkManager.IsHost)
            {
                return $"현재 상태  ·  호스트  ·  접속 {networkManager.ConnectedClientsIds.Count}/{connectionSettings.MaximumPlayers}";
            }

            return networkManager.IsConnectedClient
                ? "현재 상태  ·  클라이언트 연결됨"
                : "현재 상태  ·  클라이언트 연결 중";
        }

        private static string GetLocalRoleName()
        {
            return DualPlayNetworkPlayer.LocalRoleName == DualPlayNetworkPlayer.PlayerRole.Turtle.ToString()
                ? "거북이"
                : DualPlayNetworkPlayer.LocalRoleName == DualPlayNetworkPlayer.PlayerRole.Bunny.ToString()
                    ? "토끼"
                    : "선택 전";
        }
    }
}
