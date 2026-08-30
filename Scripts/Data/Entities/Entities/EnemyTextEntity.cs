using System.Collections.Generic;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using UnityEngine;

[Table("enemy_texts")]
public class EnemyTextEntity : BaseModel
{
    [Column("text_id")] public int Id { get; set; }
    [Column("enemy_id")] public int EnemyId { get; set; }
    [Column("text_type")] public EnumTypes.EnemyTextType TextType { get; set; }
    [Column("extra_data")] public Dictionary<string, object> ExtraData { get; set; }
}

[Table("enemy_text_locales")]
public class EnemyTextLocaleEntity : BaseModel
{
    [Column("text_id")] public int TextId { get; set; }
    [Column("lan_code")] public EnumTypes.LanguageType LanguageCode { get; set; }
    [Column("text")] public string TextValue { get; set; }
}


/// <summary>
/// 적 몬스터 선택지 엔터티
/// </summary>
[Table("enemy_text_choice")]
public class EnemyTextChoiceEntity : BaseModel
{
    [Column("choice_id")] public int Id { get; set; }
    [Column("text_id")] public int TextId { get; set; }
    [Column("choice_order")] public int ChoiceOrder { get; set; }
    [Column("next_index")] public int NextIndex { get; set; }
    [Column("extra_data")] public Dictionary<string, object> ExtraData { get; set; }
}

[Table("enemy_text_choice_locales")]
public class EnemyTextChoiceLocaleEntity : BaseModel
{
    [Column("choice_id")] public int ChoiceId { get; set; }
    [Column("lan_code")] public EnumTypes.LanguageType LanguageCode { get; set; }
    [Column("choice_text")] public string ChoiceText { get; set; }
}