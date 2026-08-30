using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class UserMainScenarioStoryCardDAO : Singleton<UserMainScenarioStoryCardDAO>
{
    public async Task<List<UserMainScenarioStoryCardDTO>> GetUserMainscenarioStoryCardAsync(string userId)
    {
        var client = SupabaseClientProvider.Instance.Client;

        var response = await SupabaseWrap.ExecuteWithRefresh(() => client
            .From<UserMainScenarioStoryCard>()
            .Where(u => u.UserId == userId)
            .Get());

        var entities = response.Models;

        return entities.ConvertAll(entity => new UserMainScenarioStoryCardDTO {
            CardId = entity.CardId,
            IsUse = entity.IsUse,
            GetAt = entity.GetAt
        });
    }
}
