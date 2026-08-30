using System;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class EquipmentFunction : Singleton<EquipmentFunction>
{

    /// <summary>
    /// 장착카드 효과 (뽑는 카드 업그레이드 확률)
    /// </summary>
    public int UpgradeCardWhenDraw(CharacterBase player)
    {
        int upgradeTime = 0;
        for (int k = 0; k < player.EquipList.Count; k++)
        {
            var equip = player.EquipList[k].cardDTO;
            for (int c = 1; c < equip.CardActions.Count; c++)
            {
                var cardAction = equip.CardActions[c];
                if (cardAction.ActionType == EnumTypes.Action.upgrade)
                {
                    if (cardAction.ExtraData != null && cardAction.ExtraData.ContainsKey("percent"))
                    {
                        float percent = float.Parse(cardAction.ExtraData["percent"].ToString());
                        if (UnityEngine.Random.value < percent)
                        {
                            upgradeTime = 1;
                            break;
                        }
                    }
                }
            }
        }
        return upgradeTime;
    }

    /// <summary>
    /// 턴 시작 시 적 전체에게 상태 부여
    /// </summary>
    public void StartEquipAction(CharacterBase player, Enemy enemy)
    {
        for (int k = 0; k < player.EquipList.Count; k++)
        {
            var equip = player.EquipList[k].cardDTO;
            var extra = equip.CardActions[0].ExtraData;
            if (extra == null || !extra.ContainsKey("action_time") || extra["action_time"].ToString() != "start_turn") continue;

            for (int c = 1; c < equip.CardActions.Count; c++)
            {
                var cardAction = equip.CardActions[c];

                if (cardAction.ActionType == EnumTypes.Action.buff || cardAction.ActionType == EnumTypes.Action.debuff)
                {
                    if (cardAction.ExtraData != null && cardAction.ExtraData.ContainsKey("get_status"))
                    {
                        int statusId = int.Parse(cardAction.ExtraData["get_status"].ToString());
                        EnumTypes.Status statusType = cardAction.ActionType == EnumTypes.Action.buff ? EnumTypes.Status.buff : EnumTypes.Status.debuff;
                        int value = cardAction.Value[equip.CardUpgrade];
                        switch (cardAction.Target)
                        {
                            case EnumTypes.Target.enemy:
                                if (enemy != null)
                                {
                                    enemy.GetStatusEnemy(statusId, statusType, value);
                                }
                                else
                                {
                                    Debug.LogWarning("Enemy is null when applying equipment action.");
                                }
                                break;
                            case EnumTypes.Target.enemys:
                                Debug.Log("Applying status to all enemies: " + statusId);
                                foreach (var e in EnemyManager.Instance.enemies)
                                {
                                    if (e != null) e.GetStatusEnemy(statusId, statusType, value);
                                }
                                break;
                            case EnumTypes.Target.self:
                                if (player != null)
                                {
                                    player.GetStatusBase(statusId, statusType, value);
                                }
                                break;
                        }
                    }
                }

            }
        }
    }


    /// <summary>
    /// 장착 카드 능력 실행 - 적에게 상태 부여 시 동작
    /// </summary>
    public void SetStatusEquipmentAction(int statusId, EnumTypes.Status type, Enemy enemy)
    {
        if (BattleManager.Instance.player == null)
        {
            Debug.LogWarning("Player is null when applying equipment action.");
            return;
        }
        var player = BattleManager.Instance.player;
        for (int k = 0; k < player.EquipList.Count; k++)
        {
            var equip = player.EquipList[k].cardDTO;
            for (int c = 1; c < equip.CardActions.Count; c++)
            {
                var cardAction = equip.CardActions[c];
                if (cardAction.ExtraData != null && cardAction.ExtraData.ContainsKey("condition"))
                {
                    var condition = cardAction.ExtraData["condition"] as JObject;
                    if (condition != null && condition.ContainsKey("when_give_status"))
                    {
                        var status = condition["when_give_status"] as JObject;

                        EnumTypes.Status statusType = status.ContainsKey("type") ? Enum.Parse<EnumTypes.Status>(status["type"].ToString()) : EnumTypes.Status.debuff;
                        int statusIndex = status.ContainsKey("status_id") ? int.Parse(status["status_id"].ToString()) : 0;

                        // 상태 타입과 인덱스가 일치하는지 확인 -> 상태 부여가 올바르게 설정 되었다는 뜻.
                        if (statusType == type && statusIndex == statusId)
                        {
                            if (cardAction.ActionType == EnumTypes.Action.shield)
                            {
                                int shieldValue = cardAction.Value[equip.CardUpgrade];
                                if (player != null)
                                {
                                    player.GetShieldBase(shieldValue);
                                }
                            }

                            if (cardAction.ActionType == EnumTypes.Action.attack)
                            {
                                int damageValue = cardAction.Value[equip.CardUpgrade];
                                if (enemy != null)
                                {
                                    enemy.GetDamage(null, damageValue, EnumTypes.EffectType.hit, false, null);
                                }
                            }
                        }
                    }
                }
            }
        }
    }


    /// <summary>
    /// 장착 카드 능력 실행 - 소환수 N번 소환 시 동작
    /// </summary>
    public void SetSummonEqipmentAction(Player player)
    {
        for (int k = 0; k < player.EquipList.Count; k++)
        {
            var equip = player.EquipList[k].cardDTO;
            if (equip.CardActions[k].ExtraData == null || !equip.CardActions[k].ExtraData.ContainsKey("condition"))
            {
                continue;
            }
            else
            {
                var condition = equip.CardActions[k].ExtraData["condition"] as JObject;
                if (condition == null || !condition.ContainsKey("when_summon"))
                {
                    continue;
                }
            }

            // 소환 조건이 충족되는 지 확인
            for (int c = 1; c < equip.CardActions.Count; c++)
            {
                var cardAction = equip.CardActions[c];
                if (cardAction.ExtraData != null && cardAction.ExtraData.ContainsKey("equip_criteria"))
                {
                    player.EquipList[k].equipAmount++;
                    int criteria = cardAction.Value[equip.CardUpgrade];
                    Debug.Log($"Checking equip criteria: {criteria} for action {cardAction.ActionType}");
                    if (player.EquipList[k].equipAmount >= criteria)
                    {
                        player.EquipList[k].equipAmount = 0; // 조건 충족 시 초기화
                        c++;
                        if (equip.CardActions[c].ActionType == EnumTypes.Action.get_action_point)
                        {
                            int actionPoint = equip.CardActions[c].Value[equip.CardUpgrade];
                            if (player != null)
                            {
                                player.GetAction(actionPoint);
                            }
                            Debug.Log($"Gained {actionPoint} action points from equipment action.");
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 장착 카드 능력 실행 - 탈 장착 시 동작
    /// </summary>
    public void SetMaskEquipmentAction(CharacterBase player)
    {
        for (int k = 0; k < player.EquipList.Count; k++)
        {
            var equip = player.EquipList[k].cardDTO;
            for (int c = 1; c < equip.CardActions.Count; c++)
            {
                var cardAction = equip.CardActions[c];
                if (cardAction.ExtraData != null && cardAction.ExtraData.ContainsKey("condition"))
                {
                    var condition = cardAction.ExtraData["condition"] as JObject;
                    Debug.Log($"Checking condition for card action: {cardAction.ActionType}");
                    if (condition != null && condition.ContainsKey("when_set_mask"))
                    {
                        // 조건 만족 실행
                        if (cardAction.ActionType == EnumTypes.Action.shield)
                        {
                            int shieldValue = cardAction.Value[equip.CardUpgrade];
                            if (player != null)
                            {
                                player.GetShieldBase(shieldValue);
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 장착 카드 능력 실행 - 악기 종료 시 동작
    /// </summary>
    public void SetInstrumentEndEquipmentAction(Player player)
    {
        for (int k = 0; k < player.EquipList.Count; k++)
        {
            var equip = player.EquipList[k].cardDTO;
            if (equip.CardActions[k].ExtraData == null || !equip.CardActions[k].ExtraData.ContainsKey("condition"))
            {
                continue;
            }
            else
            {
                var condition = equip.CardActions[k].ExtraData["condition"] as JObject;
                if (condition == null || !condition.ContainsKey("when_instrument_end"))
                {
                    continue;
                }
            }
            for (int c = 1; c < equip.CardActions.Count; c++)
            {
                var cardAction = equip.CardActions[c];
                if (cardAction.ExtraData != null && cardAction.ExtraData.ContainsKey("equip_criteria"))
                {
                    player.EquipList[k].equipAmount++;
                    int criteria = cardAction.Value[equip.CardUpgrade];
                    Debug.Log($"Checking equip criteria: {criteria} for action {cardAction.ActionType}");
                    if (player.EquipList[k].equipAmount >= criteria)
                    {
                        player.EquipList[k].equipAmount = 0; // 조건 충족 시 초기화
                        c++;
                        if (equip.CardActions[c].ActionType == EnumTypes.Action.get_action_point)
                        {
                            int actionPoint = equip.CardActions[c].Value[equip.CardUpgrade];
                            if (player != null)
                            {
                                player.GetAction(actionPoint);
                            }
                            Debug.Log($"Gained {actionPoint} action points from equipment action.");
                        }
                    }
                }
            }
        }
    }
}
