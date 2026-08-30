using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainScenarioStartManager : SceneSingleton<MainScenarioStartManager>
{
    [SerializeField] private GameObject mainScenarioCanvas; //메인 시나리오 캔버스
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

    /// <summary>
    /// 메인 컨텐츠 선택 -> 스토리 모드
    /// </summary>
    public void MainStoryScenarioButtonClick()
    {
        //SFX
        AudioManager.Instance.ButtonClickSound3();
        // 시나리오 선택 그룹 비활성화
        GameStartManager.Instance?.scenarioGroup.SetActive(false);
        SelectMainContent();
    }
    private void SelectMainContent()
    {
        mainScenarioCanvas.SetActive(true);
        for (int i = 0; i < GameStartManager.Instance?.mainBattleButtons.Length; i++)
        {
            int temp = i;
            GameStartManager.Instance?.mainBattleButtons[temp].GetComponent<Button>().onClick.RemoveAllListeners();
            GameStartManager.Instance?.mainBattleButtons[temp].GetComponent<Button>().onClick.AddListener(() => { SelectScrollActive(temp); });
            // 이어하기 없으면 비활성화
            if (UserData.Instance.MainScenarioData == null && temp == 0)
            {
                GameStartManager.Instance?.mainBattleButtons[temp].SetActive(false);
            }
            else
            {
                ButtonAnim.Instance.ButtonFadeInScale(GameStartManager.Instance?.mainBattleButtons[temp], 0.3f, false);
            }

            //로그
            var logText = LogManager.Instance.GetMainLogText("scenario_main_click");
            LogManager.Instance.AddLogMain(logText);

            //뒤로가기 버튼 세팅
            SetBackButton(EnumMainType.MainScrollBackType.StartToScroll);
        }
    }

    public void SelectScrollActive(int num)
    {
        //SFX
        AudioManager.Instance.ButtonClickSound3();

        if (num == 0) { // 이어하기
            ContinueGame();
        }
        if (num == 1)
        {
            //새로하기
            //캐릭터 선택 창
            chIndex = 1;
            StartCoroutine(NewStartMotionCo());
            leftButtonCh.SetActive(false);
        }
    }

    //캐릭터 선택 화면(새로시작)
    IEnumerator NewStartMotionCo()
    {
        var chText = LogManager.Instance.GetMainLogText("scenario_main_character");
        LogManager.Instance.AddLogMain(chText);

        for (int i = 0; i < GameStartManager.Instance?.mainBattleButtons.Length; i++)
        {
            ButtonAnim.Instance.ButtonFadeOutScale(GameStartManager.Instance?.mainBattleButtons[i]);
        }
        // 맨처음 = 대장장이 캐릭터
        CharacterSelect.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = InGameData.Instance.Jobs.Find(job => job.Id == 1).Name;
        CharacterSelect.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = InGameData.Instance.Jobs.Find(job => job.Id == 1).Description;

        CharGrid.transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
        CharacterSelect.SetActive(true);

        CharGrid.GetComponent<RectTransform>().localScale = new Vector3(0, 0, 0);
        for (int i = 1; i < CharGrid.transform.childCount; i++)
        {
            CharGrid.transform.GetChild(i).gameObject.SetActive(false);
        }

        CharGrid.GetComponent<RectTransform>().DOScale(1, 0.3f).SetEase(Ease.OutBack).OnComplete(() => {
            for (int i = 1; i < CharGrid.transform.childCount; i++)
            {
                CharGrid.transform.GetChild(i).gameObject.SetActive(true);
            }
        });

        chSelectBtn.SetActive(true);
        SetBackButton(EnumMainType.MainScrollBackType.chToStart);
        yield return null;
    }

    //캐릭터 선택 후 난이도 선택
    public async void SelectCharacter()
    {
        //SFX
        AudioManager.Instance.ButtonClickSound1();
        //캐릭터 선택 후 난이도 선택
        CharacterSelect.SetActive(false);
        LvSelect.SetActive(true);

        LvGrid.GetComponent<RectTransform>().localScale = new Vector3(0, 0, 0);
        for (int i = 1; i < LvGrid.transform.childCount; i++)
        {
            LvGrid.transform.GetChild(i).gameObject.SetActive(false);
        }

        LvGrid.GetComponent<RectTransform>().DOScale(1, 0.3f).SetEase(Ease.OutBack).OnComplete(() => {
            for (int i = 1; i < LvGrid.transform.childCount; i++)
            {
                LvGrid.transform.GetChild(i).gameObject.SetActive(true);
            }
        });

        string LvName = LogManager.Instance.GetMainLogText("main_level_name");
        string LvDes = LogManager.Instance.GetMainLogText("main_level_descriptions");

        lvIndex = 1;
        LvSelect.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = LvName.Split("^")[lvIndex - 1];
        LvSelect.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = LvDes.Split("^")[lvIndex - 1];
        leftButtonLv.SetActive(false);

        //유저 id를 이용해 난이도 정보 불러오기
        UserData.Instance.MainScenarioClear = await UserScenarioClearDAO.Instance.GetUserMainSceneClearAsync(UserData.Instance.UserAuthId);

        //뒤로가기 버튼 세팅
        SetBackButton(EnumMainType.MainScrollBackType.lvToCh);
    }

    public void BackStoryToMainScroll()
    {
        //SFX
        AudioManager.Instance.ButtonClickSound2();

        mainScenarioCanvas.SetActive(false);
        GameStartManager.Instance?.scenarioGroup.SetActive(true);

        for (int i = 0; i < GameStartManager.Instance?.mainBattleButtons.Length; i++)
        {
            GameStartManager.Instance?.mainBattleButtons[i].SetActive(false);
        }
    }
    public void BackCharacterToStoryScenario(bool justClose = false)
    {
        //SFX
        AudioManager.Instance.ButtonClickSound2();
        CharGrid.transform.localPosition = new Vector2(0, 0);
        leftButtonCh.SetActive(true);
        rightButtonCh.SetActive(true);
        CharacterSelect.SetActive(false);

        if (justClose) return;

        for (int i = 1; i < GameStartManager.Instance?.mainBattleButtons.Length; i++)
        {
            GameStartManager.Instance?.mainBattleButtons[i].SetActive(true);
        }

        //이어하기 존재 여부 확인
        GameStartManager.Instance?.mainBattleButtons[0].SetActive(UserData.Instance.MainScenarioData != null);
    }
    public void BackLvToCharacter(bool justClose = false)
    {
        //SFX
        AudioManager.Instance.ButtonClickSound2();
        LvSelect.SetActive(false);
        LvSelect.transform.GetChild(0).GetChild(0).transform.localPosition = new Vector2(0, 0);
        if (justClose) return;
        CharacterSelect.SetActive(true);
    }

    public void SetBackButton(EnumMainType.MainScrollBackType backType)
    {
        GameStartManager.Instance?.backButton.GetComponent<Button>().onClick.RemoveAllListeners();

        switch (backType)
        {
            case EnumMainType.MainScrollBackType.ScrollToMain:
                GameStartManager.Instance?.backButton.GetComponent<Button>().onClick.AddListener(() => GameStartManager.Instance?.MainscrollClose());
                break;
            case EnumMainType.MainScrollBackType.StartToScroll:
                GameStartManager.Instance?.backButton.GetComponent<Button>().onClick.AddListener(() => {
                    BackStoryToMainScroll();
                    SetBackButton(EnumMainType.MainScrollBackType.ScrollToMain);
                });
                break;
            case EnumMainType.MainScrollBackType.chToStart:
                GameStartManager.Instance?.backButton.GetComponent<Button>().onClick.AddListener(() => {
                    BackCharacterToStoryScenario();
                    SetBackButton(EnumMainType.MainScrollBackType.StartToScroll);
                });
                break;
            case EnumMainType.MainScrollBackType.lvToCh:
                GameStartManager.Instance?.backButton.GetComponent<Button>().onClick.AddListener(() => {
                    BackLvToCharacter();
                    SetBackButton(EnumMainType.MainScrollBackType.chToStart);
                });
                break;
            case EnumMainType.MainScrollBackType.ChallengeToScroll:
                GameStartManager.Instance?.backButton.GetComponent<Button>().onClick.AddListener(() => {
                    ChallengeScenarioStartManager.Instance?.BackChallengeToMainScroll();
                    SetBackButton(EnumMainType.MainScrollBackType.ScrollToMain);
                });
                break;
        }
    }
    /// <summary>
    /// 캐릭터 및 난이도 좌우 이동
    /// </summary>

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
    public void LevelMove(bool isLeft)
    {
        //난이도 선택
        int maxStage = SelectLevelUtil.Instance.GetMainLevelCount(); // 최대임 +2 하는 이유는 쉬움, 어려움 있기 때문
        int maxLvIndex = SelectLevelUtil.Instance.GetMainClearLevel() + 1;
        if (maxLvIndex > maxStage) maxLvIndex = maxStage;

        if (isLeft)
        {
            rightButtonLv.SetActive(true);
            if (lvIndex >= 2)
                lvIndex -= 1;
            if (lvIndex == 1)
                leftButtonLv.SetActive(false);
        }
        else
        {
            leftButtonLv.SetActive(true);
            if (lvIndex <= maxLvIndex - 1)
                lvIndex += 1;
            if (lvIndex == maxLvIndex)
                rightButtonLv.SetActive(false);
        }

        Vector2 nextPos = new Vector2(-1080 * (lvIndex - 1), 0);
        LvSelect.transform.GetChild(0).GetChild(0).transform.DOLocalMove(nextPos, 0.5f).SetEase(Ease.OutBack);
        LvSelect.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = LogManager.Instance.GetMainLogText("main_level_name").Split("^")[lvIndex - 1];
        LvSelect.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = LogManager.Instance.GetMainLogText("main_level_descriptions").Split("^")[lvIndex - 1];
        var logText = LogManager.Instance.GetMainLogText("scenario_main_level");
        LogManager.Instance.AddLogMain(logText);

        if (maxLvIndex == lvIndex && maxLvIndex != maxStage)
        {
            LvSelect.transform.GetChild(4).GetComponent<Button>().interactable = false;
            string text = LogManager.Instance.GetMainLogText("main_level_locked");
            LvSelect.transform.GetChild(4).GetChild(0).GetComponent<TextMeshProUGUI>().text = text;
        }
        else
        {
            LvSelect.transform.GetChild(4).GetComponent<Button>().interactable = true;
            string text = LogManager.Instance.GetMainLogText("start");
            LvSelect.transform.GetChild(4).GetChild(0).GetComponent<TextMeshProUGUI>().text = text;
        }
    }

    public async void SetCharacterLock()
    {
        Vector2 nextPos = new Vector2(-1080 * (chIndex - 1), 0);
        CharGrid.transform.DOLocalMove(nextPos, 0.5f).SetEase(Ease.OutBack);
        CharacterSelect.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = InGameData.Instance.Jobs[chIndex].Name;
        CharacterSelect.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = InGameData.Instance.Jobs[chIndex].Description;

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




    /// <summary>
    /// 게임 이어하기
    /// </summary>
    public async void ContinueGame()
    {
        //SFX
        AudioManager.Instance.ButtonClickSound1();
        // 유저의 메인 시나리오 데이터 불러오기
        UserData.Instance.MainScenarioData = await UserMainScenarioDAO.Instance.GetUserMainScenarioDTO(UserData.Instance.UserAuthId);
        if (UserData.Instance.MainScenarioData == null)
        {
            var text = LogManager.Instance?.GetLocalText("fail_data_load");
            NotificationManager.Instance.SetShownNotification(text);
            return;
        }
        // user의 현재 시나리오 업데이트
        await SupabaseScenario.Instance.SetUserScenarioType(EnumMainType.ScenarioType.story);
        // 로그
        var logText = LogManager.Instance.GetMainLogText("scenario_main_start");
        LogManager.Instance.AddLogMain(logText);

        StartCoroutine(StartGameSceneCo());
    }

    /// <summary>
    /// 새로운 게임 시작
    /// </summary>
    public async void StartNewGame()
    {
        // 검은 화면
        StartCoroutine(MainSceneLifeCycleManager.Instance?.BlackcanvasFadeOut());

        UserData.Instance.MainScenarioData = await UserMainScenarioDAO.Instance.CreateNewMainScenario(UserData.Instance.UserAuthId, chIndex, lvIndex);
        if (UserData.Instance.MainScenarioData == null)
        {
            var text = LogManager.Instance?.GetLocalText("fail_data_load");
            NotificationManager.Instance.SetShownNotification(text);
            return;
        }
        // user의 현재 시나리오 업데이트
        await SupabaseScenario.Instance.SetUserScenarioType(EnumMainType.ScenarioType.story);

        Debug.Log("새로운 게임 시작123123");
        StartCoroutine(StartGameSceneCo());
    }

    IEnumerator StartGameSceneCo()
    {
        yield return new WaitForSecondsRealtime(0.1f);
        SceneManager.LoadScene("GameScene");
    }
}
