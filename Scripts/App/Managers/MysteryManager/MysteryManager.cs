using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Spine.Unity;
using UnityEngine.Localization;
using System.Linq;

public class MysteryManager : SceneSingleton<MysteryManager>
{
    public GameObject mysteryCanvas,scroll,backgroundImg;
    public Image eventImage;
    public TextMeshProUGUI eventText;
    public Button[] selectButtons;
    public Button backButtons;
    public int selectTime=0;
    public GameObject forwardButton,actionButton;
    public ScenarioDTO SCENARIO_DATA;
    private bool isNextEvent = false;
    public void StartMystery(int nodeIndex)
    {
        SCENARIO_DATA = MoveSystem.Instance.SCENARIO_DATA;
        selectTime = 0;
        StartCoroutine(ShowEvent(nodeIndex));
        mysteryCanvas.SetActive(true);
        backgroundImg.SetActive(true);
    }

    // 이벤트 리스트를 시드에 따라 무작위로 설정
    private List<int> SetMysteryListBySeed(int seed)
    {
        var rand = new System.Random(seed);
        string publicPlace = "public_" + SCENARIO_DATA.CurrStage;
        string storyPlace = "story_" + SCENARIO_DATA.StageList[SCENARIO_DATA.CurrStage - 1];

        var eventDatas = InGameData.Instance.Events
            .Where(x => x.Place == publicPlace || x.Place == storyPlace)
            .OrderBy(x => rand.Next())
            .ToList();
        // 이거 지워야 함!!!!
        // eventDatas = InGameData.Instance.Events
        //    .Where(x => x.EventNum == 4)
        //    .OrderBy(x => rand.Next())
        //    .ToList();

        var eventList = new List<int>();
        foreach (var eventData in eventDatas)
        {
            if (eventData.EventNum > 0)
            {
                eventList.Add(eventData.EventNum);
            }
        }
        return eventList;
    }

