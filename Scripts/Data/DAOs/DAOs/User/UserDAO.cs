using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using UnityEngine;

public class UserDAO : Singleton<UserDAO>
{
    /// <summary>
    /// 사용자 정보를 가져옵니다. 사용자가 없으면 null을 반환합니다.
    /// </summary>
    public async Task<UserDTO> GetUserAsync(string authId)
    {
        // 사용자 id를 이용해 정보를 가져오기
        var user = await SupabaseClientProvider.Instance.Client
            .From<UserEntity>()
            .Where(x => x.AuthId == authId)
            .Single();


        if (user == null)
        {
            Debug.LogWarning($"사용자 {authId} 를 찾을 수 없습니다.");
            return null;
        }

        if (user.IsBanned)
        {
            Debug.LogWarning($"사용자 {authId} 는 차단되었습니다.");
            return null;
        }

        user.LastLoginAt = DateTime.UtcNow;
        await user.Update<UserEntity>();

        return await UserMapper.Instance.ToDTO(user);
    }
    /// <summary>
    /// 사용자 정보를 email을 이용해 가져옵니다. 사용자가 없으면 null을 반환합니다.
    /// </summary>
    public async Task<UserDTO> TryGetUserByEmail(string email)
    {
        // 임시 익명 사용자 정보 생성
        var client = SupabaseClientProvider.Instance.Client;

        // 기기 ID로 사용자 정보를 가져옵니다.
        var user = await client
            .From<UserEntity>()
            .Where(x => x.Email == email)
            .Single();

        if (user == null)
        {
            Debug.LogWarning($"기기 ID {email}에 해당하는 사용자를 찾을 수 없습니다.");
            return null;
        }
        if (user.IsBanned)
        {
            Debug.LogWarning($"기기 ID {email}에 해당하는 사용자는 차단되었습니다.");
            return null;
        }

        Debug.Log($"기기 ID로 사용자 정보 복구 성공: {user.Email}");
        user.LastLoginAt = DateTime.UtcNow;
        await user.Update<UserEntity>();
        return await UserMapper.Instance.ToDTO(user);
    }

    public async Task UpdateUserTutorial()
    {
        var client = SupabaseClientProvider.Instance.Client;
        var userEntity = await client
            .From<UserEntity>()
            .Where(x => x.AuthId == UserData.Instance.UserAuthId)
            .Single();

        userEntity.IsTutorial = true;
        await userEntity.Update<UserEntity>();
    }
}

