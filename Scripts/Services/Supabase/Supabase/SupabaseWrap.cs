using System;
using System.Threading.Tasks;
using Supabase.Postgrest.Exceptions;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;

public class SupabaseWrap
{
    public static async Task<T> ExecuteWithRefresh<T>(Func<Task<T>> apiFunc)
    {
        try
        {
            return await apiFunc();
        }
        catch (PostgrestException ex)
        {
            Debug.LogError($"Supabase API error: {ex.Message}");
            if (ex.Message.Contains("JWT expired"))
            {
                Debug.LogWarning("Access token expired, attempting to refresh...");
                bool refreshed = await AuthService.Instance.RefreshAccessToken();
                if (refreshed)
                {
                    return await apiFunc();  // 재시도
                }
                else
                {
                    SceneManager.LoadScene("StartScene");
                    var message = new LocalizedString("LocalTable", "session_error").GetLocalizedString();
                    NotificationManager.Instance.SetShownNotification(message);
                    throw new Exception("Session expired. Please log in again.");
                }
            }
            throw;
        }
    }
}
