using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using UnityEngine;


[Table("item_view")]
public class ArtifactEntity : BaseModel
{
    [Column("id")] public int Id { get; set; }
    [Column("name")] public string ArtifactName { get; set; }
    [Column("ability")] public string ArtifactAbility { get; set; }
    [Column("rare")] public EnumTypes.RarityType Rarity { get; set; }
    [Column("place")] public string Place { get; set; }
    [Column("img_path")] public string ImgPath { get; set; }
    [Column("lan_code")] public EnumTypes.LanguageType LanguageCode { get; set; }
    [Column("flavor_text")] public string FlavorText { get; set; }
    [Column("effects")] public List<JsonArtifactEffect> ArtifactEffects { get; set; }
    [Column("is_icon")] public bool IsIcon { get; set; } // 아이콘 여부
}

// 중간 매핑을 위한 클래스
public class JsonArtifactEffect
{
    public string item_trigger { get; set; }
    public string item_effect_type { get; set; }
    public string target { get; set; }
    public int value { get; set; }
    public string value_type { get; set; }
    public JObject extra_data { get; set; }
}