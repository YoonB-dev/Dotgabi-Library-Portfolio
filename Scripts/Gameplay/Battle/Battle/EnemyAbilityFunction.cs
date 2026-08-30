using System.Collections;
using UnityEngine;

public class EnemyAbilityFunction : Singleton<EnemyAbilityFunction>
{
    /// <summary>
    /// 적 능력치를 실행하는 함수
    /// 이 함수는 적의 능력에 따라 행동을 수행합니다.
    /// enemy: 적 캐릭터
    /// player: 플레이어 캐릭터
    /// </summary>
    public IEnumerator EnemyAbilityAction(Enemy enemy, Player player, int abilityIndex = 0)
    {
        var ability = enemy.enemyDTO.EnemyAbilities[abilityIndex];
        for (int e = 0; e < ability.Abilities.Count; e++)
        {
            var abilityDetail = ability.Abilities[e];
            switch (abilityDetail.Type)
            {
                case EnumTypes.EnemyActionType.attack:
                    int damage = abilityDetail.Value;
                    switch (abilityDetail.Target)
                    {
                        case EnumTypes.Target.player:
                            yield return enemy.AttackPlayer(damage, abilityDetail.ExtraData);
                            break;
                    }
                    break;
                case EnumTypes.EnemyActionType.heal:
                    int healAmount = abilityDetail.Value;
                    yield return enemy.GetHealEnemy(healAmount);
                    break;
                case EnumTypes.EnemyActionType.shield:
                    int shieldAmount = abilityDetail.Value;
                    switch (abilityDetail.Target)
                    {
                        case EnumTypes.Target.enemy:
                            yield return enemy.GetShieldEnemy(shieldAmount);
                            break;
                        case EnumTypes.Target.enemys:
                            foreach (var en in EnemyManager.Instance?.enemies)
                            {
                                if (en != null && !en.isDie)
                                {
                                    yield return en.GetShieldEnemy(shieldAmount);
                                }
                            }
                            break;
                    }
                    break;
                case EnumTypes.EnemyActionType.buff:
                case EnumTypes.EnemyActionType.debuff:
                    var statusType = abilityDetail.Type == EnumTypes.EnemyActionType.buff ? EnumTypes.Status.buff : EnumTypes.Status.debuff;
                    if (abilityDetail.ExtraData != null && abilityDetail.ExtraData.ContainsKey("status_id"))
                    {
                        int statusId = int.Parse(abilityDetail.ExtraData["status_id"].ToString());
                        switch (abilityDetail.Target)
                        {
                            case EnumTypes.Target.player:
                                player.GetStatusBase(statusId, statusType, abilityDetail.Value);
                                break;
                            case EnumTypes.Target.enemy:
                                enemy.GetStatusBase(statusId, statusType, abilityDetail.Value);
                                break;
                            case EnumTypes.Target.enemys:
                                foreach (var en in EnemyManager.Instance?.enemies)
                                {
                                    if (en != null && !en.isDie)
                                    {
                                        en.GetStatusBase(statusId, statusType, abilityDetail.Value);
                                    }
                                }
                                break;
                        }
                    }
                    break;
                case EnumTypes.EnemyActionType.get_max_hp:
                    int maxHpAmount = abilityDetail.Value;
                    switch (abilityDetail.Target)
                    {
                        case EnumTypes.Target.enemy:
                            enemy.GetMaxHpBase(maxHpAmount);
                            break;
                        case EnumTypes.Target.enemys:
                            foreach (var en in EnemyManager.Instance?.enemies)
                            {
                                if (en != null && !en.isDie)
                                {
                                    en.GetMaxHpBase(maxHpAmount);
                                }
                            }
                            break;
                    }
                    break;
            }
        }
    }


}
