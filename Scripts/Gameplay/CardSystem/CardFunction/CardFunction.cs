using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using UnityEngine.Localization.SmartFormat;

public class CardFunction : SceneSingleton<CardFunction>
{
    public GameObject selectEnemyObj; // 카드로 선택되는 적 오브젝트
    public List<GameObject> allEnemy = new List<GameObject>();
    [SerializeField] private bool isTutorial;

    private ConditionRegistry conditionRegistry = new ();
    private ActionRegistry actionRegistry = new ();
    Action lateAction = null;
    void Start()
    {
        // 조건 등록
        conditionRegistry.Register("status_exist", new StatusCheckCondition());
        conditionRegistry.Register("is_summon", new SummonCountCondition());
        conditionRegistry.Register("is_change", new IsChangeCondition());
        conditionRegistry.Register("shield_amount", new ShieldAmountCondition());
        conditionRegistry.Register("is_play_instrument", new IsPlayInstrumentCondition());

        // 액션 등록
        actionRegistry.Register("percent", new ValueChangeAction());
        actionRegistry.Register("value", new ValueChangeAction());
        actionRegistry.Register("get_status", new GetStatusAction());

        selectEnemyObj = null;
    }


    /// <summary>
    /// 카드 능력 실행
    /// </summary>

    public void CardAbility(CardDTO cardInfo)
    {
        var player = isTutorial ? TutorialBattle.Instance?.player : BattleManager.Instance?.player;

        lateAction = null;
        for (int i = 0; i < cardInfo.CardActions.Count; i++)
        {
            var cardAction = cardInfo.CardActions[i];
            int actionIndex = i;

            // 확률적으로 동작하는 카드의 경우 확률 계산 후 실행
            if (cardAction.ExtraData != null && cardAction.ExtraData.ContainsKey("probability"))
            {
                int index = GetProbabilityActionIndex(cardInfo);
                cardAction = cardInfo.CardActions[index];
                AbilityFunction(cardInfo, cardAction, actionIndex);
                return;
            }

            AbilityFunction(cardInfo, cardAction, actionIndex);
            if (cardAction.ActionType == EnumTypes.Action.equip) { break; } // 장비 획득 액션이 있다면, 다음 액션은 실행하지 않음 -> 추후에 장비 실행 시 할 예정.
            if (cardAction.ExtraData != null && cardAction.ExtraData.ContainsKey("play_instrument")) { break; } // 연주 중인 액션이 있다면, 다음 액션은 실행하지 않음 -> 악기 연주 실행 타이밍에 할 예정.
        }
        // 마지막 행동이 있다면 실행 & null로 초기화
        lateAction?.Invoke();
        lateAction = null;

        // 유물 효과 적용 - 카드 사용 시 동작
        if (!isTutorial)
        {
            ArtifactFunction.Instance.ArtifactCardUse(BattleManager.Instance.player, null);
            ArtifactFunction.Instance.ArtifactCardUseFirst(player, cardInfo);
        }
    }

    // 랜덤 확률의 카드 액션이라면 뽑힌 인덱스 반환
    private int GetProbabilityActionIndex(CardDTO cardDTO)
    {
        float totalProbability = 0f;
        for (int i = 0; i < cardDTO.CardActions.Count; i++)
        {
            if (cardDTO.CardActions[i].ExtraData != null && cardDTO.CardActions[i].ExtraData.ContainsKey("probability"))
            {
                totalProbability += float.Parse(cardDTO.CardActions[i].ExtraData["probability"].ToString());
            }
        }

        if (totalProbability <= 0) return 0; // 확률값 없으면 종료

        float rand = UnityEngine.Random.Range(0f, totalProbability);
        float cumulative = 0f;
        for (int i = 0; i < cardDTO.CardActions.Count; i++)
        {
            if (cardDTO.CardActions[i].ExtraData != null && cardDTO.CardActions[i].ExtraData.ContainsKey("probability"))
            {

                float prob = float.Parse(cardDTO.CardActions[i].ExtraData["probability"].ToString());
                cumulative += prob;

                if (rand <= cumulative)
                {
                    // 이 부분에서 i번째 액션 실행 처리
                    return i;
                }
            }
        }

        return 0; // 기본적으로 첫 번째 액션을 반환

    }

