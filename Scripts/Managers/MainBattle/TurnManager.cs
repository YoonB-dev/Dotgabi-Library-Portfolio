using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

public class TurnManager : SceneSingleton<TurnManager>
{
    public bool isFinish = false;
    public bool isTutorial = false;
    private readonly int START_CARD_COUNT = 3;
    private readonly int START_ACTION_COUNT = 2;
    private bool isStartBattle = false; // 맨 처음 전투 시작시만 true
    [SerializeField] private CardSystem cardSystem;
    [SerializeField] private GameObject startTurnObj, endTurnObj;

    public IEnumerator StartTurn()
    {
        if (isFinish) yield break;
        var pCom = BattleManager.Instance.player;

        var text = LogManager.Instance?.GetLocalizedText("start_turn");
        LogManager.Instance?.AddLogBattle(text);

        pCom.ShieldBreakBase();
        var co = StartCoroutine(TurnCo(true));
        SetTurn(true);
        yield return co;


        //전투시작시 먼저 적용되어야 하는것
        if (isStartBattle)
        {
            // if (cardSystem.enemy[0].GetComponent<Enemy>().index == 38)
            // {
            //     GameData.CardData cardData = GameManager.gameManager.SetCardIndex(53, "Public");
            //     Debug.Log("asd:" + cardData.cardNum);
            //     yield return cardSystem.StartCoroutine(cardSystem.CardCreateMotion(cardData));
            //     cardSystem.SuffleDeck(10);
            // }
        }


        //도사일 경우 소환수 능력 발동
        if (pCom.Job == EnumTypes.JobType.Dosa) { SummonFunction.Instance.PlayAllSummonAbility(); }
        //탈춤꾼일 경우 탈 바꾸기
        if (pCom.Job == EnumTypes.JobType.Performer) { MaskFunction.Instance.SwitchMaskNext(); }
        //유물
        ArtifactFunction.Instance.ArtifactStartTurn(pCom, null);
        //장착 카드
        EquipmentFunction.Instance.StartEquipAction(pCom, null);

        //적 모션 세팅
        for (int i = 0; i < EnemyManager.Instance.enemies.Count; i++)
        {
            EnemyManager.Instance.enemies[i].GetComponent<Enemy>().SetNextAction();
        }

        //버프 디버프
        int actionNum = START_ACTION_COUNT;
        if (pCom.CheckHaveBuffOrDebuff(EnumTypes.Status.debuff, 11)) { actionNum -= pCom.GetBuffOrDebuffValue(EnumTypes.Status.debuff, 11); }
        if (pCom.CheckHaveBuffOrDebuff(EnumTypes.Status.buff, 11)) { actionNum += pCom.GetBuffOrDebuffValue(EnumTypes.Status.buff, 11); }
        if (pCom.CheckHaveBuffOrDebuff(EnumTypes.Status.buff, 12)) { pCom.GetShield(pCom.GetBuffOrDebuffValue(EnumTypes.Status.buff, 12)); }

        //스탯 정리
        pCom.SetBuffStartTurn();
        //몬스터 시작 알림
        if (isStartBattle)
        {
            SetMonsterNotification();
            isStartBattle = false;
        }

        //나머지 상태 정리
        StartCoroutine(cardSystem.DrawCard(START_CARD_COUNT));
        pCom.GetAction(actionNum, true);
        //cardSystem.CardAlignment(1);

        yield return null;
    }

    public IEnumerator TurnCo(bool isMy)
    {
        //SFX
        if (isMy) AudioManager.Instance.MyTurnSound();
        else AudioManager.Instance.EnemyTurnSound();

        Debug.Log("TurnCo: " + isMy);

        startTurnObj.SetActive(isMy);
        endTurnObj.SetActive(!isMy);
        var targetObj = isMy ? startTurnObj : endTurnObj;
        ButtonAnim.Instance.ButtonScaleIn(targetObj, 0f, 1f);
        yield return new WaitForSecondsRealtime(1f);

        targetObj.SetActive(false);
        yield return null;
    }

