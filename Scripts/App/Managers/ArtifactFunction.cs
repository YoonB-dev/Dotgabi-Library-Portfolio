using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Localization.SmartFormat;

public class DamageReceiveResult
{
    public bool IsDamageNullified { get; set; } // 피해 무효 여부
    public int DrawCard { get; set; } = 0; // 카드 드로우 여부
    public List<string> BuffsToApply { get; set; } = new List<string>(); // 적용할 버프 목록
    public bool IsMultifulDamage { get; set; } = false; // 배율 피해 여부
    public int MultiAmount { get; set; } = 1; // 배율 값, 기본값은 1
    public bool IsRevive { get; set; } = false; // 부활 여부
    public bool IsBlockCurse { get; set; } = false; // 저주 카드 막기 여부
    public bool PredictEnemyACtion { get; set; } = false; // 적의 다음 행동 예측 여부
    public int SalePercent { get; set; } = 0; // 상점 할인 비율
    public int UpgradeCardAmount { get; set; } = 0; // 카드 업그레이드 개수
    public int DeleteCardAmount { get; set; } = 0; // 카드 삭제 개수
    public bool IsRandom { get; set; } = false; // 랜덤 상태 적용 여부
}
public class ArtifactFunction : Singleton<ArtifactFunction>
{
    private bool isFirstCardUse = false; // 첫번째 카드 사용 여부

    private ScenarioDTO GetCurrData()
    {
        switch (GameData.Instance.CurrScenarioType)
        {
            case EnumMainType.ScenarioType.story:
                return UserData.Instance.MainScenarioData;
            case EnumMainType.ScenarioType.challenge:
                return UserData.Instance.ChallengeScenarioData;
            default:
                Debug.LogError("GetCurrData: Invalid ScenarioType");
                return null;
        }
    }

    public async void ArtifactMainSceneEffect(ArtifactEffectDTO effectDTO)
    {
        var SCENARIO_DATA = GetCurrData();

        switch (effectDTO.ItemEffectType)
        {
            case EnumTypes.ArtifaceEffectType.get_max_hp:
                if (effectDTO.ItemTrigger != EnumTypes.ArtifactTriggerType.on_obtain)
                {
                    await SupabaseGetScenarioCoin.Instance.GetMaxHp(effectDTO.Value, SCENARIO_DATA);
                }
                break;
            case EnumTypes.ArtifaceEffectType.heal_hp:
                if (effectDTO.ItemTrigger != EnumTypes.ArtifactTriggerType.on_obtain)
                {
                    int healAmount = effectDTO.Value;
                    if (effectDTO.ValueType == "percent")
                    {
                        healAmount = Mathf.RoundToInt(SCENARIO_DATA.MaxHp * (effectDTO.Value / 100f));
                    }
                    await SupabaseGetScenarioCoin.Instance.GetHp(healAmount, SCENARIO_DATA);
                }
                break;
        }
        if (effectDTO.ItemEffectType == EnumTypes.ArtifaceEffectType.get_max_hp || effectDTO.ItemEffectType == EnumTypes.ArtifaceEffectType.heal_hp)
        {
            SetFooterText.Instance?.SetMoveText(effectDTO.Value, EnumTypes.MoveTextType.heal);
            SetFooterText.Instance?.SetHpBar(EnumTypes.TextMotionType.up);
        }
    }
    public DamageReceiveResult ProcessMainSceneArtifactEffect(EnumTypes.ArtifactTriggerType triggerType)
    {
        var SCENARIO_DATA = GetCurrData();

        var artifactList = SCENARIO_DATA.OwnedArtifactList;
        DamageReceiveResult finalResult = null;
        foreach (var ownedArtifact in artifactList)
        {
            if (ownedArtifact.IsUse) continue;
            var artifact = InGameData.Instance.Artifacts.Find(a => a.Id == ownedArtifact.ArtifactId);
            if (artifact == null) continue;

            foreach (var effect in artifact.ArtifactEffects)
            {
                if (effect.ItemTrigger != triggerType) continue;
                switch (effect.ItemEffectType)
                {
                    case EnumTypes.ArtifaceEffectType.get_max_hp:
                        ArtifactMainSceneEffect(effect);
                        break;
                    case EnumTypes.ArtifaceEffectType.heal_hp:
                        ArtifactMainSceneEffect(effect);
                        break;
                    case EnumTypes.ArtifaceEffectType.shop_sale:
                        if (finalResult == null)
                        {
                            finalResult = new()
                            {
                                SalePercent = effect.Value // 상점 할인 비율
                            };
                        }
                        else
                        {
                            if (finalResult.SalePercent > 0 && finalResult.SalePercent > effect.Value)
                            {
                                continue; // 이미 더 높은 할인율이 적용된 경우 무시
                            }
                            else
                            {
                                finalResult.SalePercent = effect.Value; // 상점 할인 비율 업데이트
                            }
                        }
                        return finalResult; // 상점 할인 효과는 즉시 반환
                    case EnumTypes.ArtifaceEffectType.upgrade_card:
                        if (effect.ExtraData != null && effect.ExtraData.ContainsKey("random"))
                        {
                            finalResult = new DamageReceiveResult {
                                UpgradeCardAmount = effect.Value,
                                IsRandom = effect.ExtraData["random"].Value<bool>() // 랜덤 상태 적용 여부
                            };
                        }
                        else
                        {
                            finalResult = new DamageReceiveResult {
                                UpgradeCardAmount = effect.Value,
                                IsRandom = false // 랜덤 상태 적용 여부
                            };
                        }
                        break;
                }
            }
        }
        return finalResult;
    }

