using UnityEngine;

public class EnemyTextEvidenceAction : Singleton<EnemyTextEvidenceAction>
{
    public async void StartEvidence()
    {
        var cards = UserData.Instance.MainScenarioData.OwnedStoryCardList;
        Debug.Log("Owned Cards Count: " + cards.Count);
        for (int i = 0; i < cards.Count; i++)
        {
            var itemDTO = InGameData.Instance.MainStoryItems.Find(x => x.Id == cards[i].CardId);
            StoryCardManager.Instance?.AddCard(itemDTO);
            // 시간 지연
            await System.Threading.Tasks.Task.Delay(300);
        }

        if (cards.Count == 0)
        {
            Debug.Log("No owned story cards to show.");
            EnemyStoryManager.Instance?.NoCardPursuadeFail();
        }
    }
}