    public void AbilityFunction(CardDTO cardInfo, CardActionDTO cardAction, int actionIndex)
    {
        var player = isTutorial ? TutorialBattle.Instance.player : BattleManager.Instance.player;
        switch (cardAction.ActionType)
        {
            // 공격 & 방어 & 회복
            case EnumTypes.Action.attack:
                int damage = DamageCal.GetUnLimitcard(cardAction, cardInfo.CardUpgrade);
                int repeatCount = 1;
                if (cardInfo.CardActions.Count > actionIndex + 1 && cardInfo.CardActions[actionIndex + 1].ActionType == EnumTypes.Action.repeat)
                {
                    // 만약에 반복에 조건이 있으면, 그걸 만족해야 동작하게 함.
                    if (cardInfo.CardActions[actionIndex + 1].ExtraData != null && cardInfo.CardActions[actionIndex + 1].ExtraData.ContainsKey("condition"))
                    {
                        var conditionObj = cardInfo.CardActions[actionIndex + 1].ExtraData["condition"] as JObject;
                        var conditionKey = conditionObj.Properties().First().Name;
                        Debug.Log($"조건 확인: {conditionKey}, 액션: {cardAction.ActionType}");
                        if (conditionRegistry.Check(conditionKey, cardInfo.CardActions[actionIndex + 1], player, selectEnemyObj))
                        {
                            repeatCount = cardInfo.CardActions[actionIndex + 1].Value[cardInfo.CardUpgrade];
                        }
                        else
                        {
                            repeatCount = 1; // 조건이 만족하지 않으면 반복 횟수는 1로 설정
                        }
                    }
                    else
                    {
                        repeatCount = cardInfo.CardActions[actionIndex + 1].Value[cardInfo.CardUpgrade];
                    }
                }

                if (cardAction.Target == EnumTypes.Target.enemy)
                {
                    Attack(cardAction, damage, cardAction.Effect, selectEnemyObj, repeatCount);
                    Debug.Log(cardAction.ExtraData);
                }
                else if (cardAction.Target == EnumTypes.Target.enemys)
                {
                    Attack(cardAction, damage, cardAction.Effect, null, repeatCount);
                }
                else if (cardAction.Target == EnumTypes.Target.self)
                {
                    // 플레이어가 공격을 받는 경우
                    Debug.Log($"플레이어가 받는 공격: {damage}");
                    player.GetDamagePlayer(damage, cardAction.ExtraData);
                }
                break;
            case EnumTypes.Action.shield:
                Debug.Log($"방어도 획득: {cardAction.Value[cardInfo.CardUpgrade]}");
                int shieldAmount = DamageCal.GetUnLimitcard(cardAction, cardInfo.CardUpgrade);
                if (cardAction.Target == EnumTypes.Target.self)
                {
                    GetShield(cardAction: cardAction, amount: shieldAmount, selectEnemyObj);
                }
                break;
            case EnumTypes.Action.heal:
                int healAmount = DamageCal.GetUnLimitcard(cardAction, cardInfo.CardUpgrade);
                if (cardAction.Target == EnumTypes.Target.self)
                {
                    GetHeal(cardAction, healAmount, selectEnemyObj?.GetComponent<Enemy>());
                }
                else
                {
                    // 적 회복시키기
                }
                break;
            // 버프 - 디버프
            case EnumTypes.Action.buff:
            case EnumTypes.Action.debuff:
                if (cardAction.ExtraData != null)
                {
                    int amount = cardAction.Value[cardInfo.CardUpgrade];
                    var type = cardAction.ActionType == EnumTypes.Action.buff ? EnumTypes.Status.buff : EnumTypes.Status.debuff;
                    GetStatus(type, cardAction, amount, selectEnemyObj?.GetComponent<Enemy>());
                }
                break;
            // 카드 드로우
            case EnumTypes.Action.draw:
                int drawAmount = cardAction.Value[cardInfo.CardUpgrade];
                if (cardAction.Target == EnumTypes.Target.self)
                {
                    DrawCard(cardAction: cardAction, drawAmount, selectEnemyObj.GetComponent<Enemy>());
                }
                break;
            case EnumTypes.Action.get_action_point:
                int getActionAmount = cardAction.Value[cardInfo.CardUpgrade];
                GetActionPoint(cardAction, getActionAmount, selectEnemyObj?.GetComponent<Enemy>());
                break;
            // 장비 획득
            case EnumTypes.Action.equip:
                GetEquip(cardInfo.Id, cardInfo.CardUpgrade);
                break;
            // 기타 행동들
            case EnumTypes.Action.action:
                int actionAmount = cardAction.Value[cardInfo.CardUpgrade];
                ActionStart(cardAction, actionAmount, cardInfo);
                break;
            // 직업 별 특수 능력
            // 카드 강화
            case EnumTypes.Action.upgrade:
                CardUpgrade(cardAction, cardAction.Value[cardInfo.CardUpgrade]);
                break;
        }
        // 상태 아이콘 갱신
        if (player != null)
        {
            player.SetStatusIcon();
            player.SetStatText();
        }
    }

