using System.Collections.Generic;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using UnityEngine;

[Table("user_main_scenario_data")]
public class UserMainScenarioEntity : BaseModel
{
    [PrimaryKey("user_auth_id")]
    [Column("user_auth_id")] public string userAuthId { get; set; }
    [Column("map_seed")] public int mapSeed { get; set; }
    [Column("generate_seed")] public int generateSeed { get; set; }
    [Column("stage_list")] public List<int> stageList { get; set; }
    [Column("curr_stage_level")] public int currStageLevel { get; set; }
    [Column("select_list")] public List<int> selectList { get; set; }
    [Column("event_clear")] public int eventClear { get; set; }
    [Column("next_event")] public int nextEvent { get; set; }
    [Column("game_coin")] public int gameCoins { get; set; }
    [Column("total_game_coin")] public int totalGameCoins { get; set; }
    [Column("curr_hp")] public int currHp { get; set; }
    [Column("max_hp")] public int maxHp { get; set; }
    [Column("job_id")] public int jobId { get; set; }
    [Column("first_piece")] public bool firstPiece { get; set; }
    [Column("second_piece")] public bool secondPiece { get; set; }
    [Column("third_piece")] public bool thirdPiece { get; set; }
    [Column("difficulty")] public string difficulty { get; set; }
    [Column("is_next_enemy_story")] public bool isNextEnemyStory { get; set; }
    [Column("is_elite_clear")] public bool isEliteClear { get; set; }
    [Column("fight_time")] public int fightTime { get; set; } // 현재 스테이지에서 싸운 횟수
}

[Table("user_main_scenario_artifact_data")]
public class UserMainScenarioArtifactEntity : BaseModel
{
    [Column("owned_id")] public int ownedId { get; set; }
    [Column("user_auth_id")] public string userAuthId { get; set; }
    [Column("artifact_id")] public int artifactId { get; set; }
    [Column("is_use")] public bool isUse { get; set; }
    [Column("get_at")] public string getAt { get; set; } // 획득 시각
}

[Table("user_main_scenario_card_data")]
public class UserMainScenarioCardEntity : BaseModel
{
    [Column("owned_id")] public int ownedId { get; set; }
    [Column("user_auth_id")] public string userAuthId { get; set; }
    [Column("card_id")] public int cardId { get; set; }
    [Column("upgrade_time")] public int upgradeTime { get; set; }
}