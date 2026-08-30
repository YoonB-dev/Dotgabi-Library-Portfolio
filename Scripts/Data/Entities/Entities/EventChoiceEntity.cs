using System.Collections.Generic;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using UnityEngine;

[Table("event_choices")]
public class EventChoiceEntity : BaseModel
{
    [PrimaryKey("choice_id")]
    [Column("choice_id")] public int EventChoiceId { get; set; }
    [Column("event_id")] public int EventId { get; set; }
    [Column("order_index")] public int OrderIndex { get; set; }
    [Reference(typeof(EventChoiceLocaleEntity), useInnerJoin: false)]
    public List<EventChoiceLocaleEntity> EventChoiceLocale { get; set; } = new();
}

[Table("event_choice_locales")]
public class EventChoiceLocaleEntity : BaseModel
{
    [Column("choice_id")] public int EventChoiceId { get; set; }
    [Column("lan_code")] public EnumTypes.LanguageType LanguageCode { get; set; }
    [Column("choice_text")] public string ChoiceText { get; set; }
}
