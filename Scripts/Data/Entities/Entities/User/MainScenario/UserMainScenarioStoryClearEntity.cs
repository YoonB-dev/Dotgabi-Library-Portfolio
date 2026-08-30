using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using UnityEngine;

[Table("user_main_scenario_story_data")]
public class UserMainScenarioStoryClearEntity : BaseModel
{
    [Column("user_auth_id")] public string UserAuthId { get; set; }
    [Column("crime_scene_clear")] public bool CrimeSceneClear { get; set; }
    [Column("onu_house_clear")] public bool OnuHouseClear { get; set; }
    [Column("tiger_arrest")] public bool? TigerArrest { get; set; }
    [Column("onu_trust")] public bool? OnuTrust { get; set; }
}
