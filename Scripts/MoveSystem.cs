using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Spine.Unity;
using DG.Tweening;
using Spine;
using UnityEngine.Localization;
using System.Text.Json;
using System;
using UnityEngine.Localization.SmartFormat;
using System.Threading.Tasks;
using Unity.VisualScripting;

public class MoveSystem : SceneSingleton<MoveSystem>
{
    public GameObject forwardButton, demoButton, nextStageBtn, selectConfirmButton, walkingAnim;
    public GameObject[] selectButtons = new GameObject[3];
    [SerializeField] private GameObject storyScrollButton;
    private IEnumerator[] selectButtonsCo = new IEnumerator[3];
    private Coroutine storyScrollCo;
    public List<int> stageList;
    public int stage = 1;
    [SerializeField] private ShopManager shopManager;
    private List<Vector3[]> scrollPos = new List<Vector3[]>{
        new Vector3[1]{new (0,90,0)},
        new Vector3[2]{new (-200,90,0),new (200,90,0)},
        new Vector3[3]{new (-350,90,0),new (0,90,0),new (350,90,0)},
    };
    [SerializeField]
    private GameObject backGround, dotgabiKeyButton;
    private bool isRealBoss = false;
    public ScenarioDTO SCENARIO_DATA;
    public bool isNextStage = false;
    void Update()
    {
        //GameManager.gameManager.gameData.playTime += Time.deltaTime;
    }

    public async Task TriggerStart()
    {
        await SetScenarioData();
        SetMoveData();
        if (!isNextStage)
        {
            ButtonAnim.Instance.ButtonScaleIn(forwardButton, 0f, 1f);
        }
    }

    public void SetStartStory()
    {
        // 텍스트 스토리 진행
        var trigger = GetTextStoryType();
        if (trigger != null)
        {
            Debug.Log("텍스트 스토리 진행: " + trigger.ToString());
            MainStoryManager.Instance.ShowMainStoryCanvas(trigger);
        }
    }

    public async Task SetScenarioData()
    {
        switch (GameData.Instance.CurrScenarioType)
        {
            case EnumMainType.ScenarioType.story:
                UserData.Instance.MainScenarioData = await UserMainScenarioDAO.Instance.GetUserMainScenarioDTO(UserData.Instance.UserAuthId);
                SCENARIO_DATA = UserData.Instance.MainScenarioData;
                Debug.Log("MainScenarioData Loaded");
                break;
            case EnumMainType.ScenarioType.challenge:
                UserData.Instance.ChallengeScenarioData = await UserChallengeScenarioDAO.Instance.GetUserChallengeScenarioDTO(UserData.Instance.UserAuthId);
                SCENARIO_DATA = UserData.Instance.ChallengeScenarioData;
                break;
        }
    }

