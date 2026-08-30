using UnityEngine;

public class CardClickCommand : ClickCommand
{
    public override void Execute()
    {
        // 마우스 클릭 위치 확인 (드래그인지 확인)
        if (ClickChecker.Instance.CheckMousePos())
        {
            // 실행
            PopupManager.Instance.ShowPopup(EnumTypes.PopupType.Card);
            // 로그
            var logText = LogManager.Instance?.GetMainLogText("collection_card_open");
            LogManager.Instance?.AddLogMain(logText);
        }
    }
}