    public DamageReceiveResult ProcessArtifactEffects(CharacterBase player, CharacterBase enemy, EnumTypes.ArtifactTriggerType triggerType, bool isFirseUse = false, CardDTO card = null)
    {
        if (player == null || player.ArtifactList == null) return null;
        var artifactList = player.ArtifactList;
        DamageReceiveResult finalResult = null;

        foreach (var ownedArtifact in artifactList)
        {
            if (ownedArtifact.IsUse) continue;
            var artifact = InGameData.Instance.Artifacts.Find(a => a.Id == ownedArtifact.ArtifactId);
            if (artifact == null) continue;

            foreach (var effect in artifact.ArtifactEffects)
            {
                if (effect.ItemTrigger != triggerType) continue;
                switch (effect.ItemEffectType)
                {
                    case EnumTypes.ArtifaceEffectType.attack:
                        if (effect.Target == EnumTypes.Target.self)
                        {
                            player.GetComponent<Player>()?.GetDamagePlayer(effect.Value, null);
                        }
                        else if (effect.Target == EnumTypes.Target.enemy && enemy != null)
                        {
                            // 적에게 공격 효과 적용
                            enemy.GetComponent<Enemy>()?.GetDamage(null, effect.Value, EnumTypes.EffectType.hit, false, null);
                        }
                        else if (effect.Target == EnumTypes.Target.enemys)
                        {
                            foreach (var targetEnemy in EnemyManager.Instance.enemies)
                            {
                                if (targetEnemy.isDie) continue;
                                targetEnemy.GetDamage(null, effect.Value, EnumTypes.EffectType.hit, false, null);
                            }
                        }

                        if (effect.ExtraData != null && effect.ExtraData.ContainsKey("reflection"))
                        {
                            var reflectText = LogManager.Instance?.GetLocalizedText("character_get_reflect_damage_by_artifact").FormatSmart(enemy.characterName, artifact.Name, effect.Value);
                            LogManager.Instance?.AddLogBattle(reflectText);
                        }
                        break;
                    case EnumTypes.ArtifaceEffectType.execute:
                        if (enemy != null)
                        {
                            ArtifactExecuteEffect(enemy, effect);
                        }
                        break;

                    case EnumTypes.ArtifaceEffectType.draw_card:
                        ArtifactDrawCardEffect(effect);
                        if (isFirseUse)
                        {
                            ownedArtifact.IsUse = true;
                        }
                        break;

                    case EnumTypes.ArtifaceEffectType.nullify_damage:
                        finalResult = new DamageReceiveResult { IsDamageNullified = true };
                        if (isFirseUse)
                        {
                            ownedArtifact.IsUse = true;
                        }
                        break;

                    case EnumTypes.ArtifaceEffectType.damage_up:
                        finalResult = new DamageReceiveResult {
                            IsMultifulDamage = true,
                            MultiAmount = effect.Value
                        };
                        if (isFirseUse)
                        {
                            ownedArtifact.IsUse = true;
                        }
                        break;

                    case EnumTypes.ArtifaceEffectType.heal_hp:
                        if (effect.ExtraData != null && effect.ExtraData.ContainsKey("condition"))
                        {
                            // 조건이 있는 경우
                            var condition = effect.ExtraData["condition"] as JObject;
                            if (condition != null && condition.ContainsKey("hp_below_percent"))
                            {
                                int hpBelowPercent = condition["hp_below_percent"].Value<int>();
                                // 플레이어의 현재 체력이 조건에 맞는지 확인
                                if (player.Stats.currHp <= player.Stats.maxHp * hpBelowPercent / 100)
                                {
                                    GetHeal(player, effect.Value);
                                }
                                else
                                {
                                    Debug.Log($"전투 종료 시 체력 회복 조건 불충족: 현재 체력 {player.Stats.currHp}, 최대 체력 {player.Stats.maxHp}, 조건 {hpBelowPercent}% 미만");
                                }
                            }
                        }
                        else
                        {
                            GetHeal(player, effect.Value);
                        }
                        break;

                    case EnumTypes.ArtifaceEffectType.get_shield:
                        if (effect.ExtraData != null && effect.ExtraData.ContainsKey("condition"))
                        {
                            // 조건이 있는 경우
                            var condition = effect.ExtraData["condition"] as JObject;
                            if (condition != null && condition.ContainsKey("is_shield"))
                            {
                                bool isShield = condition["is_shield"].Value<bool>();
                                // 방어막이 존재하면 실행
                                if (isShield && player.Stats.currShield != 0)
                                {
                                    GetShield(effect.Value, player);
                                }
                            }
                        }
                        else
                        {
                            // 조건이 없는 경우 그냥 방어막 얻음
                            if (effect.Value > 0) { GetShield(effect.Value, player); }
                        }
                        break;
                    case EnumTypes.ArtifaceEffectType.get_action:
                        int amount = effect.Value;
                        player.GetComponent<Player>()?.GetAction(amount);
                        break;
                    case EnumTypes.ArtifaceEffectType.buff:
                    case EnumTypes.ArtifaceEffectType.debuff:
                        var statusType = effect.ItemEffectType == EnumTypes.ArtifaceEffectType.buff
                            ? EnumTypes.Status.buff : EnumTypes.Status.debuff;

                        if (effect.Target == EnumTypes.Target.self)
                        {
                            GetStatus(player, statusType, effect.Value, effect.Target, effect.ExtraData);
                        }
                        else if (effect.Target == EnumTypes.Target.enemy && enemy != null)
                        {
                            GetStatus(enemy, statusType, effect.Value, effect.Target, effect.ExtraData);
                        }
                        else if (effect.Target == EnumTypes.Target.enemys)
                        {
                            GetStatus(enemy, statusType, effect.Value, effect.Target, effect.ExtraData);
                        }
                        break;
                    case EnumTypes.ArtifaceEffectType.revive:
                        player.Stats.currHp = (int)(player.Stats.maxHp * 0.25f);
                        player.isDie = false;
                        ownedArtifact.IsUse = true;
                        finalResult = new DamageReceiveResult { IsRevive = true };
                        break;
                    case EnumTypes.ArtifaceEffectType.get_coin:
                        ArtifactGetCoin(effect);
                        break;
                    case EnumTypes.ArtifaceEffectType.copy_card:
                        // 카드 복사 효과
                        isFirstCardUse = true; // 첫 번째 카드 사용 처리 완료
                        if (card != null)
                        {
                            CardSystem.Instance.CopyCard(card, effect.Value);
                        }
                        if (isFirseUse)
                        {
                            ownedArtifact.IsUse = true;
                        }
                        break;
                    case EnumTypes.ArtifaceEffectType.block_curse:
                        // 저주 카드 막기 효과
                        finalResult = new DamageReceiveResult { IsBlockCurse = true };
                        break;
                    // 패시브
                    case EnumTypes.ArtifaceEffectType.predict_action:
                        finalResult = new DamageReceiveResult { PredictEnemyACtion = true };
                        break;
                }
            }
        }
        return finalResult;
    }

