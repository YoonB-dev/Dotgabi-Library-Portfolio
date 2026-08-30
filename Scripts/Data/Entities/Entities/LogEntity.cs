using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using UnityEngine;


[Table("log_data")]
public class LogEntity : BaseModel
{
    [PrimaryKey("log_id")]
    [Column("log_id")] public int LogID { get; set; }
    [Column("log_action")] public EnumTypes.LogActionType LogAction { get; set; }
    [Column("extra_data")] public JObject ExtraData { get; set; } // JSON 형태로 추가 정보 저장

    [JsonProperty("log_locales")] public List<LogLocaleEntity> LogLocales { get; set; } // 로컬라이즈된 텍스트 정보
}

[Table("log_locales")]
public class LogLocaleEntity : BaseModel
{
    [PrimaryKey("log_id")]
    [Column("log_id")] public int LogID { get; set; }
    [Column("lan_code")] public EnumTypes.LanguageType LanguageCode { get; set; }
    [Column("text")] public string Text { get; set; } // 로컬라이즈된 텍스트
}