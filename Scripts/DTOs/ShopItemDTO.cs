using UnityEngine;

public class ShopItemDTO
{
    public int ItemId { get; set; }
    public int? ItemPrice { get; set; }
    public string PriceType { get; set; }
    public EnumTypes.ShopItemType ItemType { get; set; }
    public EnumMainType.ItemSourceType ItemSource { get; set; }
    public int ItemValue { get; set; }
    public int Count { get; set; }
    public string ImgPath { get; set; }
    public string ItemName { get; set; }
    public string ItemDescription { get; set; }
}
