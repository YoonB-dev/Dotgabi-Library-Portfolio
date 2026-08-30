using System.Collections.Generic;
using Newtonsoft.Json;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using UnityEngine;

[Table("achieve")]
public class AchieveEntity : BaseModel
{
    [Column("achieve_id")] public int AchieveId { get; set; }
    [Column("achieve_type")] public EnumTypes.AchieveType AchieveType { get; set; }
    [Column("level")] public int Level { get; set; }
    [Column("target_value")] public int TargetValue { get; set; }
    [Column("price_type")] public EnumMainType.CurrencyType PriceType { get; set; }
    [Column("price_amount")] public int PriceAmount { get; set; }
    [JsonProperty("achieve_locales")] public List<AchieveLocaleEntity> AchieveLocales { get; set; } // 로컬라이즈된 텍스트 정보
}

[Table("achieve_locales")]
public class AchieveLocaleEntity : BaseModel
{
    [Column("achieve_id")] public int AchieveId { get; set; }
    [Column("lan_code")] public EnumTypes.LanguageType LanCode { get; set; }
    [Column("achieve_description")] public string Description { get; set; }
}