using UnityEngine;

public class AchievementClickCommand : ClickCommand
{
    public override void Execute()
    {
        PopupSceneManager.Instance.ShowPopup(EnumTypes.PopupType.Achieve);
    }
}