    /// <summary>
    /// 적에게 피해를 처음 줌
    /// </summary>
    public DamageReceiveResult ArtifactAttackEnemyFirst(CharacterBase playerCharacter, CharacterBase enemyCharacter)
    {
        return ProcessArtifactEffects(playerCharacter, enemyCharacter, EnumTypes.ArtifactTriggerType.on_first_card_attack, true);
    }
    /// <summary>
    /// 적에게 피해를 줌 -> 죽이면 발동하는 트리거도 동작
    /// </summary>
    public void ArtifactAttackEnemy(CharacterBase player, CharacterBase enemy)
    {
        ProcessArtifactEffects(player, enemy, EnumTypes.ArtifactTriggerType.on_attack);

        if (enemy.isDie)
        {
            ProcessArtifactEffects(player, enemy, EnumTypes.ArtifactTriggerType.on_kill_enemy);
        }
    }
    /// <summary>
    /// 적에게 피해를 처음으로 받음
    /// </summary>
    public DamageReceiveResult ArtifactGetDamageFirst(CharacterBase playerCharacter, CharacterBase enemyCharacter)
    {
        return ProcessArtifactEffects(playerCharacter, enemyCharacter, EnumTypes.ArtifactTriggerType.on_get_damage_first, true);
    }
    /// <summary>
    /// 적에게 피해를 받음
    /// </summary>
    public DamageReceiveResult ArtifactGetDamage(CharacterBase playerCharacter, CharacterBase enemyCharacter)
    {
        return ProcessArtifactEffects(playerCharacter, enemyCharacter, EnumTypes.ArtifactTriggerType.on_get_damage);
    }
    /// <summary>
    /// 전투가 시작됨
    /// </summary>
    public void ArtifactStartBattle(CharacterBase playerCharacter, CharacterBase enemyCharacter)
    {
        ProcessArtifactEffects(playerCharacter, enemyCharacter, EnumTypes.ArtifactTriggerType.on_battle_start, true);
    }
    /// <summary>
    /// 적투가 끝남
    /// </summary>
    public void ArtifactEndBattle(CharacterBase playerCharacter, CharacterBase enemyCharacter)
    {
        ProcessArtifactEffects(playerCharacter, enemyCharacter, EnumTypes.ArtifactTriggerType.on_battle_end);
    }
    /// <summary>
    /// 나의 턴이 시작됨
    /// </summary>
    public void ArtifactStartTurn(CharacterBase playerCharacter, CharacterBase enemyCharacter)
    {
        Debug.Log("Artifact Start Turn");
        ProcessArtifactEffects(playerCharacter, enemyCharacter, EnumTypes.ArtifactTriggerType.on_action_start);
    }

