using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class StageManager : SceneSingleton<StageManager>
{
    public Transform stageGroupPos;
    [SerializeField] private GameObject stagePrefab, stageLine, TreeMapCanvas, treeScroll, spriteMaskObj;
    [SerializeField] private GameObject treeBackCanvas;
    [SerializeField] private Transform rootStage;
    [SerializeField] StageTreeMov stageTreeMov;
    [SerializeField] private GameObject playerPosIcon;
    [SerializeField] private List<List<GameObject>> stageNodeDicts = new();
    private List<GameObject> stageLines = new List<GameObject>();
    private float scaleFactor = 1f;
    private ScenarioDTO scenarioData;
    void Start()
    {
        var targetRectTransform = TreeMapCanvas.GetComponent<RectTransform>();
        // 스케일 조정
        targetRectTransform.localScale *= UserData.Instance.ratio;
        stageTreeMov.maxHeight *= scaleFactor;
    }
    public void ClearTreeMap()
    {
        // 기존에 생성한 스테이지 오브젝트들 삭제
        foreach (var stageLevelList in stageNodeDicts)
        {
            foreach (var stageObj in stageLevelList)
            {
                Destroy(stageObj);
            }
        }
        stageNodeDicts.Clear();

        // 기존 선(LineRenderer 프리팹)들도 따로 보관 중이라면 삭제
        foreach (var lineObj in stageLines)
        {
            Destroy(lineObj);
        }
        stageLines.Clear();
    }

    public void SetTree()
    {
        // 초기 설정용 활성화후 비활성화
        treeBackCanvas.SetActive(true);
        treeBackCanvas.SetActive(false);

        ClearTreeMap();
        scenarioData = MoveSystem.Instance.SCENARIO_DATA;

        StageGenerator stageGenerator = new StageGenerator();
        int seed = scenarioData.MapSeed;
        bool isElite = scenarioData.CurrStageLevel < 4;
        if (isElite)
        {
            stageGenerator.Generate(mapSeed: seed, 9, stageGenerator.typeCounts);
        }
        else
        {
            stageGenerator.Generate(mapSeed: 0309, 1, stageGenerator.bossMap, isElite: false);
        }

        Debug.Log("Stage Map Generated");

        var MapData = scenarioData.StageMapData;
        Dictionary<StageNode, GameObject> nodeToObj = new();

        // 1. 스테이지 노드 생성
        Debug.Log($"Total Stage Levels: {MapData.StageLevels.Count}");
        for (int level = 1; level < MapData.StageLevels.Count; level++)
        {
            var stageLevel = MapData.StageLevels[level];
            List<GameObject> stageLevelNodeDict = new();
            Debug.Log($"Generating Level {level} with {stageLevel.StageNodes.Count} nodes.");
            for (int node = 0; node < stageLevel.StageNodes.Count; node++)
            {
                var stageNode = stageLevel.StageNodes[node];
                GameObject stageObj = Instantiate(stagePrefab, stageGroupPos);
                SetStagePosition(stageObj, stageLevel, node);
                nodeToObj[stageNode] = stageObj;
                SetStageImage(stageObj, stageNode);
                stageLevelNodeDict.Add(stageObj);

                Debug.Log($"  Created Node {node} of type {stageNode.NodeType} at position {stageObj.transform.localPosition}");
            }
            stageNodeDicts.Add(stageLevelNodeDict);
        }
        Debug.Log("Stage Nodes created and positioned.");

        // 2. 선 그리기
        foreach (var stageLevel in MapData.StageLevels)
        {
            if (stageLevel.StageNodes[0].NodeType == EnumTypes.StageType.start)
            {
                continue; // 시작 노드는 연결하지 않음
            }
            foreach (var stageNode in stageLevel.StageNodes)
            {
                var fromObj = nodeToObj[stageNode];
                foreach (var nextNode in stageNode.NextNodes)
                {
                    var toObj = nodeToObj[nextNode];
                    var lineObj = Instantiate(stageLine, stageGroupPos); // stageLine: LineRenderer 프리팹
                    var lr = lineObj.GetComponent<LineRenderer>();
                    lr.positionCount = 2;
                    lr.SetPosition(0, fromObj.transform.position);
                    lr.SetPosition(1, toObj.transform.position);

                    // 선 객체를 리스트에 추가
                    stageLines.Add(lineObj);
                }
            }
        }

        // 3. 시작 위치 선 연결
        if (MapData.StageLevels.Count > 1)
        {
            for (int i = 0; i < MapData.StageLevels[1].StageNodes.Count; i++)
            {
                var startNode = MapData.StageLevels[1].StageNodes[i];
                if (nodeToObj.ContainsKey(startNode))
                {
                    var startObj = nodeToObj[startNode];
                    var lineObj = Instantiate(stageLine, stageGroupPos);
                    var lr = lineObj.GetComponent<LineRenderer>();
                    lr.positionCount = 2;
                    lr.SetPosition(0, playerPosIcon.transform.position);
                    lr.SetPosition(1, startObj.transform.position);

                    // 선 객체를 리스트에 추가
                    stageLines.Add(lineObj);
                }
            }
        }

        //SetScreenSize();
        rootStage.GetComponent<Transform>().localPosition = new Vector2(0, -6);
        spriteMaskObj.GetComponent<Transform>().localScale = new Vector2(1, 2.5f);
        //int boss = GameManager.gameManager.gameData.isLastBoss ? 1 : 2;
        //SetMap(boss);
    }

    private void SetStagePosition(GameObject target, StageLevel stageLevel, int nodeIndex)
    {
        // 스테이지 위치 설정
        var rand = new System.Random(scenarioData.MapSeed + (stageLevel.LevelIndex * 39) + (nodeIndex * 2));
        float posX = 0;
        if (stageLevel.StageNodes.Count == 1)
        {
            posX = 0;
        }
        else if (stageLevel.StageNodes.Count == 2)
        {
            posX = nodeIndex == 0 ? -2 : 2;
        }
        else if (stageLevel.StageNodes.Count == 3)
        {
            posX = nodeIndex == 0 ? -3 : nodeIndex == 1 ? 0 : 3;
        }
        else
        {
            posX = (nodeIndex - (stageLevel.StageNodes.Count / 2)) * 2;
        }
        posX += (float)rand.NextDouble() * 2 - 1;
        float posY = stageLevel.LevelIndex * 2f - 3; // 레벨에 따라 Y 위치 조정
        posY += (float)rand.NextDouble() * 1 - 0.5f; // 약간의 랜덤성 추가
        target.transform.localPosition = new Vector2(posX, posY);
    }

    private void SetStageImage(GameObject target, StageNode stageNode)
    {
        // 스테이지 이미지 설정
        string imgURL = stageNode.NodeType switch {
            EnumTypes.StageType.enemy => "Image/Icon/icon_enemyAttack01",
            EnumTypes.StageType.shop => "Image/Icon/icon_shop",
            EnumTypes.StageType.artifact => "Image/Icon/icon_treasure",
            EnumTypes.StageType.mystery => "Image/Icon/icon_questionMark",
            EnumTypes.StageType.rest => "Image/Icon/icon_bonfire",
            EnumTypes.StageType.elite => "Image/Icon/icon_enemyAttack02",
            EnumTypes.StageType.boss => "Image/Icon/icon_enemyAttack03",
            _ => "Image/Icon/icon_questionMark"
        };
        if (stageNode.NodeType == EnumTypes.StageType.boss || stageNode.NodeType == EnumTypes.StageType.elite)
        {
            target.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
        }

        target.transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(imgURL);
    }

    public void OpenTreeMap()
    {
        //SFX
        AudioManager.Instance.ButtonClickSound1();
        if (TreeMapCanvas.activeSelf) { return; }
        TreeMapCanvas.SetActive(true);
        treeBackCanvas.SetActive(true);

        treeScroll.SetActive(true);
        treeScroll.GetComponent<SkeletonAnimation>().skeleton.SetToSetupPose();
        stageGroupPos.gameObject.SetActive(true);

        SetStageMark();
        float posY = scenarioData.SelectList.Count >= 4 ? ((scenarioData.SelectList.Count - 4) * 2 + 5) * scaleFactor : 0;
        if (posY >= 35) posY = 35;
        Vector3 camPos = new(0, posY, -10);
        Camera.main.GetComponent<StageTreeMov>().SetTreeMove(true);
        Camera.main.GetComponent<Transform>().position = camPos;
    }

    public void HideTreeMap()
    {
        //SFX
        AudioManager.Instance.ButtonClickSound3();
        TreeMapCanvas.SetActive(false);
        treeBackCanvas.SetActive(false);
        Camera.main.GetComponent<StageTreeMov>().SetTreeMove(false);
        Camera.main.GetComponent<Transform>().position = new Vector3(0, 0, -10);
    }

    // 스테이지 이동 경로 표시 및 다음 선택지 표시
    private void SetStageMark()
    {
        var stageSelectList = scenarioData.SelectList;
        Debug.Log("SelectList updated: " + JsonSerializer.Serialize(scenarioData.SelectList));

        // 지나온 경로는 회색으로 표시
        for (int i = 0; i < stageSelectList.Count - 1; i++)
        {
            stageNodeDicts[i][stageSelectList[i + 1]].transform.GetChild(1).GetComponent<SpriteRenderer>().color = Color.gray;
        }

        Debug.Log("stageLevelCount: " + scenarioData.StageMapData.StageLevels.Count);

        // 현재 선택한 스테이지는 플레이어 아이콘 이동
        int currLevel = stageSelectList.Count - 2;
        if (currLevel < 0 || currLevel >= stageNodeDicts.Count)
        {
            var startNode = scenarioData.StageMapData.StageLevels[0].StageNodes[0];
            for (int i = 0; i < startNode.NextNodes.Count; i++)
            {
                int nextIndex = startNode.NextNodes[i].NodeIndex;
                stageNodeDicts[0][nextIndex].transform.GetChild(2).gameObject.SetActive(true);
            }
            return;
        }
        var currNodeObj = stageNodeDicts[currLevel][stageSelectList[stageSelectList.Count - 1]];
        playerPosIcon.transform.position = currNodeObj.transform.position;

        // 다음 선택지 표시
        var stageMapData = scenarioData.StageMapData;
        Debug.Log("현재 스테이지 레벨: " + (stageSelectList.Count - 1) + ", 선택한 노드 인덱스: " + stageSelectList[stageSelectList.Count - 1]);
        Debug.Log("stageLevelCount: " + stageMapData.StageLevels.Count);
        Debug.Log(stageMapData.StageLevels[stageSelectList.Count - 1]);
        Debug.Log(stageMapData.StageLevels[stageSelectList.Count - 1].StageNodes.Count);
        var currStageNode = stageMapData.StageLevels[stageSelectList.Count - 1].StageNodes[stageSelectList[stageSelectList.Count - 1]];

        // 다음 선택지 초기화
        for (int i = 0; i < stageNodeDicts[currStageNode.NodeLevel].Count; i++)
        {
            stageNodeDicts[currStageNode.NodeLevel][i].transform.GetChild(2).gameObject.SetActive(false);
        }

        for (int i = 0; i < currStageNode.NextNodes.Count; i++)
        {
            int nextIndex = currStageNode.NextNodes[i].NodeIndex;
            stageNodeDicts[stageSelectList.Count - 1][nextIndex].transform.GetChild(2).gameObject.SetActive(true);
        }
    }
}
