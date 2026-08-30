using System;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Localization.SmartFormat;

public class EnemyTextChoiceResult : Singleton<EnemyTextChoiceResult>
{
    //선택 결과
    public void SetChoiceResult(EnemyTextDTO enemyTextDTO)
    {
        if (enemyTextDTO == null || enemyTextDTO.ExtraData == null) return;
        Debug.Log("Setting choice result actions.");

        // 유물 획득
        if (enemyTextDTO.ExtraData.ContainsKey("get_artifact"))
        {
            int artifactId = int.Parse(enemyTextDTO.ExtraData["get_artifact"].ToString());
            Debug.Log($"Artifact ID: {artifactId}");
            GetArtifact(artifactId);
        }
        // 코인 획득/손실
        if (enemyTextDTO.ExtraData.ContainsKey("get_coin"))
        {
            Debug.Log("Get Coin Triggered");
            var getCoinObj = enemyTextDTO.ExtraData["get_coin"] as JObject;
            if (getCoinObj != null)
            {
                int coinValue = 0;
                if (getCoinObj.ContainsKey("value"))
                {
                    if (int.TryParse(getCoinObj["value"].ToString(), out int c))
                    {
                        coinValue = c;
                        GetCoin(coinValue);
                    }
                }
                else if (getCoinObj.ContainsKey("percent"))
                {
                    if (float.TryParse(getCoinObj["percent"].ToString(), out float coinPercent))
                    {
                        coinValue = (int)(UserData.Instance.MainScenarioData.GameCoins * coinPercent);
                        GetCoin(coinValue);
                    }
                }
            }
        }
        if (enemyTextDTO.ExtraData.ContainsKey("lose_coin"))
        {
            var loseCoinObj = enemyTextDTO.ExtraData["lose_coin"] as JObject;
            if (loseCoinObj != null)
            {
                if (loseCoinObj.ContainsKey("value"))
                {
                    if (int.TryParse(loseCoinObj["value"].ToString(), out int coinValue))
                    {
                        GetCoin(-coinValue);
                    }
                }
                else if (loseCoinObj.ContainsKey("percent"))
                {
                    if (float.TryParse(loseCoinObj["percent"].ToString(), out float coinPercent))
                    {
                        int coinValue = (int)(UserData.Instance.MainScenarioData.GameCoins * coinPercent);
                        GetCoin(-coinValue);
                    }
                }
            }
        }
        if (enemyTextDTO.ExtraData.ContainsKey("spine_change"))
        {
            int imgId = int.Parse(enemyTextDTO.ExtraData["spine_change"].ToString());
            string path = InGameData.Instance.Enemys.Find(x => x.Id == imgId).ImgSpinePath;
            EnemyManager.Instance?.ChangeEnemySpine(path);
        }
        if (enemyTextDTO.ExtraData.ContainsKey("image_change"))
        {
            var imgPath = enemyTextDTO.ExtraData["image_change"].ToString();
            EnemyManager.Instance?.ChangeEnemyImage(imgPath, true);
        }
        // 별도의 액션
        if (enemyTextDTO.ExtraData.ContainsKey("action"))
        {
            var action = enemyTextDTO.ExtraData["action"] as JObject;
            DoExtraAction(action);
        }
    }

    public async void GetArtifact(int artifactId)
    {
        var data = BattleManager.Instance?.SCENARIO_DATA;
        bool response = await SupabaseArtifact.Instance.GetArtifact(artifactId, data);

        if (response)
        {
            Debug.Log($"Artifact {artifactId} obtained successfully.");

            string artifactName = InGameData.Instance.Artifacts.Find(a => a.Id == artifactId).Name;
            string textValue = LogManager.Instance?.GetDBLogText(EnumTypes.LogActionType.player_get_something).FormatSmart(artifactName);
            NotificationManager.Instance.SetShownNotification(textValue);
        }
        else
        {
            string text = LocalString.Instance.GetLocalizedString("GetArtifactFail");
            NotificationManager.Instance.SetShownNotification(text);
        }
    }

    public async void GetCoin(int amount)
    {
        var data = BattleManager.Instance?.SCENARIO_DATA;
        await SupabaseGetScenarioCoin.Instance.GetCoin(amount, data);
        string text = string.Empty;
        if (amount >= 0)
        {
            text = LogManager.Instance?.GetDBLogText(EnumTypes.LogActionType.player_get_something);
        }
        else
        {
            text = LogManager.Instance?.GetDBLogText(EnumTypes.LogActionType.player_lose_something);
        }


        string textValue = text.FormatSmart(Math.Abs(amount).ToString() + LogManager.Instance?.GetLocalText("coin"));
        NotificationManager.Instance.SetShownNotification(textValue);

        BattleManager.Instance.GetCoinIngameData(amount);
    }

    // 결과가 action인 경우 -> deal extra action
    public void DoExtraAction(JObject action)
    {
        if (action == null) return;

        if (action.ContainsKey("arrest_tiger"))
        {
            var isArrest = action["arrest_tiger"].ToObject<bool>();
            SupabaseMainScenarioStoryUpdate.Instance.UpdateMainScenarioStoryClearData(EnumTypes.MainStoryType.tiger_arrest, isArrest);
        }
    }
}