    /// <summary>
    /// 나의 턴이 종료됨 -> 방어도 관련
    /// </summary>
    public void ArtifactEndTurnShield(CharacterBase playerCharacter, CharacterBase enemyCharacter)
    {
        ArtifactGetShieldConditionZero(playerCharacter);
        ProcessArtifactEffects(playerCharacter, enemyCharacter, EnumTypes.ArtifactTriggerType.on_action_end);
    }
    /// <summary>
    /// 방어도가 없는 조건일때 실행되는 효과 적용
    /// </summary>
    private void ArtifactGetShieldConditionZero(CharacterBase playerCharacter)
    {
        int shieldAmount = 0;
        var artifactList = playerCharacter.ArtifactList;
        for (int i = 0; i < artifactList.Count; i++)
        {
            var artifact = InGameData.Instance.Artifacts.Find(a => a.Id == artifactList[i].ArtifactId);
            for (int j = 0; j < artifact.ArtifactEffects.Count; j++)
            {
                var effect = artifact.ArtifactEffects[j];
                if (effect.ItemTrigger == EnumTypes.ArtifactTriggerType.on_action_end)
                {
                    // 턴 종료 시 효과 적용
                    switch (effect.ItemEffectType)
                    {
                        case EnumTypes.ArtifaceEffectType.get_shield:
                            if (effect.ExtraData != null && effect.ExtraData.ContainsKey("condition"))
                            {
                                // 조건이 있는 경우
                                var condition = effect.ExtraData["condition"] as JObject;
                                if (condition != null && condition.ContainsKey("is_shield"))
                                {
                                    bool isShield = condition["is_shield"].Value<bool>();
                                    // 방어막이 존재하지 않으면 실행
                                    if (!isShield && playerCharacter.Stats.currShield == 0)
                                    {
                                        shieldAmount += effect.Value;
                                    }
                                }
                            }
                            break;
                    }
                }
            }
        }
        if (shieldAmount > 0)
        {
            GetShield(shieldAmount, playerCharacter);
        }
    }