    public void Attack(CardActionDTO cardAction, int damage, EnumTypes.EffectType effect, GameObject enemy , int repeatTime = 1 )
    {
        var player = isTutorial ? TutorialBattle.Instance?.player : BattleManager.Instance?.player;
        if (isTutorial) { enemy.GetComponent<EnemyTu>().GetDamage(player, damage, effect, true, cardAction.ExtraData); return; }

        // 유물 발동
        var damageResult = ArtifactFunction.Instance.ArtifactAttackEnemyFirst(player, enemy?.GetComponent<Enemy>());
        if (damageResult != null && damageResult.IsMultifulDamage)
        {
            int amount = damageResult.MultiAmount;
            damage *= amount; // 배율 적용
        }

        // 발동 조건이 존재하는 경우
        if (cardAction.ExtraData != null && cardAction.ExtraData.ContainsKey("condition"))
        {
            var conditionObj = cardAction.ExtraData["condition"] as JObject;
            var conditionKey = conditionObj.Properties().First().Name;

            if (conditionRegistry.Check(conditionKey, cardAction, player, enemy))
            {
                if (cardAction.ExtraData.ContainsKey("action"))
                {
                    var actionObj = cardAction.ExtraData["action"] as JObject;
                    var actionKey = actionObj.Properties().First().Name;
                    actionRegistry.Execute(actionKey, cardAction, ref damage, enemy.GetComponent<Enemy>());
                }
            }
            else
            {
                if (!cardAction.ExtraData.ContainsKey("action"))
                {
                    return;
                    // 조건이 불만족 + 액션이 없는 경우는 아무것도 하지 않음.
                    // 조건이 불만족 + 액션이 있는 경우는 액션만 실행하지 않고 기본 공격은 수행하게 됨.
                }
            }
        }
        // 발동 조건은 없고 그냥 실행하는 경우
        else
        {
            // 공격력 증가 - percent, value
            if (cardAction.ExtraData != null && cardAction.ExtraData.ContainsKey("percent"))
            {
                actionRegistry.Execute("percent", cardAction, ref damage, enemy?.GetComponent<Enemy>());
            }
            if (cardAction.ExtraData != null && cardAction.ExtraData.ContainsKey("value"))
            {
                actionRegistry.Execute("value", cardAction, ref damage, enemy?.GetComponent<Enemy>());
            }
            // 적 행동 수정
            if (cardAction.ExtraData != null && cardAction.ExtraData.ContainsKey("change_enemy_action"))
            {
                enemy.GetComponent<Enemy>().SetNextAction(false, UnityEngine.Random.Range(1, 390));
            }
        }
        StartCoroutine(AttackCoroutine(damage, effect, enemy, repeatTime, extraData: cardAction.ExtraData));
    }

