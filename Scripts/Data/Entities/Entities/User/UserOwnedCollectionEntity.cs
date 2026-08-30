using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using UnityEngine;

public class UserOwnedCollectionEntity
{

}

[Table("user_owned_card_data")]
public class UserOwnedCardDataEntity : BaseModel
{
    [Column("auth_id")] public string UserAuthId { get; set; }
    [Column("card_id")] public int CardId { get; set; }
}

[Table("user_owned_artifact_data")]
public class UserOwnedArtifactDataEntity : BaseModel
{
    [Column("auth_id")] public string UserAuthId { get; set; }
    [Column("artifact_id")] public int ArtifactId { get; set; }
}