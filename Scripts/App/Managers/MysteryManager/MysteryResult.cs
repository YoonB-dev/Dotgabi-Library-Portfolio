using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Localization.SmartFormat;

public class MysteryResult : SceneSingleton<MysteryResult>
{
    private ScenarioDTO SCENARIO_DATA;

    private void SetData()
    {
        Debug.Log("SetData 호출됨");
        SCENARIO_DATA = MoveSystem.Instance?.SCENARIO_DATA;
        if (SCENARIO_DATA == null)
        {
            Debug.LogError("SCENARIO_DATA is null");
        }
        Debug.Log("SCENARIO_DATA 타입: " + SCENARIO_DATA?.GetType().Name);
    }
    public async void ResultAction(EventResultDTO eventChoice)
    {
        SetData();
        Debug.Log("이벤트 결과 처리 시작: "+ eventChoice.ResultId +"\n이벤트 Action: " + eventChoice.ResultAction);

        var rand = new System.Random(SCENARIO_DATA.GenerateSeed + SCENARIO_DATA.SelectList.Count * 3 + 39);
        //결과 처리
        switch (eventChoice.ResultAction)
        {
            case "null":
                break;
            case "hp_max_get":
                if (eventChoice.ExtraData.ContainsKey("amount"))
                {
                    int amount = int.Parse(eventChoice.ExtraData["amount"].ToString());
                    GetMaxHp(eventChoice.ResultId, amount);
                }
                else if (eventChoice.ExtraData.ContainsKey("percent"))
                {
                    float percent = float.Parse(eventChoice.ExtraData["percent"].ToString());
                    int amount = (int)Math.Round(SCENARIO_DATA.MaxHp * percent * 0.01f);
                    GetMaxHp(eventChoice.ResultId, amount);
                }
                break;
            case "hp_get":
                if (eventChoice.ExtraData.ContainsKey("amount"))
                {
                    int amount = int.Parse(eventChoice.ExtraData["amount"].ToString());
                    GetHp(eventChoice.ResultId, amount);
                }
                else if (eventChoice.ExtraData.ContainsKey("percent"))
                {
                    float percent = float.Parse(eventChoice.ExtraData["percent"].ToString());
                    int amount = (int)Math.Round(SCENARIO_DATA.MaxHp * percent * 0.01f);
                    GetHp(eventChoice.ResultId, amount);
                }
                break;
            case "artifact_get":
                Debug.Log("Artifact Get");
                if (eventChoice.ExtraData.ContainsKey("item_index"))
                {
                    int artifactId = int.Parse(eventChoice.ExtraData["item_index"].ToString());
                    var artifact = InGameData.Instance.Artifacts.Find(a => a.Id == artifactId);
                    await ScenarioArtifactUtils.Instance.GetArtifact(artifact, SCENARIO_DATA);

                }
                else if (eventChoice.ExtraData.ContainsKey("random"))
                {
                    var artifactType = (EnumTypes.RarityType)Enum.Parse(typeof(EnumTypes.RarityType), eventChoice.ExtraData["random"].ToString());
                    ScenarioArtifactUtils.Instance.GetRandomArtifact(SCENARIO_DATA, artifactType);
                }
                else
                {
                    Debug.LogError("Artifact ID not found in event choice extra data.");
                }
                break;
            case "artifact_delete":
                if (SCENARIO_DATA.OwnedArtifactList.Count == 0)
                {
                    Debug.LogError("No artifacts to delete.");
                    //NotificationManager.Instance.ShowNotification("No artifacts to delete.");
                    return;
                }
                if (eventChoice.ExtraData.ContainsKey("item_index"))
                {
                    int artifactId = int.Parse(eventChoice.ExtraData["item_index"].ToString());
                    var artifact = InGameData.Instance.Artifacts.Find(a => a.Id == artifactId);
                    DeleteArtifact(artifact, SCENARIO_DATA);
                }
                else if (eventChoice.ExtraData.ContainsKey("random"))
                {
                    Debug.Log(SCENARIO_DATA.OwnedArtifactList.Count);
                    int randomIndex = rand.Next(0, SCENARIO_DATA.OwnedArtifactList.Count);
                    var artifact = SCENARIO_DATA.OwnedArtifactList[randomIndex];
                    var artifactDTO = InGameData.Instance.Artifacts.Find(a => a.Id == artifact.ArtifactId);
                    DeleteArtifact(artifactDTO, SCENARIO_DATA);
                }
                else
                {
                    Debug.LogError("Artifact ID not found in event choice extra data.");
                }
                break;
            case "card_get":
                if (eventChoice.ExtraData.ContainsKey("card_index"))
                {
                    int cardId = int.Parse(eventChoice.ExtraData["card_index"].ToString());
                    var card = InGameData.Instance.Cards.Find(c => c.Id == cardId);
                    await SupabaseCard.Instance.GetCard(SCENARIO_DATA, card);
                    SetFooterText.Instance.SetAllText();
                }
                else
                {
                    Debug.LogError("Card ID not found in event choice extra data.");
                }
                break;
            case "card_delete":
                if (eventChoice.ExtraData.ContainsKey("card_index"))
                {
                    int cardId = int.Parse(eventChoice.ExtraData["card_index"].ToString());
                    var card = InGameData.Instance.Cards.Find(c => c.Id == cardId);
                    var ownedCard = SCENARIO_DATA.OwnedCardList.Find(oc => oc.CardId == card.Id);
                    DeleteCardById(ownedCard.OwnedId, card.Name);
                }
                else if (eventChoice.ExtraData.ContainsKey("random"))
                {
                    if (SCENARIO_DATA.OwnedCardList.Count == 0)
                    {
                        var text = LogManager.Instance?.GetLocalText("no_card_to_delete");
                        NotificationManager.Instance.SetShownNotification(text);
                        return;
                    }
                    int randomIndex = rand.Next(0, SCENARIO_DATA.OwnedCardList.Count);
                    var ownedCard = SCENARIO_DATA.OwnedCardList[randomIndex];
                    var card = InGameData.Instance.Cards.Find(c => c.Id == ownedCard.CardId);
                    DeleteCardById(ownedCard.OwnedId, card.Name);
                }
                else
                {
                    Debug.LogError("Card ID not found in event choice extra data.");
                }
                break;
            case "coin_get":
                if (eventChoice.ExtraData.ContainsKey("amount"))
                {
                    int amount = int.Parse(eventChoice.ExtraData["amount"].ToString());
                    GetCoin(eventResultId: eventChoice.ResultId, amount: amount, SCENARIO_DATA);
                }
                else if (eventChoice.ExtraData.ContainsKey("percent"))
                {
                    float percent = float.Parse(eventChoice.ExtraData["percent"].ToString());
                    int amount = (int)(SCENARIO_DATA.GameCoins * percent * 0.01f);
                    GetCoin(eventResultId: eventChoice.ResultId, amount: amount, SCENARIO_DATA);
                }
                else
                {
                    Debug.LogError("Amount not found in event choice extra data for coin use.");
                }
                break;
            case "set_next_event":
                if (eventChoice.ExtraData.ContainsKey("next_event_id"))
                {
                    int nextEvent = int.Parse(eventChoice.ExtraData["next_event_id"].ToString());
                    SetNextAction(eventResultId: eventChoice.ResultId, eventNextId: nextEvent);
                }
                else
                {
                    Debug.LogError("Next event not found in event choice extra data.");
                }
                break;
            case "battle_enemy":
                if (eventChoice.ExtraData.ContainsKey("battle"))
                {
                    int battleId = int.Parse(eventChoice.ExtraData["battle"].ToString());
                    StartCoroutine(MoveSystem.Instance.GoToBattleScene(battleId));
                }
                break;
        }
    }

