using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;
using LoadSceneMode = UnityEngine.SceneManagement.LoadSceneMode;

public class LevelSceneLoader : MonoBehaviour
{
    [SerializeField] private GameObject levelPanel;
    [SerializeField] private GameObject loadPanel;
    [SerializeField] private Slider loadingBar;

    private bool isLoading;

    private void Start()
    {
        UpdateSelectionAuthority();
    }

    public void LoadScene(string sceneName)
    {
        if (isLoading)
        {
            return;
        }

        NetworkManager networkManager = NetworkManager.Singleton;
        if (networkManager != null && networkManager.IsListening && !networkManager.IsHost)
        {
            Debug.Log("레벨 선택은 Host만 할 수 있습니다.");
            return;
        }

        if (string.IsNullOrWhiteSpace(sceneName) ||
            !Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.Log($"로드할 수 없는 씬입니다: {sceneName}");
            return;
        }

        isLoading = true;

        if (levelPanel != null)
        {
            levelPanel.SetActive(false);
        }

        if (loadPanel != null)
        {
            loadPanel.SetActive(true);
        }

        if (loadingBar != null)
        {
            loadingBar.value = 0f;
        }

        if (networkManager != null && networkManager.IsListening)
        {
            SceneEventProgressStatus result = networkManager.SceneManager.LoadScene(
                sceneName,
                LoadSceneMode.Single);

            if (result != SceneEventProgressStatus.Started)
            {
                isLoading = false;
                Debug.LogError($"네트워크 씬 로드에 실패했습니다: {sceneName} ({result})");

                if (levelPanel != null)
                {
                    levelPanel.SetActive(true);
                }

                if (loadPanel != null)
                {
                    loadPanel.SetActive(false);
                }
            }

            return;
        }

        StartCoroutine(LoadAsync(sceneName));
    }

    private void UpdateSelectionAuthority()
    {
        NetworkManager networkManager = NetworkManager.Singleton;
        bool canSelect = networkManager == null ||
                         !networkManager.IsListening ||
                         networkManager.IsHost;

        if (levelPanel == null)
        {
            return;
        }

        Button[] levelButtons = levelPanel.GetComponentsInChildren<Button>(true);
        foreach (Button levelButton in levelButtons)
        {
            levelButton.interactable = canSelect;
        }
    }

    private IEnumerator LoadAsync(string sceneName)
    {
        float minLoadTime = 0.5f;
        float timer = 0f;

        AsyncOperation operation = UnitySceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            timer += Time.deltaTime;

            float realProgress = Mathf.Clamp01(operation.progress / 0.9f);
            float timeProgress = Mathf.Clamp01(timer / minLoadTime);
            float targetProgress = Mathf.Min(realProgress, timeProgress);

            if (loadingBar != null)
            {
                loadingBar.value = Mathf.Lerp(loadingBar.value, targetProgress, realProgress);
            }

            if (realProgress >= 1f && timeProgress >= 1f)
            {
                if (loadingBar != null)
                {
                    loadingBar.value = 1f;
                }

                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