    public void SetMoveData()
    {
        SetBackgroud();
        stage = SCENARIO_DATA.SelectList.Count;
        Debug.Log("Stage: " + stage);

        for (int i = 0; i < selectButtonsCo.Length; i++)
        {
            selectButtonsCo[i] = emptyCo();
        }
        SetFooterText.Instance.SetHpBar(EnumTypes.TextMotionType.direct);

        selectConfirmButton.SetActive(false);

        //시작시 선택 스크롤 족자 닫기
        for (int i = 0; i < selectButtons.Length; i++)
        {
            selectButtons[i].transform.GetChild(0).GetComponent<SkeletonAnimation>().AnimationState.SetAnimation(0, "closed_stop", false);
            selectButtons[i].transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
            selectButtons[i].transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
            selectButtons[i].SetActive(false);
        }

        //BGM
        AudioManager.Instance.StartinGameBGM();
        Debug.Log("MoveSystem Call");
        // 유물 데이터 셋
        ArtifactShowManager.Instance.SetItems(SCENARIO_DATA);

        // 로그 데이터 초기화
        LogManager.Instance?.InitLogData(SCENARIO_DATA.LogList);
    }
    IEnumerator delayActive(GameObject target)
    {
        yield return 0.1f;
        target.SetActive(false);
    }
    public void SelectRoom(int nodeIndex)
    {
        // SFX
        AudioManager.Instance.ButtonClickSound1();

        for (int i = 0; i < selectButtons.Length; i++)
        {
            var anim = selectButtons[i].transform.GetChild(0).GetComponent<SkeletonAnimation>();
            var currTrack = anim.AnimationState.SetAnimation(0, "closed_stop", false);

            currTrack.MixTime = currTrack.Animation.Duration;

            selectButtons[i].transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
            selectButtons[i].transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
            //selectButtons[i].SetActive(false);
            StartCoroutine(delayActive(selectButtons[i].gameObject));
        }
        selectConfirmButton.SetActive(false);
        forwardButton.SetActive(false);

        var roomType = SCENARIO_DATA.StageMapData.StageLevels[SCENARIO_DATA.SelectList.Count].StageNodes[nodeIndex].NodeType;
        Debug.Log(nodeIndex);
        Debug.Log(roomType);
        switch (roomType)
        {
            case EnumTypes.StageType.enemy:
            case EnumTypes.StageType.elite:
            case EnumTypes.StageType.boss:
                GameManager.Instance.nextNodeIndex = nodeIndex;
                GameManager.Instance.nextEnemyType = Enum.TryParse<EnumTypes.EnemyType>(roomType.ToString(), false, out var result) ? result : EnumTypes.EnemyType.normal;
                StartCoroutine(GoToBattleScene());
                break;
            case EnumTypes.StageType.shop:
                shopManager.ShopStart();
                AudioManager.Instance.StartShopBGM();
                //로그
                SetGeneralLog(EnumTypes.LogActionType.shop_enter);
                SupabaseScenarioStage.Instance.AddScenarioSelectList(nodeIndex, SCENARIO_DATA);
                break;
            case EnumTypes.StageType.artifact:
                // SFX
                AudioManager.Instance.OpenScrollSound();
                TreasureManager.Instance.CallArtifactRoom();
                //로그
                SetGeneralLog(EnumTypes.LogActionType.artifact_enter);
                SupabaseScenarioStage.Instance.AddScenarioSelectList(nodeIndex, SCENARIO_DATA);
                break;
            case EnumTypes.StageType.mystery:
                // SFX
                AudioManager.Instance.OpenScrollSound();
                GameManager.Instance.nextNodeIndex = nodeIndex;
                MysteryManager.Instance.StartMystery(nodeIndex);
                //로그
                SetGeneralLog(EnumTypes.LogActionType.mystery_enter);
                break;
            case EnumTypes.StageType.rest:
                RestManager.Instance.OpenRestManager();
                SupabaseScenarioStage.Instance.AddScenarioSelectList(nodeIndex, SCENARIO_DATA);
                //로그
                SetGeneralLog(EnumTypes.LogActionType.rest_find);
                break;
        }
    }
    public IEnumerator GoToBattleScene(int enemyId = -1)
    {
        yield return StartCoroutine(GameSceneLifeCycleManager.Instance.BlackcanvasFadeOut());

        if (enemyId != -1)
        {
            GameManager.Instance.nextEnemyId = enemyId;
        }
        SceneManager.LoadScene("BattleRoomScene", LoadSceneMode.Single);
    }
    public void SelectBattleIndex()
    {
        //SCENARIO_DATA.SelectList.Add(buttonNum[buttonIndex]);
    }
    int moveCount = 0;
    async void SetSmallEvent(EventSmallDTO smallEventDTO)
    {
        System.Random random = new(SCENARIO_DATA.MapSeed + SCENARIO_DATA.SelectList.Count * 3 + moveCount);
        Debug.Log("SmallEvent Seed: " + (SCENARIO_DATA.MapSeed + SCENARIO_DATA.SelectList.Count * 3 + moveCount));

        int amount = random.Next(smallEventDTO.AmountMin, smallEventDTO.AmountMax + 1);

        switch (smallEventDTO.EventType)
        {
            case EnumTypes.EventSmallType.get_coin:
                await SupabaseGetScenarioCoin.Instance.GetCoin(amount, SCENARIO_DATA);
                SetFooterText.Instance.SetMoveText(amount, EnumTypes.MoveTextType.money);
                var logData = new UserScenarioLogDTO {
                    value = amount
                };
                LogManager.Instance.SetLogMainScene(EnumTypes.LogActionType.small_event_coin, logData, SCENARIO_DATA);
                break;
            case EnumTypes.EventSmallType.get_damage:
                await SupabaseGetScenarioCoin.Instance.GetHp(-amount, SCENARIO_DATA);
                var logData2 = new UserScenarioLogDTO {
                    value = amount
                };
                LogManager.Instance.SetLogMainScene(EnumTypes.LogActionType.small_event_damage, logData2, SCENARIO_DATA);
                break;
            case EnumTypes.EventSmallType.get_heal:
                await SupabaseGetScenarioCoin.Instance.GetHp(amount, SCENARIO_DATA);
                var logData3 = new UserScenarioLogDTO {
                    value = amount
                };
                LogManager.Instance.SetLogMainScene(EnumTypes.LogActionType.small_event_heal, logData3, SCENARIO_DATA);
                break;
        }

        var text = smallEventDTO.Text.FormatSmart(amount);
        NotificationManager.Instance.SetShownNotification(text);
    }
    public void MoveSelect()
    {
        stage = SCENARIO_DATA.SelectList.Count;

        System.Random random = new(SCENARIO_DATA.MapSeed + (SCENARIO_DATA.SelectList.Count + 1) * 3 + moveCount);
        moveCount++;
        int var = random.Next(0, 10);
        //다시 걷기
        if (var < 3 && moveCount < 3)
        {
            //int smallEvent = random.Next(0, 2);
            int smallEvent = 0;

            if (smallEvent == 0)
            {
                int smallIndex = random.Next(0, InGameData.Instance.EventSmalls.Count);
                SetSmallEvent(InGameData.Instance.EventSmalls[smallIndex]);
                //여기에 능력 능력 발동
                ButtonAnim.Instance.ButtonScaleIn(forwardButton, 0f, 1f);
            }
            else
            {
                // 로그
                SetGeneralLog(EnumTypes.LogActionType.find_nothing);
                ButtonAnim.Instance.ButtonScaleIn(forwardButton, 0f, 1f);
            }
            moveCount++;
        }
        else
        {
            //방 작동
            moveCount = 0;
            ShowRoom();
        }
    }
    public void ShowRoom()
    {
        Debug.Log("ShowRoom");
        stageList = SCENARIO_DATA.SelectList;
        int stageLevel = SCENARIO_DATA.SelectList.Count;

        if (GetTextStoryType() != null && stageLevel >= 2)
        {
            var trigger = GetTextStoryType();
            MainStoryManager.Instance.ShowMainStoryCanvas(trigger);
            return;
        }

        StageNode currLevelNode;
        bool isFirstLog;

        if (stageLevel == 0)
        {
            currLevelNode = SCENARIO_DATA.StageMapData.StageLevels[stageLevel].StageNodes[0];
            isFirstLog = true;
        }
        else
        {
            currLevelNode = SCENARIO_DATA.StageMapData.StageLevels[stageLevel - 1].StageNodes[stageList[stageList.Count - 1]];
            isFirstLog = false;
        }

        Debug.Log("Current Level Node NextNodes Count: " + SCENARIO_DATA.StageMapData.StageLevels.Count);

        if (currLevelNode.NextNodes.Count == 0 || currLevelNode.NextNodes == null)
        {
            // 스테이지 클리어
            Debug.LogError("No Next Nodes Available");
            return;
        }


        for (int i = 0; i < currLevelNode.NextNodes.Count; i++)
        {
            var targetScrollButton = selectButtons[currLevelNode.NextNodes[i].NodeIndex];

            targetScrollButton.SetActive(true);
            targetScrollButton.transform.localPosition = scrollPos[currLevelNode.NextNodes.Count - 1][i];
            var targetNodeType = currLevelNode.NextNodes[i].NodeType;

            if (targetNodeType == EnumTypes.StageType.elite || targetNodeType == EnumTypes.StageType.boss)
            {
                targetScrollButton.transform.localPosition = scrollPos[0][0];
            }

            switch (targetNodeType)
            {
                case EnumTypes.StageType.enemy:
                    SetScrollButton(targetScrollButton, "Image/Icon/icon_enemyAttack01", "red_enemy", "Scroll-Monster", false, EnumTypes.LogActionType.none);
                    break;
                case EnumTypes.StageType.shop:
                    SetScrollButton(targetScrollButton, "Image/Icon/icon_shop", "yellow_shop", "Shop", false, EnumTypes.LogActionType.none);
                    break;
                case EnumTypes.StageType.artifact:
                    SetScrollButton(targetScrollButton, "Image/Icon/icon_treasure", "yellow_treasure", "Scroll-Relics", false, EnumTypes.LogActionType.none);
                    break;
                case EnumTypes.StageType.mystery:
                    SetScrollButton(targetScrollButton, "Image/Icon/icon_questionMark", "blue_mystery", "Scroll-Mystery", false, EnumTypes.LogActionType.none);
                    break;
                case EnumTypes.StageType.rest:
                    SetScrollButton(targetScrollButton, "Image/Icon/icon_bonfire", "green_rest", "Scroll-Rest", true, EnumTypes.LogActionType.rest_find);
                    break;
                case EnumTypes.StageType.elite:
                    if (SCENARIO_DATA.GetType() == typeof(UserMainScenarioDTO))
                    {
                        var userMainData = (UserMainScenarioDTO)SCENARIO_DATA;
                        if (userMainData.CurrStage > 3 && userMainData.FirstPiece && userMainData.SecondPiece && userMainData.ThirdPiece)
                        {
                            SetScrollButton(targetScrollButton, "Image/Icon/icon_enemyAttack03", "red_enemyBoss", "Scroll-Deep", true, EnumTypes.LogActionType.boss);
                        }
                        else
                        {
                            SetScrollButton(targetScrollButton, "Image/Icon/icon_enemyAttack02", "red_enemyBoss", "Scroll-Elite", true, EnumTypes.LogActionType.elite);
                        }
                    }
                    else
                    {
                        SetScrollButton(targetScrollButton, "Image/Icon/icon_enemyAttack02", "red_enemyBoss", "Scroll-Elite", true, EnumTypes.LogActionType.elite);
                    }
                    break;
                case EnumTypes.StageType.boss:
                    SetScrollButton(targetScrollButton, "Image/Icon/icon_enemyAttack03", "red_enemyBoss", "Scroll-Boss", isFirstLog, EnumTypes.LogActionType.boss);
                    break;
            }
        }
    }

