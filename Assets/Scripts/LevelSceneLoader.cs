using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

public class LevelSceneLoader : MonoBehaviour
{
    [SerializeField] private GameObject levelPanel;
    [SerializeField] private GameObject loadPanel;
    [SerializeField] private Slider loadingBar;

    private bool isLoading;

    public void LoadScene(string sceneName)
    {
        if (isLoading)
        {
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

        StartCoroutine(LoadAsync(sceneName));
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
