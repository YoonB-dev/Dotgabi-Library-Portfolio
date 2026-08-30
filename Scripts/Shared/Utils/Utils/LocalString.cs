using UnityEngine;
using UnityEngine.Localization;

public class LocalString : Singleton<LocalString>
{
    public string GetLocalizedString(string key, string table = "LocalTable")
    {
        string textMoney = new LocalizedString(table, key).GetLocalizedString();
        return textMoney;
    }
}

