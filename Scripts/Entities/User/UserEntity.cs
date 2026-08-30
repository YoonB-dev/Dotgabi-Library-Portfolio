using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

[Table("users")]
public class UserEntity : BaseModel
{
    [PrimaryKey("auth_id")]
    [Column("auth_id")]
    public string AuthId { get; set; }
    [Column("email")]
    public string Email { get; set; }
    [Column("last_login_at")]
    public DateTime? LastLoginAt { get; set; }
    [Column("is_banned")]
    public bool IsBanned { get; set; }
    [Column("select_card_frame_id")] public int? SelectCardFrameId { get; set; }
    [Column("select_deco_id")] public int? SelectDecoId { get; set; }
    [Column("curr_scenario_type")] public string CurrScenarioType { get; set; }
    [Column("is_tutorial")] public bool IsTutorial { get; set; } = false;
}
