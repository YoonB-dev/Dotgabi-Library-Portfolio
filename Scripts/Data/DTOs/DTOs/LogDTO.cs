using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

[System.Serializable]
public class LogDTO
{
    public int LogId { get; set; }
    public EnumTypes.LogActionType LogAction { get; set; }
    public string LogText { get; set; } // 로컬라이즈된 텍스트
}

[System.Serializable]
public class UserScenarioLogDTO
{
    [JsonProperty("log_id")] public int LogId { get; set; }
    [JsonProperty("value")] public int? value { get; set; } // 로그 값 (예: 획득한 카드 수, 사용한 아이템 등)
    [JsonProperty("card_id")] public int? CardId { get; set; } // 관련 카드 ID
    [JsonProperty("artifact_id")] public int? ArtifactId { get; set; } // 관련 유물 ID
    [JsonProperty("log_at")] public string LogAt { get; set; } // 로그 기록 시각
    [JsonProperty("extra_data")] public Dictionary<string, object> ExtraData { get; set; } // 추가 정보 (JSON 형태로 저장)
}
