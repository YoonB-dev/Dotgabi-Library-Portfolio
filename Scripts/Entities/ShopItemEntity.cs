using System.Collections.Generic;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using UnityEngine;

[Table("shop_item")]
public class ShopItemEntity : BaseModel
{
    [PrimaryKey("item_id")]
    [Column("item_id")] public int ItemId { get; set; }
    [Column("item_price")] public int? ItemPrice { get; set; }
    [Column("price_type")] public string PriceType { get; set; }
    [Column("item_type")] public EnumTypes.ShopItemType ItemType { get; set; }
    [Column("item_source")] public EnumMainType.ItemSourceType ItemSource { get; set; }
    [Column("item_value")] public int ItemValue { get; set; }
    [Column("count")] public int Count { get; set; }
    [Column("img_path")] public string ImgPath { get; set; }
    [Reference(typeof(ShopItemLocaleEntity), useInnerJoin: false)]
    public List<ShopItemLocaleEntity> ShopItemLocale { get; set; } = new ();
}


[Table("shop_item_locales")]
public class ShopItemLocaleEntity : BaseModel
{
    [Column("item_id")] public int ItemId { get; set; }
    [Column("item_name")] public string ItemName { get; set; }
    [Column("item_description")] public string ItemDescription { get; set; }
    [Column("lan_code")] public EnumTypes.LanguageType LanCode { get; set; }
}