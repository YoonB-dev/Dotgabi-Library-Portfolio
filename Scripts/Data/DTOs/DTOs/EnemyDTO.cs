using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyDTO
{
    [field: SerializeField] public int Id { get; set; }
    [field: SerializeField] public string Name { get; set; }
    [field: SerializeField] public string Description { get; set; }
    [field: SerializeField] public string FlavorText { get; set; }
    [field: SerializeField] public List<EnemyAbilityDTO> EnemyAbilities { get; set; }
    [field: SerializeField] public string ImgPath { get; set; }
    [field: SerializeField] public string ImgSpinePath { get; set; }
    [field: SerializeField] public int Count { get; set; }
    [field: SerializeField] public int HealthMin { get; set; }
    [field: SerializeField] public int HealthMax { get; set; }
    [field: SerializeField] public int AttackMin { get; set; }
    [field: SerializeField] public int AttackMax { get; set; }
    [field: SerializeField] public int DefenseMin { get; set; }
    [field: SerializeField] public int DefenseMax { get; set; }
    [field: SerializeField] public int HealMin { get; set; }
    [field: SerializeField] public int HealMax { get; set; }
    [field: SerializeField] public string Stage { get; set; }
    [field: SerializeField] public string ImgFacePath { get; set; }
    [field: SerializeField] public List<EnemyPassiveDTO> Passive { get; set; }
    public EnemyDTO Copy()
    {
        return new EnemyDTO {
            Id = this.Id,
            Name = this.Name,
            Description = this.Description,
            FlavorText = this.FlavorText,
            EnemyAbilities = new List<EnemyAbilityDTO>(this.EnemyAbilities ?? new List<EnemyAbilityDTO>()),
            ImgPath = this.ImgPath,
            ImgSpinePath = this.ImgSpinePath,
            Count = this.Count,
            HealthMin = this.HealthMin,
            HealthMax = this.HealthMax,
            AttackMin = this.AttackMin,
            AttackMax = this.AttackMax,
            DefenseMin = this.DefenseMin,
            DefenseMax = this.DefenseMax,
            HealMin = this.HealMin,
            HealMax = this.HealMax,
            Stage = this.Stage,
            ImgFacePath = this.ImgFacePath,
            Passive = new List<EnemyPassiveDTO>(this.Passive ?? new List<EnemyPassiveDTO>())
        };
    }
}
