using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class AuthService : MonoBehaviorSingleton<AuthService>
{
    private const string AccessTokenKey = "accessToken";
    private const string RefreshTokenKey = "refreshToken";
    private static SynchronizationContext MainThreadContext;

    private void Start()
    {
        if (MainThreadContext == null) MainThreadContext = SynchronizationContext.Current;
    }

    private Task<T> RunOnMainThread<T>(Func<Task<T>> func)
    {
        if (MainThreadContext == null) return func();
        var tcs = new TaskCompletionSource<T>();
        MainThreadContext.Post(async _ => {
            try
            {
                var res = await func();
                tcs.SetResult(res);
            }
            catch (Exception e)
            {
                tcs.SetException(e);
            }
        }, null);
        return tcs.Task;
    }

    public void CacheSession(string accessToken, string refreshToken, string email = null)
    {
        if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
        {
            Debug.LogError("Access token or refresh token is null or empty.");
            return;
        }

        PlayerPrefs.SetString(AccessTokenKey, accessToken);
        PlayerPrefs.SetString(RefreshTokenKey, refreshToken);

        if (!string.IsNullOrEmpty(email))
        {
            PlayerPrefs.SetString("VirtualEmail", email);
        }
        PlayerPrefs.Save();
    }

    private async Task SetSessionOnAllClients(string accessToken, string refreshToken)
    {
        try
        {
            var client = SupabaseClientProvider.Instance.Client;
            var clientGameData = SupabaseClientProvider.Instance.ClientGameData;
            if (client != null) await client.Auth.SetSession(accessToken, refreshToken);
            if (clientGameData != null) await clientGameData.Auth.SetSession(accessToken, refreshToken);
            Debug.Log("SetSessionOnAllClients: sessions set for both clients");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"SetSessionOnAllClients failed: {e}");
        }
    }

    public void ClearSession()
    {
        PlayerPrefs.DeleteKey(AccessTokenKey);
        PlayerPrefs.DeleteKey(RefreshTokenKey);
        PlayerPrefs.DeleteKey("VirtualEmail");
        PlayerPrefs.Save();
        Debug.Log("Session cleared.");
    }

    public async Task<bool> TryAutoLoginAsync()
    {
        if (!PlayerPrefs.HasKey(AccessTokenKey) || !PlayerPrefs.HasKey(RefreshTokenKey))
        {
            Debug.Log("No session found, skipping auto-login.");
            return false;
        }

        var client = SupabaseClientProvider.Instance.Client;
        var clientGameData = SupabaseClientProvider.Instance.ClientGameData;
        var accessToken = PlayerPrefs.GetString(AccessTokenKey);
        var refreshToken = PlayerPrefs.GetString(RefreshTokenKey);

        Debug.Log("Attempting auto-login with cached session.");

        try
        {
            var restoredSession = await client.Auth.SetSession(accessToken, refreshToken);
            var restoredGameSession = await clientGameData.Auth.SetSession(accessToken, refreshToken);

            if (restoredSession != null && restoredGameSession != null)
            {
                CacheSession(restoredSession.AccessToken, restoredSession.RefreshToken);
                await SetSessionOnAllClients(restoredSession.AccessToken, restoredSession.RefreshToken);
                if (await RunOnMainThread(() => UserManager.Instance.SetSession(restoredSession)))
                {
                    Debug.Log("Auto session login success.");
                    return true;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Auto session login failed: {e}");
        }

        if (await TryLoginWithVirtualEmail())
        {
            var virtualEmail = PlayerPrefs.GetString("VirtualEmail");
            var password = GenerateEmail.Instance.GenerateVirtualPassWord();
            var signInResult = await client.Auth.SignInWithPassword(email: virtualEmail, password: password);
            if (signInResult != null)
            {
                CacheSession(signInResult.AccessToken, signInResult.RefreshToken, virtualEmail);
                Debug.Log("Virtual email login success.");
                return true;
            }
        }

        return false;
    }

    public async Task<bool> TryLoginWithVirtualEmail()
    {
        var client = SupabaseClientProvider.Instance.Client;
        string virtualEmail = PlayerPrefs.HasKey("VirtualEmail") ? PlayerPrefs.GetString("VirtualEmail") : GenerateEmail.Instance.GenerateVirtualEmail();

        if (!PlayerPrefs.HasKey("VirtualEmail"))
        {
            PlayerPrefs.SetString("VirtualEmail", virtualEmail);
            PlayerPrefs.Save();
        }

        try
        {
            string virtualPassword = GenerateEmail.Instance.GenerateVirtualPassWord();
            var signInResult = await client.Auth.SignInWithPassword(email: virtualEmail, password: virtualPassword);

            if (signInResult != null)
            {
                CacheSession(signInResult.AccessToken, signInResult.RefreshToken);
                await SetSessionOnAllClients(signInResult.AccessToken, signInResult.RefreshToken);

                bool setSessionSuccess = await RunOnMainThread(() => UserManager.Instance.SetSession(signInResult));
                Debug.Log("Virtual email session restored successfully.");

                if (setSessionSuccess)
                {
                    Debug.Log("Auto-login succeeded with generated virtual email.");
                    return true;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"가상 이메일 로그인 실패: {e}");
        }

        return false;
    }

    public async Task<bool> TryCreateUser()
    {
        string virtualEmail = GenerateEmail.Instance.GenerateVirtualEmail();
        string virtualPassword = GenerateEmail.Instance.GenerateVirtualPassWord();

        if (!await TrySignUpAccount(virtualEmail, virtualPassword))
        {
            Debug.LogWarning("Virtual email sign-up failed, try login.");
            return await TryLoginAccount(virtualEmail, virtualPassword);
        }

        return true;
    }

    private async Task<bool> TrySignUpAccount(string email, string password)
    {
        var client = SupabaseClientProvider.Instance.Client;
        try
        {
            var signUpResult = await client.Auth.SignUp(email: email, password: password);

            if (signUpResult == null || signUpResult.User == null)
            {
                Debug.LogWarning("SignUp 실패 또는 반환 null");
                // 실패 시 로그인 시도
                return await TryLoginAccount(email, password);
            }

            CacheSession(signUpResult.AccessToken, signUpResult.RefreshToken, signUpResult.User.Email);

            // Ensure both clients have the session before creating user record and loading data
            await SetSessionOnAllClients(signUpResult.AccessToken, signUpResult.RefreshToken);

            var userDTO = await UserManager.Instance.CreateUserAsync(signUpResult.User.Id, signUpResult.User.Email);

            if (userDTO == null)
            {
                Debug.LogError("Failed to create user record in DB after sign-up");
                return false;
            }

            // 생성한 userDTO를 직접 전달하여 DB 조회 타이밍 이슈 방지
            if (await RunOnMainThread(() => UserManager.Instance.SetSession(signUpResult, userDTO)))
            {
                Debug.Log($"Virtual email user created: {signUpResult.User.Email}");
            }
            else
            {
                Debug.LogError("Failed to set UserManager session");
            }

        }
        catch (Exception ex)
        {
            Debug.LogError($"Virtual email user creation failed: {ex}");
        }
        return false;
    }

    private async Task<bool> TryLoginAccount(string email, string password)
    {
        var client = SupabaseClientProvider.Instance.Client;
        try
        {
            var signInResult = await client.Auth.SignInWithPassword(email: email, password: password);

            if (signInResult != null)
            {
                CacheSession(signInResult.AccessToken, signInResult.RefreshToken, signInResult.User.Email);
                await SetSessionOnAllClients(signInResult.AccessToken, signInResult.RefreshToken);
                bool loginSuccess = await RunOnMainThread(() => UserManager.Instance.SetSession(signInResult));
                if (loginSuccess)
                {
                    Debug.Log($"Email login success: {signInResult.User.Email}");
                    return true;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Guest account login failed: {e}");
        }

        return false;
    }

    public async Task<bool> RefreshAccessToken()
    {
        try
        {
            var newSession = await SupabaseClientProvider.Instance.Client.Auth.RefreshSession();
            if (newSession != null)
            {
                CacheSession(newSession.AccessToken, newSession.RefreshToken);
                return true;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Token refresh failed: {e}");
        }
        return false;
    }
    public async Task<bool> CreateUserAndAuthenticate(string email, string password)
    {
        var client = SupabaseClientProvider.Instance.Client;
        try
        {
            var signUpResult = await client.Auth.SignUp(email: email, password: password);
            if (signUpResult?.User != null)
            {
                CacheSession(signUpResult.AccessToken, signUpResult.RefreshToken, email);

                // 먼저 사용자 DB 생성 (ensure sessions applied to clients)
                await SetSessionOnAllClients(signUpResult.AccessToken, signUpResult.RefreshToken);
                var newUser = await UserManager.Instance.CreateUserAsync(signUpResult.User.Id, email);
                if (newUser == null)
                {
                    Debug.LogError("Failed to create user record in DB");
                    return false;
                }

                // 생성한 newUser를 직접 전달하여 DB 조회 타이밍 이슈 방지
                bool sessionSet = await RunOnMainThread(() => UserManager.Instance.SetSession(signUpResult, newUser));
                if (!sessionSet)
                {
                    Debug.LogError("Failed to set session after user creation");
                    return false;
                }

                Debug.Log($"Virtual email user created and session set: {signUpResult.User.Email}");
                return true;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Sign up failed: {e}");
        }
        return false;
    }

    public async Task<bool> AuthenticateAndSetSession(string email, string password)
    {
        var client = SupabaseClientProvider.Instance.Client;
        try
        {
            var signInResult = await client.Auth.SignInWithPassword(email: email, password: password);
            if (signInResult?.User != null)
            {
                CacheSession(signInResult.AccessToken, signInResult.RefreshToken, signInResult.User.Email);

                await SetSessionOnAllClients(signInResult.AccessToken, signInResult.RefreshToken);
                bool sessionSet = await RunOnMainThread(() => UserManager.Instance.SetSession(signInResult));
                if (!sessionSet)
                {
                    Debug.LogWarning("User session set 실패, 사용자 데이터 로드 실패");
                    return false;
                }
                return true;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Authenticate failed: {ex}");
        }
        return false;
    }
}
