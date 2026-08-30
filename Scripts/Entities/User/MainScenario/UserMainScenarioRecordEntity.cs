using Newtonsoft.Json.Linq;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using UnityEngine;

[Table("user_main_clear_record")]
public class UserMainScenarioRecordEntity : BaseModel
{
    [Column("id")] public int Id { get; set; }
    [Column("user_auth_id")] public string UserAuthId { get; set; }
    [Column("version")] public int Version { get; set; }
    [Column("clear_time")] public int ClearTime { get; set; }
    [Column("scenario_data")] public JObject ScenarioData { get; set; }
    [Column("artifact_list")] public JArray ArtifactList { get; set; }
    [Column("card_list")] public JArray CardList { get; set; }
    [Column("log_list")] public JArray LogList { get; set; }
    [Column("story_list")] public JObject StoryList { get; set; }
}