    IEnumerator AttackCoroutine(int damage, EnumTypes.EffectType effect, GameObject enemy, int repeatTime, Dictionary<string, object> extraData = null)
    {
        var player = isTutorial ? TutorialBattle.Instance?.player : BattleManager.Instance?.player;
        for (int i = 0; i < repeatTime; i++)
        {
            //환술 디버프
            if (player != null && player.CheckHaveBuffOrDebuff(EnumTypes.Status.debuff, 15))
            {
                int ran = UnityEngine.Random.Range(0, 10);
                if (ran < 3)
                {
                    player.GetDamagePlayer(damage, extraData: extraData);
                    //LOG
                    var text = LogManager.Instance.GetLocalizedText("character_self_attack").FormatSmart(player.characterName, damage);
                    LogManager.Instance.AddLogBattle(text);
                    continue;
                }
            }
            // 단일 공격
            if (enemy != null)
            {
                int ran = 10;

                //시야차단이 있어야만 발동
                if (player.CheckHaveBuffOrDebuff(EnumTypes.Status.debuff, 12))
                {
                    ran = UnityEngine.Random.Range(0, 10);
                }

                if (ran > 2)
                {
                    // 로그 출력
                    var indexTxt = EnemyManager.Instance.enemies.Count > 1 ? (enemy.GetComponent<Enemy>().enemyIndex + 1).ToString() : "";
                    var damageText = LogManager.Instance.GetLocalizedText("character_attack").FormatSmart(player.characterName, enemy.GetComponent<Enemy>().enemyDTO.Name + indexTxt, damage);
                    LogManager.Instance.AddLogBattle(damageText);
                    // 적에게 피해 주기
                    EnemyGetDamage(enemy.GetComponent<Enemy>(), damage, effect, extraData);
                }
                else
                {
                    var missText = LogManager.Instance.GetLocalizedText("miss");
                    enemy.GetComponent<CharacterBase>().SetMoveTextBase(missText, EnumTypes.TextMotionType.up, EnumTypes.MoveTextType.none);
                    var playerMissText = LogManager.Instance.GetLocalizedText("character_attack_miss").FormatSmart(player.characterName);
                    LogManager.Instance.AddLogBattle(playerMissText);
                }

            }
            //다중 공격
            else if (enemy == null)
            {
                var enemiesCopy = new List<Enemy>(EnemyManager.Instance.enemies);
                for (int multi = 0; multi < enemiesCopy.Count; multi++)
                {
                    var enemyCopy = enemiesCopy[multi];
                    int ran = 10;
                    if (player.CheckHaveBuffOrDebuff(EnumTypes.Status.debuff, 12))
                    {
                        ran = UnityEngine.Random.Range(0, 10);
                    }

                    if (ran > 2)
                    {
                        // 로그 출력
                        var indexTxt = EnemyManager.Instance.enemies.Count > 1 ? (enemyCopy.enemyIndex + 1).ToString() : "";
                        var damageText = LogManager.Instance.GetLocalizedText("character_attack").FormatSmart(player.characterName, enemyCopy.enemyDTO.Name + indexTxt, damage);
                        LogManager.Instance.AddLogBattle(damageText);

                        EnemyGetDamage(enemyCopy, damage, effect, extraData);
                    }
                    else
                    {
                        var missText = LogManager.Instance.GetLocalizedText("miss");
                        enemyCopy.GetComponent<CharacterBase>().SetMoveTextBase(missText, EnumTypes.TextMotionType.up, EnumTypes.MoveTextType.none);
                        var playerMissText = LogManager.Instance.GetLocalizedText("character_attack_miss").FormatSmart(player.characterName);
                        LogManager.Instance.AddLogBattle(playerMissText);
                    }
                }
            }
            yield return new WaitForSecondsRealtime(0.5f);
        }

    }
    void CheckPlayerStatus(EnumTypes.Status statusType, int statusIndex)
    {
        var player = isTutorial ? TutorialBattle.Instance?.player : BattleManager.Instance?.player;
        if (player.CheckHaveBuffOrDebuff(statusType, statusIndex))
        {
            player.GetDamagePlayer(player.GetBuffOrDebuffValue(statusType, statusIndex), null);
        }
    }

    void CheckEnemyStatus(Enemy enemy, EnumTypes.Status statusType, int statusIndex)
    {
        if (enemy == null) return;
        var player = isTutorial ? TutorialBattle.Instance?.player : BattleManager.Instance?.player;

        if (enemy.CheckHaveBuffOrDebuff(statusType, statusIndex))
        {
            player.GetDamagePlayer(enemy.GetBuffOrDebuffValue(statusType, statusIndex), null);
        }
    }

