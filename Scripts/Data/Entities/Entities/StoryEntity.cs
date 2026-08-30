using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using UnityEngine;

[Table("story")]
public class StoryEntity : BaseModel
{
    [PrimaryKey("story_id")]
    [Column("story_id")] public string StoryId { get; set; }
    [Column("img_path")] public string ImgPath { get; set; }

    [Reference(typeof(StoryLocalesEntity), useInnerJoin: false)]
    public List<StoryLocalesEntity> StoryLocales { get; set; } = new();
}

[Table("story_locales")]
public class StoryLocalesEntity : BaseModel
{
    [Column("story_id")] public string StoryId { get; set; }
    [Column("lan_code")] public EnumTypes.LanguageType LanguageCode { get; set; }
    [Column("story_name")] public string Name { get; set; }
    [Column("story_description")] public string Description { get; set; }
}

[Table("main_story_item")]
public class MainStoryItemEntity : BaseModel
{
    [Column("id")] public int Id { get; set; }
    [Column("item_type")] public EnumMainType.ProductType ItemType { get; set; }
    [Column("img_path")] public string ImgPath { get; set; }
    [Column("extra_data")] public JObject ExtraData { get; set; }
    [JsonProperty("main_story_item_locale")] public List<MainStoryItemLocaleEntity> MainStoryItemLocales { get; set; } // 로컬라이즈된 텍스트 정보
}

[Table("main_story_item_locale")]
public class MainStoryItemLocaleEntity : BaseModel
{
    [Column("item_id")] public int ItemId { get; set; }
    [Column("lan_code")] public EnumTypes.LanguageType LanguageCode { get; set; }
    [Column("name")] public string Name { get; set; }
    [Column("description")] public string Description { get; set; }
}