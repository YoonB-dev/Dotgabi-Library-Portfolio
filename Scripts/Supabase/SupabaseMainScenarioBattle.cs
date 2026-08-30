using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class SupabaseMainScenarioBattle : Singleton<SupabaseMainScenarioBattle>
{
    public async Task CallUpdateBattleResult(int currHp, int maxHp, bool? isEliteClear, ScenarioDTO scenarioData)
    {
        await SupabaseClientProvider.Instance.InitializeAsync();
        var client = SupabaseClientProvider.Instance.Client;

        // 파라미터 딕셔너리 구성 (옵셔널 파라미터는 null 허용)
        var parameters = new Dictionary<string, object> {
            { "p_curr_hp", currHp },
            { "p_max_hp", maxHp }
        };

        if (isEliteClear != null && isEliteClear == true)
        {
            parameters.Add("p_is_elite_clear", isEliteClear);
        }

        try
        {
            Supabase.Postgrest.Responses.BaseResponse response = null;
            if (scenarioData is UserMainScenarioDTO)
            {
                response = await SupabaseWrap.ExecuteWithRefresh(() => client.Rpc("update_main_battle_result", parameters));
            }
            else if (scenarioData is UserChallengeScenarioDTO)
            {
                response = await SupabaseWrap.ExecuteWithRefresh(() => client.Rpc("update_challenge_battle_result", parameters));
            }

        }
        catch (System.Exception ex)
        {
            Debug.LogError("Battle result RPC 실패: " + ex.Message);
            return;
        }
    }

    public async void SetNextEventDefault(ScenarioDTO scenarioData)
    {
        var client = SupabaseClientProvider.Instance.Client;

        // 시나리오별 다른 RPC 호출
        Supabase.Postgrest.Responses.BaseResponse response = null;

        if (scenarioData is UserMainScenarioDTO)
        {
            response = await SupabaseWrap.ExecuteWithRefresh(() => client.Rpc("update_user_main_scenario_next_event_using_id", new Dictionary<string, object> {
                { "p_event_id", 0 }
            }));
        }
        else if (scenarioData is UserChallengeScenarioDTO)
        {
            response = await SupabaseWrap.ExecuteWithRefresh(() => client.Rpc("update_user_challenge_scenario_next_event_using_id", new Dictionary<string, object> {
                { "p_event_id", 0 }
            }));
        }
        else
        {
            Debug.LogError("Unknown scenario data type.");
            return;
        }

        bool result = bool.TryParse(response.Content, out result);

        if (result)
        {
            scenarioData.NextEvent = 0;
        }
        else
        {
            var errorText = LogManager.Instance.GetLocalText("system_error_try_again");
            NotificationManager.Instance.SetShownNotification(errorText);
            Debug.LogError("다음 이벤트 설정 중 오류 발생. 게임을 다시 시작해주세요.");
        }
    }
}
