using UnityEngine;

public class FooterButtons : MonoBehaviour
{
    public void OnClickCardButton()
    {
        PopupManager.Instance.ShowPopup(EnumTypes.PopupType.Card, isCollection: false);
    }
}
