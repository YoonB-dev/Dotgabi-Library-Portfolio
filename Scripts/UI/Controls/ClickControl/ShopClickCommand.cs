using UnityEngine;

public class ShopClickCommand : ClickCommand
{
    public override void Execute()
    {
        PopupSceneManager.Instance.ShowPopup(EnumTypes.PopupType.ShopItem);
    }
}
