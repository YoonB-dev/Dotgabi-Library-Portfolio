using Newtonsoft.Json.Linq;
using UnityEngine;

public class ShieldAmountCondition : IConditionEvaluator
{
    public bool Evaluate(CardActionDTO cardAction, Player player, GameObject enemy)
    {
        if (cardAction.ExtraData == null) { return false; }
        if (!cardAction.ExtraData.ContainsKey("condition")) { return true; }

        var condition = cardAction.ExtraData["condition"] as JObject;
        if (condition == null) return false;

        if (condition.ContainsKey("shield_amount"))
        {
            int shieldAmount = player.Stats.currShield;
            int amount = int.Parse(condition["shield_amount"].ToString());
            if (condition.ContainsKey("op"))
            {
                string op = condition["op"].ToString();

                switch (op)
                {
                    case "=" :
                        return shieldAmount == amount;
                    case ">" :
                        return shieldAmount > amount;
                    case "<" :
                        return shieldAmount < amount;
                    case ">=":
                        return shieldAmount >= amount;
                    case "<=":
                        return shieldAmount <= amount;
                }
            }
            else
            {
                // Default operation is equality if not specified
                return shieldAmount == amount;
            }
        }
        return false;
    }
}