    /// <summary>
    /// 부활 효과 적용 -> 체력이 0이 되면 실행
    /// </summary>
    public DamageReceiveResult ArtifactRevive(CharacterBase player, CharacterBase enemyCharacter)
    {
        return ProcessArtifactEffects(player, enemyCharacter, EnumTypes.ArtifactTriggerType.on_die, true);
    }
    /// <summary>
    /// 카드를 사용할 때마다 동작.
    /// </summary>
    public void ArtifactCardUse(CharacterBase player, CharacterBase enemy)
    {
        ProcessArtifactEffects(player, enemy, EnumTypes.ArtifactTriggerType.on_use_card);
    }
    /// <summary>
    /// 카드 사용 시 첫 번째 카드 사용 효과
    /// </summary>
    public void ArtifactCardUseFirst(CharacterBase player, CardDTO card)
    {
        ProcessArtifactEffects(player, null, EnumTypes.ArtifactTriggerType.on_first_card, true, card);
        player.GetComponent<Player>()?.SetStatusIcon();
    }

    /// <summary>
    /// 카드를 드로우 할 때마다 동작.
    /// </summary>
    public void ArtifactCardDrawCurse(CharacterBase player, CharacterBase enemy)
    {
        ProcessArtifactEffects(player, enemy, EnumTypes.ArtifactTriggerType.on_draw_curse);
    }

    /// <summary>
    /// 저주 카드 막는 효과
    /// </summary>
    public DamageReceiveResult ArtifactBlockCurse(CharacterBase player, CharacterBase enemy)
    {
        return ProcessArtifactEffects(player, enemy, EnumTypes.ArtifactTriggerType.passive);
    }

    /// <summary>
    /// 적의 다음 행동 예측 효과
    /// </summary>
    public DamageReceiveResult ArtifactPredictEnemyAction(CharacterBase player, CharacterBase enemy)
    {
        return ProcessArtifactEffects(player, enemy, EnumTypes.ArtifactTriggerType.passive, false, null);
    }
    public DamageReceiveResult ArtifactHealAmount(CharacterBase player, CharacterBase enemy)
    {
        return ProcessArtifactEffects(player, enemy, EnumTypes.ArtifactTriggerType.passive, false, null);
    }

