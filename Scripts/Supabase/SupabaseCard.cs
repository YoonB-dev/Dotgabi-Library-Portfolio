using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization.SmartFormat;

public class SupabaseCard : Singleton<SupabaseCard>
{


    public async Task<bool> GetCard(ScenarioDTO userScenario, CardDTO card)
    {
        await SupabaseClientProvider.Instance.InitializeAsync();
        var client = SupabaseClientProvider.Instance.Client;

        // 시나리오별 카드 획득 RPC 호출
        Supabase.Postgrest.Responses.BaseResponse response = null;
        if (userScenario is UserMainScenarioDTO)
        {
            userScenario.OwnedCardList = await UserMainScenarioDAO.Instance.GetUserMainScenarioOwnedCardsAsync(client.Auth.CurrentUser.Id);

            response = await SupabaseWrap.ExecuteWithRefresh(() =>
                client.Rpc("insert_user_main_scenario_card", new Dictionary<string, object>
                {
                { "p_card_id", card.Id },
                { "p_card_upgrade_time", card.CardUpgrade }
                })
            );
        }
        else if (userScenario is UserChallengeScenarioDTO)
        {
            userScenario.OwnedCardList = await UserChallengeScenarioDAO.Instance.GetUserChallengeScenarioOwnedCardsAsync(client.Auth.CurrentUser.Id);

            response = await SupabaseWrap.ExecuteWithRefresh(() =>
                client.Rpc("insert_user_challenge_scenario_card", new Dictionary<string, object>
                {
                { "p_card_id", card.Id },
                { "p_card_upgrade_time", card.CardUpgrade }
                })
            );
        }
        else
        {
            Debug.LogError("Unknown scenario type");
            return false;
        }


        bool result = bool.TryParse(response.Content, out result);

        if (result)
        {
            // SFX
            AudioManager.Instance.DrawCardSound(true);

            var cardData = new UserScenarioOwnedCardDTO {
                CardId = card.Id,
                UpgradeTime = card.CardUpgrade
            };

            userScenario.OwnedCardList.Add(cardData);
            // 텍스트
            var getText = LogManager.Instance?.GetDBLogText(EnumTypes.LogActionType.player_get_something).FormatSmart(card.Name);
            NotificationManager.Instance.SetShownNotification(getText);
            Debug.Log(getText);

            // 로그
            UserScenarioLogDTO logData = new()
            {
                CardId = card.Id,
                value = card.CardUpgrade
            };
            LogManager.Instance.SetLogMainScene(EnumTypes.LogActionType.player_get_something, logData, userScenario);
        }
        else
        {
            var errorText = LogManager.Instance?.GetLocalText("system_error_try_again");
            NotificationManager.Instance.SetShownNotification(errorText);
            Debug.LogError("카드 획득 중 오류 발생. 게임을 다시 시작해주세요.");
        }
        return result;
    }

    /// <summary>
    /// 카드 제거
    /// </summary>
    /// <param name="userMainScenario"></param>
    /// <param name="ownedId"></param>
    /// <param name="cardName"></param>

    public async void DeleteCard(ScenarioDTO userScenario, int ownedId, string cardName)
    {
        var client = SupabaseClientProvider.Instance.Client;
        var cardDTO = userScenario.OwnedCardList.Find(c => c.OwnedId == ownedId);
        // 카드 제거 가능 여부 검사
        if (!CardCheckUtils.Instance.CheckCardCanDelete(cardDTO, false))
        {
            var errorText = LogManager.Instance?.GetLocalText("card_deny_delete");
            NotificationManager.Instance.SetShownNotification(errorText);
            return;
        }

        // 시나리오별 카드 제거 RPC 호출
        Supabase.Postgrest.Responses.BaseResponse response = null;
        if (userScenario is UserMainScenarioDTO)
        {
            response = await SupabaseWrap.ExecuteWithRefresh(() => client.Rpc("delete_user_main_scenario_owned_card", new Dictionary<string, object> {
                { "p_user_auth_id", client.Auth.CurrentUser.Id },
                { "p_owned_id", ownedId }
            }));
        }
        else if (userScenario is UserChallengeScenarioDTO)
        {
            response = await SupabaseWrap.ExecuteWithRefresh(() => client.Rpc("delete_user_challenge_scenario_owned_card", new Dictionary<string, object> {
                { "p_owned_id", ownedId }
            }));
        }
        else
        {
            Debug.LogError("Unknown scenario type");
            return;
        }

        bool result = bool.TryParse(response.Content, out result);

        if (result)
        {
            // SFX
            AudioManager.Instance.DeleteCardSound();

            // 로그
            Debug.Log("카드 id:" + cardDTO.CardId);
            UserScenarioLogDTO logData = new()
            {
                CardId = cardDTO.CardId,
                value = cardDTO.UpgradeTime,
            };
            LogManager.Instance.SetLogMainScene(EnumTypes.LogActionType.card_delete, logData, userScenario);

            // 카드 제거
            var deleteText = LogManager.Instance?.GetDBLogText(EnumTypes.LogActionType.card_delete).FormatSmart(cardName);
            userScenario.OwnedCardList.RemoveAll(c => c.OwnedId == ownedId);
            NotificationManager.Instance.SetShownNotification(deleteText);
            Debug.Log(deleteText);

            if (cardDTO.CardId == 56)
            {
                // 도깨비 키 획득
                ScenarioArtifactUtils.Instance?.GetDotgabiKey(3, userScenario);
            }
        }
        else
        {
            var errorText = LogManager.Instance?.GetLocalText("system_error_try_again");
            NotificationManager.Instance.SetShownNotification(errorText);
            Debug.LogError("카드 제거 중 오류 발생. 게임을 다시 시작해주세요.");
        }
    }

