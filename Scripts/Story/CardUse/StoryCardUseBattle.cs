using UnityEngine;

public class StoryCardUseBattle : SceneSingleton<StoryCardUseBattle> , IStoryCardUse
{
    // Card Use in Battle
    public bool OnCardUse(StoryCard card)
    {
        string table = "StoryTable";
        var cardData = card.storyCardData;

        if (cardData.ExtraData == null)
        {
            Debug.LogWarning("No ExtraData found in cardData: " + cardData.Name);
            return false;
        }

        if (cardData.ExtraData.ContainsKey("text"))
        {
            string key = cardData.ExtraData["text"].ToString();
            string text = LocalString.Instance.GetLocalizedString(key, table);
            EnemyText.Instance?.SetTextBoxText(text);
        }

        if (cardData.ExtraData.ContainsKey("pursuade"))
        {
            int pursuadeValue = int.Parse(cardData.ExtraData["pursuade"].ToString());
            EnemyStoryManager.Instance.GetPursuadePoint(pursuadeValue);
        }

        return true;
    }
}