    /// <summary>
    /// 획득 시 효과 적용
    /// </summary>
    public DamageReceiveResult ArtifactOnObtainEffect(ArtifactDTO artifactData)
    {
        for (int i = 0; i < artifactData.ArtifactEffects.Count; i++)
        {
            var effect = artifactData.ArtifactEffects[i];
            if (effect.ItemTrigger == EnumTypes.ArtifactTriggerType.on_obtain)
            {
                switch (effect.ItemEffectType)
                {
                    case EnumTypes.ArtifaceEffectType.get_max_hp:
                    case EnumTypes.ArtifaceEffectType.heal_hp:
                        SetFooterText.Instance?.SetMoveText(effect.Value, EnumTypes.MoveTextType.heal);
                        SetFooterText.Instance?.SetHpBar(EnumTypes.TextMotionType.up);
                        break;
                    case EnumTypes.ArtifaceEffectType.upgrade_card:
                        return new DamageReceiveResult { UpgradeCardAmount = effect.Value, IsRandom = effect.ExtraData?["random"]?.Value<bool>() ?? false };
                    case EnumTypes.ArtifaceEffectType.remove_card:
                        return new DamageReceiveResult { DeleteCardAmount = effect.Value, IsRandom = effect.ExtraData?["random"]?.Value<bool>() ?? false };
                }
            }
        }
        return ProcessMainSceneArtifactEffect(EnumTypes.ArtifactTriggerType.on_obtain);
    }

    /// <summary>
    /// 상점 입장 시 동작
    /// </summary>
    public void ArtifactEnterShop()
    {
        ProcessMainSceneArtifactEffect(EnumTypes.ArtifactTriggerType.on_shop_enter);
    }
    /// <summary>
    /// 상점 할인
    /// </summary>
    public DamageReceiveResult ArtifactShopDiscount()
    {
        return ProcessMainSceneArtifactEffect(EnumTypes.ArtifactTriggerType.on_shop_enter);
    }


    private void ArtifactExecuteEffect(CharacterBase target, ArtifactEffectDTO effect)
    {
        if (target?.Stats.currHp <= effect.Value && effect.ItemEffectType == EnumTypes.ArtifaceEffectType.execute)
        {
            target.isDie = true;
        }
    }

    private void ArtifactDrawCardEffect(ArtifactEffectDTO effect)
    {
        // 카드 드로우 효과 처리
        int drawCount = effect.Value;
        CardFunction.Instance?.DrawCard(null, drawCount, null);
    }

    private async void ArtifactGetCoin(ArtifactEffectDTO effect)
    {
        // 코인 획득 효과 처리
        int coinAmount = effect.Value;
        var SCENARIO_DATA = GetCurrData();
        await SupabaseGetScenarioCoin.Instance.GetCoin(coinAmount, SCENARIO_DATA);
        BattleManager.Instance.GetCoinIngameData(coinAmount);
    }
    private void GetHeal(CharacterBase target, int amount)
    {
        // Heal effect
        target.GetComponent<Player>()?.GetHeal(amount);
    }
    private void GetShield(int amount, CharacterBase targetCharacter)
    {
        // Shield effect
        targetCharacter.GetShieldBase(amount);
    }
    private void GetStatus(CharacterBase targetCharacter, EnumTypes.Status statusType, int value, EnumTypes.Target target, JObject extraData = null)
    {
        // Status effect]
        if (extraData != null && extraData.ContainsKey("status_id"))
        {
            int statusId = (int)extraData["status_id"];

            if (target == EnumTypes.Target.enemys)
            {
                // 적에게 상태 적용
                foreach (var enemy in EnemyManager.Instance.enemies)
                {
                    if (enemy.isDie) continue;
                    enemy.GetStatusBase(statusId, statusType, value);
                }
            }
            else
            {
                targetCharacter.GetStatusBase(statusId, statusType, value);
            }
            return;
        }
        if (extraData != null && extraData.ContainsKey("random_status"))
        {
            Debug.Log("랜덤 상태 적용");
            if (target == EnumTypes.Target.enemys)
            {
                // 적에게 랜덤 상태 적용
                foreach (var enemy in EnemyManager.Instance.enemies)
                {
                    if (enemy.isDie) continue;
                    enemy.GetRandomStatus(statusType);
                }
            }
            else
            {
                targetCharacter.GetRandomStatus(statusType);
            }
        }
    }
}
