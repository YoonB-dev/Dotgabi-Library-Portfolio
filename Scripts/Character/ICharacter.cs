using System.Collections.Generic;
using UnityEngine;

public interface ICharacter
{
    Stat Stats { get; set; }
    List<StatusData> Buffs { get; set; }
    List<StatusData> Debuffs { get; set; }

    public void TakeDamageBase(int damage, int shieldDamage = 1, bool isPentration = false);
    public void GetShieldBase(int shieldAmount);
}
