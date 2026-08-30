using UnityEngine;

[System.Serializable]
public class UserDTO
{
    [field: SerializeField] public string AuthId { get; set; }
    [field: SerializeField] public string Email { get; set; }
    public UserGoodsDTO UserGoods { get; set; } = new UserGoodsDTO();
    [field: SerializeField] public int? SelectCardFrameId { get; set; }
    [field: SerializeField] public int? SelectCardDecoId { get; set; }
    [field: SerializeField] public EnumMainType.ScenarioType CurrScenarioType { get; set; }
    [field: SerializeField] public bool IsTutorial { get; set; }
}
