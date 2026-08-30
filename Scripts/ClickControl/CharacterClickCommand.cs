using UnityEngine;

public class CharacterClickCommand : ClickCommand
{
    public override void Execute()
    {
        // 마우스 클릭 위치 확인 (드래그인지 확인)
        if (ClickChecker.Instance.CheckMousePos())
        {
            // 실행
            PopupSceneManager.Instance.ShowPopup(EnumTypes.PopupType.Character);
        }
    }
}
