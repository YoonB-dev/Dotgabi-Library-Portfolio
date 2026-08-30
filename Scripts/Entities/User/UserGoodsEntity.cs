using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

[Table("user_goods")]
public class UserGoodsEntity : BaseModel
{
    [PrimaryKey("user_auth_id")]
    [Column("user_auth_id")] public string UserAuthId { get; set; }
    [Column("achieve_point")] public int AchievePoint { get; set; }
    [Column("achieve_total_point")] public int AchieveTotalPoint { get; set; }
    [Column("achieve_use_point")] public int AchieveUsePoint { get; set; }
    [Column("ad_point")] public int AdPoint { get; set; }
    [Column("ad_total_point")] public int AdTotalPoint { get; set; }
    [Column("ad_use_point")] public int AdUsePoint { get; set; }
}