    public IEnumerator ShowEvent(int nodeIndex)
    {
        Debug.Log("이벤트 시작");
        var eventList = SetMysteryListBySeed(UserData.Instance.MainScenarioData.GenerateSeed);
        if (SCENARIO_DATA.EventClear >= eventList.Count)
        {
            SCENARIO_DATA.EventClear = Random.Range(0, eventList.Count);
        }
        var mysteryIndex = eventList[SCENARIO_DATA.EventClear];

        selectTime+=1; // 선택 횟수

        // 만약 다음 이벤트가 정해져 있다면 그걸로 불러오기 (예: 사또 이벤트)
        if (SCENARIO_DATA.NextEvent != 0)
        {
            mysteryIndex = SCENARIO_DATA.NextEvent;
            isNextEvent = true;
        }

        var eventData = InGameData.Instance.Events
            .FirstOrDefault(x => x.EventNum == mysteryIndex);

        Debug.Log("eNum: " + eventData.EventNum);

        if (selectTime==1)
        {
            // 만약 첫번째 선택이면 스크롤 펼쳐지는 모션 추가하기
            scroll.transform.parent.gameObject.SetActive(true);
            scroll.SetActive(true);
            scroll.GetComponent<SkeletonAnimation>().AnimationState.SetAnimation(0, "open_fadeIn", false);
            yield return new WaitForSecondsRealtime(1.5f);
            ButtonAnim.Instance.ButtonScaleIn(eventImage.gameObject,0f,0.01f);
            eventImage.GetComponent<Image>().sprite = Resources.Load<Sprite>(eventData.EventList[selectTime-1].ImgPath);
            eventImage.GetComponent<Image>().DOFade(1, 1f);
        } else
        {
            Debug.Log("eCount: " + eventData.EventList.Count + "and selectTime: " + selectTime);
            if (eventImage.GetComponent<Image>().sprite != Resources.Load<Sprite>(eventData.EventList[selectTime - 1].ImgPath))
            {
                eventImage.GetComponent<Image>().DOFade(0, 0f);
                eventImage.GetComponent<Image>().sprite = Resources.Load<Sprite>(eventData.EventList[selectTime - 1].ImgPath);
                eventImage.GetComponent<Image>().DOFade(1, 1f);
            }
        }
        yield return new WaitForSecondsRealtime(1f);
        eventText.transform.parent.gameObject.SetActive(true);
        eventText.GetComponent<TextMeshProUGUI>().text = eventData.EventList[selectTime-1].EventText;
        //ButtonAnim.instance.ButtonScaleIn(eventText.transform.parent.gameObject,0f,0.01f);
        ButtonAnim.Instance.ButtonScaleIn(eventText.gameObject,0f,1f,0.3f, false);
        yield return new WaitForSecondsRealtime(1f);

        SetEventSelect(eventData.EventList[selectTime-1], nodeIndex);

        Debug.Log("stage: " + SCENARIO_DATA.StageMapData.StageLevels.Count);
        yield return null;
    }
    public void SetEventSelect(EventDTO eventDTO, int selectIndex)
    {
        // 일단 비활성화
        for(int i=0;i<selectButtons.Length;i++)
        {
            selectButtons[i].onClick.RemoveAllListeners();
            selectButtons[i].gameObject.SetActive(false);
        }
        actionButton.SetActive(false);

        for(int i=0;i< eventDTO.EventChoices.Count; i++)
        {
            selectButtons[i].gameObject.SetActive(true);
            selectButtons[i].GetComponent<Button>().interactable = true;
            ButtonAnim.Instance.ButtonScaleIn(selectButtons[i].gameObject, 0f, 0.01f);
            int temp = i;
            Debug.Log(eventDTO.EventChoices[temp].EventResult);

            selectButtons[i].onClick.AddListener(() => SelectChoice(selectIndex, eventDTO.EventChoices[temp].EventResult));
            selectButtons[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = eventDTO.EventChoices[i].ChoiceText;

            for (int k=0;k<eventDTO.EventChoices[temp].EventResult.ResultDTOs.Count;k++)
            {
                if (eventDTO.EventChoices[temp].EventResult.ResultDTOs[k].ExtraData == null)
                {
                    continue; // ExtraData가 없으면 건너뛰기
                }

                if (eventDTO.EventChoices[temp].EventResult.ResultDTOs[k].ResultAction == "coin_get")
                {
                    int amount = eventDTO.EventChoices[temp].EventResult.ResultDTOs[k].ExtraData.ContainsKey("amount") ?
                    int.Parse(eventDTO.EventChoices[temp].EventResult.ResultDTOs[k].ExtraData["amount"].ToString()) : 0;
                    // 코인 획득 시, 현재 코인보다 적은 경우 버튼 비활성화
                    if (amount < 0 && SCENARIO_DATA.GameCoins < -amount)
                    {
                        selectButtons[i].GetComponent<Button>().interactable = false;
                    }
                }
                // 아이템 필요 시, 현재 아이템 보유 여부 확인
                if (eventDTO.EventChoices[temp].EventResult.ResultDTOs[k].ExtraData.ContainsKey("item_exist"))
                {
                    int need_item_id = int.Parse(eventDTO.EventChoices[temp].EventResult.ResultDTOs[k].ExtraData["item_exist"].ToString());
                    if (UserData.Instance.MainScenarioData.OwnedArtifactList.Find(x => x.ArtifactId == need_item_id) == null)
                    {
                        selectButtons[i].GetComponent<Button>().interactable = false;
                    }
                }
            }
        }

        //대답 갯수에 따라 버튼 위치 조정
        if (eventDTO.EventChoices.Count == 3)
        {
            selectButtons[0].GetComponent<RectTransform>().localPosition = new Vector2(0, 6f);
            selectButtons[1].GetComponent<RectTransform>().localPosition = new Vector2(0, 4.15f);
            selectButtons[2].GetComponent<RectTransform>().localPosition = new Vector2(0, 2.3f);
        }
        else if(eventDTO.EventChoices.Count == 2)
        {
            selectButtons[0].GetComponent<RectTransform>().localPosition = new Vector2(0,5f);
            selectButtons[1].GetComponent<RectTransform>().localPosition = new Vector2(0,2.5f);
        }
    }
    void SelectChoice(int selectIndex, EventResultBundle eventChoice)
    {
        // SFX
        AudioManager.Instance.ButtonClickSound2();
        StartCoroutine(SelectChoiceCo(selectIndex, eventChoice));
    }
    //버튼 선택시
    IEnumerator SelectChoiceCo(int selectIndex, EventResultBundle eventRresultBundle)
    {
        for (int i = 0; i < selectButtons.Length; i++)
        {
            selectButtons[i].gameObject.SetActive(false);
        }

        // 선택지에 따른 결과 처리

        for (int i = 0; i < eventRresultBundle.ResultDTOs.Count; i++)
        {
            Debug.Log("이벤트 결과 처리: " + eventRresultBundle.ResultDTOs[i].ResultType);
            // 이벤트 결과 실행
            MysteryResult.Instance.ResultAction(eventRresultBundle.ResultDTOs[i]);
            switch (eventRresultBundle.ResultDTOs[i].ResultType)
            {
                case "continue":
                    StartCoroutine(ShowEvent(selectIndex));
                    Debug.Log("이벤트 계속 진행");
                    break;
                case "end":
                    Debug.Log("이벤트 종료");
                    StartCoroutine(SetBackButtons());
                    SupabaseScenarioStage.Instance.AddScenarioSelectList(selectIndex, SCENARIO_DATA);
                    break;
                case "chain":
                    break;
                case "upgrade":
                    SetActionButton("Upgrade");
                    if (!eventRresultBundle.ResultDTOs[i].ExtraData.ContainsKey("force"))
                    {
                        SupabaseScenarioStage.Instance.AddScenarioSelectList(selectIndex, SCENARIO_DATA);
                    }
                    break;
                case "delete":
                    SetActionButton("Delete", !eventRresultBundle.ResultDTOs[i].ExtraData.ContainsKey("force"));
                    if (!eventRresultBundle.ResultDTOs[i].ExtraData.ContainsKey("force"))
                    {
                        SupabaseScenarioStage.Instance.AddScenarioSelectList(selectIndex, SCENARIO_DATA);
                    }
                    break;
                case "mini_game_shell":
                    SupabaseScenarioStage.Instance.AddScenarioSelectList(selectIndex, SCENARIO_DATA);
                    MiniGameShell.Instance.StartMinigame();
                    break;
            }
        }
        // 결과 텍스트 표시
        eventText.GetComponent<TextMeshProUGUI>().text = eventRresultBundle.ResultText;

        yield return null;
    }

    public IEnumerator SetBackButtons()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        backButtons.gameObject.SetActive(true);
        yield return null;
    }
    public void BackButtons()
    {
        // SFX
        AudioManager.Instance.ButtonClickSound1();
        AudioManager.Instance.CloseScrollSound();

        StartCoroutine(BackButton());
        if (isNextEvent)
        {
            UserData.Instance.MainScenarioData.NextEvent = 0;
            SCENARIO_DATA.NextEvent = 0; // 다음 이벤트 초기화
            MysterySupabase.Instance.InitNextEvent(); // Supabase에 다음 이벤트 초기화 요청

        }
    }
    IEnumerator BackButton()
    {
        scroll.GetComponent<SkeletonAnimation>().AnimationState.SetAnimation(0, "closed_fadeOut", false);
        actionButton.SetActive(false);
        SCENARIO_DATA.EventClear += 1;
        // RPC로 Event clear 정보 늘리기
        yield return MysterySupabase.Instance.IncreaseClearCount(SCENARIO_DATA);
        // 게임 저장하는거 하나 넣기
        //GameManager.gameManager.SaveGame(MoveSystem.moveSystem.gameData);
        eventImage.gameObject.SetActive(false);
        eventText.transform.parent.gameObject.SetActive(false);
        selectButtons[0].gameObject.SetActive(false);
        selectButtons[1].gameObject.SetActive(false);
        selectButtons[2].gameObject.SetActive(false);
        backButtons.gameObject.SetActive(false);
        yield return new WaitForSecondsRealtime(1.5f);
        forwardButton.SetActive(true);
        scroll.SetActive(false);
        backgroundImg.SetActive(false);
        mysteryCanvas.SetActive(false);
        yield return null;
    }