    private void EnemyGetDamage(Enemy enemy, int damage, EnumTypes.EffectType effectType, Dictionary<string, object> extraData)
    {
        var player = isTutorial ? TutorialBattle.Instance?.player : BattleManager.Instance?.player;
        enemy.GetDamage(player, damage, effectType, true, extraData);
        if (player.Job == EnumTypes.JobType.Performer) { MaskFunction.Instance.SetAttackMaskAbility(enemy.GetComponent<Enemy>(), damage); }

        // 플레이어의 디버프 - 혼란 확인
        CheckPlayerStatus(EnumTypes.Status.debuff, 9);
        // 몬스터 버프 - 가시 확인
        CheckEnemyStatus(enemy, EnumTypes.Status.buff, 6);
    }


    public void GetShield(CardActionDTO cardAction, int amount, GameObject enemy)
    {
        var player = isTutorial ? TutorialBattle.Instance?.player : BattleManager.Instance?.player;
        int count = amount; // 곱하기에 적용될 기본 값 - (percent가 아니면 적용 X)

        // 발동 조건이 존재하는 경우
        if (cardAction.ExtraData != null && cardAction.ExtraData.ContainsKey("condition"))
        {
            var conditionObj = cardAction.ExtraData["condition"] as JObject;
            var conditionKey = conditionObj.Properties().First().Name;
            if (cardAction.ExtraData.ContainsKey("action"))
            {
                var actionObj = cardAction.ExtraData["action"] as JObject;
                var actionKey = actionObj.Properties().First().Name;

                Debug.Log($"조건 확인: {conditionKey}, 액션: {actionKey}");
                if (conditionRegistry.Check(conditionKey, cardAction, player, enemy))
                {
                    Debug.Log($"조건 만족: {conditionKey}");
                    actionRegistry.Execute(actionKey, cardAction, ref amount, enemy.GetComponent<Enemy>());
                }
            }
            else
            {
                if (!conditionRegistry.Check(conditionKey, cardAction, player, enemy))
                {
                    return;
                    // 조건이 불만족 + 액션이 없는 경우는 아무것도 하지 않음.
                    // 조건이 불만족 + 액션이 있는 경우는 액션만 실행하지 않고 기본 공격은 수행하게 됨.
                }
            }

        }
        // 발동 조건은 없고 그냥 실행하는 경우
        else
        {
            // 쉴드 증가
            if (cardAction.ExtraData != null && cardAction.ExtraData.ContainsKey("percent"))
            {
                Debug.Log($"GetShield: {amount}, Percent: {cardAction.ExtraData["percent"]}");
                actionRegistry.Execute("percent", cardAction, ref amount, enemy.GetComponent<Enemy>());
            }
            if (cardAction.ExtraData != null && cardAction.ExtraData.ContainsKey("value"))
            {
                Debug.Log($"GetShield: {amount}, Value: {cardAction.ExtraData["value"]}");
                actionRegistry.Execute("value", cardAction, ref amount, enemy.GetComponent<Enemy>());
            }
        }

        Debug.Log($"GetShield: {amount}");

        if (cardAction.ExtraData != null && cardAction.ExtraData.ContainsKey("late_action"))
        {
            lateAction = () => player.GetShield(amount);
        }
        else
        {
            player.GetShield(amount);
        }

    }

