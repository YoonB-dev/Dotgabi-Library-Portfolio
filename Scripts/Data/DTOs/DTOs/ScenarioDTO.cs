using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[System.Serializable]
public class ScenarioDTO
{
    // 스테이지 정보
    public List<int> StageList { get; set; } = new List<int>(); // 동화 스테이지 목록
    // 현재 몇번째 스테이지 인지, 2스테이지 이런식으로
    public int CurrStage
    {
        get {
            if (CurrStageLevel - 1 < StageList.Count)
            {
                return StageList[CurrStageLevel - 1];
            }
            else
            {
                //Debug.LogError("CurrStageLevel is out of range of StageList");
                return 1;
            }
        }
    }
    public int CurrStageLevel { get; set; } // 현재 스테이지가 몇번째 레벨인지
    public List<int> SelectList { get; set; } = new List<int>(); // 현재 스테이지에서 선택한 노드 목록
    // 소유 목록
    public List<UserScenarioOwnedCardDTO> OwnedCardList { get; set; } = new();
    public List<UserScenarioOwnedArtifactDTO> OwnedArtifactList { get; set; } = new();
    // 로그 리스트
    public List<UserScenarioLogDTO> LogList { get; set; } = new();

    // SEED 값 & 맵 데이터
    public int MapSeed { get; set; }
    public int GenerateSeed { get; set; }
    public StageMap StageMapData { get; set; } = new StageMap();

    // 이벤트 진행 및 전투 횟수
    public int EventClear { get; set; }
    public int NextEvent { get; set; }
    public int FightTime { get; set; } // 현재 스테이지에서 싸운 횟수

    // 기타 정보
    [JsonProperty("game_coin")] public int GameCoins { get; set; } // 현재 보유 골드
    [JsonProperty("total_game_coin")] public int TotalGameCoins { get; set; }
    [JsonProperty("curr_hp")] public int CurrHp { get; set; }
    [JsonProperty("max_hp")]public int MaxHp { get; set; }
    [JsonProperty("job_id")] public int JobId { get; set; }

    // 유물 보유 여부
    public bool IsArtifact(int artifactId)
    {
        return OwnedArtifactList.Exists(a => a.ArtifactId == artifactId);
    }
}

[System.Serializable]
public class UserScenarioOwnedArtifactDTO
{
    [JsonProperty("artifact_id")] public int ArtifactId { get; set; }
    public bool IsUse { get; set; }
}

[System.Serializable]
public class UserScenarioOwnedCardDTO
{
    public int OwnedId { get; set; }
    [JsonProperty("card_id")] public int CardId { get; set; }
    [JsonProperty("upgrade_time")] public int UpgradeTime { get; set; }
}
