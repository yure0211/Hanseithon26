using System.Collections;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;
using Hanseithon.UI;

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
        [Header("Existing pixel UI assets")]
        [SerializeField] private Sprite buttonFrame;
        [SerializeField] private Font uiFont;
        [SerializeField] private TMP_FontAsset uiTmpFont;
        [SerializeField] private Sprite turtlePortrait;
        [SerializeField] private Sprite bunnyPortrait;

        private static DualPlayNetworkLauncher persistentInstance;

        private string address;
        private string statusMessage = "호스트 또는 클라이언트를 선택하세요.";
        private bool isPrimaryInstance = true;
        private bool isStartingGame;

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
            DualPlayUiTheme.UseAssets(buttonFrame, uiFont);
            DualPlayUiTheme.StyleSceneButtons(buttonFrame, uiTmpFont);

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

            DualPlayUiTheme.UseAssets(buttonFrame, uiFont);

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
            Matrix4x4 previousMatrix = DualPlayUiTheme.BeginCanvas(true);

            GUILayout.BeginArea(
                DualPlayUiTheme.CenteredPanel(620f, 500f),
                DualPlayUiTheme.PanelStyle);
            GUILayout.Label("CONNECTION LOBBY", DualPlayUiTheme.CaptionStyle);
            GUILayout.Space(8f);
            GUILayout.Label("연결 대기실", DualPlayUiTheme.HeaderStyle);
            GUILayout.Label("두 플레이어의 네트워크 연결을 준비합니다.", DualPlayUiTheme.SubtitleStyle);
            GUILayout.Space(24f);
            GUILayout.Label("호스트 주소", DualPlayUiTheme.LabelStyle);

            GUI.enabled = !networkManager.IsListening;
            address = GUILayout.TextField(
                address,
                64,
                DualPlayUiTheme.TextFieldStyle,
                GUILayout.Height(52f));
            GUILayout.Space(8f);
            GUILayout.Label(
                $"UDP 포트  {connectionSettings.Port}",
                DualPlayUiTheme.CaptionStyle);
            GUILayout.Space(16f);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(
                    "호스트 열기",
                    DualPlayUiTheme.ButtonStyle,
                    GUILayout.Height(60f)))
            {
                StartHost();
            }
            if (GUILayout.Button(
                    "클라이언트 접속",
                    DualPlayUiTheme.ButtonStyle,
                    GUILayout.Height(60f)))
            {
                StartClient();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10f);
            GUI.enabled = networkManager.IsListening;
            if (GUILayout.Button(
                    "연결 끊기",
                    DualPlayUiTheme.SecondaryButtonStyle,
                    GUILayout.Height(44f)))
            {
                Disconnect();
            }
            GUI.enabled = true;

            GUILayout.Space(14f);
            GUILayout.Label(GetConnectionSummary(), DualPlayUiTheme.StatusStyle, GUILayout.Height(40f));
            GUILayout.Space(6f);
            GUILayout.Label(statusMessage, DualPlayUiTheme.CenteredLabelStyle);
            GUILayout.EndArea();
            DualPlayUiTheme.EndCanvas(previousMatrix);
        }

        private void DrawCharacterSelectGui()
        {
            Matrix4x4 previousMatrix = DualPlayUiTheme.BeginCanvas(true);

            DualPlayNetworkPlayer localPlayer = DualPlayNetworkPlayer.LocalPlayer;
            bool canChoose = localPlayer != null && localPlayer.IsSpawned && !isStartingGame;
            bool turtleTaken = DualPlayNetworkPlayer.IsRoleTakenByOther(DualPlayNetworkPlayer.PlayerRole.Turtle);
            bool bunnyTaken = DualPlayNetworkPlayer.IsRoleTakenByOther(DualPlayNetworkPlayer.PlayerRole.Bunny);

            GUILayout.BeginArea(
                DualPlayUiTheme.CenteredPanel(820f, 640f),
                DualPlayUiTheme.PanelStyle);
            GUILayout.Label("CHOOSE YOUR CHARACTER", DualPlayUiTheme.CaptionStyle);
            GUILayout.Space(8f);
            GUILayout.Label("캐릭터 선택", DualPlayUiTheme.HeaderStyle);
            GUILayout.Label("두 플레이어는 서로 다른 캐릭터를 선택해야 합니다.", DualPlayUiTheme.SubtitleStyle);
            GUILayout.Space(12f);

            GUILayout.BeginHorizontal();
            DrawRoleOption(
                localPlayer,
                DualPlayNetworkPlayer.PlayerRole.Turtle,
                turtlePortrait,
                turtleTaken,
                canChoose);
            DrawRoleOption(
                localPlayer,
                DualPlayNetworkPlayer.PlayerRole.Bunny,
                bunnyPortrait,
                bunnyTaken,
                canChoose);
            GUILayout.EndHorizontal();

            GUILayout.Space(14f);
            if (localPlayer == null)
            {
                GUILayout.Label("로컬 플레이어를 준비하는 중입니다...", DualPlayUiTheme.StatusStyle);
            }
            else if (localPlayer.HasSelectedRole)
            {
                GUILayout.Label(
                    $"내 캐릭터  ·  {GetRoleName(localPlayer.Role)}",
                    DualPlayUiTheme.StatusStyle);
                GUILayout.Label("상대 플레이어의 선택을 기다리고 있습니다.", DualPlayUiTheme.CenteredLabelStyle);
            }
            else
            {
                GUILayout.Label("토끼 또는 거북이를 선택하세요.", DualPlayUiTheme.StatusStyle);
            }

            if (isStartingGame)
            {
                GUILayout.Label("선택 완료 · 레벨 화면으로 이동합니다...", DualPlayUiTheme.CenteredLabelStyle);
            }

            GUILayout.Space(12f);
            if (GUILayout.Button(
                    "연결 끊고 시작 화면으로",
                    DualPlayUiTheme.SecondaryButtonStyle,
                    GUILayout.Height(46f)))
            {
                DisconnectAndReturnToMainMenu();
            }
            GUILayout.EndArea();
            DualPlayUiTheme.EndCanvas(previousMatrix);
        }

        private static void DrawRoleOption(
            DualPlayNetworkPlayer localPlayer,
            DualPlayNetworkPlayer.PlayerRole role,
            Sprite portrait,
            bool isTaken,
            bool canChoose)
        {
            GUILayout.BeginVertical(GUILayout.Width(365f));
            Rect portraitArea = GUILayoutUtility.GetRect(320f, 165f, GUILayout.ExpandWidth(true));
            DualPlayUiTheme.DrawSprite(portraitArea, portrait);

            GUI.enabled = canChoose && !isTaken;
            if (GUILayout.Button(
                    GetRoleButtonText(localPlayer, role, isTaken),
                    DualPlayUiTheme.ButtonStyle,
                    GUILayout.Height(88f)))
            {
                localPlayer.RequestRoleSelection(role);
            }
            GUI.enabled = true;
            GUILayout.EndVertical();
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

            if (localPlayer != null && localPlayer.HasSelectedRole && localPlayer.Role == targetRole)
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

        private void DrawGameplayHud()
        {
            Matrix4x4 previousMatrix = DualPlayUiTheme.BeginCanvas(false);

            GUILayout.BeginArea(new Rect(20f, 20f, 330f, 160f), DualPlayUiTheme.PanelStyle);
            GUILayout.Label(
                $"내 역할  ·  {GetLocalRoleName()}",
                DualPlayUiTheme.HeaderStyle);
            GUILayout.Label(GetSessionSummary(), DualPlayUiTheme.CaptionStyle);
            GUILayout.Space(6f);
            if (GUILayout.Button(
                    "연결 끊고 시작 화면으로",
                    DualPlayUiTheme.SecondaryButtonStyle,
                    GUILayout.Height(42f)))
            {
                DisconnectAndReturnToMainMenu();
            }
            GUILayout.EndArea();
            DualPlayUiTheme.EndCanvas(previousMatrix);
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
            DualPlayUiTheme.UseAssets(buttonFrame, uiFont);
            DualPlayUiTheme.StyleSceneButtons(buttonFrame, uiTmpFont);

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
