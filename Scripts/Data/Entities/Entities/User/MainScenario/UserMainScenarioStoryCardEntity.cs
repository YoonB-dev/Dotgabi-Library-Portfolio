using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using UnityEngine;

[Table("user_main_scenario_owned_story_card")]
public class UserMainScenarioStoryCard : BaseModel
{
    [Column("user_id")] public string UserId { get; set; }
    [Column("card_id")] public int CardId { get; set; }
    [Column("is_use")] public bool IsUse { get; set; }
    [Column("get_at")] public string GetAt { get; set; }
}
