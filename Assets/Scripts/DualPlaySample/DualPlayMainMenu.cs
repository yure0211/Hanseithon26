using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Hanseithon.DualPlaySample
{
    public sealed class DualPlayMainMenu : MonoBehaviour
    {
        [SerializeField] private string connectionSceneName = "DualPlayConnectionTestScene";
        [SerializeField] private Button startButton;
        [SerializeField] private Button quitButton;

        private void Awake()
        {
            if (startButton == null)
            {
                startButton = FindButton("StartButton");
            }
            if (quitButton == null)
            {
                quitButton = FindButton("QuitButton");
            }
        }

        private void OnEnable()
        {
            if (startButton != null) startButton.onClick.AddListener(EnterConnectionLobby);
            if (quitButton != null) quitButton.onClick.AddListener(QuitGame);
        }

        private void OnDisable()
        {
            if (startButton != null) startButton.onClick.RemoveListener(EnterConnectionLobby);
            if (quitButton != null) quitButton.onClick.RemoveListener(QuitGame);
        }

        public void ConfigureSceneButtons(Button start, Button quit)
        {
            startButton = start;
            quitButton = quit;
        }

        private static Button FindButton(string objectName)
        {
            GameObject target = GameObject.Find(objectName);
            return target != null ? target.GetComponent<Button>() : null;
        }

        private void EnterConnectionLobby()
        {
            SceneManager.LoadScene(connectionSceneName, LoadSceneMode.Single);
        }

        private static void QuitGame()
        {
            Application.Quit();
        }
    }
}
