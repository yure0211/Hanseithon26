using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Hanseithon.DualPlaySample
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NetworkObject))]
    public sealed class GameOverMenuController : NetworkBehaviour
    {
        [SerializeField] private string levelSceneName = "Level";
        [SerializeField] private Button returnButton;
        [SerializeField] private TMP_Text statusText;

        private bool isReturning;

        private void Awake()
        {
            if (returnButton != null)
            {
                returnButton.onClick.AddListener(ReturnToLevel);
            }
        }

        public override void OnDestroy()
        {
            if (returnButton != null)
            {
                returnButton.onClick.RemoveListener(ReturnToLevel);
            }

            base.OnDestroy();
        }

        public void ReturnToLevel()
        {
            if (isReturning)
            {
                return;
            }

            NetworkManager manager = NetworkManager.Singleton;
            if (manager != null && manager.IsListening)
            {
                if (!IsSpawned)
                {
                    SetStatus("네트워크 씬 준비 중...");
                    return;
                }

                if (manager.IsServer)
                {
                    BeginNetworkReturn();
                }
                else
                {
                    SetStatus("호스트에 이동을 요청하는 중...");
                    RequestReturnToLevelRpc();
                }

                return;
            }

            BeginOfflineReturn();
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void RequestReturnToLevelRpc()
        {
            BeginNetworkReturn();
        }

        private void BeginNetworkReturn()
        {
            if (!IsServer || isReturning || !CanLoadLevelScene())
            {
                return;
            }

            isReturning = true;
            ShowReturningStateClientRpc();

            SceneEventProgressStatus result = NetworkManager.SceneManager.LoadScene(
                levelSceneName,
                LoadSceneMode.Single);
            if (result != SceneEventProgressStatus.Started)
            {
                isReturning = false;
                Debug.LogError($"레벨 선택 씬 로드에 실패했습니다: {result}", this);
            }
        }

        private void BeginOfflineReturn()
        {
            if (!CanLoadLevelScene())
            {
                return;
            }

            isReturning = true;
            ShowReturningState();
            SceneManager.LoadScene(levelSceneName, LoadSceneMode.Single);
        }

        [ClientRpc]
        private void ShowReturningStateClientRpc()
        {
            isReturning = true;
            ShowReturningState();
        }

        private void ShowReturningState()
        {
            if (returnButton != null)
            {
                returnButton.interactable = false;
            }

            SetStatus("레벨 선택 화면으로 이동 중...");
        }

        private bool CanLoadLevelScene()
        {
            if (!string.IsNullOrWhiteSpace(levelSceneName) &&
                Application.CanStreamedLevelBeLoaded(levelSceneName))
            {
                return true;
            }

            Debug.LogError(
                $"레벨 선택 씬 '{levelSceneName}'이 Build Settings에 없습니다.",
                this);
            SetStatus("레벨 선택 씬을 불러올 수 없습니다.");
            return false;
        }

        private void SetStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
        }

#if UNITY_EDITOR
        public void ConfigureInEditor(Button button, TMP_Text text, string sceneName)
        {
            returnButton = button;
            statusText = text;
            levelSceneName = sceneName;
        }

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(levelSceneName))
            {
                levelSceneName = "Level";
            }
        }
#endif
    }
}
