using System.Collections;
using UnityEngine;

public class TurnManagerTu : SceneSingleton<TurnManagerTu>
{
    public bool isFinish = false;
    public bool isTutorial = false;
    private readonly int START_CARD_COUNT = 3;
    private readonly int START_ACTION_COUNT = 2;
    private bool isStartBattle = false; // 맨 처음 전투 시작시만 true
    [SerializeField] private CardSystemTu cardSystem;
    [SerializeField] private GameObject startTurnObj, endTurnObj;

    public IEnumerator StartTurn()
    {
        if (isFinish) yield break;
        var pCom = TutorialBattle.Instance.player;

        var text = LogManager.Instance?.GetLocalizedText("start_turn");
        LogManager.Instance?.AddLogBattle(text);

        pCom.ShieldBreakBase();
        var co = StartCoroutine(TurnCo(true));
        SetTurn(true);
        yield return co;

        //적 모션 세팅
        for (int i = 0; i < EnemyManagerTu.Instance.enemies.Count; i++)
        {
            EnemyManagerTu.Instance.enemies[i].GetComponent<EnemyTu>().SetNextAction();
        }

        //버프 디버프
        int actionNum = START_ACTION_COUNT;
        if (pCom.CheckHaveBuffOrDebuff(EnumTypes.Status.debuff, 11)) { actionNum -= pCom.GetBuffOrDebuffValue(EnumTypes.Status.debuff, 11); }
        if (pCom.CheckHaveBuffOrDebuff(EnumTypes.Status.buff, 11)) { actionNum += pCom.GetBuffOrDebuffValue(EnumTypes.Status.buff, 11); }
        if (pCom.CheckHaveBuffOrDebuff(EnumTypes.Status.buff, 12)) { pCom.GetShield(pCom.GetBuffOrDebuffValue(EnumTypes.Status.buff, 12)); }

        //몬스터 시작 알림
        if (isStartBattle)
        {
            isStartBattle = false;
        }

        //나머지 상태 정리
        StartCoroutine(cardSystem.DrawCard(START_CARD_COUNT));
        pCom.GetAction(actionNum, true);

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

    //버튼 누르면 실행
    public void EndTurn()
    {
        //SFX
        AudioManager.Instance.ButtonClickSound3();

        if (isFinish) return;
        if (!TutorialBattle.Instance.player.isTurn) return;

        StartCoroutine(EndTurnCo());
    }
    public IEnumerator EndTurnCo()
    {
        cardSystem.tuBattle.GetComponent<TutorialBattle>().ClickTurnButton();
        //cardSystem.EnemyShieldBreak();
        StartCoroutine(TurnCo(false));
        SetTurn(false);
        //EnemyActionCo = StartCoroutine(cardSystem.EnemyAction());
            //return;


        for (int i = 0; i < EnemyManagerTu.Instance.enemies.Count; i++)
        {
            EnemyManagerTu.Instance.enemies[i]?.ShieldBreakBase();
        }
        StartCoroutine(TurnCo(false));
        SetTurn(false);
        var pCom = TutorialBattle.Instance.player.GetComponent<Player>();
        // var data = targetData;
        // if (isTutorial || !player.TryGetComponent<Player>(out var pCom))
        // {
        //     //enemy
        //     //EnemyActionCo = StartCoroutine(cardSystem.EnemyAction());
        //     return;
        // }


        yield return new WaitForSecondsRealtime(0.5f);

        //상태 정리
        pCom.SetDebuffEndTurn();
        //enemy
        StartCoroutine(EnemysTurn());
    }

    IEnumerator EnemysTurn()
    {
        for (int i = 0; i < EnemyManagerTu.Instance.enemies.Count; i++)
        {
            var enemy = EnemyManagerTu.Instance.enemies[i];
            if (enemy.isDie) continue;

            enemy.GetComponent<EnemyTu>().ReSetAmount(true);
        }

        for (int i = 0; i < EnemyManagerTu.Instance.enemies.Count; i++)
        {
            var enemy = EnemyManagerTu.Instance.enemies[i];
            if (enemy.isDie) continue;

            yield return enemy.GetComponent<EnemyTu>().StartTurn();
            yield return StartCoroutine(EnemyManagerTu.Instance.CheckEnemyDie());
        }
        yield return new WaitForSecondsRealtime(0.5f);
        // 내 턴 돌아옴
        StartCoroutine(StartTurn());
    }


    public void SetTurn(bool playerTurn)
    {
        TutorialBattle.Instance.player.isTurn = playerTurn;
    }
}
