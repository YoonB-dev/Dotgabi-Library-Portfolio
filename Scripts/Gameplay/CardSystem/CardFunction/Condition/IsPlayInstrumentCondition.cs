using Newtonsoft.Json.Linq;
using UnityEngine;

public class IsPlayInstrumentCondition : IConditionEvaluator
{
    public bool Evaluate(CardActionDTO cardAction, Player player, GameObject enemy)
    {
        Debug.Log("IsPlayInstrumentCondition Evaluate called");
        if (cardAction.ExtraData == null) { return false; }
        if (!cardAction.ExtraData.ContainsKey("condition")) { return true; }

        Debug.Log("IsPlayInstrumentCondition Evaluate called");

        var condition = cardAction.ExtraData["condition"] as JObject;
        if (condition == null) return false;

        if (condition.ContainsKey("is_play_instrument"))
        {
            bool isPlayInstrument = condition["is_play_instrument"].ToObject<bool>();
            if (isPlayInstrument)
            {
                return PlayFunction.Instance.isPlay;
            }
            else
            {
                return !PlayFunction.Instance.isPlay;
            }
        }

        return false;
    }
}
