using System.Collections;
using DG.Tweening;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// <summary>
// 게임 시작 메인 스크롤을 관리하는 매니저. -> 열고 닫기 기능을 제공
// </summary>
public class GameStartManager : SceneSingleton<GameStartManager>
{
    public CamBox cambox;
    public GameObject scenarioGroup; //메인 시나리오

    public GameObject mainStartScroll;
    public GameObject scrollCloseButton;
    public GameObject[] mainBattleButtons, scenarioButtons;
    public GameObject backButton;

    [Header("Character Select")]
    [SerializeField] private GameObject CharacterSelect; //캐릭터 선택
    [SerializeField] private GameObject CharGrid, chSelectBtn; //캐릭터 선택 위치
    [SerializeField] private GameObject leftButtonCh;
    [SerializeField] private GameObject rightButtonCh;
    [SerializeField] private Button characterSelectButton; // 캐릭터 선택 버튼

    [Header("Level Select")]
    [SerializeField] private GameObject LvSelect; //레벨 선택
    [SerializeField] private GameObject LvGrid, lvSelectBtn; //레벨 선택 위치
    [SerializeField] private GameObject leftButtonLv;
    [SerializeField] private GameObject rightButtonLv;
    [SerializeField] private Button lvSelectButton; // 레벨 선택 버튼

    private int chIndex = 1, lvIndex = 1; // 캐릭터 인덱스, 레벨 인덱스

    void Start()
    {
        characterSelectButton.onClick.RemoveAllListeners();
        characterSelectButton.onClick.AddListener(() => {
            MainScenarioStartManager.Instance?.SelectCharacter();
        });

        lvSelectButton.onClick.RemoveAllListeners();
        lvSelectButton.onClick.AddListener(() => {
            MainScenarioStartManager.Instance?.StartNewGame();
        });
    }

    public void ClickMainScroll()
    {
        StartCoroutine(MainScrollOpen());
        //SFX
        AudioManager.Instance.OpenScrollSound();
        //로그
        var logText = LogManager.Instance.GetMainLogText("main_scroll_open");
        LogManager.Instance.AddLogMain(logText);
    }

    IEnumerator MainScrollOpen()
    {
        //업적 버튼 비활성화
        MainManager.Instance.SetAchieveButton(false);
        //헤더 비활성화
        for (int i = 0; i < MainManager.Instance.Headers.Length; i++)
        {
            MainManager.Instance.Headers[i].SetActive(false);
        }
        //블록 이미지 활성화 및 카메라 움직임 제한
        mainStartScroll.transform.GetChild(0).gameObject.SetActive(true);
        cambox.SetCanMove(false);
        //메인 스크롤 열기
        mainStartScroll.transform.GetChild(1).GetComponent<SkeletonAnimation>().AnimationState.SetAnimation(0, "open", false); // 스크롤 모션
        mainStartScroll.GetComponent<RectTransform>().DOLocalMove(Vector2.zero, 0.5f); // 가운데 위치 시키기
        mainStartScroll.transform.GetChild(2).gameObject.SetActive(false); // 텍스트 숨기기
        mainStartScroll.GetComponent<Button>().interactable = false; // 추가 터치 비활성화
        // SFX
        AudioManager.Instance.OpenMiniScrollSound();
        scrollCloseButton.SetActive(true);
        yield return new WaitForSecondsRealtime(1f);
        // 메인 시나리오, 동화 시나리오, 심연 시나리오 보여주기
        for (int i = 0; i < scenarioButtons.Length; i++)
        {
            ButtonAnim.Instance.ButtonScaleIn(scenarioButtons[i], 0f, 0.8f);
        }
        //뒤로가기 배경 생성
        scrollCloseButton.transform.GetChild(0).GetComponent<Button>().onClick.RemoveAllListeners();
        scrollCloseButton.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(() => MainscrollClose());
        //뒤로 가기 버튼 설정
        backButton.SetActive(true);
        MainScenarioStartManager.Instance?.SetBackButton(EnumMainType.MainScrollBackType.ScrollToMain);
    }

    public void MainscrollClose()
    {
        StartCoroutine(MainScrollCloseCo());
        AudioManager.Instance.CloseScrollSound();

        backButton.SetActive(false);

        //로그
        var logText = LogManager.Instance.GetMainLogText("main_scroll_close");
        LogManager.Instance.AddLogMain(logText);

        //시나리오 다 종료
        MainScenarioStartManager.Instance?.BackStoryToMainScroll();
        ChallengeScenarioStartManager.Instance?.BackChallengeToMainScroll();
    }

    IEnumerator MainScrollCloseCo()
    {
        mainStartScroll.transform.GetChild(1).GetComponent<SkeletonAnimation>().AnimationState.SetAnimation(0, "closed", false); // 스크롤 모션
        scrollCloseButton.transform.GetChild(0).GetComponent<Button>().onClick.RemoveAllListeners();
        //선택지 비활성화
        for (int i = 0; i < scenarioButtons.Length; i++)
        {
            ButtonAnim.Instance.ButtonFadeOutScale(scenarioButtons[i]);
        }
        for (int i = 0; i < mainBattleButtons.Length; i++)
        {
            ButtonAnim.Instance.ButtonFadeOutScale(mainBattleButtons[i]);
        }

        MainScenarioStartManager.Instance?.BackStoryToMainScroll();
        MainScenarioStartManager.Instance?.BackCharacterToStoryScenario(true);
        MainScenarioStartManager.Instance?.BackLvToCharacter(true);

        //로그
        var closeTxt = LogManager.Instance.GetMainLogText("main_scroll_close");
        LogManager.Instance.AddLogMain(closeTxt);

        yield return new WaitForSecondsRealtime(1f); // 스크롤 모션 대기
        //뒤로가기 배경 제거
        scrollCloseButton.SetActive(false);
        mainStartScroll.transform.GetChild(0).gameObject.SetActive(false); // 블록 이미지 비 활성화 및 카메라 움직임 활성화
        cambox.SetCanMove(true);

        mainStartScroll.GetComponent<RectTransform>().DOLocalMove(new Vector2(0, -650), 0.5f); // 위치 이동
        mainStartScroll.transform.GetChild(2).gameObject.SetActive(true); // 텍스트 다시 띄우기
        //터치 활성화
        mainStartScroll.GetComponent<Button>().interactable = true;
        //업적 버튼 활성화
        MainManager.Instance.SetAchieveButton(true);

        //헤더 활성화
        for (int i = 0; i < MainManager.Instance.Headers.Length; i++)
        {
            MainManager.Instance.Headers[i].SetActive(true);
        }
    }
}
