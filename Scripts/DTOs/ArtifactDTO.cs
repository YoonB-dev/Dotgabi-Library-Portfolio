using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ArtifactDTO
{
    [field: SerializeField] public int Id { get; set; }
    [field: SerializeField] public string Name { get; set; }
    [field: SerializeField] public string Ability { get; set; }
    [field: SerializeField] public string FlavorText { get; set; }
    [field: SerializeField] public string ImageUrl { get; set; }
    [field: SerializeField] public EnumTypes.RarityType Rarity { get; set; }
    [field: SerializeField] public List<ArtifactEffectDTO> ArtifactEffects { get; set; }
    [field: SerializeField] public string Place { get; set; }
    [field: SerializeField] public bool IsIcon { get; set; } // 아이콘 여부

}
