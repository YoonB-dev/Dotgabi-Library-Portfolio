using System.Threading.Tasks;
using UnityEngine;

public class UserScenarioClearDAO : Singleton<UserScenarioClearDAO>
{
    public async Task<UserScenarioClearDTO> GetUserMainSceneClearAsync(string auth_id)
    {
        var client = SupabaseClientProvider.Instance.Client;

        // 사용자 메인 시나리오 클리어 정보를 가져옵니다.
        var response = await client
            .From<UserClearEntity>()
            .Select(x => new object[] {
                x.IsBalanceClear,
                x.IsHardClear,
                x.IsDotGabi1Clear,
                x.IsDotGabi2Clear,
                x.IsDotGabi3Clear,
                x.IsDotGabi4Clear,
                x.IsDotGabi5Clear
            })
            .Where(x => x.AuthId == auth_id)
            .Get();

        var userScenarioClear = response.Models;

        return new UserScenarioClearDTO {
            IsBalanceClear = userScenarioClear[0].IsBalanceClear,
            IsHardClear = userScenarioClear[0].IsHardClear,
            IsDotGabi1Clear = userScenarioClear[0].IsDotGabi1Clear,
            IsDotGabi2Clear = userScenarioClear[0].IsDotGabi2Clear,
            IsDotGabi3Clear = userScenarioClear[0].IsDotGabi3Clear,
            IsDotGabi4Clear = userScenarioClear[0].IsDotGabi4Clear,
            IsDotGabi5Clear = userScenarioClear[0].IsDotGabi5Clear
        };
    }
}