    //------------------------------------------버프 & 디버프------------------------------------------------
    public void GetStatus(EnumTypes.Status type, CardActionDTO cardAction, int amount, CharacterBase enemy)
    {
        var player = isTutorial ? TutorialBattle.Instance?.player : BattleManager.Instance?.player;
        if (cardAction.ExtraData == null) return;

        string statusType = type.ToString();
        Debug.Log($"GetStatus: {statusType}, Amount: {amount}");
        bool conditionCheck = true;
        //int statusIndex;

        if (cardAction.ExtraData.ContainsKey("condition"))
        {
            var conditionObj = cardAction.ExtraData["condition"] as JObject;
            var conditionKey = conditionObj.Properties().First().Name;
            conditionCheck = conditionRegistry.Check(conditionKey, cardAction, player, enemy.gameObject);
            Debug.Log($"조건 확인: {conditionKey}, 결과: {conditionCheck}");
        }

        if (!conditionCheck) return;


        if (cardAction.ExtraData.ContainsKey("action"))
        {
            var actionObj = cardAction.ExtraData["action"] as JObject;
            var actionKey = actionObj.Properties().First().Name;
            Debug.Log($"액션 실행: {actionKey}");
            switch (cardAction.Target)
            {
                case EnumTypes.Target.self:
                    actionRegistry.Execute(actionKey, cardAction, ref amount, player);
                    break;
                case EnumTypes.Target.enemy:
                    actionRegistry.Execute(actionKey, cardAction, ref amount, enemy);
                    break;
                case EnumTypes.Target.enemys:
                    for (int i = 0; i < EnemyManager.Instance.enemies.Count; i++)
                    {
                        actionRegistry.Execute(actionKey, cardAction, ref amount, EnemyManager.Instance.enemies[i]);
                    }
                    break;
            }

            //statusIndex = int.Parse(actionObj[statusType].ToString());
        }
        else if (cardAction.ExtraData.ContainsKey("get_status"))
        {

            foreach (var key in cardAction.ExtraData.Keys)
            {
                if (key != "get_status")
                {
                    actionRegistry.Execute(key, cardAction, ref amount, enemy.GetComponent<Enemy>());
                }
            }

            switch (cardAction.Target)
            {
                case EnumTypes.Target.self:
                    actionRegistry.Execute("get_status", cardAction, ref amount, player);
                    break;
                case EnumTypes.Target.enemy:
                    actionRegistry.Execute("get_status", cardAction, ref amount, enemy);
                    break;
                case EnumTypes.Target.enemys:
                    for (int i = 0; i < EnemyManager.Instance.enemies.Count; i++)
                    {
                        actionRegistry.Execute("get_status", cardAction, ref amount, EnemyManager.Instance.enemies[i]);
                    }
                    break;
            }
        }
        else if (cardAction.ExtraData.ContainsKey("delete_status_id"))
        {
            Debug.Log($"fuck");
            int statusId = int.Parse(cardAction.ExtraData["delete_status_id"].ToString());
            switch (cardAction.Target)
            {
                case EnumTypes.Target.self:
                    player.DeleteStatusBase(statusId, type, amount);
                    break;
                case EnumTypes.Target.enemy:
                    enemy.DeleteStatusBase(statusId, type, amount);
                    break;
                case EnumTypes.Target.enemys:
                    for (int i = 0; i < EnemyManager.Instance.enemies.Count; i++)
                    {
                        EnemyManager.Instance.enemies[i].DeleteStatusBase(statusId, type, amount);
                    }
                    break;
            }
        }


        if (cardAction.Target != EnumTypes.Target.self) { SetAllEnemyAmount(); }

    }
    public void GetHeal(CardActionDTO cardAction, int amount, CharacterBase enemy)
    {
        var player = isTutorial ? TutorialBattle.Instance?.player : BattleManager.Instance?.player;
        bool conditionCheck = true;
        if (cardAction.ExtraData != null && cardAction.ExtraData.ContainsKey("condition"))
        {
            var conditionObj = cardAction.ExtraData["condition"] as JObject;
            var conditionKey = conditionObj.Properties().First().Name;
            conditionCheck = conditionRegistry.Check(conditionKey, cardAction, player, enemy.gameObject);
            Debug.Log($"조건 확인: {conditionKey}, 결과: {conditionCheck}");
        }

        if (!conditionCheck) return;

        if (cardAction.ExtraData != null && cardAction.ExtraData.ContainsKey("action"))
        {
            var actionObj = cardAction.ExtraData["action"] as JObject;
            var actionKey = actionObj.Properties().First().Name;
            actionRegistry.Execute(actionKey, cardAction, ref amount, enemy.GetComponent<Enemy>());
        }

        player.GetComponent<Player>().GetHeal(amount);
    }
    public void GetMoney(int amount)
    {
        //SFX

        //Text
        //if(amount>=0)player.GetComponent<Player>().SetMoveTextBase(amount,"CoinGet");
        //else player.GetComponent<Player>().ShowDamageText(amount, "CoinUse")
        //player.GetComponent<Player>().gameMoney.text = GameManager.gameManager.gameData.gMoney.ToString();
    }

