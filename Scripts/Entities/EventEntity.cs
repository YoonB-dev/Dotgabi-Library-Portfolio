using System.Collections.Generic;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using UnityEngine;

[Table("events")]
public class EventEntity : BaseModel
{
    [Column("id")] public int eventId { get; set; }
    [Column("event_num")] public int EventNum { get; set; }
    [Column("event_order")] public int EventOrder { get; set; }
    [Column("place")] public string Place { get; set; }
    [Column("img_path")] public string ImgPath { get; set; }

    [Reference(typeof(EventLocaleEntity), useInnerJoin: false)]
    public List<EventLocaleEntity> EventLocales { get; set; } = new();
}

[Table("event_locales")]
public class EventLocaleEntity : BaseModel
{
    [Column("event_id")] public int EventId { get; set; }
    [Column("lan_code")] public EnumTypes.LanguageType LanguageCode { get; set; }
    [Column("event_text")] public string EventText { get; set; }
}

