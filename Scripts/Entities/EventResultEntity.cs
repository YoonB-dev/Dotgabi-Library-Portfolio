using System.Collections.Generic;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using UnityEngine;

[Table("event_results")]
public class EventResultEntity : BaseModel
{
    [PrimaryKey("result_id")]
    [Column("result_id")] public int ResultId { get; set; }
    [Column("choice_id")] public int ChoiceId { get; set; }
    [Column("result_type")] public string ResultType { get; set; }
    [Column("result_action")] public string ResultAction { get; set; }
    [Column("weight")] public int Weight { get; set; }
    [Column("extra_data")] public Dictionary<string, object> ExtraData { get; set; } = new();
    [Reference(typeof(EventResultLocaleEntity), useInnerJoin: false)]
    public List<EventResultLocaleEntity> EventResultLocale { get; set; } = new();
}

[Table("event_result_locales")]
public class EventResultLocaleEntity : BaseModel
{
    [Column("result_id")] public int ResultId { get; set; }
    [Column("lan_code")] public EnumTypes.LanguageType LanguageCode { get; set; }
    [Column("result_text")] public string ResultText { get; set; }
}
