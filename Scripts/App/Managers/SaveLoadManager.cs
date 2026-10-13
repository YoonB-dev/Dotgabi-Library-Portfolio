using System.Threading.Tasks;
using UnityEngine;

public class SaveLoadManager : Singleton<SaveLoadManager>
{
    public async Task<bool> LoadMainScenarioData()
    {
        var client = SupabaseClientProvider.Instance.Client;
        var user = client.Auth.CurrentUser;
        var response = await UserMainScenarioDAO.Instance.GetUserMainScenarioDTO(user.Id);
        if (response == null)
        {
            Debug.LogError("Failed to load main scenario data.");
            return false;
        }
        UserData.Instance.MainScenarioData = response;
        return true;

    }

    public void ResetGameData()
    {
        // Implement reset logic here
        Debug.Log("Game data reset.");
    }

}
