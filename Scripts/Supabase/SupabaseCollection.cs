using System.Collections.Generic;
using UnityEngine;

public class SupabaseCollection : Singleton<SupabaseCollection>
{
    // 카드 도감 추가
    public async void AddCardCollection(int cardId)
    {
        var existingCard = UserData.Instance.UserOwnedCardList.Find(x => x.CardId == cardId);
        if (existingCard == null)
        {
            var client = SupabaseClientProvider.Instance.Client;

            var response = await SupabaseWrap.ExecuteWithRefresh(() => client.Rpc("insert_user_collection_card", new Dictionary<string, object> {
                { "p_card_id", cardId }
            }));

            if (response == null)
            {
                Debug.LogError("UserMainScenarioEntity not found for user: " + client.Auth.CurrentUser.Id);
                return;
            }
            UserData.Instance.UserOwnedCardList.Add(new UserOwnedCardDataDTO { CardId = cardId });
        }
        else
        {
            // 이미 존재
        }
    }

    public async void AddArtifactCollection(int artifactId)
    {
        var existingArtifact = UserData.Instance.UserOwnedArtifactList.Find(x => x.ArtifactId == artifactId);
        if (existingArtifact == null)
        {
            var client = SupabaseClientProvider.Instance.Client;

            var response = await SupabaseWrap.ExecuteWithRefresh(() => client.Rpc("insert_user_collection_artifact", new Dictionary<string, object> {
                { "p_artifact_id", artifactId }
            }));

            if (response == null)
            {
                Debug.LogError("UserMainScenarioEntity not found for user: " + client.Auth.CurrentUser.Id);
                return;
            }
            UserData.Instance.UserOwnedArtifactList.Add(new UserOwnedArtifactDataDTO { ArtifactId = artifactId });
        }
        else
        {
            // 이미 존재
        }
    }
}
