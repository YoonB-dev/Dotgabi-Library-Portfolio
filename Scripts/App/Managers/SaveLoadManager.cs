using System.Threading.Tasks;
using UnityEngine;

public class SaveLoadManager : Singleton<SaveLoadManager>
{
    public async Task<bool> SaveGameData()
    {
        var client = SupabaseClientProvider.Instance.Client;
        var user = client.Auth.CurrentUser;
        UserMainScenarioEntity userData = new UserMainScenarioEntity {
            selectList = UserData.Instance.MainScenarioData.SelectList,
            eventClear = UserData.Instance.MainScenarioData.EventClear,
            nextEvent = UserData.Instance.MainScenarioData.NextEvent,
            gameCoins = UserData.Instance.MainScenarioData.GameCoins,
            totalGameCoins = UserData.Instance.MainScenarioData.TotalGameCoins,
            currHp = UserData.Instance.MainScenarioData.CurrHp,
            maxHp = UserData.Instance.MainScenarioData.MaxHp,
            firstPiece = UserData.Instance.MainScenarioData.FirstPiece,
            secondPiece = UserData.Instance.MainScenarioData.SecondPiece,
            thirdPiece = UserData.Instance.MainScenarioData.ThirdPiece,
        };

        await client
            .From<UserMainScenarioEntity>()
            .Where(x => x.userAuthId == user.Id)
            .Update(userData);

        if (client != null)
        {
            Debug.LogError($"Error saving game data: {client}");
            return false;
        }

        return true;
    }

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
