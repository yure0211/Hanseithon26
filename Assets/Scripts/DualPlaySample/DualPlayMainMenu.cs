using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Hanseithon.DualPlaySample
{
    public sealed class DualPlayMainMenu : MonoBehaviour
    {
        private const string StartSceneName = "Start";
        private const string SampleButtonName = "Button_Sample";
        private const string QuitButtonName = "QuitButton";

        [SerializeField] private string connectionSceneName = "DualPlayConnectionTestScene";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterStartSceneMenu()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureInitialStartMenu()
        {
            EnsureMenuForScene(SceneManager.GetActiveScene());
        }

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode loadMode)
        {
            EnsureMenuForScene(scene);
        }

        private static void EnsureMenuForScene(Scene scene)
        {
            if (scene.name != StartSceneName ||
                Object.FindFirstObjectByType<DualPlayMainMenu>(FindObjectsInactive.Include) != null)
            {
                return;
            }

            GameObject menuObject = new GameObject("DualPlay Start Menu");
            SceneManager.MoveGameObjectToScene(menuObject, scene);
            menuObject.AddComponent<DualPlayMainMenu>();
        }

        private void Awake()
        {
            if (SceneManager.GetActiveScene().name == StartSceneName)
            {
                ConfigureExistingPixelButtons();
            }
        }

        private void ConfigureExistingPixelButtons()
        {
            Button sampleButton = FindButton(SampleButtonName);
            if (sampleButton == null)
            {
                Debug.LogError(
                    "Start scene is missing Button_Sample. Restore the UI from bak_Start before playing.",
                    this);
                return;
            }

            Canvas canvas = sampleButton.GetComponentInParent<Canvas>();
            CanvasScaler scaler = canvas != null ? canvas.GetComponent<CanvasScaler>() : null;
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
            }

            ConfigureButton(
                sampleButton,
                "연결 대기실",
                new Vector2(0f, -245f),
                EnterConnectionLobby);

            Button quitButton = FindButton(QuitButtonName);
            if (quitButton == null)
            {
                quitButton = Instantiate(sampleButton, sampleButton.transform.parent);
                quitButton.gameObject.name = QuitButtonName;
            }

            ConfigureButton(
                quitButton,
                "게임 종료",
                new Vector2(0f, -360f),
                Application.Quit);
        }

        private static Button FindButton(string objectName)
        {
            Button[] buttons = Object.FindObjectsByType<Button>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i].gameObject.name == objectName)
                {
                    return buttons[i];
                }
            }

            return null;
        }

        private static void ConfigureButton(
            Button button,
            string labelText,
            Vector2 anchoredPosition,
            UnityEngine.Events.UnityAction clickAction)
        {
            button.gameObject.SetActive(true);

            RectTransform rect = button.transform as RectTransform;
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = anchoredPosition;
                rect.sizeDelta = new Vector2(420f, 96f);
            }

            Image image = button.targetGraphic as Image;
            if (image != null)
            {
                image.type = Image.Type.Sliced;
                image.color = Color.white;
                image.pixelsPerUnitMultiplier = 1f;
            }

            TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
            if (label != null)
            {
                label.text = labelText;
                label.color = new Color32(45, 45, 45, 255);
                label.fontSize = 38f;
                label.enableAutoSizing = true;
                label.fontSizeMin = 24f;
                label.fontSizeMax = 38f;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(clickAction);
        }

        private void EnterConnectionLobby()
        {
            SceneManager.LoadScene(connectionSceneName, LoadSceneMode.Single);
        }
    }
}
