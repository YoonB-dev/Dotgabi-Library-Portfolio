using System;
using System.Collections;
using System.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityNote;

public class StartManager : SceneSingleton<StartManager>
{
    [SerializeField] private CanvasScaler canvasScaler;
    [SerializeField] private GameObject startButton;
    [SerializeField] private TextMeshProUGUI userVirtualEmailText;

    [SerializeField] private Canvas FadeCanvas;

    public override void Awake()
    {
        base.Awake();
        //60프레임으로 고정시키기
        Application.targetFrameRate = 60;
        startButton.SetActive(false);

        // 세션 지우기 (테스트용)
        // AuthService.Instance.ClearSession();
    }

    private void SetRatio()
    {
        float screenRatio = (float)Screen.width / Screen.height;
        float referenceRatio = canvasScaler.referenceResolution.x / canvasScaler.referenceResolution.y;

        // 가로와 세로 비율 중 작은 값을 기준으로 스케일 조정
        float ratio = Mathf.Min(screenRatio / referenceRatio, 1f); // 최대 1배로 제한
        ButtonAnim.Instance.ratio = ratio;
    }

    private void Start()
    {
        StartAction();
    }

    private async void StartAction()
    {
        // 브금
        AudioManager.Instance.StartinGameBGM();
        // 화면 비율 설정
        SetRatio();
        await DataSettings();
        // 가상 이메일 표시
        userVirtualEmailText.text = PlayerPrefs.HasKey("VritualEmail") ? PlayerPrefs.GetString("VritualEmail").Split('@')[0] : "";
        LoginButtonHandler.Instance.SetLoginButton(false);
        var tryLoginTask = AuthService.Instance.TryAutoLoginAsync();
        bool success = await tryLoginTask;

        if (success)
        {
            // 자동 로그인 성공
            // 게임 시작 하면 됨.
            try
            {
                await InGameDataLoadManager.Instance.DataSettings();
                await UserDataLoadManager.Instance.DataSettings();
                Debug.Log("자동 로그인 성공, 게임 시작");
                StartGame();
                return;
            }
            catch (Exception e)
            {
                Debug.LogError($"InGameDataLoadeManager DataSettings 실패: {e.Message}");
            }
        }
        // 게스트 로그인 버튼 활성화
        StartCoroutine(ActivateGuestLoginButtonNextFrame());
        Debug.Log("자동 로그인 실패 - 게스트 로그인 버튼 활성화 완료");
    }

    private IEnumerator ActivateGuestLoginButtonNextFrame()
    {
        yield return null; // 한 프레임 대기
        LoginButtonHandler.Instance.SetLoginButton();
    }

    public async Task DataSettings()
    {
        await SupabaseClientProvider.Instance.InitializeAsync();
        await SupabaseClientProvider.Instance.InitializeGameDataAsync();

        Debug.Log("Supabase 클라이언트 초기화 완료");
    }

    private void StartGame()
    {
        startButton.SetActive(true);
        startButton.GetComponent<Button>().onClick.AddListener(() => {
            GoToMainScene();
        });
    }

    public void GoToMainScene()
    {
        bool istutorialCompleted = UserData.Instance.istutorialCompleted;
        FadeIn(() => {
            try
            {
                if (istutorialCompleted)
                {
                    SceneManager.LoadScene("MainScene");
                }
                else
                {
                    SceneManager.LoadScene("T_MainScene");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"씬 전환 실패: {e.Message}");
            }
        });
    }

    public void FadeIn(Action onComplete)
    {
        FadeCanvas.gameObject.SetActive(true);
        var image = FadeCanvas.transform.GetChild(0).GetComponent<Image>();
        // 시작은 완전히 투명 (alpha = 0)
        image.color = new Color(image.color.r, image.color.g, image.color.b, 0f);
        // 1초 동안 alpha 0 -> 1 (점점 어두워지는)
        image.DOFade(1f, 0.5f).OnComplete(() => {
            onComplete?.Invoke();  // 페이드 완료 후 콜백
        }).SetEase(Ease.OutSine);
    }

    // 새로 시작하기 - 게스트 용도
    public async Task GoToFirstMainSceneAsync()
    {
        await DataSettings();

        bool istutorialCompleted = UserData.Instance.istutorialCompleted;
        await InGameDataLoadManager.Instance.DataSettings();
        await UserDataLoadManager.Instance.DataSettings();

        // 필요하다면 여기서 씬 전환도 처리
        if (istutorialCompleted)
        {
            SceneManager.LoadScene("MainScene");
        }
        else
        {
            SceneManager.LoadScene("T_MainScene");
        }
    }

    public Task FadeInAsync()
    {
        var tcs = new TaskCompletionSource<bool>();
        FadeCanvas.gameObject.SetActive(true);
        var image = FadeCanvas.transform.GetChild(0).GetComponent<Image>();
        image.color = new Color(image.color.r, image.color.g, image.color.b, 0f);

        image.DOFade(1f, 0.5f).OnComplete(() => {
            tcs.SetResult(true);
        }).SetEase(Ease.OutSine);

        return tcs.Task;
    }

    public void FailLoad()
    {
        FadeCanvas.gameObject.SetActive(false);
        NotificationManager.Instance.SetShownNotification("Fail to Load Data. Please Try Again.");
    }
}
