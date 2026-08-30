using System;
using System.Linq;
using System.Threading.Tasks;
using Supabase.Gotrue;
using UnityEngine;

public class UserManager : Singleton<UserManager>
{
    public string AuthId { get; private set; }
    public UserDTO User { get; set; }

    public async Task<bool> SetSession(Session session, UserDTO existingUser = null)
    {
        if (session?.User == null)
        {
            Debug.LogError("SetSession: session or session.User is null");
            return false;
        }

        AuthId = session.User.Id;
        Debug.Log($"SetSession 호출됨, AuthId 할당됨: {AuthId}");

        // existingUser가 있으면 그대로 사용, 없으면 DB에서 가져오기
        if (existingUser != null)
        {
            Debug.Log($"SetSession: Using provided UserDTO for AuthId={AuthId}");
            User = existingUser;
        } else
        {
            User = await UserDAO.Instance.GetUserAsync(AuthId);
            if (User == null)
            {
                Debug.LogError($"SetSession: User data not found for AuthId={AuthId}");
                return false;
            }
        }

        // Cache session information
        AuthService.Instance.CacheSession(session.AccessToken, session.RefreshToken, User.Email);

        // Update user data singleton
        UserData.Instance.UserAuthId = AuthId;
        UserData.Instance.AchievePoint = User.UserGoods?.AchievePoint ?? 0;
        UserData.Instance.AdPoint = User.UserGoods?.AdPoint ?? 0;

        UserData.Instance.SelectCardFrameId = User.SelectCardFrameId ?? 1;
        UserData.Instance.SelectDecoId = User.SelectCardDecoId ?? 2;

        UserData.Instance.istutorialCompleted = User.IsTutorial;

        GameData.Instance.CurrScenarioType = User.CurrScenarioType;

        return true;
    }

    public void Clear()
    {
        AuthId = null;
    }

    public async Task<UserDTO> CreateUserAsync(string authId, string email)
    {
        Debug.Log($"Creating new user: AuthId={authId}, Email={email}");
        var newUser = new UserEntity {
            AuthId = authId,
            Email = email,
            LastLoginAt = DateTime.UtcNow
        };

        var client = SupabaseClientProvider.Instance.Client;
        var response = await client
            .From<UserEntity>()
            .Insert(newUser);

        if (response?.Models == null || response.Models.Count == 0)
        {
            Debug.LogError("CreateUserAsync: Failed to insert new user.");
            return null;
        }

        UserData.Instance.UserAuthId = response.Models[0].AuthId;
        UserData.Instance.AchievePoint = 0; // initial AchievePoint
        UserData.Instance.AdPoint = 0; // initial AdPoint

        return await UserMapper.Instance.ToDTO(response.Models[0]);
    }
}

