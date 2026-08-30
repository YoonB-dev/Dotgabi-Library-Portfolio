using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginButtonHandler : SceneSingleton<LoginButtonHandler>
{
    [SerializeField] private Button guestLoginButton;

    void Start()
    {
        // 게스트 로그인 버튼 비활성화
        guestLoginButton.gameObject.SetActive(false);
    }
    /// <summary>
    /// 게스트 로그인 버튼을 활성화합니다.
    /// </summary>
    public void SetLoginButton()
    {
        Debug.Log("게스트 로그인 버튼 활성화 시작");
        guestLoginButton.gameObject.SetActive(true);
        //ButtonAnim.Instance.ButtonScaleIn(guestLoginButton.gameObject, 0.3f, 1f);
        Debug.Log("게스트 로그인 버튼 활성화 완료");
    }

    public void SetLoginButton(bool active)
    {
        guestLoginButton.gameObject.SetActive(active);
    }

    /// <summary>
    /// 게스트 로그인 버튼 클릭 이벤트 핸들러
    /// </summary>
    public async void GuestLoginButtonClicked()
    {
        guestLoginButton.gameObject.SetActive(false);
        await StartManager.Instance?.FadeInAsync();
        try
        {
            bool success = await LoginStrategy.Instance.LoginAsync();
            await Task.Delay(50); // 약간의 딜레이 추가

            if (success)
            {
                await StartManager.Instance?.GoToFirstMainSceneAsync();
                Debug.Log("게스트 로그인 성공, 게임 시작");
            }
            else
            {
                Debug.LogError("게스트 로그인 실패, 다시 시도해주세요.");
            }
        }
        catch (System.Exception ex)
        {
            NotificationManager.Instance.SetShownNotification("fail to guest login");
            Debug.LogError($"게스트 로그인 중 오류 발생: {ex.Message}");
        }
    }
}
