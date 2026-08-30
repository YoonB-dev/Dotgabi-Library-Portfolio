using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class LogDAO : Singleton<LogDAO>
{
    public async Task<List<LogDTO>> GetAllLogDataAsync(EnumTypes.LanguageType language)
    {
        var response = await SupabaseClientProvider.Instance.ClientGameData
            .From<LogEntity>()
            .Select("*, log_locales(*)")  // log_locales 테이블의 모든 열 포함
            .Filter("log_locales.lan_code", Supabase.Postgrest.Constants.Operator.Equals, language.ToString())
            .Get();

        var logList = response.Models;
        Debug.Log("총 " + logList.Count + "개의 로그 데이터 로드됨");
        return logList.ConvertAll(log => new LogDTO {
            LogId = log.LogID,
            LogAction = log.LogAction,
            LogText = log.LogLocales.Find(locale => locale.LogID == log.LogID)?.Text ?? "No text available"
        });
    }

    public async Task<List<UserScenarioLogDTO>> GetUserAllMainScenarioLogsAsync(string userAuthId)
    {
        var response = await SupabaseClientProvider.Instance.Client
            .From<UserMainScenarioLogEntity>()
            .Where(x => x.UserAuthId == userAuthId)
            .Order("log_at", ordering: Supabase.Postgrest.Constants.Ordering.Ascending)
            .Get();

        return response.Models.Select(log => new UserScenarioLogDTO {
            LogId = log.LogId,
            value = log.Value,
            CardId = log.CardId,
            ArtifactId = log.ArtifactId,
            LogAt = log.LogAt,
            ExtraData = log.ExtraData
        }).ToList();
    }
}