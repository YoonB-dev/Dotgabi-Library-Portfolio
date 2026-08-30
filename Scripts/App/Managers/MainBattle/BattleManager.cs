using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable]
public class jobCanvas
{
    public EnumTypes.JobType jobType;
    public Canvas canvas;
}

public class BattleManager : SceneSingleton<BattleManager>
{
    /// <summary>
    /// 1. 적 정보 설정 및 소환
    /// 2. 덱 정보 보기(나의 덱, 사용한 카드, 사용 가능한 카드)
    /// 3. 아이템 정보 보기
    /// </summary>
    [SerializeField] private int fightEnemyCount = 0;
    //GameObject realted to card
    [Header("Item")]
    public List<int> haveTreasure = new List<int>();
    public bool isVictory = false;
    public bool isBattleFinish = false;
    public ScenarioDTO SCENARIO_DATA;
    private int useCardCount = 0; // 사용한 카드 수
    public Player player;
    [SerializeField] private GameObject useCardShowButton, canCardShowButton;

    [Header("----------[Canvas]----------")]
    [SerializeField] private List<jobCanvas> jobCanvases;
    [SerializeField] private Canvas BlackCanvas;
    [SerializeField] private GameObject backgroundGroup; // 전투 배경 그룹
    private readonly int victoryNotSelectCardCoinAmount = 20; // 승리 후 카드 선택 안하면 추가로 주는 재화

    [Header("----------[Text]----------")]
    [SerializeField] private TextMeshProUGUI gameMoney;
    [SerializeField] private Transform motionTextPos; // 재화 획득 움직이는 텍스트들 위치
    [SerializeField] private GameObject moveTextPrefab; // 움직이는 텍스트 프리팹

    void Start()
    {
        SetScenarioData();
        player.Job = (EnumTypes.JobType)SCENARIO_DATA.JobId;
        SetJobCanvas();

        // 전투 시작 브금
        AudioManager.Instance.StartbattleBGM();
        StartCoroutine(SetStartFadeOutCanvas());
        StartCoroutine(SetBattleStart());
    }

    public void SetScenarioData()
    {
        switch (GameData.Instance.CurrScenarioType)
        {
            case EnumMainType.ScenarioType.story:
                SCENARIO_DATA = UserData.Instance.MainScenarioData;
                break;
            case EnumMainType.ScenarioType.challenge:
                SCENARIO_DATA = UserData.Instance.ChallengeScenarioData;
                break;
        }
    }

    private IEnumerator SetBattleStart()
    {
        LogManager.Instance.AddLogBattle(LogManager.Instance?.GetLocalizedText("start_battle"));
        StartCoroutine(SummonEnemy());
        // 사용 가능한 카드를 세팅합니다.
        CardSystem.Instance.SetCard(SCENARIO_DATA);
        // popup canvas의 카메라를 설정
        PopupManager.Instance.SetCanvasCamera(Camera.main);
        // 유물 아이콘 세팅
        ArtifactShowManager.Instance.SetItems(SCENARIO_DATA);

        player.SetPlayerStats();
        SetBackGround();
        SetMoneyText();

        yield return null;  // 한 프레임 기다림
        StartCoroutine(player.HpbarMotion(EnumTypes.TextMotionType.up));
        yield return new WaitForSecondsRealtime(0.5f);
        yield return StartCoroutine(TurnManager.Instance.StartTurn()); // 여기서 1초 기다림
        yield return null; // 한 프레임 기다림

        //적 패시브 (전투 시작 시)
        for (int i = 0; i < EnemyManager.Instance.enemies.Count; i++)
        {
            if (!EnemyManager.Instance.enemies[i].isDie)
            {
                yield return StartCoroutine(PassiveFunction.Instance?.PassiveAction(EnumTypes.EnemyPassiveTrigger.start_battle, EnemyManager.Instance.enemies[i], player));
            }
        }


        // 시작 시 유물 효과 적용
        ArtifactFunction.Instance.ArtifactStartBattle(player, null);
        // 카드 버튼 활성화
        useCardShowButton.GetComponent<Button>().onClick.AddListener(() => ShowUserCardPopup());
        canCardShowButton.GetComponent<Button>().onClick.AddListener(() => ShowCanCardPopup());
    }

    public void GetCoinIngameData(int amount)
    {
        SetMoneyTextMotion(amount);
    }

    private void SetMoneyTextMotion(int amount)
    {
        GameObject motionText = null;
        foreach (Transform child in motionTextPos)
        {
            if (!child.gameObject.activeSelf)
            {
                motionText = child.gameObject;
                break;
            }
        }
        if (motionText == null)
        {
            motionText = Instantiate(moveTextPrefab, motionTextPos, false);
        }

        DOTween.Kill(motionText); // Stop previous tweens safely

        motionText.SetActive(true);
        motionText.transform.position = motionTextPos.position;

        motionText.transform.DOLocalMoveY(motionTextPos.position.y - 100, 1f).SetEase(Ease.OutCirc).OnComplete(() => {
            motionText.SetActive(false);
        });
        var textMesh = motionText.GetComponent<TextMeshProUGUI>();
        textMesh.text = amount.ToString();
        textMesh.color = amount > 0 ? Color.green : Color.red;

        SetMoneyText();
    }

