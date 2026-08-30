using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

[Table("user_owned_card_frame")]
public class UserOwnCardFrameEntity : BaseModel
{
    [PrimaryKey("user_auth_id")]
    [Column("user_auth_id")] public string UserAuthId { get; set; }
    [Column("frame_id")] public int CardFrameId { get; set; }
    [Column("count")] public int Count { get; set; }
    [Column("frame_type")] public EnumTypes.ShopItemType CardFrameType { get; set; }
}

[Table("user_owned_character")]
public class UserOwnCharacterEntity : BaseModel
{
    [Column("auth_id")] public string UserAuthId { get; set; }
    [Column("owned_blacksmith")] public bool OwnedBlacksmith { get; set; }
    [Column("owned_dosa")] public bool OwnedDosa { get; set; }
    [Column("owned_performer")] public bool OwnedPerformer { get; set; }
}