    public void GetMoney(int num, bool isSave = false)
    {
        // SFX
        AudioManager.Instance.MoneySound();

        //수치 적용
        SCENARIO_DATA.GameCoins+=num;
        SCENARIO_DATA.TotalGameCoins+=num;

        //수치 텍스트 생성
        SetFooterText.Instance.SetMoveText(num, EnumTypes.MoveTextType.money);

        if (num >= 0)
        {
            // 로그 출력
            var logData = new UserScenarioLogDTO {
                value = num,
                ExtraData = new Dictionary<string, object> {
                    { "coin", true },
                },
            };
            LogManager.Instance.SetLogMainScene(EnumTypes.LogActionType.player_get_something, logData, SCENARIO_DATA);
        }
        else
        {
            var logData = new UserScenarioLogDTO {
                value = num,
                ExtraData = new Dictionary<string, object> {
                    { "coin", true },
                },
            };
            LogManager.Instance.SetLogMainScene(EnumTypes.LogActionType.player_lose_something, logData, SCENARIO_DATA);
        }
    }
    public void GetMaxHp(int num)
    {
        // SFX
        if(num>=0)AudioManager.Instance.GetMaxHpSound();
        else AudioManager.Instance.GetDamageSound();

        SCENARIO_DATA.MaxHp+=num;
        SCENARIO_DATA.CurrHp+=num;
        Debug.Log("currHp: " + SCENARIO_DATA.CurrHp);
        if(SCENARIO_DATA.MaxHp<1)SCENARIO_DATA.MaxHp=1;
        if(SCENARIO_DATA.CurrHp<1)SCENARIO_DATA.CurrHp=1;

        //if(SCENARIO_DATA.CurrHp<=0)
        //MoveSystem.moveSystem.GameOver();
        //수치 텍스트 생성

        if (num>=0)
        {
            SetFooterText.Instance.SetMoveText(num, EnumTypes.MoveTextType.heal);
            SetFooterText.Instance.SetHpBar(EnumTypes.TextMotionType.up);
        }
        else
        {
            SetFooterText.Instance.SetMoveText(num, EnumTypes.MoveTextType.damage);
            SetFooterText.Instance.SetHpBar(EnumTypes.TextMotionType.down);
        }
    }

