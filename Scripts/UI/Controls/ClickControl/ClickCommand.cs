using UnityEngine;

public abstract class ClickCommand : MonoBehaviour
{
    abstract public void Execute();
    public void ClickIn()
    {
        // 마우스 클릭 위치 저장
        ClickChecker.Instance.SetMouseDownPos();
    }
    public void ClickOut()
    {
        // 마우스 클릭 위치 저장
        ClickChecker.Instance.SetMouseUpPos();
    }
}
