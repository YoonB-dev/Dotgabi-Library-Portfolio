using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using UnityEngine;

[Table("user_main_scenario_log_data")]
public class UserMainScenarioLogEntity : BaseModel
{
    [PrimaryKey("id")]
    [Column("id")] public int Id { get; set; }
    [Column("log_id")] public int LogId { get; set; }
    [Column("user_auth_id")] public string UserAuthId { get; set; }
    [Column("value")] public int? Value { get; set; }
    [Column("card_id")] public int? CardId { get; set; }
    [Column("artifact_id")] public int? ArtifactId { get; set; }
    [Column("log_at")] public string LogAt { get; set; } // 로그 기록 시각
    [Column("extra_data")] public Dictionary<string, object> ExtraData { get; set; } // 추가 정보
}
