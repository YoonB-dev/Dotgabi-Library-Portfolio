using System.Collections.Generic;
using UnityEngine;

public class SupabaseAchieve : Singleton<SupabaseAchieve>
{
    public async void AchieveCurrData(EnumTypes.AchieveType achieveType, int value)
    {
        await SupabaseClientProvider.Instance.InitializeAsync();
        var client = SupabaseClientProvider.Instance.Client;

        var response = await SupabaseWrap.ExecuteWithRefresh(() =>
            client.Rpc("update_user_achieve_curr_data", new Dictionary<string, object>
            {
                {"p_attribute_name", $"{achieveType}"},
                {"p_value", value}
            })
        );

        Debug.Log($"LogUserAction: {response}");
    }
}
