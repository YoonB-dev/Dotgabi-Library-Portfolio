using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using UnityEngine;

[Table("main_story_result")]
public class MainStoryResultEntity : BaseModel
{
    [Column("result_id")] public int Id { get; set; }
    [Column("choose_id")] public int ChooseId { get; set; } // 어떤 선택지를 고르면 나오는지
    [Column("next_text_id")] public int? NextTextId { get; set; } // 선택 시 다음 텍스트 이동
    [Column("text_target")] public EnumTypes.Target TextTarget { get; set; } // 텍스트 출력 위치
    [Column("extra_data")] public JObject ExtraData { get; set; }
    [JsonProperty("main_story_result_locale")] public List<MainStoryResultLocale> ResultTextLocal { get; set; } // 로컬라이즈된 텍스트 정보
}

// 선택지 결과 로컬라이즈된 텍스트 정보
[Table("main_story_result_locale")]
public class MainStoryResultLocale : BaseModel
{
    [Column("result_id")] public int Id { get; set; }
    [Column("lan_code")] public EnumTypes.LanguageType LanguageType { get; set; }
    [Column("text")] public string Text { get; set; }
}
