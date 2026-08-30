using UnityEngine;

public class UserOwnCardFrameDTO
{
    [field: SerializeField] public int CardFrameId { get; set; }
    [field: SerializeField] public int Count { get; set; }
    [field: SerializeField] public EnumTypes.ShopItemType CardFrameType { get; set; }
}

public class UserOwnCharacterDTO
{
    [field: SerializeField] public bool OwnedBlacksmith { get; set; }
    [field: SerializeField] public bool OwnedDosa { get; set; }
    [field: SerializeField] public bool OwnedPerformer { get; set; }
}