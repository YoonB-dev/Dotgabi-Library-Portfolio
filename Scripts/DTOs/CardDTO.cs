using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class CardDTO
{
    [field: SerializeField] public int Id { get; set; }
    [field: SerializeField] public string Name { get; set; }
    [field: SerializeField] public string Description { get; set; }
    [field: SerializeField] public EnumTypes.CardType CardType { get; set; }
    [field: SerializeField] public List<int> Cost { get; set; }
    [field: SerializeField] public string ImageUrl { get; set; }
    [field: SerializeField] public List<int> CardJob { get; set; }
    [field: SerializeField] public List<CardActionDTO> CardActions { get; set; }
    [field: SerializeField] public int CardUpgrade = 0; // 카드 업그레이드 레벨, 기본값은 0

    public CardDTO Copy()
    {
        return new CardDTO {
            Id = this.Id,
            Name = this.Name,
            Description = this.Description,
            CardType = this.CardType,
            Cost = this.Cost,
            ImageUrl = this.ImageUrl,
            CardJob = new List<int>(this.CardJob),
            CardActions = this.CardActions != null ? this.CardActions.Select(action => action.DeepCopy()).ToList() : null,
            CardUpgrade = this.CardUpgrade,
        };
    }
}

[System.Serializable]
public class EquipDTO
{
    public CardDTO cardDTO;
    public int equipAmount; // 장비 현재 수치 (몇번 충족했는 지 등, 조건 검사용.)
}


