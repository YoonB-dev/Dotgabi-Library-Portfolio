using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class UserOwnedCharacterDAO : Singleton<UserOwnedCharacterDAO>
{
    public async Task<UserOwnCharacterDTO> GetUserOwnedCharacterAsync(string auth_id)
    {
        var client = SupabaseClientProvider.Instance.Client;

        // 사용자 소유 캐릭터 정보를 가져옵니다.
        var response = await client
            .From<UserOwnCharacterEntity>()
            .Select(x => new object[] { x.UserAuthId, x.OwnedBlacksmith, x.OwnedDosa, x.OwnedPerformer })
            .Where(x => x.UserAuthId == auth_id)
            .Get();

        var entity = response.Models.FirstOrDefault();

        return new UserOwnCharacterDTO {
            OwnedBlacksmith = entity.OwnedBlacksmith,
            OwnedDosa = entity.OwnedDosa,
            OwnedPerformer = entity.OwnedPerformer
        };
    }
}
