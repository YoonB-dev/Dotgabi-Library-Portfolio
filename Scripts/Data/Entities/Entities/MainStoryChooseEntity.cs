using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using UnityEngine;

[Table("main_story_choose")]
public class MainStoryChooseEntity : BaseModel
{
    [Column("id")] public int Id { get; set; }
    [Column("text_id")] public int TextId { get; set; } // 어떤 선택지를 고르면 나오는지(이걸 기준으로 MainStoryText와 연결)
    [Column("choose_order")] public int ChooseOrder { get; set; } // 선택지 순서
    [Column("next_text_id")] public int? NextTextId { get; set; } // 선택 시 다음 텍스트 이동
    [Column("extra_data")] public JObject ExtraData { get; set; }
    [JsonProperty("main_story_choose_locale")] public List<MainStoryChooseLocaleEntity> ChooseTextLocal { get; set; } // 로컬라이즈된 텍스트 정보
}

[Table("main_story_choose_locale")]
public class MainStoryChooseLocaleEntity : BaseModel
{
    [Column("id")] public int Id { get; set; }
    [Column("lan_code")] public EnumTypes.LanguageType LanguageType { get; set; }
    [Column("text")] public string Text { get; set; }
}
