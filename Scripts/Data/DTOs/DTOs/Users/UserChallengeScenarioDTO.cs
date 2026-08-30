using System.Collections.Generic;
using UnityEngine;

public class UserChallengeScenarioDTO : ScenarioDTO
{
    public bool IsNextEnemyStory { get; set; }
    public bool IsEliteClear { get; set; }
}

[System.Serializable]
public class UserChallengeScenarioOwnedArtifactDTO
{
    public int ArtifactId { get; set; }
    public bool IsUse { get; set; }
}

[System.Serializable]
public class UserChallengeScenarioOwnedCardDTO
{
    public int OwnedId { get; set; }
    public int CardId { get; set; }
    public int UpgradeTime { get; set; }
}