    public async void GetMaxHp(int eventResultId, int num)
    {
        SetData();
        // SFX
        if (num >= 0) AudioManager.Instance.GetMaxHpSound();
        else AudioManager.Instance.GetDamageSound();

        var client = SupabaseClientProvider.Instance.Client;

        // 시나리오별 최대 체력 증가 RPC 호출
        Supabase.Postgrest.Responses.BaseResponse response = null;
        if (SCENARIO_DATA is UserMainScenarioDTO)
        {
            response = await SupabaseWrap.ExecuteWithRefresh(() => client.Rpc("update_user_main_scenario_hp_using_event_id", new Dictionary<string, object> {
                { "p_user_auth_id", client.Auth.CurrentUser.Id },
                { "p_event_result_id", eventResultId },
            }));
        }
        else if (SCENARIO_DATA is UserChallengeScenarioDTO)
        {
            response = await SupabaseWrap.ExecuteWithRefresh(() => client.Rpc("update_user_challenge_scenario_hp_using_event_id", new Dictionary<string, object> {
                { "p_event_result_id", eventResultId },
            }));
        }
        else
        {
            Debug.LogError("GetMaxHp: Invalid ScenarioDTO type");
            return;
        }

        bool result = bool.Parse(response.Content);
        if (!result)
        {
            Debug.LogError("Max HP update failed.");
            return;
        }

        SCENARIO_DATA.MaxHp += num;
        SCENARIO_DATA.CurrHp += num;
        Debug.Log("currHp: " + SCENARIO_DATA.CurrHp);
        if (SCENARIO_DATA.MaxHp < 1) SCENARIO_DATA.MaxHp = 1;
        if (SCENARIO_DATA.CurrHp < 1) SCENARIO_DATA.CurrHp = 1;

        //if(SCENARIO_DATA.CurrHp<=0)
        //MoveSystem.moveSystem.GameOver();
        //수치 텍스트 생성

        if (num >= 0)
        {
            SetFooterText.Instance.SetMoveText(num, EnumTypes.MoveTextType.heal);
            SetFooterText.Instance.SetHpBar(EnumTypes.TextMotionType.up);
        }
        else
        {
            SetFooterText.Instance.SetMoveText(num, EnumTypes.MoveTextType.damage);
            SetFooterText.Instance.SetHpBar(EnumTypes.TextMotionType.down);
        }
    }

