using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class UserMainClearRecordDAO : Singleton<UserMainClearRecordDAO>
{
    public async Task<List<UserMainClearRecordDTO>> GetUserMainClearRecord(string auth_id)
    {
        var client = SupabaseClientProvider.Instance.Client;

        var response = await SupabaseWrap.ExecuteWithRefresh(() => client
            .From<UserMainScenarioRecordEntity>()
            .Where(x => x.UserAuthId == auth_id)
            .Get());

        if (response == null || response.Models.Count == 0)
        {
            Debug.LogWarning($"No main clear record found for user: {auth_id}");
            return null;
        }
        var entities = response.Models;
        var dtos = new List<UserMainClearRecordDTO>();
        foreach (var entity in entities)
        {
            var dto = new UserMainClearRecordDTO {
                Version = entity.Version,
                ClearTime = entity.ClearTime,
                ScenarioData = entity.ScenarioData == null ? null : entity.ScenarioData.ToObject<UserMainScenarioDTO>(),
                ArtifactList = entity.ArtifactList == null ? new List<UserScenarioOwnedArtifactDTO>() : entity.ArtifactList.ToObject<List<UserScenarioOwnedArtifactDTO>>(),
                CardList = entity.CardList == null ? new List<UserScenarioOwnedCardDTO>() : entity.CardList.ToObject<List<UserScenarioOwnedCardDTO>>(),
                LogList = entity.LogList == null ? new List<UserScenarioLogDTO>() : entity.LogList.ToObject<List<UserScenarioLogDTO>>(),
                StoryList = entity.StoryList == null ? null : entity.StoryList.ToObject<UserMainscenarioStoryClearDTO>()
            };
            dtos.Add(dto);
        }
        return dtos;
    }
}
