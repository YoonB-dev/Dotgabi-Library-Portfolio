using System.Collections.Generic;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using UnityEngine;

[Table("enemy_view")]
public class EnemyEntity : BaseModel
{
    [Column("enemy_id")] public int Id { get; set; }
    [Column("lan_code")] public EnumTypes.LanguageType LanguageCode { get; set; }
    [Column("name")] public string EnemyName { get; set; }
    [Column("description")] public string Description { get; set; }
    [Column("flavor_text")] public string FlavorText { get; set; }
    [Column("abilities")] public List<EnemyAbilityDTO> EnemyAbilities { get; set; }
    [Column("img_path")] public string ImgPath { get; set; }
    [Column("spine_path")] public string ImgSpinePath { get; set; }
    [Column("enemy_count")] public int Count { get; set; }
    [Column("health_min")] public int HealthMin { get; set; }
    [Column("health_max")] public int HealthMax { get; set; }
    [Column("attack_min")] public int AttackMin { get; set; }
    [Column("attack_max")] public int AttackMax { get; set; }
    [Column("defense_min")] public int DefenseMin { get; set; }
    [Column("defense_max")] public int DefenseMax { get; set; }
    [Column("heal_min")] public int HealMin { get; set; }
    [Column("heal_max")] public int HealMax { get; set; }
    [Column("stage")] public string Stage { get; set; }
    [Column("img_face_path")] public string ImgFacePath { get; set; }
    [Column("passives")] public List<EnemyPassiveDTO> PassiveAbilities { get; set; }
}


// 중간 매핑을 위한 클래스
public class EnemyAbilityEntity
{
    public EnumTypes.EnemyActionType type { get; set; }
    public EnumTypes.Target target { get; set; }
    public int value { get; set; }
    public Dictionary<string, object> extra_data { get; set; }
}