    public void SetActionButton(string type, bool back = true)
    {
        selectButtons[0].gameObject.SetActive(false);
        selectButtons[1].gameObject.SetActive(false);
        selectButtons[2].gameObject.SetActive(false);
        StartCoroutine(SetActionButtonCo(type, back));
    }
    IEnumerator SetActionButtonCo(string type, bool canBack = true)
    {
        yield return new WaitForSecondsRealtime(0.5f);
        actionButton.SetActive(true);

        switch (type)
        {
            case "Upgrade":
                string textUpgrade = new LocalizedString("LocalTable","Card-Upgrade").GetLocalizedString();
                actionButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = textUpgrade;
                actionButton.GetComponent<Button>().onClick.RemoveAllListeners();
                actionButton.GetComponent<Button>().onClick.AddListener(() => {PopupManager.Instance.ShowCardUpgradePopup(); actionButton.SetActive(false);});

                backButtons.gameObject.SetActive(canBack);
                break;
            case "Delete":
                string textDelete = new LocalizedString("LocalTable", "Card-Delete").GetLocalizedString();
                actionButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = textDelete;
                actionButton.GetComponent<Button>().onClick.RemoveAllListeners();
                actionButton.GetComponent<Button>().onClick.AddListener(() => {PopupManager.Instance.ShowCardDeletePopup(canBack); actionButton.SetActive(false);});

                backButtons.gameObject.SetActive(canBack);

                break;
        }
        yield return null;
    }
}
