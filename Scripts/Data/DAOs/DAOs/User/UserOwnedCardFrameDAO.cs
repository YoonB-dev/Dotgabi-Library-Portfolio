using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class UserOwnedCardFrameDAO : Singleton<UserOwnedCardFrameDAO>
{
    public async Task<List<UserOwnCardFrameDTO>> GetUserOwnedCardFrameAsync(string auth_id)
    {
        var client = SupabaseClientProvider.Instance.Client;

        // 사용자 소유 카드 프레임을 가져옵니다.
        var response = await client
            .From<UserOwnCardFrameEntity>()
            .Select(x => new object[] { x.CardFrameId, x.Count, x.CardFrameType })
            .Where(x => x.UserAuthId == auth_id)
            .Get();

        var userOwnedCardFrames = response.Models;

        return userOwnedCardFrames.ConvertAll(entity => new UserOwnCardFrameDTO {
            CardFrameId = entity.CardFrameId,
            Count = entity.Count,
            CardFrameType = entity.CardFrameType
        });
    }
}
