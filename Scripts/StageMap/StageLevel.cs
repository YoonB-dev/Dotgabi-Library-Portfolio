using System.Collections.Generic;
using UnityEngine;

public class StageLevel
{
    public int LevelIndex { get; set; }
    public List<StageNode> StageNodes { get; set; } = new List<StageNode>();
}