    public void DrawCard(CardActionDTO cardAction, int amount, CharacterBase enemy)
    {
        var player = isTutorial ? TutorialBattle.Instance?.player : BattleManager.Instance?.player;
        int upgrade = 0; // 카드 업그레이드 횟수
        bool conditionCheck = true;

        if (cardAction != null)
        {
            if (cardAction.ExtraData != null && cardAction.ExtraData.ContainsKey("condition"))
            {
                var conditionObj = cardAction.ExtraData["condition"] as JObject;
                var conditionKey = conditionObj.Properties().First().Name;
                conditionCheck = conditionRegistry.Check(conditionKey, cardAction, player, enemy.gameObject);
                Debug.Log($"조건 확인: {conditionKey}, 결과: {conditionCheck}");
            }
            if (!conditionCheck) return;

            // 조건이 있다면 참이거나, 조건이 없다면 그냥 실행

            if (cardAction.ExtraData != null && cardAction.ExtraData.ContainsKey("action"))
            {
                var actionObj = cardAction.ExtraData["action"] as JObject;
                var actionKey = actionObj.Properties().First().Name;
                actionRegistry.Execute(actionKey, cardAction, ref amount, enemy.GetComponent<Enemy>());
            }
            else if (cardAction.ExtraData != null && cardAction.ExtraData.ContainsKey("upgrade"))
            {
                upgrade = 1;
            }
            else
            {
                if (cardAction.ExtraData != null)
                {
                    var actionKey = cardAction.ExtraData.First().Key;
                    actionRegistry.Execute(actionKey, cardAction, ref amount, enemy.GetComponent<Enemy>());
                }
            }
        }

        StartCoroutine(CardSystem.Instance.DrawCard(amount, upgrade: upgrade));
    }
    private void GetActionPoint(CardActionDTO cardAction, int amount, CharacterBase enemy)
    {
        var player = isTutorial ? TutorialBattle.Instance?.player : BattleManager.Instance?.player;
        bool conditionCheck = true;

        if (cardAction.ExtraData != null && cardAction.ExtraData.ContainsKey("condition"))
        {
            var conditionObj = cardAction.ExtraData["condition"] as JObject;
            var conditionKey = conditionObj.Properties().First().Name;
            conditionCheck = conditionRegistry.Check(conditionKey, cardAction, player, enemy.gameObject);
            Debug.Log($"조건 확인: {conditionKey}, 결과: {conditionCheck}");
        }
        if (!conditionCheck) return;

        if (cardAction.ExtraData != null && cardAction.ExtraData.ContainsKey("action"))
        {
            var actionObj = cardAction.ExtraData["action"] as JObject;
            var actionKey = actionObj.Properties().First().Name;
            actionRegistry.Execute(actionKey, cardAction, ref amount, enemy.GetComponent<Enemy>());
        }

        player.GetComponent<Player>().GetAction(amount);
    }
    private void GetEquip(int equipIndex, int upgradeTime)
    {
        var player = isTutorial ? TutorialBattle.Instance?.player : BattleManager.Instance?.player;
        player.GetComponent<Player>().GetEquipBase(equipIndex, upgradeTime);
    }

