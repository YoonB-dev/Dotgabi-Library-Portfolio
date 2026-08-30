using System.Collections.Generic;
using System.Text.Json;
using UnityEngine;

public class SupabaseScenarioStage : Singleton<SupabaseScenarioStage>
{
    // 메인 시나리오 스테이지 이동 데이터 저장
    public async void AddScenarioSelectList(int selectNum, ScenarioDTO scenarioData)
    {
        var client = SupabaseClientProvider.Instance.Client;

        // 시나리오별 이동 데이터 RPC
        Supabase.Postgrest.Responses.BaseResponse response = null;
        if (scenarioData is UserMainScenarioDTO)
        {
            response = await SupabaseWrap.ExecuteWithRefresh(() => client.Rpc("update_main_scenario_select_list", new Dictionary<string, object> {
                { "p_user_auth_id", client.Auth.CurrentUser.Id },
                { "p_select_id", selectNum }
            }));
        }
        else if (scenarioData is UserChallengeScenarioDTO)
        {
            response = await SupabaseWrap.ExecuteWithRefresh(() => client.Rpc("update_challenge_scenario_select_list", new Dictionary<string, object> {
                { "p_select_id", selectNum }
            }));
        }
        else
        {
            Debug.LogError("AddMainSelectList: Invalid ScenarioDTO type");
            return;
        }


        if (response == null)
        {
            Debug.LogError("UserMainScenarioEntity not found for user: " + client.Auth.CurrentUser.Id);
            return;
        }
        scenarioData.SelectList.Add(selectNum);
        Debug.Log("SelectList updated: " + JsonSerializer.Serialize(scenarioData.SelectList));
    }
}
