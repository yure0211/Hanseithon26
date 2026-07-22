using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hanseithon.DualPlaySample
{
    public sealed class DualPlaySceneUi : MonoBehaviour
    {
        public enum SceneUiMode
        {
            Connection,
            CharacterSelect,
            GameplayHud
        }

        [SerializeField] private SceneUiMode mode;

        [Header("Connection")]
        [SerializeField] private TMP_InputField addressInput;
        [SerializeField] private TMP_Text portText;
        [SerializeField] private TMP_Text connectionSummaryText;
        [SerializeField] private TMP_Text connectionStatusText;
        [SerializeField] private Button hostButton;
        [SerializeField] private Button clientButton;

        [Header("Character Select")]
        [SerializeField] private Button turtleButton;
        [SerializeField] private Button bunnyButton;
        [SerializeField] private TMP_Text turtleButtonText;
        [SerializeField] private TMP_Text bunnyButtonText;
        [SerializeField] private TMP_Text characterStatusText;

        [Header("Shared")]
        [SerializeField] private Button disconnectButton;
        [SerializeField] private TMP_Text roleText;
        [SerializeField] private TMP_Text sessionText;

        public event Action HostRequested;
        public event Action ClientRequested;
        public event Action DisconnectRequested;
        public event Action TurtleRequested;
        public event Action BunnyRequested;

        public SceneUiMode Mode => mode;

        public string Address
        {
            get => addressInput != null ? addressInput.text : string.Empty;
            set
            {
                if (addressInput != null && !addressInput.isFocused && addressInput.text != value)
                {
                    addressInput.SetTextWithoutNotify(value);
                }
            }
        }

        private void Awake()
        {
            if (hostButton != null) hostButton.onClick.AddListener(InvokeHostRequested);
            if (clientButton != null) clientButton.onClick.AddListener(InvokeClientRequested);
            if (disconnectButton != null) disconnectButton.onClick.AddListener(InvokeDisconnectRequested);
            if (turtleButton != null) turtleButton.onClick.AddListener(InvokeTurtleRequested);
            if (bunnyButton != null) bunnyButton.onClick.AddListener(InvokeBunnyRequested);
        }

        private void OnDestroy()
        {
            if (hostButton != null) hostButton.onClick.RemoveListener(InvokeHostRequested);
            if (clientButton != null) clientButton.onClick.RemoveListener(InvokeClientRequested);
            if (disconnectButton != null) disconnectButton.onClick.RemoveListener(InvokeDisconnectRequested);
            if (turtleButton != null) turtleButton.onClick.RemoveListener(InvokeTurtleRequested);
            if (bunnyButton != null) bunnyButton.onClick.RemoveListener(InvokeBunnyRequested);
        }

        public void ConfigureConnection(
            TMP_InputField input,
            TMP_Text port,
            TMP_Text summary,
            TMP_Text status,
            Button host,
            Button client,
            Button disconnect)
        {
            mode = SceneUiMode.Connection;
            addressInput = input;
            portText = port;
            connectionSummaryText = summary;
            connectionStatusText = status;
            hostButton = host;
            clientButton = client;
            disconnectButton = disconnect;
        }

        public void ConfigureCharacterSelect(
            Button turtle,
            TMP_Text turtleLabel,
            Button bunny,
            TMP_Text bunnyLabel,
            TMP_Text status,
            Button disconnect)
        {
            mode = SceneUiMode.CharacterSelect;
            turtleButton = turtle;
            turtleButtonText = turtleLabel;
            bunnyButton = bunny;
            bunnyButtonText = bunnyLabel;
            characterStatusText = status;
            disconnectButton = disconnect;
        }

        public void ConfigureGameplayHud(
            TMP_Text role,
            TMP_Text session,
            Button disconnect)
        {
            mode = SceneUiMode.GameplayHud;
            roleText = role;
            sessionText = session;
            disconnectButton = disconnect;
        }

        public void SetConnectionState(
            string currentAddress,
            ushort port,
            bool isListening,
            string summary,
            string status)
        {
            Address = currentAddress;
            if (portText != null) portText.text = $"UDP 포트  {port}";
            if (connectionSummaryText != null) connectionSummaryText.text = summary;
            if (connectionStatusText != null) connectionStatusText.text = status;
            if (hostButton != null) hostButton.interactable = !isListening;
            if (clientButton != null) clientButton.interactable = !isListening;
            if (disconnectButton != null) disconnectButton.interactable = isListening;
        }

        public void SetCharacterState(
            bool canChoose,
            bool turtleTaken,
            bool bunnyTaken,
            string turtleLabel,
            string bunnyLabel,
            string status)
        {
            if (turtleButton != null) turtleButton.interactable = canChoose && !turtleTaken;
            if (bunnyButton != null) bunnyButton.interactable = canChoose && !bunnyTaken;
            if (turtleButtonText != null) turtleButtonText.text = turtleLabel;
            if (bunnyButtonText != null) bunnyButtonText.text = bunnyLabel;
            if (characterStatusText != null) characterStatusText.text = status;
        }

        public void SetGameplayState(string role, string session)
        {
            if (roleText != null) roleText.text = role;
            if (sessionText != null) sessionText.text = session;
        }

        private void InvokeHostRequested() => HostRequested?.Invoke();
        private void InvokeClientRequested() => ClientRequested?.Invoke();
        private void InvokeDisconnectRequested() => DisconnectRequested?.Invoke();
        private void InvokeTurtleRequested() => TurtleRequested?.Invoke();
        private void InvokeBunnyRequested() => BunnyRequested?.Invoke();
    }
}
