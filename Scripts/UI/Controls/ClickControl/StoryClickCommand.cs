using UnityEngine;

public class StoryClickCommand : ClickCommand
{
    public override void Execute()
    {
        PopupSceneManager.Instance.ShowPopup(EnumTypes.PopupType.Story);
    }
}
