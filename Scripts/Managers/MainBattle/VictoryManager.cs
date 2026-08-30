using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using TMPro;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

public class VictoryManager : SceneSingleton<VictoryManager>
{
    private ScenarioDTO SCENARIO_DATA;
    [SerializeField] private GameObject[] selectCards;
    [SerializeField] private Canvas headerMoneyCanvas;
    [SerializeField] private GameObject vicNotificationBox, vicBox, vicBackground, BG_obj;
    [SerializeField] private Button SwitchCardbutton, OwnedCardShowButton;
    [SerializeField] private Canvas BlackCanvas, victoryCanvas;
    private int vicCardUpgrade; //카드 업그레이드 수치
    public bool vicSelectCard = false; //승리후 카드 획득했는지 확인 - 안했으면 돈 주기
    public bool isVictory = false;
    public EnemyText enemyText;
    private int victoryCoinAmount = 30; // 승리 후 지급되는 코인 수량
    private int victoryResteCardCoinAmount = 20; // 승리 후 카드 재설정 시 지급되는 코인 수량
    [Header("----------[Talk Box]----------")]
    public GameObject textBackButton;
    public Image textBox;

    [Header("----------[GameOver]----------")]
    [SerializeField] private Canvas gameOverCanvas;
    [SerializeField] private Button gameOverButton;
    [SerializeField] private Button gameReviveButton;
    private bool isRevive = false; // 부활 여부


    [Header("----------[Final]----------")]
    [SerializeField] private GameObject finishBox;
    [SerializeField] private GameObject finalButton, finalCanvas, backFadeInImg, webImg, thxTxt, finalBackButton, backFadeImg;
    [SerializeField] Canvas main, header, footer;
    void Start()
    {
        SwitchCardbutton.onClick.AddListener(() => ResetVicCard());
        OwnedCardShowButton.onClick.AddListener(() => PopupManager.Instance.ShowPopup(EnumTypes.PopupType.Card, false));

        gameOverButton.onClick.AddListener(() => GameOverEndButton());
        gameReviveButton.onClick.AddListener(() => GameOverReviveButton());
    }
    public async void CallVictoryManager(EnumTypes.EnemyType enemyType, bool isFinalBoss = false)
    {
        SCENARIO_DATA = BattleManager.Instance.SCENARIO_DATA;
        CardSystem.Instance?.CardGroupSetActive(false);
        //Final Boss
        if (isFinalBoss)
        {
            EnemyText.Instance?.EndEnemyText();
            finishBox.SetActive(true);

            finishBox.transform.GetChild(1).GetComponent<Button>().onClick.RemoveAllListeners();
            finishBox.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(() => FinishGame(true));

            victoryCanvas.gameObject.SetActive(true);
            CardSystem.Instance.CardAlignment(1);
            vicBackground.SetActive(true);
            vicBox.SetActive(false);
            BG_obj.SetActive(true);

            ButtonAnim.Instance.ButtonScaleIn(finishBox, 0f, 1f);
            AudioManager.Instance.VictorySound();
            return;
        }
        //SFX
        AudioManager.Instance.VictorySound();

        victoryCanvas.gameObject.SetActive(true);
        SetHeaderCoinCanvasUp();
        CardSystem.Instance.CardAlignment(1);
        vicBackground.SetActive(true);
        BG_obj.SetActive(false);
        ButtonAnim.Instance.ButtonScaleIn(vicBox, 0f, 1f);

        vicCardUpgrade = (int)enemyType - 1;
        SetVictoryCard(vicCardUpgrade);

        // 핸드 카드들 재정렬
        CardSystem.Instance.CardAlignment(1);


        // gamemanager
        if (GameManager.Instance.nextEnemyId != null)
        {
            Debug.Log("현재 몬스터: " + GameManager.Instance.nextEnemyId);
            if (GameManager.Instance.nextEnemyId == 9)
            {
                Debug.Log("스토리 몹 처리");
                SupabaseMainScenarioBattle.Instance.SetNextEventDefault(SCENARIO_DATA);
            }

            GameManager.Instance.nextEnemyId = null;
        }
        // 승리 후 재화 지급
        await SupabaseGetScenarioCoin.Instance.GetCoin(victoryCoinAmount, SCENARIO_DATA);
        BattleManager.Instance.GetCoinIngameData(victoryCoinAmount);
        // 시간 조절
        await Task.Delay(500); // delays for 0.5 seconds (500 milliseconds)
        // 승리 후 유물 효과 적용
        ArtifactFunction.Instance.ArtifactEndBattle(BattleManager.Instance.player, null);
        // 승리 데이터 저장
        bool isElite = enemyType == EnumTypes.EnemyType.elite ? true : false;
        SCENARIO_DATA.CurrHp = BattleManager.Instance.player.Stats.currHp;
        SCENARIO_DATA.MaxHp = BattleManager.Instance.player.Stats.maxHp;
        await SupabaseMainScenarioBattle.Instance.CallUpdateBattleResult(SCENARIO_DATA.CurrHp, SCENARIO_DATA.MaxHp, isElite, scenarioData: SCENARIO_DATA);
    }
    private void SetVictoryCard(int vicCardUpgrade)
    {
        //카드 선택
        List<CardDTO> selectCardList = new List<CardDTO>();
        var publicCards = InGameData.Instance.Cards.FindAll(x => x.CardJob.Contains(0) && x.CardType != EnumTypes.CardType.curse && x.CardType != EnumTypes.CardType.special && x.Id <= 25);
        var jobCards = InGameData.Instance.Cards.FindAll(x => x.CardJob.Contains((int)SCENARIO_DATA.JobId) && x.CardType != EnumTypes.CardType.curse && x.CardType != EnumTypes.CardType.special);

        List<CardDTO> combinedCards = new List<CardDTO>();
        combinedCards.AddRange(publicCards);
        combinedCards.AddRange(jobCards);

        var shuffledCards = combinedCards.OrderBy(x => Random.value).ToList();

        for (int i = 0; i < selectCards.Length; i++)
        {
            var card = shuffledCards[i].Copy();
            int upgradeRandom = Random.Range(0, 10);

            if (upgradeRandom < 1)
            {
                card.CardUpgrade = Mathf.Max(2, vicCardUpgrade);
            }
            else if (upgradeRandom < 4)
            {
                card.CardUpgrade = Mathf.Max(1, vicCardUpgrade);
            }
            else
            {
                card.CardUpgrade = vicCardUpgrade;
            }
            selectCardList.Add(card);
        }

        for (int i = 0; i < selectCardList.Count; i++)
        {
            int temp = i;
            selectCards[i].transform.GetChild(6).GetComponent<Button>().onClick.RemoveAllListeners();
            selectCards[i].transform.GetChild(6).GetComponent<Button>().onClick.AddListener(() => SelectCard(selectCardList[temp], temp));

            selectCards[i].GetComponent<Button>().onClick.RemoveAllListeners();
            selectCards[i].GetComponent<Button>().onClick.AddListener(() => PopupManager.Instance.ShowCardDetail(selectCardList[temp]));

            CardDTOToObj.DTOToObj(selectCards[i], selectCardList[temp]);
        }
    }
    public async void ResetVicCard()
    {
        if (SCENARIO_DATA.GameCoins >= victoryResteCardCoinAmount)
        {
            SetVictoryCard(vicCardUpgrade);

            await SupabaseGetScenarioCoin.Instance.GetCoin(-victoryResteCardCoinAmount, SCENARIO_DATA);
            BattleManager.Instance.GetCoinIngameData(-victoryResteCardCoinAmount);

        }
        else
        {
            string text = new LocalizedString("LocalTable", "Money-Less").GetLocalizedString();
            NotificationManager.Instance.SetShownNotification(text);
        }
    }

