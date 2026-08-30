using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UserMainScenarioDTO : ScenarioDTO
{
    public bool FirstPiece { get; set; }
    public bool SecondPiece { get; set; }
    public bool ThirdPiece { get; set; }
    public EnumTypes.Difficulty Difficulty { get; set; }
    public bool IsNextEnemyStory { get; set; }
    public bool IsEliteClear { get; set; }

    public UserMainscenarioStoryClearDTO StoryClearData { get; set; } = new();
    public List<UserMainScenarioStoryCardDTO> OwnedStoryCardList { get; set; } = new ();
}



[System.Serializable]
public class UserMainscenarioStoryClearDTO
{
    public bool CrimeSceneClear { get; set; }
    public bool OnuHouseClear { get; set; }
    public bool? TigerArrest { get; set; }
    public bool? OnuTrust { get; set; }
}
