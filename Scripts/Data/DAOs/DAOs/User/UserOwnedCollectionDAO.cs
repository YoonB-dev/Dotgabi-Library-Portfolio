using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class UserOwnedCollectionDAO : Singleton<UserOwnedCollectionDAO>
{
    public async Task<List<UserOwnedCardDataDTO>> GetUserOwnedCardDataAsync(string userAuthId)
    {
        var client = SupabaseClientProvider.Instance.Client;

        var response = await client
            .From<UserOwnedCardDataEntity>()
            .Where(x => x.UserAuthId == userAuthId)
            .Get();

        var entity = response.Models;

        if (entity == null)
        {
            Debug.LogWarning($"No UserOwnedCardData found for UserAuthId: {userAuthId}");
            return null;
        }

        return entity.ConvertAll(e => new UserOwnedCardDataDTO {
            CardId = e.CardId
        });
    }

    public async Task<List<UserOwnedArtifactDataDTO>> GetUserOwnedArtifactDataAsync(string userAuthId)
    {
        var client = SupabaseClientProvider.Instance.Client;

        var response = await client
            .From<UserOwnedArtifactDataEntity>()
            .Where(x => x.UserAuthId == userAuthId)
            .Get();

        var entity = response.Models;

        if (entity == null)
        {
            Debug.LogWarning($"No UserOwnedArtifactData found for UserAuthId: {userAuthId}");
            return null;
        }

        return entity.ConvertAll(e => new UserOwnedArtifactDataDTO {
            ArtifactId = e.ArtifactId
        });
    }
}
