using System.Collections.Generic;
using UnityEngine;

public class SupabaseMainScenarioStoryUpdate : Singleton<SupabaseMainScenarioStoryUpdate>
{
    public async void UpdateMainScenarioStoryClearData(EnumTypes.MainStoryType storyName, bool isClear)
    {
        var client = SupabaseClientProvider.Instance.Client;

        var response = await SupabaseWrap.ExecuteWithRefresh(() => client.Rpc("update_user_main_scenario_story_clear", new Dictionary<string, object> {
            { "p_column_name", storyName.ToString() },
            { "p_value", isClear }
        }));

        UserData.Instance.MainScenarioData.StoryClearData = await UserMainScenarioDAO.Instance.GetUserMainscenarioStoryClearDataAsync(UserData.Instance.UserAuthId);
    }

    public async void InsertMainScenarioStoryOwnedCard(List<int> cardIds)
    {
        var client = SupabaseClientProvider.Instance.Client;

        var response = await SupabaseWrap.ExecuteWithRefresh(() => client.Rpc("insert_user_main_scenario_story_card", new Dictionary<string, object> {
            { "p_card_ids", cardIds.ToArray() }
        }));


        UserData.Instance.MainScenarioData.OwnedStoryCardList = await UserMainScenarioStoryCardDAO.Instance.GetUserMainscenarioStoryCardAsync(UserData.Instance.UserAuthId);
    }
}
