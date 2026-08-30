using Newtonsoft.Json.Linq;
using UnityEngine;

public class IsChangeCondition : IConditionEvaluator
{
    public bool Evaluate(CardActionDTO cardAction, Player player, GameObject enemy)
    {
        if (cardAction.ExtraData == null) return false;
        if (!cardAction.ExtraData.ContainsKey("condition")) return true;

        var condition = cardAction.ExtraData["condition"] as JObject;
        if (condition == null) return false;

        if (condition["is_change"] != null)
        {
            return player.Stats.isChange;
        }

        return false;
    }
}
