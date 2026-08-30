using Newtonsoft.Json.Linq;
using UnityEngine;

public class ValueChangeAction : IActionExecutor
{
    public void Execute(CardActionDTO cardAction, ref int amount, CharacterBase enemy)
    {
        if (cardAction.ExtraData == null) return;

        // case: condition - action
        if (cardAction.ExtraData != null && cardAction.ExtraData.ContainsKey("action"))
        {
            var action = cardAction.ExtraData["action"] as JObject;
            if (action == null) return;

            if (action.ContainsKey("percent"))
            {
                var percentToken = action["percent"];
                if (percentToken.Type == JTokenType.Integer)
                {
                    int percentValue = int.Parse(action["percent"].ToString());
                    amount *= percentValue;
                }
                else
                {
                    var percent = action["percent"] as JObject;

                    if (percent != null && percent.ContainsKey("summon_count"))
                    {
                        int summonId = int.Parse(percent["summon_count"].ToString());
                        int summonCount = summonId == 0 ? SummonFunction.Instance.GetSummonCount(summonId, true) : SummonFunction.Instance.GetSummonCount(summonId);
                        amount *= summonCount;
                    }
                }

            }
        }
        // no condition - just check percent
        else if (cardAction.ExtraData.ContainsKey("percent"))
        {
            var percentToken = cardAction.ExtraData["percent"] as JObject ?? cardAction.ExtraData["percent"];
            if (percentToken is JValue jValue && jValue.Type == JTokenType.Integer)
            {
                int percentValue = int.Parse(jValue.ToString());
                amount *= percentValue;
            }
            else
            {
                // 손에 들고 있는 카드 개수에 따라 percent가 바뀌는 경우
                if (cardAction.ExtraData["percent"].ToString() == "hand_count")
                {
                    int handCount = CardSystem.Instance.cards.Count + 1;
                    amount *= handCount;
                }

                // 1. 손에 들고 있는 강화 카드에 따라 percent가 바뀌는 경우
                if (percentToken is JObject jobjectUpgrade && jobjectUpgrade != null && jobjectUpgrade.ContainsKey("hand_upgrade_count"))
                {
                    int handUpgradeCount = 0;
                    for (int i = 0; i < CardSystem.Instance.cards.Count; i++)
                    {
                        var card = CardSystem.Instance.cards[i].GetComponent<Card>();
                        if (card != null && card.cardData.CardUpgrade != 0)
                        {
                            if (jobjectUpgrade["hand_upgrade_count"].ToString() == "all")
                            {
                                handUpgradeCount++;
                            }
                            else
                            {
                                handUpgradeCount++;
                            }

                        }
                    }
                    amount *= handUpgradeCount;
                }
                // 2. 소환수 개수에 따라 percent가 바뀌는 경우
                if (percentToken is JObject jobjectSummonCount && jobjectSummonCount != null && jobjectSummonCount.ContainsKey("summon_count"))
                {
                    int summonId = int.Parse(jobjectSummonCount["summon_count"].ToString());
                    int summonCount = summonId == 0 ? SummonFunction.Instance.GetSummonCount(summonId, true) : SummonFunction.Instance.GetSummonCount(summonId);
                    amount *= summonCount;
                }
            }
        }

        // no condition - just check value
        else if (cardAction.ExtraData.ContainsKey("value"))
        {
            var valueToken = cardAction.ExtraData["value"] as JObject ?? cardAction.ExtraData["value"];
            if (valueToken is JValue jvalue && jvalue.Type == JTokenType.Integer)
            {
                // value후 숫자가 온다면 이거 동작
            }
            else
            {
                // 1. status에 따라 value가 바뀌는 경우
                if (valueToken is JObject jobject && jobject != null && jobject.ContainsKey("status_count"))
                {
                    Debug.Log("Status Count Change");
                    var statusCountData = jobject["status_count"] as JObject;
                    int statusId = statusCountData != null && statusCountData.ContainsKey("status_id") ? int.Parse(statusCountData["status_id"].ToString()) : 0;
                    EnumTypes.Status targetStatus = statusCountData != null && statusCountData.ContainsKey("type") ?
                        (EnumTypes.Status)System.Enum.Parse(typeof(EnumTypes.Status), statusCountData["type"].ToString()) : EnumTypes.Status.buff;
                    EnumTypes.Target target = statusCountData != null && statusCountData.ContainsKey("target") ?
                        (EnumTypes.Target)System.Enum.Parse(typeof(EnumTypes.Target), statusCountData["target"].ToString()) : EnumTypes.Target.self;

                    if (target == EnumTypes.Target.self)
                    {
                        amount = BattleManager.Instance.player.GetBuffOrDebuffValue(targetStatus, statusId);
                    }
                    else if (target == EnumTypes.Target.enemy)
                    {
                        amount = enemy.GetBuffOrDebuffValue(targetStatus, statusId);
                    }
                    else if (target == EnumTypes.Target.enemys)
                    {
                        int totalCount = 0;
                        for (int i = 0; i < EnemyManager.Instance.enemies.Count; i++)
                        {
                            totalCount += EnemyManager.Instance.enemies[i].GetBuffOrDebuffValue(targetStatus, statusId);
                        }
                        amount = totalCount;
                    }

                }
                // 2. 손에 들고 있는 강화 카드에 따라 value가 바뀌는 경우
                if (valueToken is JObject jobjectUpgrade && jobjectUpgrade != null && jobjectUpgrade.ContainsKey("hand_upgrade_count"))
                {
                    int handUpgradeCount = 0;
                    for (int i = 0; i < CardSystem.Instance.cards.Count; i++)
                    {
                        var card = CardSystem.Instance.cards[i].GetComponent<Card>();
                        if (card != null && card.cardData.CardUpgrade != 0)
                        {
                            if (jobjectUpgrade["hand_upgrade_count"].ToString() == "all")
                            {
                                handUpgradeCount++;
                            }
                            else
                            {
                                handUpgradeCount++;
                            }
                        }
                    }
                    amount = handUpgradeCount;
                }
                // 3. 현재 방어도에 비례해 value가 바뀌는 경우
                if (valueToken is JObject jobjectShield && jobjectShield != null && jobjectShield.ContainsKey("shield_value"))
                {
                    Debug.Log("Shield Value Change");
                    string target = jobjectShield["shield_value"].ToString();
                    if (target == "self")
                    {
                        amount = BattleManager.Instance.player.Stats.currShield;
                    }
                    else if (target == "enemy")
                    {
                        amount = enemy.Stats.currShield;
                    }
                    else if (target == "enemys")
                    {
                        int totalShield = 0;
                        for (int i = 0; i < EnemyManager.Instance.enemies.Count; i++)
                        {
                            totalShield += EnemyManager.Instance.enemies[i].Stats.currShield;
                        }
                        amount = totalShield;
                    }
                }
                // 4. 소환수 개수에 따라 value가 바뀌는 경우
                if (valueToken is JObject jobjectSummonCount && jobjectSummonCount != null && jobjectSummonCount.ContainsKey("summon_count"))
                {
                    int summonId = int.Parse(jobjectSummonCount["summon_count"].ToString());
                    int summonCount = summonId == 0 ? SummonFunction.Instance.GetSummonCount(summonId, true) : SummonFunction.Instance.GetSummonCount(summonId);
                    amount = summonCount;
                }
                // 5. 악기 연주 횟수에 따라 value가 바뀌는 경우
                if (valueToken.ToString() == "play_instrument_time")
                {
                    int playCount = PlayFunction.Instance.playTime;
                    amount = playCount;
                    Debug.Log($"Play Count Change: {playCount}");
                }
            }
        }

    }
}
