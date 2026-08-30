using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using UnityEngine;

[Table("user_clear_achievement")]
public class UserAchieveEntity : BaseModel
{
    [Column("user_auth_id")] public string UserAuthId { get; set; }
    [Column("achieve_id")] public int AchieveId { get; set; }
}

[Table("user_achieve_curr_data")]
public class UserAchieveCurrDataEntity : BaseModel
{
    [Column("auth_id")] public string UserAuthId { get; set; }
    [Column("move_forward_count")] public int MoveForwardCount { get; set; }
    [Column("battle_count")] public int BattleCount { get; set; }
    [Column("shop_purchase_count")] public int ShopPurchaseCount { get; set; }
    [Column("rest_count")] public int RestCount { get; set; }
    [Column("show_ad_count")] public int ShowAdCount { get; set; }
    [Column("total_use_card")] public int TotalUseCard { get; set; }
}

[Table("user_achieve_price_get")]
public class UserAchievePriceGetEntity : BaseModel
{
    [Column("auth_id")] public string AuthId { get; set; }
    [Column("big_price_1")] public bool BigPrice1 { get; set; }
    [Column("big_price_2")] public bool BigPrice2 { get; set; }
    [Column("big_price_3")] public bool BigPrice3 { get; set; }
    [Column("big_price_4")] public bool BigPrice4 { get; set; }
}