    public async void GetHp(int eventResultId, int num)
    {
        SetData();
        // num이 음수면 데미지, 양수면 회복
        var client = SupabaseClientProvider.Instance.Client;
        // 시나리오별 현재 체력 증가 RPC 호출
        Supabase.Postgrest.Responses.BaseResponse response = null;
        if (SCENARIO_DATA is UserMainScenarioDTO)
        {
            response = await SupabaseWrap.ExecuteWithRefresh(() => client.Rpc("update_user_main_scenario_hp_using_event_id", new Dictionary<string, object> {
                { "p_user_auth_id", client.Auth.CurrentUser.Id },
                { "p_event_result_id", eventResultId }
            }));
        }
        else if (SCENARIO_DATA is UserChallengeScenarioDTO)
        {
            response = await SupabaseWrap.ExecuteWithRefresh(() => client.Rpc("update_user_challenge_scenario_hp_using_event_id", new Dictionary<string, object> {
                { "p_event_result_id", eventResultId }
            }));
        }
        else
        {
            Debug.LogError("GetHp: Invalid ScenarioDTO type");
            return;
        }


        bool result = bool.Parse(response.Content);
        if (!result)
        {
            Debug.LogError("Current HP update failed.");
            return;
        }

        SCENARIO_DATA.CurrHp += num;
        if (SCENARIO_DATA.CurrHp > SCENARIO_DATA.MaxHp) { SCENARIO_DATA.CurrHp = SCENARIO_DATA.MaxHp; }
        if (SCENARIO_DATA.CurrHp <= 0) {SCENARIO_DATA.CurrHp = 1;}
        if (num >= 0)
        {
            // SFX
            AudioManager.Instance.HealSound();
            SetFooterText.Instance.SetMoveText(num, EnumTypes.MoveTextType.heal);
            SetFooterText.Instance.SetHpBar(EnumTypes.TextMotionType.up);
        }
        else
        {
            // SFX
            AudioManager.Instance.DamageSound();
            SetFooterText.Instance.SetMoveText(num, EnumTypes.MoveTextType.damage);
            SetFooterText.Instance.SetHpBar(EnumTypes.TextMotionType.down);
        }



        SetFooterText.Instance.SetAllText();
    }



