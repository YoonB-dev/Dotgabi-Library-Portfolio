using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using UnityEngine;

[Table("user_main_scenario_clear")]
public class UserClearEntity : BaseModel
{
    [PrimaryKey("user_auth_id")]
    [Column("user_auth_id")] public string AuthId { get; set; }
    [Column("balance")] public bool IsBalanceClear { get; set; }
    [Column("hard")] public bool IsHardClear { get; set; }
    [Column("dotgabi_1")] public bool IsDotGabi1Clear { get; set; }
    [Column("dotgabi_2")] public bool IsDotGabi2Clear { get; set; }
    [Column("dotgabi_3")] public bool IsDotGabi3Clear { get; set; }
    [Column("dotgabi_4")] public bool IsDotGabi4Clear { get; set; }
    [Column("dotgabi_5")] public bool IsDotGabi5Clear { get; set; }
}
