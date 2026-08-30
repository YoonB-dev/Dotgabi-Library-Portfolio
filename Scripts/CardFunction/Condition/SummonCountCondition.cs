using Newtonsoft.Json.Linq;
using UnityEngine;

public class SummonCountCondition : IConditionEvaluator
{
    public bool Evaluate(CardActionDTO cardAction, Player player, GameObject enemy)
    {
        if (cardAction.ExtraData == null) return false;
        if (!cardAction.ExtraData.ContainsKey("condition")) return true;

        var condition = cardAction.ExtraData["condition"] as JObject;
        if (condition == null) return false;

        if (condition["is_summon"] != null)
        {
            int summonId;
            if (!int.TryParse(condition["is_summon"].ToString(), out summonId))
            {
                summonId = -1; // all summon count
            }

            int count;
            if (summonId == -1)
                count = SummonFunction.Instance.GetSummonCount(summonId, true);
            else
                count = SummonFunction.Instance.GetSummonCount(summonId);

            return count >= 1;
        }

        return false;
    }
}