    void SetScrollButton(GameObject targetScrollButton, string spritePath, string skinName, string localizedKey, bool isFirstLog, EnumTypes.LogActionType logType = EnumTypes.LogActionType.none)
    {
        targetScrollButton.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(spritePath);
        var text = new LocalizedString("LocalTable", localizedKey);
        targetScrollButton.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text = text.GetLocalizedString();
        if (!isFirstLog && logType != EnumTypes.LogActionType.none)
        {
            SetGeneralLog(logType);
        }
        // 모션
        var skeleton = targetScrollButton.transform.GetChild(0).GetComponent<SkeletonAnimation>();
        Skin scrollSkin = skeleton.skeleton.Data.FindSkin(skinName);
        skeleton.skeleton.SetSkin(scrollSkin);
        skeleton.skeleton.SetSlotsToSetupPose();
        skeleton.AnimationState.Apply(skeleton.skeleton);
        skeleton.initialSkinName = skinName;
    }
    

    private EnumTypes.MainStoryTrigger? GetTextStoryType()
    {
        if (SCENARIO_DATA == null || SCENARIO_DATA.GetType() != typeof(UserMainScenarioDTO)) { return null; }

        var mainData = (UserMainScenarioDTO)SCENARIO_DATA;
        int currStage = mainData.StageList[mainData.CurrStage - 1];
        int CurrStageLevel = mainData.SelectList.Count;

        // 4 스테이지(도깨비) 시작
        if (mainData.CurrStageLevel >= 4)
        {
            //return EnumTypes.MainStoryTrigger.story_4_start;
            return null;
        }

        // 스토리 트리거 조건들
        // 1 스테이지 시작
        if (currStage == 1 && CurrStageLevel == 1)
        {
            return EnumTypes.MainStoryTrigger.story_1_start;
        }
        if (currStage == 1 && CurrStageLevel == 4 && !mainData.StoryClearData.CrimeSceneClear)
        {
            return EnumTypes.MainStoryTrigger.story_1_before_elite;
        }
        else if (currStage == 1 && CurrStageLevel == 14 && !mainData.StoryClearData.OnuHouseClear && mainData.StoryClearData.TigerArrest != true)
        {
            return EnumTypes.MainStoryTrigger.story_1_after_elite;
        }

        // 2 스테이지 시작
        if (currStage == 2 && CurrStageLevel == 1)
        {
            return EnumTypes.MainStoryTrigger.story_2_start;
        }

        if (currStage == 3 && CurrStageLevel == 1)
        {
            return EnumTypes.MainStoryTrigger.story_3_start;
        }

        return null;
    }

