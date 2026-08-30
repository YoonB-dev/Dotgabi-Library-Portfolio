using UnityEngine;

public class ClickChecker : Singleton<ClickChecker>
{
    [SerializeField] private Vector2 mouseDownPos, mouseUpPos;
    public void SetMouseDownPos()
    {
        mouseDownPos = Input.mousePosition;
    }
    public void SetMouseUpPos()
    {
        mouseUpPos = Input.mousePosition;
    }
    public bool CheckMousePos()
    {
        return Vector2.Distance(mouseDownPos, mouseUpPos) < 10 ? true : false;
    }
}