    public async void DeleteArtifact(ArtifactDTO artifact, ScenarioDTO scenarioData)
    {
        SetData();
        await SupabaseClientProvider.Instance.InitializeAsync();
        var client = SupabaseClientProvider.Instance.Client;

        // 시나리오별 유물 삭제 RPC 호출
        Supabase.Postgrest.Responses.BaseResponse response = null;
        if (scenarioData is UserMainScenarioDTO)
        {
            response = await SupabaseWrap.ExecuteWithRefresh(() => client.Rpc("delete_user_main_scenario_artifact", new Dictionary<string, object> {
                { "p_user_auth_id", client.Auth.CurrentUser.Id },
                { "p_artifact_id", artifact.Id }
            }));
        }
        else if (scenarioData is UserChallengeScenarioDTO)
        {
            response = await SupabaseWrap.ExecuteWithRefresh(() => client.Rpc("delete_user_challenge_scenario_artifact", new Dictionary<string, object> {
                { "p_artifact_id", artifact.Id }
            }));
        }
        else
        {
            Debug.LogError("DeleteArtifact: Invalid ScenarioDTO type");
            return;
        }


        bool result = bool.Parse(response.Content);
        Debug.Log(response.Content);
        if (result)
        {
            // SFX
            AudioManager.Instance.DeleteCardSound();
            SCENARIO_DATA.OwnedArtifactList.RemoveAll(a => a.ArtifactId == artifact.Id);
            SetFooterText.Instance.SetAllText();

            var text = LogManager.Instance.GetDBLogText(EnumTypes.LogActionType.player_lose_something).FormatSmart(artifact.Name);
            NotificationManager.Instance.SetShownNotification(text);
            Debug.Log(text);

            // 로그
            UserScenarioLogDTO logData = new()
            {
                ArtifactId = artifact.Id,
            };
            LogManager.Instance.SetLogMainScene(EnumTypes.LogActionType.card_delete, logData, scenarioData);

            ArtifactShowManager.Instance?.SetArtifactIcon();
        }
        else
        {
            var errorText = LogManager.Instance.GetLocalText("system_error_try_again");
            NotificationManager.Instance.SetShownNotification(errorText);
            Debug.LogError("삭제된 유물이 없습니다.");
        }
    }
    public void DeleteCardById(int ownedId, string cardName)
    {
        SetData();

        SupabaseCard.Instance.DeleteCard(SCENARIO_DATA, ownedId, cardName);
        SetFooterText.Instance.SetAllText();

        StartCoroutine(MysteryManager.Instance.SetBackButtons());
    }
    public async void GetCoin(int eventResultId, int amount, ScenarioDTO scenarioData)
    {
        SetData();
        await SupabaseClientProvider.Instance.InitializeAsync();
        var client = SupabaseClientProvider.Instance.Client;

        // 시나리오별 코인 획득 RPC 호출
        Supabase.Postgrest.Responses.BaseResponse response = null;
        Debug.Log("event_id:" + eventResultId);

        if (scenarioData is UserMainScenarioDTO)
        {
            response = await SupabaseWrap.ExecuteWithRefresh(() => client.Rpc("update_user_main_scenario_coins_using_event_id", new Dictionary<string, object> {
                { "p_user_auth_id", client.Auth.CurrentUser.Id },
                { "p_event_result_id", eventResultId },
            }));
        }
        else if (scenarioData is UserChallengeScenarioDTO)
        {
            response = await SupabaseWrap.ExecuteWithRefresh(() => client.Rpc("update_user_challenge_scenario_coins_using_event_id", new Dictionary<string, object> {
                { "p_event_result_id", eventResultId },
            }));
        }
        else
        {
            Debug.LogError("GetCoin: Invalid ScenarioDTO type");
            return;
        }

        bool result = bool.TryParse(response.Content, out result);

        if (result)
        {
            // SFX
            SCENARIO_DATA.GameCoins += amount;
            SCENARIO_DATA.TotalGameCoins += amount;
            AudioManager.Instance.MoneySound();
            SetFooterText.Instance.SetAllText();
            SetFooterText.Instance.SetMoveText(amount, EnumTypes.MoveTextType.money);
            Debug.Log($"코인 획득: {amount}개");

            // 로그
            UserScenarioLogDTO logData = new()
            {
                value = amount,
                ExtraData = new Dictionary<string, object> {
                    { "coin", true },
                },
            };
            if (amount >= 0)
            {
                LogManager.Instance.SetLogMainScene(EnumTypes.LogActionType.player_get_something, logData, scenarioData);
            }
            else
            {
                LogManager.Instance.SetLogMainScene(EnumTypes.LogActionType.player_lose_something, logData, scenarioData);
            }


        }
        else
        {
            var errorText = LogManager.Instance.GetLocalText("system_error_try_again");
            NotificationManager.Instance.SetShownNotification(errorText);
            Debug.LogError("코인 획득 중 오류 발생. 게임을 다시 시작해주세요.");
        }
    }
    public async void SetNextAction(int eventResultId, int eventNextId)
    {
        SetData();
        await SupabaseClientProvider.Instance.InitializeAsync();
        var client = SupabaseClientProvider.Instance.Client;

        // 시나리오별 다음 이벤트 설정 RPC 호출
        Supabase.Postgrest.Responses.BaseResponse response = null;

        if (SCENARIO_DATA is UserMainScenarioDTO)
        {
            response = await SupabaseWrap.ExecuteWithRefresh(() => client.Rpc("update_user_main_scenario_next_event", new Dictionary<string, object> {
                { "p_user_auth_id", client.Auth.CurrentUser.Id },
                { "p_event_result_id", eventResultId }
            }));
        }
        else if (SCENARIO_DATA is UserChallengeScenarioDTO)
        {
            response = await SupabaseWrap.ExecuteWithRefresh(() => client.Rpc("update_user_challenge_scenario_next_event", new Dictionary<string, object> {
                { "p_event_result_id", eventResultId }
            }));
        }
        else
        {
            Debug.LogError("SetNextAction: Invalid ScenarioDTO type");
            return;
        }

        bool result = bool.TryParse(response.Content, out result);

        if (result)
        {
            SCENARIO_DATA.NextEvent = eventNextId;
            Debug.Log($"다음 이벤트 설정: {eventNextId}");
        }
        else
        {
            var errorText = LogManager.Instance.GetLocalText("system_error_try_again");
            NotificationManager.Instance.SetShownNotification(errorText);
            Debug.LogError("다음 이벤트 설정 중 오류 발생. 게임을 다시 시작해주세요.");
        }
    }
}
