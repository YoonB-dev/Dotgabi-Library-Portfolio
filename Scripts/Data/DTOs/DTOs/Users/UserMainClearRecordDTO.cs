using System.Collections.Generic;
using UnityEngine;

public class UserMainClearRecordDTO
{
    public int Version { get; set; }
    public int ClearTime { get; set; }
    public UserMainScenarioDTO ScenarioData { get; set; }
    public List<UserScenarioOwnedCardDTO> CardList { get; set; }
    public List<UserScenarioOwnedArtifactDTO> ArtifactList { get; set; }
    public List<UserScenarioLogDTO> LogList { get; set; }
    public UserMainscenarioStoryClearDTO StoryList { get; set; }
}