    //승리후 카드 선택 메서드
    public async void SelectCard(CardDTO card, int cardIndex)
    {
        // cardIndex는 victory후 카드 위치임 최대 3, card의 cardIndex와는 다름
        //SFX
        AudioManager.Instance.ButtonClickSound1();

        // //카드 도감 채우기
        // switch (cardData.cardOcc)
        // {
        //     case "Blacksmith":
        //         GameManager.gameManager.totalGameData.blackSmithCardCollection[cardData.cardNum - 1] = 1;
        //         break;
        //     case "Dosa":
        //         GameManager.gameManager.totalGameData.DosaCardCollection[cardData.cardNum - 1] = 1;
        //         break;
        //     case "Performer":
        //         GameManager.gameManager.totalGameData.PerformerCardCollection[cardData.cardNum - 1] = 1;
        //         break;
        // }


        SwitchCardbutton.interactable = false;
        vicSelectCard = true;

        for (int i = 0; i < selectCards.Length; i++)
        {
            selectCards[i].transform.GetChild(6).gameObject.SetActive(false);
            selectCards[i].transform.GetChild(0).GetComponent<Image>().color = Color.gray;
            selectCards[i].transform.GetChild(1).GetComponent<Image>().color = Color.gray;
            selectCards[i].transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = System.Text.RegularExpressions.Regex.Replace(selectCards[i].transform.GetChild(3).GetComponent<TextMeshProUGUI>().text, @"<color=.*?>|</color>", "");
            selectCards[i].transform.GetChild(3).GetComponent<TextMeshProUGUI>().color = Color.gray;
            selectCards[i].transform.GetChild(4).GetComponent<TextMeshProUGUI>().text = System.Text.RegularExpressions.Regex.Replace(selectCards[i].transform.GetChild(4).GetComponent<TextMeshProUGUI>().text, @"<color=.*?>|</color>", "");
            selectCards[i].transform.GetChild(4).GetComponent<TextMeshProUGUI>().color = Color.gray;
            selectCards[i].transform.GetChild(5).GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.gray;
            selectCards[i].transform.GetChild(5).GetComponent<Image>().color = Color.gray;
        }
        selectCards[cardIndex].transform.GetChild(7).gameObject.SetActive(true);


        bool success = await SupabaseCard.Instance.GetCard(SCENARIO_DATA, card);
        if (!success) return;


        BattleManager.Instance.GoToGameScene();
    }



