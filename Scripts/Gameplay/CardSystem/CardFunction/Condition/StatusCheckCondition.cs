using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class StatusCheckCondition : IConditionEvaluator
{
    public bool Evaluate(CardActionDTO cardAction, Player player, GameObject enemy)
    {
        if (cardAction.ExtraData == null) return false;
        if (!cardAction.ExtraData.ContainsKey("condition")) return true;

        var condition = cardAction.ExtraData["condition"] as JObject;
        if (condition == null) return false;
        EnumTypes.Status targetStatus = EnumTypes.Status.buff; // Default to buff
        int statusId = 0;


        if (condition.ContainsKey("status_exist"))
        {
            bool statusExist = false;
            var checkStatusData = condition["status_exist"] as JObject;

            if (checkStatusData == null) return false;

            if (checkStatusData["buff"] != null)
            {
                statusId = int.Parse(checkStatusData["buff"].ToString());
                targetStatus = EnumTypes.Status.buff;
            }
            if (checkStatusData["debuff"] != null)
            {
                statusId = int.Parse(checkStatusData["debuff"].ToString());
                targetStatus = EnumTypes.Status.debuff;
            }

            if (checkStatusData.ContainsKey("target"))
            {
                switch (checkStatusData["target"].ToString())
                {
                    case "enemy":
                        statusExist = enemy.GetComponent<Enemy>().CheckHaveBuffOrDebuff(targetStatus, statusId);
                        break;
                    case "self":
                        statusExist = player.CheckHaveBuffOrDebuff(targetStatus, statusId);
                        break;
                    case "enemys":
                        for (int i = 0; i < EnemyManager.Instance.enemies.Count; i++)
                        {
                            if (EnemyManager.Instance.enemies[i].CheckHaveBuffOrDebuff(targetStatus, statusId))
                            {
                                return true; // If any enemy has the status, return true
                            }
                        }
                        break;
                    default:
                        return false; // Invalid target
                }
            }

            return statusExist;
        }

        return false;
    }
}
