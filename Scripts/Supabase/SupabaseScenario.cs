using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class SupabaseScenario : Singleton<SupabaseScenario>
{
    public async Task SetUserScenarioType(EnumMainType.ScenarioType scenarioType)
    {
        var client = SupabaseClientProvider.Instance.Client;
        var response = await SupabaseWrap.ExecuteWithRefresh(() => client.Rpc("update_curr_scenario_type", new Dictionary<string, object>
            {
                { "p_type", scenarioType.ToString() } // "story" 또는 "challenge"
            })
        );

        GameData.Instance.CurrScenarioType = scenarioType;
    }

    public async Task UpdateUserScenarioNextStage(EnumMainType.ScenarioType scenarioType)
    {
        var client = SupabaseClientProvider.Instance.Client;
        switch (scenarioType)
        {
            case EnumMainType.ScenarioType.story:
                await SupabaseWrap.ExecuteWithRefresh(() => client.Rpc("updata_main_scenario_next_stage", new Dictionary<string, object>()));
                break;
            case EnumMainType.ScenarioType.challenge:
                await SupabaseWrap.ExecuteWithRefresh(() => client.Rpc("update_challenge_scenario_next_stage", new Dictionary<string, object>()));
                break;
        }
    }

    public async Task InsertUserScenarioClear(EnumMainType.ScenarioType scenarioType)
    {
        Debug.Log("InsertUserScenarioClear 호출");
        var client = SupabaseClientProvider.Instance.Client;
        switch (scenarioType)
        {
            case EnumMainType.ScenarioType.story:
                await SupabaseWrap.ExecuteWithRefresh(() => client.Rpc("clear_main_scenario_data", new Dictionary<string, object>()));
                break;
            case EnumMainType.ScenarioType.challenge:
                Debug.Log("챌린지 시나리오 클리어 기록은 없음");
                break;
        }
    }
}
