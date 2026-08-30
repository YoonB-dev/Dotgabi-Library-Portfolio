using System;
using Newtonsoft.Json.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization.SmartFormat;

public class GetStatusAction : IActionExecutor
{
    public void Execute(CardActionDTO cardAction, ref int amount, CharacterBase target)
    {
        Debug.Log($"GetStatusAction Execute: {cardAction.ActionType} with amount: {amount}");
        if (cardAction.ExtraData == null) return;

        if (cardAction.ExtraData.ContainsKey("get_status"))
        {
            var statusType = cardAction.ActionType;
            int statusValue = amount;
            int statusId = int.Parse(cardAction.ExtraData["get_status"].ToString());

            Debug.Log($"Applying status: {statusType} with ID: {statusId} and value: {statusValue} to target: {target.name}");

            // Apply the status to the target
            if (target is Enemy enemyTarget)
            {
                enemyTarget.GetStatusEnemy(statusId, Enum.Parse<EnumTypes.Status>(statusType.ToString(), true), statusValue);
                // Log the status application
                // string player = LogManager.Instance?.GetLocalizedText("player");
                // string statusName = statusType == EnumTypes.Action.buff ? $"<color=green>{InGameData.Instance?.Buffs.Find(x => x.Id == statusId).Name}</color>": $"<color=red>{InGameData.Instance?.Debuffs.Find(x => x.Id == statusId).Name}</color>";
                // var statusText = LogManager.Instance?.GetLocalizedText("character_status_give").FormatSmart(player, enemyTarget.characterName, statusName, statusValue);
                // LogManager.Instance?.AddLogBattle(statusText);
            }
            else
            {
                target.GetStatusBase(statusId, Enum.Parse<EnumTypes.Status>(statusType.ToString(), true), statusValue);
            }
        }
        else if (cardAction.ExtraData.ContainsKey("action"))
        {
            var action = cardAction.ExtraData["action"] as JObject;
            if (action == null) return;

            Debug.Log($"Executing action: {action}");
            if (action.ContainsKey("get_status"))
            {
                var statusType = cardAction.ActionType;
                int statusValue = amount;
                int statusId = int.Parse(action["get_status"].ToString());

                Debug.Log($"Applying status: {statusType} with ID: {statusId} and value: {statusValue} to target: {target.name}");

                // Apply the status to the target
                if (target is Enemy enemyTarget)
                {
                    enemyTarget.GetStatusEnemy(statusId, Enum.Parse<EnumTypes.Status>(statusType.ToString(), true), statusValue);
                    // Log the status application
                    // string player = LogManager.Instance?.GetLocalizedText("player");
                    // string statusName = statusType == EnumTypes.Action.buff ? InGameData.Instance?.Buffs[statusId].Name : InGameData.Instance?.Debuffs[statusId].Name;
                    // var statusText = LogManager.Instance?.GetLocalizedText("character_status_give").FormatSmart(player, enemyTarget.characterName, statusName, statusValue);
                    // LogManager.Instance?.AddLogBattle(statusText);
                }
                else
                {
                    target.GetStatusBase(statusId, Enum.Parse<EnumTypes.Status>(statusType.ToString(), true), statusValue);
                }
            }
        }
    }
}