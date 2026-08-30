using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class MysterySupabase : Singleton<MysterySupabase>
{

    public async void InitNextEvent()
    {
        var client = SupabaseClientProvider.Instance.Client;
        var response = await SupabaseWrap.ExecuteWithRefresh(() => client.Rpc("reset_main_scenario_next_event", new Dictionary<string, object> {
            { "p_user_auth_id", client.Auth.CurrentUser.Id }
        }));

        bool result = bool.Parse(response.Content);

        if (result)
        {
            Debug.Log("Next event reset successfully.");
        }
        else
        {
            Debug.LogError($"Failed to reset next event: {response}");
        }
    }

    public async Task IncreaseClearCount(ScenarioDTO scenarioData)
    {
        var client = SupabaseClientProvider.Instance.Client;

        Supabase.Postgrest.Responses.BaseResponse response;
        if (scenarioData.GetType() == typeof(UserMainScenarioDTO))
        {
            response = await SupabaseWrap.ExecuteWithRefresh(() => client.Rpc("increase_main_scenario_mystery_clear_count", new Dictionary<string, object> { }));
        }
        else
        {
            response = await SupabaseWrap.ExecuteWithRefresh(() => client.Rpc("increase_challenge_scenario_mystery__clear_count", new Dictionary<string, object> { }));
        }

        bool result = bool.Parse(response.Content);

        if (result)
        {
            Debug.Log("Clear count increased successfully.");
        }
        else
        {
            Debug.LogError($"Failed to increase clear count: {response}");
        }
    }
}
