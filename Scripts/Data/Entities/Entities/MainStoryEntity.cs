using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using UnityEngine;

[Table("main_story_text")]
public class MainStoryEntity : BaseModel
{
    [Column("text_id")] public int TextId { get; set; }
    [Column("text_trigger")] public EnumTypes.MainStoryTrigger TextTrigger { get; set; }
    [Column("text_target")] public EnumTypes.Target TextTarget { get; set; }
    [Column("next_text_id")] public int? NextTextId { get; set; }
    [Column("extra_data")] public JObject ExtraData { get; set; }
    [JsonProperty("main_story_text_locale")] public List<MainStoryLocaleEntity> StoryTextLocal { get; set; } // 로컬라이즈된 텍스트 정보
}

[Table("main_story_text_locale")]
public class MainStoryLocaleEntity : BaseModel
{
    [Column("text_id")] public int TextId { get; set; }
    [Column("lan_code")] public EnumTypes.LanguageType LanguageType { get; set; }
    [Column("text")] public string Text { get; set; }
}
