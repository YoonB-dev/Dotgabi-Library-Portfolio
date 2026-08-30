using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum SceneName
{
    StartScene, MainScene, GameScene, BattleRoomScene,
    // tutorial
    T_MainScene, T_GameScene, T_Battle,
    // story
    CrimeScene, OnuHouse
}

namespace UnityNote
{
    public class SceneLoader : MonoBehaviorSingleton<SceneLoader>
    {
        [SerializeField]
        private GameObject loadingScreen; // 로딩 화면
        [SerializeField]
        private Image loadingBackground; // 로딩 화면 배경 이미지
        [SerializeField]
        private Sprite[] loadingBackgrounds; // 배경 이미지 목록
        [SerializeField]
        private Slider progressBar; // 진행 바
        [SerializeField]
        private TextMeshProUGUI textProgress; // 진행 텍스트

        private WaitForSeconds waitTime = new WaitForSeconds(0.1f); // 대기 시간

        float startTime = Time.time;  // 로딩 시작 시간 기록
        float minLoadingTime = 2.0f;  // 최소 2초 보장

        public void LoadScene(string sceneName)
        {
            int ranIndex = Random.Range(0, loadingBackgrounds.Length);
            loadingBackground.sprite = loadingBackgrounds[ranIndex];
            loadingBackground.SetNativeSize();

            progressBar.value = 0;
            loadingScreen.SetActive(true);

            StartCoroutine(LoadSceneAsync(sceneName));
        }

        public void LoadScene(SceneName sceneName)
        {
            LoadScene(sceneName.ToString());
        }

        public IEnumerator LoadSceneAsync(string sceneName)
        {
            AsyncOperation operation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
            operation.allowSceneActivation = false;

            loadingScreen.transform.DOScale(1.1f, 0.1f).SetEase(Ease.OutBack);
            yield return loadingScreen.GetComponent<CanvasGroup>().DOFade(1f, 0.3f).WaitForCompletion();

            while (!operation.isDone)
            {
                if (operation.progress >= 0.9f && Time.time - startTime >= minLoadingTime)
                {
                    progressBar.value = 1f;
                    textProgress.text = "100%";

                    // Fade Out
                    yield return loadingScreen.GetComponent<CanvasGroup>().DOFade(0f, 0.5f).WaitForCompletion();
                    loadingScreen.SetActive(false);
                    operation.allowSceneActivation = true;
                    yield break;  // 즉시 종료
                }

                progressBar.value = Mathf.Clamp01(operation.progress / 0.9f);
                textProgress.text = (progressBar.value * 100f).ToString("F0") + "%";
                yield return null;

                yield return null;
            }
        }
    }

}