    /// <summary>
    /// 카드 업그레이드
    /// </summary>
    public async void UpgradeCard(ScenarioDTO scenarioDTO, int ownedId, string cardName, bool isBattle = false)
    {
        var client = SupabaseClientProvider.Instance.Client;
        var card = scenarioDTO.OwnedCardList.Find(c => c.OwnedId == ownedId);

        // 카드 업그레이드 가능 여부 검사
        if (!CardCheckUtils.Instance.CheckCardCanUpgrade(card, false))
        {
            var errorText = LogManager.Instance?.GetLocalText("card_deny_upgrade");
            NotificationManager.Instance.SetShownNotification(errorText);
            return;
        }

        // 시나리오 별 업그레이드 RPC 호출
        Supabase.Postgrest.Responses.BaseResponse response;
        if (scenarioDTO.GetType() == typeof(UserMainScenarioDTO))
        {
            response = await SupabaseWrap.ExecuteWithRefresh(() => client.Rpc("upgrade_user_main_scenario_owned_card", new Dictionary<string, object> {
                { "p_user_auth_id", client.Auth.CurrentUser.Id },
                { "p_owned_id", ownedId }
            }));
            UserData.Instance.MainScenarioData.OwnedCardList = await UserMainScenarioDAO.Instance.GetUserMainScenarioOwnedCardsAsync(client.Auth.CurrentUser.Id);
        }
        else if (scenarioDTO.GetType() == typeof(UserChallengeScenarioDTO))
        {
            response = await SupabaseWrap.ExecuteWithRefresh(() => client.Rpc("upgrade_user_challenge_scenario_owned_card", new Dictionary<string, object> {
                { "p_user_auth_id", client.Auth.CurrentUser.Id },
                { "p_owned_id", ownedId }
            }));
            UserData.Instance.ChallengeScenarioData.OwnedCardList = await UserChallengeScenarioDAO.Instance.GetUserChallengeScenarioOwnedCardsAsync(client.Auth.CurrentUser.Id);
        }
        else
        {
            Debug.LogError("Unknown scenario type");
            return;
        }


        bool result = bool.TryParse(response.Content, out result);

        if (result)
        {
            // SFX
            AudioManager.Instance.UpgradeSound();

            // 카드 업그레이드 - 강화 수치 증가 및 예외
            if (card.CardId == 55)
            {
                card.CardId = 56; // 찢어진 동화 조각 -> 완성된 동화 조각
                card.UpgradeTime = 0;
            }
            else
            {
                card.UpgradeTime++;
            }
            var upgradeText = LogManager.Instance?.GetDBLogText(EnumTypes.LogActionType.card_upgrade).FormatSmart(cardName);
            NotificationManager.Instance.SetShownNotification(upgradeText);

            // 로그
            var logData = new UserScenarioLogDTO {
                CardId = card.CardId,
                value = card.UpgradeTime - 1 // 이전 업그레이드 레벨
            };
            LogManager.Instance?.SetLogMainScene(EnumTypes.LogActionType.card_upgrade, logData, scenarioDTO);
        }
        else
        {
            var errorText = LogManager.Instance?.GetLocalText("system_error_try_again");
            NotificationManager.Instance.SetShownNotification(errorText);
        }
    }
}