    public void MoveForward()
    {
        // SFX
        AudioManager.Instance.WalkingSound();
        forwardButton.SetActive(false);

        //업적
        //AchieveManager.instance.AchieveCheck("Battle_Forward",1);

        StartCoroutine(ForwardButton());
    }
    IEnumerator ForwardButton()
    {
        walkingAnim.SetActive(true);
        // 로그
        SetGeneralLog(EnumTypes.LogActionType.move_forward);
        SupabaseAchieve.Instance.AchieveCurrData(EnumTypes.AchieveType.move_forward_count, 1);
        var tween = backGround.GetComponent<RectTransform>().DOLocalMoveY(10, 0.15f).SetLoops(2, LoopType.Yoyo);
        yield return tween.WaitForCompletion();
        yield return new WaitForSecondsRealtime(0.2f);
        var tween2 = backGround.GetComponent<RectTransform>().DOLocalMoveY(10, 0.15f).SetLoops(2, LoopType.Yoyo);
        yield return tween2.WaitForCompletion();
        yield return new WaitForSecondsRealtime(0.2f);

        walkingAnim.SetActive(false);
        backGround.GetComponent<RectTransform>().localScale = Vector3.one;
        MoveSelect();
    }
    //스크롤 클릭하면 발생하는 이벤트
    public void ShowConfirmButton(int num)
    {
        // SFX
        AudioManager.Instance.OpenMiniScrollSound();

        selectConfirmButton.SetActive(true);
        selectConfirmButton.GetComponent<RectTransform>().localScale = new Vector2(0.5f, 0.5f);
        selectConfirmButton.GetComponent<RectTransform>().DOScale(1, 0.3f).SetEase(Ease.OutBack);

        selectConfirmButton.GetComponent<Button>().onClick.RemoveAllListeners();
        selectConfirmButton.GetComponent<Button>().onClick.AddListener(() => SelectRoom(num));

        for (int i = 0; i < selectButtons.Length; i++)
        {
            if (i != num)
            {
                selectButtons[i].transform.GetChild(0).GetComponent<SkeletonAnimation>().AnimationState.SetAnimation(0, "closed_stop", false);
                StopCoroutine(selectButtonsCo[i]);
                selectButtonsCo[i] = ScrollInfoWait(selectButtons[i], num);
                StartCoroutine(selectButtonsCo[i]);
            }
            else
            {
                selectButtons[i].transform.GetChild(0).GetComponent<SkeletonAnimation>().AnimationState.SetAnimation(0, "open", false);
                StopCoroutine(selectButtonsCo[i]);
                selectButtonsCo[i] = ScrollInfoWait(selectButtons[i], num, true);
                StartCoroutine(selectButtonsCo[i]);
            }
        }
    }