    public void CardUpgrade(CardActionDTO cardAction, int amount)
    {

        // amount의 경우 강화 횟수에 제한이 있을 경우 사용함.
        if (cardAction.ExtraData == null) { Debug.LogError("CardActionDTO ExtraData is null"); return; }

        for (int upgradeTime = 0; upgradeTime < amount; upgradeTime++)
        {
            List<Card> canUpgradeCardList = new();

            if (cardAction.ExtraData.ContainsKey("upgrade_all"))
            {
                string upgradeType = cardAction.ExtraData["upgrade_all"].ToString();
                // 타입에 맞는 카드 강화
                for (int i = 0; i < CardSystem.Instance.handCards.Count; i++)
                {
                    if (CardCheckUtils.Instance.checkCardCanUpgradeDTO(CardSystem.Instance.cards[i].GetComponent<Card>().cardData, true)
                    && Enum.TryParse<EnumTypes.CardType>(upgradeType, out var cardType) && cardType == CardSystem.Instance.handCards[i].CardType)
                    {
                        canUpgradeCardList.Add(CardSystem.Instance.cards[i].GetComponent<Card>());
                    }
                }
            }
            else if (cardAction.ExtraData.ContainsKey("upgrade_random"))
            {
                if (cardAction.ExtraData["upgrade_random"].ToString() == "all")
                {
                    for (int i = 0; i < CardSystem.Instance.handCards.Count; i++)
                    {
                        if (CardCheckUtils.Instance.checkCardCanUpgradeDTO(CardSystem.Instance.cards[i].GetComponent<Card>().cardData, true))
                        {
                            canUpgradeCardList.Add(CardSystem.Instance.cards[i].GetComponent<Card>());
                        }
                    }
                }
            }
            if (canUpgradeCardList.Count == 0)
            {
                string noUpgradeCard = LogManager.Instance.GetLocalText("no_can_upgrade_card");
                NotificationManager.Instance.SetShownNotification(noUpgradeCard);
                Debug.Log(noUpgradeCard);
                return;
            }
            // 전체 카드 업그레이드
            if (cardAction.ExtraData.ContainsKey("upgrade_all"))
            {
                for (int i = 0; i < canUpgradeCardList.Count; i++)
                {
                    StartCoroutine(canUpgradeCardList[i].UpgradeCard());
                }
            }
            // 랜덤으로 카드 선택
            else if (cardAction.ExtraData.ContainsKey("upgrade_random"))
            {
                int randomIndex = UnityEngine.Random.Range(0, canUpgradeCardList.Count);
                var selectedCard = canUpgradeCardList[randomIndex];
                StartCoroutine(selectedCard.UpgradeCard());
            }
        }
    }

    private void ActionStart(CardActionDTO action, int amount, CardDTO cardDTO = null)
    {
        var player = isTutorial ? TutorialBattle.Instance?.player : BattleManager.Instance?.player;
        if (action.ExtraData == null)
        {
            Debug.LogError("CardActionDTO ExtraData is null");
            return;
        }
        // 행동력 회복
        if (action.ExtraData.ContainsKey("get_action"))
        {
            int actionAmount = amount;
            player.GetComponent<Player>().GetAction(actionAmount);
        }

        // 핸드 전부 사용한 덱으로 이동
        if (action.ExtraData.ContainsKey("all_in_use"))
        {
            StartCoroutine(AllInUse());
        }
        // 도사 전용 - 소환술
        if (action.ExtraData.ContainsKey("summon"))
        {
            int index = int.Parse(action.ExtraData["summon"].ToString());

            if (index == 1)
            {
                SummonFunction.Instance.Summon(num: 1, state: 1, time: amount);
            }
            else
            {
                SummonFunction.Instance.Summon(num: index, state: amount);
            }
        }
        // 도사 전용 - 변신
        if (action.ExtraData.ContainsKey("change_mode"))
        {
            string modeType = action.ExtraData["change_mode"].ToString();
            var modeEnumType = Enum.Parse<EnumTypes.DosaModeType>(modeType);
            player.GetComponent<Player>().changeMode.ChangePlayerMode(modeEnumType, amount);
        }
        // 도사 전용 - 소환수 능력 사용
        if (action.ExtraData.ContainsKey("action_summon_skill"))
        {
            SummonFunction.Instance.PlayAllSummonAbility();
        }

        // 탈춤꾼 전용
        if (action.ExtraData.ContainsKey("play_instrument"))
        {
            string instrumentType = action.ExtraData["play_instrument"].ToString();
            PlayFunction.Instance.Play(Enum.Parse<EnumTypes.PerformerPlayType>(instrumentType), cardDTO);
        }
        if (action.ExtraData.ContainsKey("set_mask"))
        {
            Debug.Log("Set Mask");
            MaskFunction.Instance.ShowMaskDetail();
        }
        if (action.ExtraData.ContainsKey("stop_play_instrument"))
        {
            PlayFunction.Instance.PlayNone();
        }

    }

    IEnumerator AllInUse()
    {
        int count = CardSystem.Instance.cards.Count;
        for (int i = 0; i < count; i++)
        {
            CardSystem.Instance.canDrag = false;
            CardSystem.Instance.canActive = false;
            yield return StartCoroutine(CardSystem.Instance.cards[0].GetComponent<Card>().DeleteObject());
        }
    }
    public void SetAllEnemyAmount()
    {
        for(int i=0;i<allEnemy.Count;i++)
        {
            if(!allEnemy[i])return;
            allEnemy[i].GetComponent<Enemy>().ReSetAmount();
        }
    }
}