    private void SetMonsterNotification()
    {
        string text = "";
        if (EnemyManager.Instance.enemies[0].GetComponent<Enemy>().enemyDTO.Id == 12)
        {
            text = new LocalizedString("LocalTable", "Warning-Tiger").GetLocalizedString();
        }
        if (EnemyManager.Instance.enemies[0].GetComponent<Enemy>().enemyDTO.Id == 13)
        {
            text = new LocalizedString("LocalTable", "Warning-Sun").GetLocalizedString();
        }
        if (EnemyManager.Instance.enemies[0].GetComponent<Enemy>().enemyDTO.Id == 25)
        {
            text = new LocalizedString("LocalTable", "Warning-Nolbu").GetLocalizedString();
        }
        if (EnemyManager.Instance.enemies[0].GetComponent<Enemy>().enemyDTO.Id == 26)
        {
            text = new LocalizedString("LocalTable", "Warning-Swallow").GetLocalizedString();
        }
        if (EnemyManager.Instance.enemies[0].GetComponent<Enemy>().enemyDTO.Id == 38)
        {
            text = new LocalizedString("LocalTable", "Warning-Rabbit").GetLocalizedString();
        }
        NotificationManager.Instance.SetCheckNotification(text);
    }

    //버튼 누르면 실행
    public void EndTurn()
    {
        //SFX
        AudioManager.Instance.ButtonClickSound3();

        if (isFinish) return;
        if (!BattleManager.Instance.player.isTurn) return;

        StartCoroutine(EndTurnCo());
    }
    public IEnumerator EndTurnCo()
    {

        // if (isTutorial)
        // {
        //     cardSystem.tuBattle.GetComponent<TutorialBattle>().ClickTurnButton();
        //     cardSystem.EnemyShieldBreak();
        //     StartCoroutine(TurnCo(false));
        //     SetTurn(false);
        //     //EnemyActionCo = StartCoroutine(cardSystem.EnemyAction());
        //     return;
        // }

        // 전투 종료 로그
        var text = LogManager.Instance.GetLocalizedText("end_turn");
        LogManager.Instance.AddLogBattle(text);

        for (int i = 0; i < EnemyManager.Instance.enemies.Count; i++)
        {
            EnemyManager.Instance.enemies[i]?.ShieldBreakBase();
        }
        StartCoroutine(TurnCo(false));
        SetTurn(false);
        var pCom = BattleManager.Instance.player.GetComponent<Player>();
        // var data = targetData;
        // if (isTutorial || !player.TryGetComponent<Player>(out var pCom))
        // {
        //     //enemy
        //     //EnemyActionCo = StartCoroutine(cardSystem.EnemyAction());
        //     return;
        // }

        //유물
        ArtifactFunction.Instance.ArtifactEndTurnShield(pCom, null);

        // check the player's play state

        yield return new WaitForSecondsRealtime(0.5f);

        if (PlayFunction.Instance.isPlay)
        {
            PlayFunction.Instance.UsePlay();
            yield return new WaitForSecondsRealtime(0.3f);
        }
        //상태 정리
        pCom.SetDebuffEndTurn();
        //enemy
        StartCoroutine(EnemysTurn());
    }

    IEnumerator EnemysTurn()
    {
        for (int i = 0; i < EnemyManager.Instance.enemies.Count; i++)
        {
            var enemy = EnemyManager.Instance.enemies[i];
            if (enemy.isDie) continue;

            enemy.GetComponent<Enemy>().ReSetAmount(true);
        }

        for (int i = 0; i < EnemyManager.Instance.enemies.Count; i++)
        {
            var enemy = EnemyManager.Instance.enemies[i];
            if (enemy.isDie) continue;

            yield return enemy.GetComponent<Enemy>().StartTurn();
            yield return StartCoroutine(EnemyManager.Instance.CheckEnemyDie());
        }
        yield return new WaitForSecondsRealtime(0.5f);
        // 내 턴 돌아옴
        StartCoroutine(StartTurn());
    }


    public void SetTurn(bool playerTurn)
    {
        BattleManager.Instance.player.isTurn = playerTurn;
    }
}
