using UnityEngine;

public class SelectLevelUtil : Singleton<SelectLevelUtil>
{
    public int GetMainClearLevel()
    {
        if (UserData.Instance.MainScenarioClear.IsDotGabi5Clear)
        {
            return 7;
        } else if (UserData.Instance.MainScenarioClear.IsDotGabi4Clear)
        {
            return 6;
        } else if (UserData.Instance.MainScenarioClear.IsDotGabi3Clear)
        {
            return 5;
        } else if (UserData.Instance.MainScenarioClear.IsDotGabi2Clear)
        {
            return 4;
        } else if (UserData.Instance.MainScenarioClear.IsDotGabi1Clear)
        {
            return 3;
        } else if (UserData.Instance.MainScenarioClear.IsHardClear)
        {
            return 3;
        } else if (UserData.Instance.MainScenarioClear.IsBalanceClear)
        {
            return 3;
        } else
        {
            return 3; // No levels cleared
        }
    }
    public int GetMainLevelCount()
    {
        return 7;
    }
}
