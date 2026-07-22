using UnityEngine;
using UnityEngine.SceneManagement;

namespace Hanseithon.DualPlaySample
{
    public sealed class DualPlayMainMenu : MonoBehaviour
    {
        [SerializeField] private string connectionSceneName = "DualPlayConnectionTestScene";

        private GUIStyle titleStyle;
        private GUIStyle subtitleStyle;

        private void Awake()
        {
            Application.runInBackground = true;
        }

        private void Start()
        {
            string[] arguments = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < arguments.Length; i++)
            {
                if (arguments[i] == "-dualPlayHost" || arguments[i] == "-dualPlayClient")
                {
                    SceneManager.LoadScene(connectionSceneName, LoadSceneMode.Single);
                    return;
                }
            }
        }

        private void OnGUI()
        {
            EnsureStyles();

            float width = Mathf.Min(520f, Screen.width - 40f);
            float height = 330f;
            Rect panel = new Rect(
                (Screen.width - width) * 0.5f,
                (Screen.height - height) * 0.5f,
                width,
                height);

            GUILayout.BeginArea(panel, GUI.skin.box);
            GUILayout.Space(28f);
            GUILayout.Label("TURTLE & BUNNY", titleStyle);
            GUILayout.Label("Two-player cooperative prototype", subtitleStyle);
            GUILayout.Space(28f);
            GUILayout.Label("Host always plays Turtle. Client always plays Bunny.", subtitleStyle);
            GUILayout.Label("Both players connect in the lobby before InGame starts.", subtitleStyle);
            GUILayout.Space(24f);

            if (GUILayout.Button("Enter Connection Lobby", GUILayout.Height(52f)))
            {
                SceneManager.LoadScene(connectionSceneName, LoadSceneMode.Single);
            }

            GUILayout.Space(8f);
            if (GUILayout.Button("Quit", GUILayout.Height(36f)))
            {
                Application.Quit();
            }
            GUILayout.EndArea();
        }

        private void EnsureStyles()
        {
            if (titleStyle != null)
            {
                return;
            }

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 34,
                fontStyle = FontStyle.Bold
            };
            subtitleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 16,
                wordWrap = true
            };
        }
    }
}