    IEnumerator ScrollInfoWait(GameObject butt, int selectNum, bool isSelect = false)
    {
        butt.transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
        butt.transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
        if (isSelect)
        {
            yield return new WaitForSecondsRealtime(0.15f);
            butt.transform.GetChild(0).GetChild(0).gameObject.SetActive(true);
            butt.transform.GetChild(0).GetChild(1).gameObject.SetActive(true);
            yield return null;
        }
    }

    public void GameClear()
    {
        Debug.Log("게임 클리어 입니다.");
        InsertClearData();
        //업적
        StartCoroutine(GameClearCo());
    }

    private async void InsertClearData()
    {
        await SupabaseScenario.Instance.InsertUserScenarioClear(GameData.Instance.CurrScenarioType);
    }
    IEnumerator GameClearCo()
    {
        yield return null;
        StartCoroutine(GoToMainSceneCo());
    }

    public void NextStage()
    {
        if (SCENARIO_DATA != null && SCENARIO_DATA.GetType() == typeof(UserMainScenarioDTO))
        {
            var userMainData = (UserMainScenarioDTO)SCENARIO_DATA;
            if (userMainData.CurrStage == 3)
            {
                if ((int)userMainData.Difficulty < 3)
                {
                    GameClear();
                    return;
                }
                else
                {
                    if (!isRealBoss)
                    {
                        GameClear();
                        return;
                    }
                    else
                    {
                        // 보스방 조건 달성

                    }
                }

            }
        }

        // 다음 스테이지로 데이터 업데이트 및 배경 설정
        UpdateNextStage();

        //애니메이션 및 버튼 삭제
        nextStageBtn.SetActive(false);
        SetFooterText.Instance.SetAllText();

        // 초기화
        isNextStage = false;
    }

    private async void UpdateNextStage()
    {
        // 검은 화면
        StartCoroutine(GameSceneLifeCycleManager.Instance.BlackcanvasFadeOut());
        await SupabaseScenario.Instance.UpdateUserScenarioNextStage(GameData.Instance.CurrScenarioType);
        // 다시 시작
        await GameSceneLifeCycleManager.Instance?.GameSceneStart();
    }
    public void GameOver()
    {
        Debug.Log("게임오버 입니다.");
    }
    private IEnumerator emptyCo()
    {
        yield return null;
    }