    private void SetMoneyText()
    {
        gameMoney.text = SCENARIO_DATA.GameCoins.ToString();
    }



    private void SetBackGround()
    {
        foreach (Transform child in backgroundGroup.transform)
        {
            child.gameObject.SetActive(false);
        }

        int bgIndex = SCENARIO_DATA.StageList[SCENARIO_DATA.CurrStage - 1] - 1;

        if (bgIndex < 0) bgIndex = 0;
        if (bgIndex >= backgroundGroup.transform.childCount) bgIndex = backgroundGroup.transform.childCount - 1;

        backgroundGroup.transform.GetChild(bgIndex).gameObject.SetActive(true);
    }

    private void SetStartFadeInCanvas()
    {
        BlackCanvas.gameObject.SetActive(true);
        BlackCanvas.transform.GetChild(0).GetComponent<Image>().DOFade(1, 0f);
    }
    IEnumerator SetStartFadeOutCanvas()
    {
        BlackCanvas.gameObject.SetActive(true);
        BlackCanvas.transform.GetChild(0).GetComponent<Image>().DOFade(0, 1f);
        yield return new WaitForSeconds(1f);
        BlackCanvas.gameObject.SetActive(false);
    }

    private void SetJobCanvas()
    {
        Debug.Log(player.Job);
        for (int i = 0; i < jobCanvases.Count; i++)
        {
            if (jobCanvases[i].jobType == player.Job)
            {
                jobCanvases[i].canvas.enabled = true;
            }
            else
            {
                jobCanvases[i].canvas.enabled = false;
            }
        }
    }

    private void ShowUserCardPopup()
    {
        if (!CardSystem.Instance.canActive) return;

        if (CardSystem.Instance.usedCards.Count > 0)
        {
            PopupManager.Instance.ShowUsedCardPopup();
        }
        else
        {
            var noCardText = LogManager.Instance.GetLocalText("no_card");
            NotificationManager.Instance.SetShownNotification(noCardText);
        }
    }

    private void ShowCanCardPopup()
    {
        if (!CardSystem.Instance.canActive) return;

        if (CardSystem.Instance.canCards.Count > 0)
        {
            PopupManager.Instance.ShowCanCardPopup();
        }
        else
        {
            var noCardText = LogManager.Instance.GetLocalText("no_card_can_use");
            NotificationManager.Instance.SetShownNotification(noCardText);
        }
    }

    public IEnumerator SummonEnemy()
    {
        yield return new WaitForSecondsRealtime(1f);

        EnemyManager.Instance.SummonEnemy();
        fightEnemyCount++;
    }

    public void SetCardButton(bool isActive)
    {
        useCardShowButton.SetActive(isActive);
        canCardShowButton.SetActive(isActive);
    }

    public void GoToGameScene()
    {
        //승리 후 스테이지 저장
        SupabaseScenarioStage.Instance.AddScenarioSelectList(GameManager.Instance.nextNodeIndex ?? 0, SCENARIO_DATA);
        //다음 노드 초기화
        GameManager.Instance.nextNodeIndex = null;
        //업적
        SupabaseAchieve.Instance.AchieveCurrData(EnumTypes.AchieveType.battle_count, 1);
        SupabaseAchieve.Instance.AchieveCurrData(EnumTypes.AchieveType.total_use_card, useCardCount);
        StartCoroutine(GoToGameSceneCo());
    }

    private IEnumerator GoToGameSceneCo()
    {
        BlackCanvas.gameObject.SetActive(true);
        BlackCanvas.transform.GetChild(0).GetComponent<Image>().DOFade(1, 0.5f);

        //로그
        UserScenarioLogDTO logData = new();
        LogManager.Instance.SetLogMainScene(EnumTypes.LogActionType.battle_enter, logData, SCENARIO_DATA);

        if (!VictoryManager.Instance.vicSelectCard)
        {
            Task coinTask = SupabaseGetScenarioCoin.Instance.GetCoin(victoryNotSelectCardCoinAmount, SCENARIO_DATA);
            yield return new WaitUntil(() => coinTask.IsCompleted);
        }
        yield return new WaitForSecondsRealtime(0.5f);
        SceneManager.LoadScene("GameScene");
    }

    public IEnumerator GoToMainSceneCo()
    {
        BlackCanvas.gameObject.SetActive(true);
        BlackCanvas.transform.GetChild(0).GetComponent<Image>().DOFade(1, 0.5f);
        yield return new WaitForSecondsRealtime(0.5f);
        SceneManager.LoadScene("MainScene");
    }


    public void EndBattle()
    {
        CardSystem.Instance.isFinish = true;
        isBattleFinish = true;
        isVictory = true;

        VictoryManager.Instance.CallVictoryManager(GameManager.Instance.nextEnemyType);
    }
    public void AddUseCardCount(int count)
    {
        useCardCount += count;
    }

}