    // 사망 시 게임오버 버튼 활성화
    public void GameOverButtonActive()
    {
        //SFX
        AudioManager.Instance.OpenMiniScrollSound();
        gameOverCanvas.gameObject.SetActive(true);
        ButtonAnim.Instance.ButtonScaleIn(gameOverButton.gameObject, 0f, 1f);
        if (!isRevive) { gameReviveButton.gameObject.SetActive(true); }
        else { gameReviveButton.gameObject.SetActive(false); }
        CardSystem.Instance.CardReSetAll();
    }
    // 부활 버튼 클릭 시
    public void GameOverReviveButton()
    {
        //SFX
        AudioManager.Instance.HealSound();

        var player = BattleManager.Instance.player;
        int reviveHp = player.Stats.maxHp / 2;

        player.Stats.currHp = reviveHp;
        player.Stats.currShield = 0;
        player.isDie = false;
        player.StartCoroutine(player.HpbarMotion(EnumTypes.TextMotionType.up));
        isRevive = true;

        //UI 비활성화
        gameOverCanvas.gameObject.SetActive(false);
        gameOverButton.gameObject.SetActive(false);
        gameReviveButton.gameObject.SetActive(false);

        StartCoroutine(TurnManager.Instance.StartTurn());
    }
    // 게임오버 끝내기 버튼 클릭 시
    public void GameOverEndButton()
    {
        //SFX
        AudioManager.Instance.ButtonClickSound1();
        Debug.Log("GameOverEndButton Clicked");
    }

    //도깨비 처치 시 나오게 되는 캔버스 세팅
    public void SetEndMotion()
    {
        //battleManager.enemies[0].transform.GetChild(1).GetComponent<SpriteRenderer>().DOFade(0, 0.5f);
        headerMoneyCanvas.gameObject.SetActive(false);
        StartCoroutine(SetEndMotionCo());
    }
    IEnumerator SetEndMotionCo()
    {
        yield return new WaitForSecondsRealtime(1f);
        ButtonAnim.Instance.ButtonScaleIn(finalButton, 0f, 1f);
    }

    //다음 버튼 클릭 시 웹툰 등장 및 마무리
    public void SetEndWebToon()
    {
        finalCanvas.SetActive(true);
        for (int i = 0; i < finalCanvas.transform.childCount; i++)
        {
            finalCanvas.transform.GetChild(i).gameObject.SetActive(false);
        }
        backFadeInImg.SetActive(true);
        backFadeInImg.GetComponent<Animator>().Play("FadeIn");
        StartCoroutine(SetEndWebToonCo());
    }
    IEnumerator SetEndWebToonCo()
    {
        yield return new WaitForSecondsRealtime(1.5f);

        main.gameObject.SetActive(false);
        header.gameObject.SetActive(false);
        footer.gameObject.SetActive(false);
        headerMoneyCanvas.gameObject.SetActive(false);

        backFadeInImg.GetComponent<Image>().DOFade(0, 1f);
        finalCanvas.transform.GetChild(0).gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(1f);
        ButtonAnim.Instance.ButtonScaleIn(thxTxt, 0f, 1f);
        yield return new WaitForSecondsRealtime(1f);
        ButtonAnim.Instance.ButtonScaleOut(thxTxt);
        yield return new WaitForSecondsRealtime(1f);
        backFadeInImg.SetActive(false);
        ButtonAnim.Instance.ButtonFadeInScale(webImg);
        yield return new WaitForSecondsRealtime(1f);
        ButtonAnim.Instance.ButtonScaleIn(finalBackButton, 0f, 1f);
        yield return null;
    }

    public void FinishGame(bool isDot = false)
    {
        //SFX
        AudioManager.Instance.ButtonClickSound1();
        if (isDot)
        {
            SetEndWebToon();
            return;
        }

        // 게임 클리어 데이터 기록
        if (SCENARIO_DATA is UserMainScenarioDTO)
        {
            InsertClearData();

            // 이거 이미 InsertClearData에서 처리함
            //UpdateMainScenarioClear(clearedStage);
        }
    }

    private async void InsertClearData()
    {
        await SupabaseScenario.Instance.InsertUserScenarioClear(GameData.Instance.CurrScenarioType);
    }

    public void FinishGameBackButton()
    {
        //SFX
        AudioManager.Instance.ButtonClickSound1();
        finalBackButton.SetActive(false);
        StartCoroutine(BattleManager.Instance?.GoToMainSceneCo());
    }
    public async void UpdateMainScenarioClear(EnumTypes.Difficulty scenarioDifficulty)
    {
        await SupabaseClientProvider.Instance.InitializeAsync();
        var client = SupabaseClientProvider.Instance.Client;

        var response = await client.Rpc("update_main_scenario_clear", new Dictionary<string, object> {
            {"stage_name", $"{scenarioDifficulty}"},
        });

        Debug.Log($"UpdateScenario: {response}");
    }

    public void SetHeaderCoinCanvasUp()
    {
        headerMoneyCanvas.sortingLayerName = "Ui_Victory";
    }
}
