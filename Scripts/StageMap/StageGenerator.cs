using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StageGenerator
{

    private StageMap map = new StageMap();
    public Dictionary<EnumTypes.StageType, int> typeCounts = new Dictionary<EnumTypes.StageType, int> {
        { EnumTypes.StageType.enemy, 13 },
        { EnumTypes.StageType.shop, 2 },
        { EnumTypes.StageType.artifact, 2 },
        { EnumTypes.StageType.mystery, 7 }
    };

    public Dictionary<EnumTypes.StageType, int> bossMap = new Dictionary<EnumTypes.StageType, int> {
        { EnumTypes.StageType.rest, 3 },
    };

    public StageMap Generate(int mapSeed, int levelCount, Dictionary<EnumTypes.StageType, int> typeCounts, bool isElite = true)
    {
        map = new StageMap();
        var rand = new System.Random(mapSeed);

        int repeatTime = isElite ? 2 : 1;

        // 1. 전체 타입 리스트 생성 및 셔플
        for (int t = 0; t < repeatTime; t++)
        {
            var allTypes = new List<EnumTypes.StageType>();
            foreach (var kv in typeCounts)
                for (int i = 0; i < kv.Value; i++)
                    allTypes.Add(kv.Key);
            allTypes = allTypes.OrderBy(_ => rand.Next()).ToList();

            // 2. 각 레벨에 기본 노드 개수 개수 할당
            int baseCount = allTypes.Count / levelCount;
            int remainder = allTypes.Count % levelCount;
            var nodesPerLevel = Enumerable.Repeat(baseCount, levelCount).ToArray();

            // 3. 나머지를 랜덤하게 분배
            var indices = Enumerable.Range(0, levelCount).ToList();
            indices = indices.OrderBy(_ => rand.Next()).ToList(); // 시드 기반 랜덤 셔플
            for (int i = 0; i < remainder; i++)
            {
                nodesPerLevel[indices[i]] += 1;
            }

            // 4. 노드에 타입 순서대로 할당
            int typeIdx = 0;
            for (int level = 0; level < levelCount; level++)
            {
                int nodeCount = nodesPerLevel[level];
                var stageLevel = new StageLevel { LevelIndex = map.StageLevels.Count, StageNodes = new List<StageNode>() };
                for (int i = 0; i < nodeCount; i++)
                {
                    var node = new StageNode {
                        NodeLevel = map.StageLevels.Count,
                        NodeIndex = i,
                        NodeType = allTypes[typeIdx++]
                    };
                    stageLevel.StageNodes.Add(node);
                }
                map.StageLevels.Add(stageLevel);
            }

            // 4-1. 휴식 노드 추가
            if (isElite)
            {
                var restNode = new StageNode {
                    NodeLevel = map.StageLevels.Count,
                    NodeIndex = 0,
                    NodeType = EnumTypes.StageType.rest
                };
                map.StageLevels.Add(new StageLevel
                {
                    LevelIndex = map.StageLevels.Count,
                    StageNodes = new List<StageNode> { restNode }
                });
            }


            // 4-2. 중간보스, 보스 노드 추가
            if (isElite)
            {
                if (t == 0)
                {
                    // 중간보스 노드 추가
                    var bossNode = new StageNode {
                        NodeLevel = map.StageLevels.Count,
                        NodeIndex = 0,
                        NodeType = EnumTypes.StageType.elite
                    };
                    map.StageLevels.Add(new StageLevel
                    {
                        LevelIndex = map.StageLevels.Count,
                        StageNodes = new List<StageNode> { bossNode }
                    });
                }
                else
                {
                    // 보스 노드 추가
                    var bossNode = new StageNode {
                        NodeLevel = map.StageLevels.Count,
                        NodeIndex = 0,
                        NodeType = EnumTypes.StageType.boss
                    };
                    map.StageLevels.Add(new StageLevel
                    {
                        LevelIndex = map.StageLevels.Count,
                        StageNodes = new List<StageNode> { bossNode }
                    });
                }
            }
            else
            {
                // 일반 맵 보스 노드 추가
                var bossNode = new StageNode {
                    NodeLevel = map.StageLevels.Count,
                    NodeIndex = 0,
                    NodeType = EnumTypes.StageType.boss
                };
                map.StageLevels.Add(new StageLevel
                {
                    LevelIndex = map.StageLevels.Count,
                    StageNodes = new List<StageNode> { bossNode }
                });
            }

        }
        Debug.Log(map.StageLevels.Count);
        MoveSystem.Instance.SCENARIO_DATA.StageMapData = map;
        Debug.Log("StageMap generated with " + map.StageLevels.Count + " levels.");


        // 5. 각 레벨의 노드에 다음 노드 연결 설정
        for (int level = 0; level < map.StageLevels.Count - 1; level++)
        {
            var currLevel = map.StageLevels[level];
            var nextLevel = map.StageLevels[level + 1];

            int currCount = currLevel.StageNodes.Count;
            int nextCount = nextLevel.StageNodes.Count;

            bool[] nextConnected = new bool[nextCount]; // 상위 레벨 노드가 연결되었는지 여부를 추적하는 배열

            // 5-1. 현재 양 끝 노드는 상위 레벨의 양 끝 노드와 연결
            currLevel.StageNodes[0].NextNodes.Add(nextLevel.StageNodes[0]);
            nextConnected[0] = true;
            currLevel.StageNodes[currCount - 1].NextNodes.Add(nextLevel.StageNodes[nextCount - 1]);
            nextConnected[nextCount - 1] = true;

            // 중간 노드 연결
            for (int i = 1; i < currCount - 1; i++)
            {
                var node = currLevel.StageNodes[i];
                List<int> possible = new List<int>();
                if (i - 1 >= 0 && i - 1 < nextCount) possible.Add(i - 1);
                if (i < nextCount) possible.Add(i);
                if (i + 1 < nextCount) possible.Add(i + 1);

                int connectCount = rand.Next(1, Math.Min(possible.Count, 3) + 1);
                var shuffled = possible.OrderBy(_ => rand.Next()).ToList();
                for (int j = 0; j < connectCount; j++)
                {
                    node.NextNodes.Add(nextLevel.StageNodes[shuffled[j]]);
                    nextConnected[shuffled[j]] = true;
                }
            }

            // 연결이 없는 nextLevel 노드 보장
            for (int idx = 0; idx < nextCount; idx++)
            {
                if (!nextConnected[idx])
                {
                    List<int> candidates = new List<int>();
                    if (idx - 1 >= 0 && idx - 1 < currCount) candidates.Add(idx - 1);
                    if (idx < currCount) candidates.Add(idx);
                    if (idx + 1 < currCount) candidates.Add(idx + 1);

                    int connectCount = rand.Next(1, Math.Min(2, candidates.Count) + 1); // 1 또는 2개
                    var shuffled = candidates.OrderBy(_ => rand.Next()).ToList();
                    for (int i = 0; i < connectCount; i++)
                    {
                        int lowerIdx = shuffled[i];
                        currLevel.StageNodes[lowerIdx].NextNodes.Add(nextLevel.StageNodes[idx]);
                        nextConnected[idx] = true;
                    }
                }
            }
        }

        Debug.Log("NextNodes 연결 완료");

        // nodeIndex에 맞게 NextNode 정렬
        foreach (var level in map.StageLevels)
        {
            foreach (var node in level.StageNodes)
            {
                node.NextNodes = node.NextNodes.OrderBy(n => n.NodeIndex).ToList();
            }
        }
        Debug.Log("NextNodes 정렬 완료");

        map.StageLevels.Insert(0, new StageLevel
        {
            LevelIndex = 0,
            StageNodes = new List<StageNode> { new StageNode { NodeType = EnumTypes.StageType.start, NodeLevel = 0 } }
        });
        for (int i = 0; i < map.StageLevels[1].StageNodes.Count; i++)
        {
            // 시작 노드와 첫 레벨 노드 연결
            map.StageLevels[0].StageNodes[0].NextNodes.Add(map.StageLevels[1].StageNodes[i]);
        }

        Debug.Log("Start 노드 연결 완료");
        return map;
    }
}
