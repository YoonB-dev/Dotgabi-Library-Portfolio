using System.Collections;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class PassiveFunction : SceneSingleton<PassiveFunction>
{
    /// <summary>
    /// 패시브 기능을 실행하는 함수
    /// 이 함수는 적의 패시브 능력에 따라 행동을 수행합니다.
    /// character: 행동을 수행할 대상 캐릭터
    /// target: 행동의 대상 캐릭터
    /// </summary>
    public IEnumerator PassiveAction(EnumTypes.EnemyPassiveTrigger trigger, CharacterBase character, CharacterBase target)
    {
        for (int i = 0; i < character.PassiveList.Count; i++)
        {
            var passive = character.PassiveList[i];
            if (passive.PassiveAbilities == null || passive.PassiveAbilities.Count == 0) continue;
            for (int k = 0; k < passive.PassiveAbilities.Count; k++)
            {
                var passiveAbility = passive.PassiveAbilities[k];
                if (passiveAbility.PassiveTrigger == trigger)
                {
                    switch (passiveAbility.Action)
                    {
                        case EnumTypes.Action.attack:
                            if (passiveAbility.ExtraData != null && passiveAbility.ExtraData.ContainsKey("condition"))
                            {
                                var condition = passiveAbility.ExtraData["condition"] as JObject;
                                if (condition.ContainsKey("player_no_card"))
                                {
                                    var isNoCard = condition["player_no_card"].Value<bool>();
                                    if (isNoCard && CardSystem.Instance?.cards.Count == 0)
                                    {
                                        int damage = passiveAbility.Value ?? 0;
                                        yield return StartCoroutine(character.GetComponent<Enemy>().AttackPlayer(damage, null));
                                    }
                                    else
                                    {
                                        Debug.Log($"패시브 조건 미충족: 플레이어가 카드가 있음");
                                    }
                                }
                            }
                            else
                            {
                                int damage = passiveAbility.Value ?? 0;
                                yield return StartCoroutine(character.GetComponent<Enemy>().AttackPlayer(damage, null));
                            }
                            break;
                        case EnumTypes.Action.action:
                            yield return StartCoroutine(SpecialAction(passiveAbility, character, target));
                            break;
                        case EnumTypes.Action.shield:
                            if (passiveAbility.ExtraData != null && passiveAbility.ExtraData.ContainsKey("condition"))
                            {
                                // 조건에 따라 적용하게 하기
                            }
                            else
                            {
                                int shieldAmount = passiveAbility.Value ?? 0;
                                switch (passiveAbility.Target)
                                {
                                    case EnumTypes.Target.self:
                                        target.GetShieldBase(shieldAmount);
                                        break;
                                    case EnumTypes.Target.enemy:
                                        character.GetShieldBase(shieldAmount);
                                        break;
                                    case EnumTypes.Target.enemys:
                                        foreach (var enemy in EnemyManager.Instance?.enemies)
                                        {
                                            if (enemy != null && !enemy.isDie)
                                            {
                                                enemy.GetShieldBase(shieldAmount);
                                            }
                                        }
                                        break;
                                }
                            }
                            break;
                        case EnumTypes.Action.heal:
                            if (passiveAbility.ExtraData != null && passiveAbility.ExtraData.ContainsKey("condition"))
                            {
                                // 조건에 따라 적용하게 하기
                            }
                            else
                            {
                                int healAmount = passiveAbility.Value ?? 0;
                                switch (passiveAbility.Target)
                                {
                                    case EnumTypes.Target.self:
                                        target.GetHealBase(healAmount);
                                        break;
                                    case EnumTypes.Target.enemy:
                                        character.GetHealBase(healAmount);
                                        break;
                                    case EnumTypes.Target.enemys:
                                        foreach (var enemy in EnemyManager.Instance?.enemies)
                                        {
                                            if (enemy != null && !enemy.isDie)
                                            {
                                                enemy.GetHealBase(healAmount);
                                            }
                                        }
                                        break;
                                    default:
                                        Debug.LogWarning($"Unknown target type: {passiveAbility.Target}");
                                        break;
                                }
                            }
                            break;
                        case EnumTypes.Action.buff:
                        case EnumTypes.Action.debuff:
                            var buffType = passiveAbility.Action == EnumTypes.Action.buff ? EnumTypes.Status.buff : EnumTypes.Status.debuff;
                            if (passiveAbility.ExtraData.ContainsKey("condition"))
                            {
                                // 조건에 따라 적용하게 하기
                            }
                            else if (passiveAbility.ExtraData.ContainsKey("status_id"))
                            {
                                int statusId = int.Parse(passiveAbility.ExtraData["status_id"].ToString());
                                int statusValue = passiveAbility.Value ?? 0;
                                switch (passiveAbility.Target)
                                {
                                    case EnumTypes.Target.self:
                                        target.GetStatusBase(statusId, buffType, statusValue);
                                        break;
                                    case EnumTypes.Target.enemy:
                                        character.GetStatusBase(statusId, buffType, statusValue);
                                        break;
                                    case EnumTypes.Target.enemys:
                                        foreach (var enemy in EnemyManager.Instance?.enemies)
                                        {
                                            if (enemy != null && !enemy.isDie)
                                            {
                                                enemy.GetStatusBase(statusId, buffType, statusValue);
                                            }
                                        }
                                        break;
                                    default:
                                        Debug.LogWarning($"Unknown target type: {passiveAbility.Target}");
                                        break;
                                }
                            }
                            else if (passiveAbility.ExtraData.ContainsKey("random_status"))
                            {
                                Debug.Log("랜덤 상태 부여");
                                switch (passiveAbility.Target)
                                {
                                    case EnumTypes.Target.self:
                                        target.GetRandomStatus(buffType);
                                        break;
                                    case EnumTypes.Target.enemy:
                                        character.GetRandomStatus(buffType);
                                        break;
                                    case EnumTypes.Target.enemys:
                                        foreach (var enemy in EnemyManager.Instance?.enemies)
                                        {
                                            if (enemy != null && !enemy.isDie)
                                            {
                                                enemy.GetRandomStatus(buffType);
                                            }
                                        }
                                        break;
                                }
                            }
                            break;

                    }
                }
            }
        }
    }

    /// <summary>
    /// 적의 행동 능력을 실행하는 함수
    /// character: 행동을 수행할 대상 캐릭터
    /// target: 행동의 대상 캐릭터
    /// </summary>
    private IEnumerator SpecialAction(EnemyPassiveAbilityDTO abilityDTO, CharacterBase character, CharacterBase target)
    {
        if (abilityDTO.ExtraData == null) yield break;

        if (abilityDTO.ExtraData.ContainsKey("eat_card"))
        {
            int eatCardCount = abilityDTO.Value ?? 0;
            for (int i = 0; i < eatCardCount; i++)
            {
                if (CardSystem.Instance.cards.Count > 0)
                {
                    CardSystem.Instance.RemoveHandCard();
                    AudioManager.Instance.EatSound();
                    yield return new WaitForSeconds(0.5f); // 카드 제거 후 잠시 대기
                }
            }
        }

        if (abilityDTO.ExtraData.ContainsKey("rabbit_passive"))
        {
            // 토끼 패시브 능력: 간 빼먹기 카드 세팅
            CardDTO cardData = InGameData.Instance.Cards.Find(c => c.Id == 54);
            yield return StartCoroutine(CardSystem.Instance.CardCreateMotion(cardData));
            CardSystem.Instance.SuffleDeck(10);
        }
    }
}
