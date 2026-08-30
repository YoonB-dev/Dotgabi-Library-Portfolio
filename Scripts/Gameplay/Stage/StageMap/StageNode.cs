using System.Collections.Generic;
using UnityEngine;

public class StageNode
{
    public EnumTypes.StageType NodeType { get; set; } // 노드가 어떤 종류인지, 전투, 상점 등등
    public int NodeLevel { get; set; } // 노드가 몇번째 레벨에 존재하는지
    public int NodeIndex { get; set; } // 노드가 해당 레벨에서 몇번째 노드인지
    public List<StageNode> NextNodes { get; set; } = new List<StageNode>();
}
