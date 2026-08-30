using System.Collections.Generic;
using Newtonsoft.Json;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using UnityEngine;

[Table("event_small")]
public class EventSmallEntity : BaseModel
{
    [Column("id")] public int Id { get; set; }
    [Column("amount_min")] public int AmountMin { get; set; }
    [Column("amount_max")] public int AmountMax { get; set; }
    [Column("event_type")] public EnumTypes.EventSmallType EventType { get; set; }
    [JsonProperty("event_small_locale")] public List<EventSmallLocaleEntity> SmallEventLocal { get; set; }
}

[Table("event_small_locale")]
public class EventSmallLocaleEntity : BaseModel
{
    [Column("id")] public int EventSmallId { get; set; }
    [Column("lan_code")] public EnumTypes.LanguageType LanguageCode { get; set; }
    [Column("text")] public string Text { get; set; }
}
