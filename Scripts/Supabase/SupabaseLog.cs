using System.Collections.Generic;
using UnityEngine;

public class SupabaseLog : Singleton<SupabaseLog>
{
    public async void LogUserAction(string userId, UserScenarioLogDTO logData, ScenarioDTO scenarioType)
    {
        await SupabaseClientProvider.Instance.InitializeAsync();
        var client = SupabaseClientProvider.Instance.Client;

        // 시나리오별 로그 데이터 삽입 RPC 호출
        Supabase.Postgrest.Responses.BaseResponse response = null;
        if (scenarioType is UserMainScenarioDTO)
        {
            response = await SupabaseWrap.ExecuteWithRefresh(() => client.Rpc("insert_user_main_scenario_log_data", new Dictionary<string, object> {
                { "p_user_auth_id", userId },
                { "p_log_id", logData.LogId },
                { "p_value", logData.value},
                { "p_card_id", logData.CardId },
                { "p_artifact_id", logData.ArtifactId },
                { "p_extra_data", logData.ExtraData ?? null },
            }));
        }
        else if (scenarioType is UserChallengeScenarioDTO)
        {
            response = await SupabaseWrap.ExecuteWithRefresh(() => client.Rpc("insert_user_challenge_scenario_log_data", new Dictionary<string, object> {
                { "p_log_id", logData.LogId },
                { "p_value", logData.value},
                { "p_card_id", logData.CardId },
                { "p_artifact_id", logData.ArtifactId },
                { "p_extra_data", logData.ExtraData ?? null },
            }));
        }
        else
        {
            Debug.LogError("LogUserAction: Invalid ScenarioDTO type");
            return;
        }


        Debug.Log($"LogUserAction: {response}");
    }
}

