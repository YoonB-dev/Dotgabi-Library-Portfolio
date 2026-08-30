namespace EnumTypes
{
    public enum LoginType { Guest, Google, Apple, Kakao }
    public enum JobType { Public = 0, Blacksmith = 1, Dosa = 2, Performer = 3 }
    public enum EnemyType
    {
        none = 0,
        normal = 1,
        elite = 2,
        boss = 3,
    }
    public enum EnemyTextType { root, branch, leaf, continue_, action }
    public enum Status { buff, debuff }
    public enum Action
    {
        attack, shield, buff, debuff, heal, draw, repeat, condition, action, upgrade, equip, get_action_point
    }
    public enum Target { self, enemy, enemys, none, random_enemy, shop, deck, player, narration }

    public enum CardType { attack, shield, action, equip, curse, upgrade, special, ethreal }

    public enum EffectType { none, buff, debuff, heal, shield, hit, slash, smash, blood, draw, fire, upgrade, equip, action, monster_die }

    public enum PopupType { Card, Artifact, Character, ShopItem, Bag, Achieve, Story }

    public enum LanguageType { ko = 0, en = 1, jp = 2, ch = 3 }
    public enum ShopItemType { frame, deco, character }
    public enum AchieveType
    {
        story_scenario_clear, card_collection_count, artifact_collection_count, story_collection_count, move_forward_count, battle_count, shop_purchase_count, rest_count, show_ad_count, total_coin_use,
        total_use_card, battle_dotgabi
    }
    public enum EventSmallType { get_coin, get_damage, get_heal }
    // <summary>
    // ----------------아티팩트 타입----------------
    // </summary>
    // 아티팩트 트리거 타입
    public enum ArtifactTriggerType
    {
        passive, on_obtain, on_battle_start, on_battle_end, on_action_start, on_action_end, on_die, on_attack, on_first_card, on_first_card_attack,
        on_get_damage_first, on_get_damage, on_kill_enemy, on_shop_enter, on_use_card, on_draw, on_draw_curse, on_heal
    }
    // 아티팩트 타입
    public enum ArtifaceEffectType
    {
        buff, debuff, attack, execute, damage_up, get_action, get_coin, get_max_hp, get_shield, hand_size_up, heal_hp, nullify_damage,
        remove_card, upgrade_card, revive, shop_sale, copy_card, predict_action, draw_card, block_curse
    }

    public enum RarityType { common, rare, epic, legendary, mythic }



    public enum ValueType { amount, percent, multiplier }
    public enum Difficulty
    {
        balance = 1,
        hard = 2,
        dotgabi_1 = 3,
        dotgabi_2 = 4,
        dotgabi_3 = 5,
        dotgabi_4 = 6,
        dotgabi_5 = 7
    }
    public enum StageType { enemy, shop, artifact, mystery, rest, elite, boss, start }
    public enum MoveTextType { damage, heal, money, none }
    public enum TextMotionType { up, down, direct }
    public enum enemyActionType { attack, shield, heal, action }

    // 도사 소환 정보
    public enum DosaSummonType { clone, wolf, log, wraith, totem }
    // 도사 변신 정보
    public enum DosaModeType { none, tiger, scarecrow, hawk }

    // 탈춤꾼 연주 종류
    public enum PerformerPlayType { none, janggu, drum, pipe, hae }
    public enum PerformerMaskType { none = 0, lion = 1, bong = 2, bang = 3, hahoe = 4 }

    public enum LogActionType
    {
        none,
        start_game, move_forward, find_nothing, find_stage_branch, battle_enter, shop_enter, shop_exit, shop_buy, artifact_enter, player_get_something, player_lose_something,
        mystery_enter, rest_find, elite, boss, card_upgrade, card_delete, player_heal, player_get_max_hp, player_lose_max_hp, player_get_damage,
        small_event_coin, small_event_damage, small_event_heal, player_use_something
    }

    // 적
    public enum EnemyPassiveTrigger
    {
        player_end_turn, player_get_shield, enemy_start_turn, enemy_got_damage, attack_to_player, start_battle
    }

    public enum EnemyActionType
    {
        attack, shield, heal, buff, debuff, take_coin, get_max_hp
    }

    // 메인 스토리
    public enum MainStoryTrigger
    {
        none, chain, end,
        story_1_start, story_1_before_elite, story_1_after_elite, story_1_before_boss,
        story_2_start, story_2_before_elite, story_2_after_elite, story_2_before_boss,
        story_3_start, story_3_before_elite, story_3_after_elite, story_3_before_boss,
        story_4_start, story_4_before_elite, story_4_after_elite, story_4_before_boss}

    // 스토리 종류
    public enum MainStoryType { crime_scene_clear, onu_house_clear, tiger_arrest, onu_trust }
}
