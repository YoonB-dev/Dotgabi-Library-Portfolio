using UnityEngine;

public class StoryCardUseOnu : SceneSingleton<StoryCardUseOnu>, IStoryCardUse
{
    public bool OnCardUse(StoryCard card)
    {
        var onuManager = OnuManager.Instance;
        if (onuManager == null)
        {
            Debug.LogError("OnuManager instance not found.");
            return false;
        }

        // 나무
        if (onuManager.currIndex == -1 && card.storyCardData.Id == 2)
        {
            onuManager.SetTreeAxe();
            return true;
        }

        // 우물
        if (onuManager.currIndex == 1 && card.storyCardData.Id == 3 && !onuManager.isWellEmpty)
        {
            onuManager.SetWellEmpty();
            return true;
        }


        return false;
    }
}
