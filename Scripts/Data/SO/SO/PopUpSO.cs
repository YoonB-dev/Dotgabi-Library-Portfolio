using UnityEngine;

[CreateAssetMenu(menuName = "Popup/PopupSO")]
public class PopUpSO : ScriptableObject
{
    public EnumTypes.PopupType popupType;
    public GameObject content;
    public Sprite backgroundImage;
}
