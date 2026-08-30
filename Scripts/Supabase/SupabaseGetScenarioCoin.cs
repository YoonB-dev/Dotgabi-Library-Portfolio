using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class SupabaseGetScenarioCoin : Singleton<SupabaseGetScenarioCoin>
{
    public async Task GetCoin(int amount, ScenarioDTO data)
    {
        await SupabaseClientProvider.Instance.InitializeAsync();
        var client = SupabaseClientProvider.Instance.Client;
        Debug.Log($"Adding coin: {amount}");

        // 시나리오별 코인 업데이트 RPC 호출
        Supabase.Postgrest.Responses.BaseResponse response = null;
        if (data is UserMainScenarioDTO)
        {
            response = await SupabaseWrap.ExecuteWithRefresh(() => client.Rpc("update_user_main_scenario_coin", new Dictionary<string, object> {
                { "p_user_auth_id", client.Auth.CurrentUser.Id },
                { "p_get_amount", amount }
                })
            );
        }
        else if (data is UserChallengeScenarioDTO)
        {
            response = await SupabaseWrap.ExecuteWithRefresh(() => client.Rpc("update_user_challenge_scenario_coin", new Dictionary<string, object> {
                { "p_get_amount", amount }
                })
            );
        }
        else
        {
            Debug.LogError("GetCoin: Invalid ScenarioDTO type");
            return;
        }


        bool result = bool.Parse(response.Content);

        if (result)
        {
            if (amount >= 0)
            {
                UserData.Instance.GetCoin(amount, data);
            }
            else
            {
                UserData.Instance.UseCoin(-amount, data);
            }
            //SFX
            AudioManager.Instance.MoneySound();
        }
        else
        {
            Debug.LogError($"Failed to add coin: {response}");
        }
        //UserData.Instance.MainScenarioData = await UserMainScenarioDAO.Instance.GetUserMainScenarioDTO(client.Auth.CurrentUser.Id);
    }

    public async Task GetHp(int amount, ScenarioDTO data)
    {
        await SupabaseClientProvider.Instance.InitializeAsync();

        var client = SupabaseClientProvider.Instance.Client;

        // 시나리오별 코인 업데이트 RPC 호출
        Supabase.Postgrest.Responses.BaseResponse response = null;
        if (data is UserMainScenarioDTO)
        {
            response = await SupabaseWrap.ExecuteWithRefresh(() => client.Rpc("update_user_main_scenario_hp", new Dictionary<string, object> {
                { "p_user_auth_id", client.Auth.CurrentUser.Id },
                { "p_get_amount", amount }
                })
            );
        }
        else if (data is UserChallengeScenarioDTO)
        {
            response = await SupabaseWrap.ExecuteWithRefresh(() => client.Rpc("update_user_challenge_scenario_hp", new Dictionary<string, object> {
                { "p_user_auth_id", client.Auth.CurrentUser.Id },
                { "p_get_amount", amount }
                })
            );
        }
        else
        {
            Debug.LogError("GetHp: Invalid ScenarioDTO type");
            return;
        }


        bool result = bool.Parse(response.Content);

        if (result)
        {
            UserData.Instance.GetHp(amount, data);
        }
        else
        {
            Debug.LogError($"Failed to add hp: {response}");
        }

        if (amount > 0)
        {
            SetFooterText.Instance?.SetMoveText(amount, EnumTypes.MoveTextType.heal);
            SetFooterText.Instance?.SetHpBar(EnumTypes.TextMotionType.up);
        }
        else
        {
            SetFooterText.Instance?.SetMoveText(-amount, EnumTypes.MoveTextType.damage);
            SetFooterText.Instance?.SetHpBar(EnumTypes.TextMotionType.down);
        }
    }

    public async Task GetMaxHp(int amount, ScenarioDTO data)
    {
        await SupabaseClientProvider.Instance.InitializeAsync();

        var client = SupabaseClientProvider.Instance.Client;

        // 시나리오별 코인 업데이트 RPC 호출
        Supabase.Postgrest.Responses.BaseResponse response = null;
        if (data is UserMainScenarioDTO)
        {
            response = await SupabaseWrap.ExecuteWithRefresh(() => client.Rpc("update_user_main_scenario_max_hp", new Dictionary<string, object> {
                { "p_user_auth_id", client.Auth.CurrentUser.Id },
                { "p_increase_amount", amount }
            })
        );
        }
        else if (data is UserChallengeScenarioDTO)
        {
            response = await SupabaseWrap.ExecuteWithRefresh(() => client.Rpc("update_user_challenge_scenario_max_hp", new Dictionary<string, object> {
                { "p_user_auth_id", client.Auth.CurrentUser.Id },
                { "p_increase_amount", amount }
            })
        );
        }
        else
        {
            Debug.LogError("GetMaxHp: Invalid ScenarioDTO type");
            return;
        }

        bool result = bool.Parse(response.Content);

        if (result)
        {
            UserData.Instance.GetMaxHp(amount, data);
        }
        else
        {
            Debug.LogError($"Failed to add max hp: {response}");
        }

        if (amount > 0)
        {
            SetFooterText.Instance?.SetMoveText(amount, EnumTypes.MoveTextType.heal);
        }
        else
        {
            SetFooterText.Instance?.SetMoveText(-amount, EnumTypes.MoveTextType.damage);
        }
    }
}
