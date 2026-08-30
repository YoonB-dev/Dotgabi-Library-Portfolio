using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChallengeScenarioStartManager : SceneSingleton<ChallengeScenarioStartManager>
{
    [SerializeField] private GameObject challengeScenarioCanvas; //챌린지 시나리오 캔버스
    [SerializeField] private GameObject challengeMainUI; //챌린지 메인 UI
    [SerializeField] private GameObject challengeCharacterSelectUI; //챌린지 캐릭터 선택 UI

    [Header("Character Select")]
    private int chIndex = 1;
    [SerializeField] private GameObject leftButtonCh;
    [SerializeField] private GameObject rightButtonCh;
    [SerializeField] private TextMeshProUGUI CharacterName, CharacterDes; //캐릭터 이름, 설명
    [SerializeField] private GameObject CharGrid, chSelectBtn; //캐릭터 선택 위치
    /// <summary>
    /// 도전 컨텐츠 선택 -> 심연 모드
    /// </summary>
    public void ChallengeStoryScenarioButtonClick()
    {
        //SFX
        AudioManager.Instance.ButtonClickSound3();
        //메인 시나리오 그룹 활성화
        GameStartManager.Instance?.scenarioGroup.SetActive(false);

        SelectChallengeContent();
    }
    public void BackChallengeToMainScroll()
    {
        //SFX
        AudioManager.Instance.ButtonClickSound2();

        challengeScenarioCanvas.SetActive(false);
        GameStartManager.Instance?.scenarioGroup.SetActive(true);

        //UI false
        challengeMainUI.SetActive(false);
        challengeCharacterSelectUI.SetActive(false);
    }

    private void SelectChallengeContent()
    {
        challengeScenarioCanvas.SetActive(true);

        challengeMainUI.SetActive(true);
        challengeCharacterSelectUI.SetActive(false);

        MainScenarioStartManager.Instance?.SetBackButton(EnumMainType.MainScrollBackType.ChallengeToScroll);
    }

    // 챌린지 모드 시작 (데이터 존재하면 이어하기)
    public async void StartChallengeGame()
    {
        //SFX
        AudioManager.Instance.ButtonClickSound1();

        if (UserData.Instance.ChallengeScenarioData == null)
        {
            return;
        }
        //유저 id를 이용해 난이도 정보 불러오기
        UserData.Instance.ChallengeScenarioData = await UserChallengeScenarioDAO.Instance.GetUserChallengeScenarioDTO(UserData.Instance.UserAuthId);
        // user의 현재 시나리오 업데이트
        await SupabaseScenario.Instance.SetUserScenarioType(EnumMainType.ScenarioType.challenge);
        StartCoroutine(StartGameSceneCo());
    }

    // 챌린지 모드 시작 (새로 생성) -> 캐릭터 선택창으로 이동
    public void NewChallengeGameButtonClick()
    {
        //SFX
        AudioManager.Instance.ButtonClickSound1();

        // 캐릭터 선택 띄우기
        challengeMainUI.SetActive(false);
        challengeCharacterSelectUI.SetActive(true);
    }


    // 챌린지 모드 캐릭터 선택 후 시작 -> 챌린지 모드 시작
    public async void ChallengeCharacterSelectStartButtonClick()
    {
        //SFX
        AudioManager.Instance.ButtonClickSound1();

        //유저 id를 이용해 챌린지 시나리오 데이터 생성
        UserData.Instance.ChallengeScenarioData = await UserChallengeScenarioDAO.Instance.CreateNewChallengeScenario(UserData.Instance.UserAuthId, chIndex);
        if (UserData.Instance.MainScenarioData == null)
        {
            var text = LogManager.Instance?.GetLocalText("fail_data_load");
            NotificationManager.Instance.SetShownNotification(text);
            return;
        }
        // user의 현재 시나리오 업데이트
        await SupabaseScenario.Instance.SetUserScenarioType(EnumMainType.ScenarioType.challenge);
        StartCoroutine(StartGameSceneCo());
    }
    IEnumerator StartGameSceneCo()
    {
        //BlackCanvas.gameObject.SetActive(true);
        //BlackCanvas.transform.GetChild(0).GetComponent<Image>().DOFade(0, 1.5f);

        Debug.Log("게임 시작");
        yield return new WaitForSecondsRealtime(0.5f);
        SceneManager.LoadScene("GameScene");
        //BlackCanvas.gameObject.SetActive(false);
    }

    /// <summary>
    /// 챌린지 캐릭터 선택
    /// </summary>
    ///
    public void CharMove(bool isLeft)
    {
        int maxChIndex = InGameData.Instance.Jobs.Count - 1; //캐릭터 개수 (public은 제외)
        //SFX
        AudioManager.Instance.ButtonClickSound1();
        //캐릭터 선택
        if (isLeft)
        {
            rightButtonCh.SetActive(true);
            if (chIndex >= 2) { chIndex -= 1; }
            if (chIndex == 1) { leftButtonCh.SetActive(false); }
        }
        else
        {
            leftButtonCh.SetActive(true);
            if (chIndex <= maxChIndex - 1) { chIndex += 1; }
            if (chIndex == maxChIndex) { rightButtonCh.SetActive(false); }
        }

        SetCharacterLock();
    }
    public async void SetCharacterLock()
    {
        Vector2 nextPos = new Vector2(-1080 * (chIndex - 1), 0);
        CharGrid.transform.DOLocalMove(nextPos, 0.5f).SetEase(Ease.OutBack);
        CharacterName.text = InGameData.Instance.Jobs[chIndex - 1].Name;
        CharacterDes.text = InGameData.Instance.Jobs[chIndex - 1].Description;

        if (await JobUtils.Instance.CheckJobUnlock(chIndex))
        {
            CharGrid.transform.GetChild(chIndex - 1).GetComponent<Image>().color = Color.white;
            chSelectBtn.SetActive(true);
            CharGrid.transform.GetChild(chIndex - 1).GetChild(0).gameObject.SetActive(false);
        }
        else
        {
            CharGrid.transform.GetChild(chIndex - 1).GetComponent<Image>().color = Color.gray;
            chSelectBtn.SetActive(false);
            CharGrid.transform.GetChild(chIndex - 1).GetChild(0).gameObject.SetActive(true);
        }
        var logText = LogManager.Instance.GetMainLogText("scenario_main_character");
        LogManager.Instance.AddLogMain(logText);
    }
}
