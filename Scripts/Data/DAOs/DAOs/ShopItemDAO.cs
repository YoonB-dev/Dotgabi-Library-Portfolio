using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ShopItemDAO : Singleton<ShopItemDAO>
{
    public async Task<List<ShopItemDTO>> GetAllShopItemsAsync(EnumTypes.LanguageType language)
    {
        var response = await SupabaseClientProvider.Instance.ClientGameData
            .From<ShopItemEntity>()
            .Get();

        Debug.Log($"GetAllShopItemsAsync: {response.Models.Count} shop items loaded");

        return response.Models.ConvertAll(entity => new ShopItemDTO {
            ItemId = entity.ItemId,
            ItemPrice = entity.ItemPrice,
            PriceType = entity.PriceType,
            ItemType = entity.ItemType,
            ItemSource = entity.ItemSource,
            ItemValue = entity.ItemValue,
            Count = entity.Count,
            ImgPath = entity.ImgPath,
            ItemName = entity.ShopItemLocale.Find(locale => locale.LanCode == language)?.ItemName ?? "Unknown",
            ItemDescription = entity.ShopItemLocale.Find(locale => locale.LanCode == language)?.ItemDescription ?? "No description"
        });
    }

    public async Task<List<ShopItemDTO>> GetShopItemsByTypeAsync(EnumTypes.ShopItemType itemType, EnumTypes.LanguageType language)
    {
        var response = await SupabaseClientProvider.Instance.ClientGameData
            .From<ShopItemEntity>()
            .Where(x => x.ItemType == itemType)
            .Filter("item_source", Supabase.Postgrest.Constants.Operator.Equals, EnumMainType.ItemSourceType.shop.ToString())
            .Get();

        Debug.Log($"GetShopItemsByTypeAsync: {response.Models.Count} shop items of type {itemType} loaded");

        return response.Models.ConvertAll(entity => new ShopItemDTO {
            ItemId = entity.ItemId,
            ItemPrice = entity.ItemPrice,
            PriceType = entity.PriceType,
            ItemType = entity.ItemType,
            ItemSource = entity.ItemSource,
            ItemValue = entity.ItemValue,
            Count = entity.Count,
            ImgPath = entity.ImgPath,
            ItemName = entity.ShopItemLocale.Find(locale => locale.LanCode == language)?.ItemName ?? "Unknown",
            ItemDescription = entity.ShopItemLocale.Find(locale => locale.LanCode == language)?.ItemDescription ?? "No description"
        });
    }
}
