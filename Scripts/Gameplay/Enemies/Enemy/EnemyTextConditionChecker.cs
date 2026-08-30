using Newtonsoft.Json.Linq;
using UnityEngine;

public class EnemyTextConditionChecker : Singleton<EnemyTextConditionChecker>
{
    public static bool CheckCondition(EnemyTextDTO textDTO, UserData userData)
    {
        if (textDTO.ExtraData != null && textDTO.ExtraData.ContainsKey("condition"))
        {
            var condition = textDTO.ExtraData["condition"] as JObject;
            if (condition != null && condition.ContainsKey("tiger_arrest"))
            {
                bool isTigerArrest = condition["tiger_arrest"].ToObject<bool>();
                return isTigerArrest == userData.MainScenarioData.StoryClearData.TigerArrest;
            }
            // 필요하다면 다른 condition도 처리
        }

        return true; // 조건이 없거나 일치하지 않으면 false 반환
    }
}