    public void GoToMainScene()
    {
        // SFX
        AudioManager.Instance.ButtonClickSound1();

        StartCoroutine(GoToMainSceneCo());
    }

    IEnumerator GoToMainSceneCo()
    {
        yield return StartCoroutine(GameSceneLifeCycleManager.Instance.BlackcanvasFadeOut());
        SceneManager.LoadScene("MainScene");
        yield return null;
    }

    private void SetBackgroud()
    {
        for (int i = 0; i < backGround.transform.childCount; i++)
        {
            if (i == SCENARIO_DATA.StageList[SCENARIO_DATA.CurrStage - 1] - 1)
            {
                backGround.transform.GetChild(i).gameObject.SetActive(true);
            }
            else backGround.transform.GetChild(i).gameObject.SetActive(false);
        }

        //도깨비 키 버튼
        if (SCENARIO_DATA != null && SCENARIO_DATA.GetType() == typeof(UserMainScenarioDTO))
        {
            var userMainData = (UserMainScenarioDTO)SCENARIO_DATA;
            if ((int)userMainData.Difficulty >= 3)
            {
                dotgabiKeyButton.SetActive(true);
                SetDotgabiKey();
            }
            else
            {
                dotgabiKeyButton.SetActive(false);
            }
        }
        else
        {
            dotgabiKeyButton.SetActive(false);
        }
    }
    public void SetDotgabiKey()
    {
        if (SCENARIO_DATA == null || SCENARIO_DATA.GetType() != typeof(UserMainScenarioDTO)) { return; }
        var userMainData = (UserMainScenarioDTO)SCENARIO_DATA;

        dotgabiKeyButton.transform.GetChild(0).gameObject.SetActive(userMainData.FirstPiece);
        dotgabiKeyButton.transform.GetChild(1).gameObject.SetActive(userMainData.SecondPiece);
        dotgabiKeyButton.transform.GetChild(2).gameObject.SetActive(userMainData.ThirdPiece);


        isRealBoss = userMainData.FirstPiece && userMainData.SecondPiece && userMainData.ThirdPiece;
        Debug.Log("도깨비 키 여부: " + isRealBoss);
    }

    public void SetForwardButtonActive(bool isActive)
    {
        forwardButton.SetActive(isActive);
    }


    /// <summary>
    /// 로그를 남기는 메서드 -> 추가적인 데이터 없이 단순 평문을 남기는 로그일 경우 실행
    /// </summary>
    private void SetGeneralLog(EnumTypes.LogActionType logActionType)
    {
        UserScenarioLogDTO logData = new();
        LogManager.Instance.SetLogMainScene(logActionType, logData, SCENARIO_DATA);
    }

    public void CheckNextStage()
    {
        int stageLevel = SCENARIO_DATA.SelectList.Count;
        if (stageLevel == 0) { return; }
        if (SCENARIO_DATA.StageMapData.StageLevels.Count < stageLevel - 1) { Debug.LogWarning("warning: SCENARIO_DATA was OUT of Range"); return; }
        var currLevelNode = SCENARIO_DATA.StageMapData.StageLevels[stageLevel - 1].StageNodes[stageList[stageList.Count - 1]];
        if (currLevelNode.NextNodes.Count == 0)
        {
            if (SCENARIO_DATA.StageList.Count > SCENARIO_DATA.CurrStageLevel)
            {
                // 스테이지 클리어
                // 다음 스테이지 버튼 활성화
                nextStageBtn.SetActive(true);
                // 이동 버튼 비활성화
                forwardButton.SetActive(false);
                isNextStage = true;
            }
            else
            {
                // 조각 다 모았는지 확인
                if (SCENARIO_DATA.GetType() == typeof(UserMainScenarioDTO))
                {
                    var userMainData = (UserMainScenarioDTO)SCENARIO_DATA;
                    if (userMainData.CurrStage == 3)
                    {
                        if (!isRealBoss)
                        {
                            // 일반 보스
                            demoButton.SetActive(true);
                        }
                        else
                        {
                            // 진짜 보스
                            demoButton.SetActive(false);
                            nextStageBtn.SetActive(true);
                            return;
                        }

                    }
                }
                // 게임 클리어
                demoButton.SetActive(true);
            }
        }
    }
}
