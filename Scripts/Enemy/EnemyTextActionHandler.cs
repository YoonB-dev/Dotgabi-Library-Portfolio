using Newtonsoft.Json.Linq;
using UnityEngine;

public class EnemyTextActionHandler : Singleton<EnemyTextActionHandler>
{
    public void HandleAction(EnemyTextDTO textDTO)
    {
        if (textDTO.ExtraData != null && textDTO.ExtraData.ContainsKey("action"))
        {
            var action = textDTO.ExtraData["action"] as JObject;
            // 증거 보여주기
            if (action.ContainsKey("show_evidence"))
            {
                var type = action["show_evidence"].ToObject<string>();
                if (type == "tiger")
                {
                    EnemyTextEvidenceAction.Instance.StartEvidence();
                }
            }
        }
    }
}
