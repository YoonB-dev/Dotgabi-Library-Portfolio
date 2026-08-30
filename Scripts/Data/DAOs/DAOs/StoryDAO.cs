using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class StoryDAO : Singleton<StoryDAO>
{
    public async Task<List<StoryDTO>> GetAllStoriesAsync(EnumTypes.LanguageType lanCode)
    {
        var response = await SupabaseClientProvider.Instance.ClientGameData
            .From<StoryEntity>()
            .Get();

        Debug.Log($"GetAllStoriesAsync: {response.Models.Count} stories loaded");


        return response.Models.ConvertAll(entity => new StoryDTO {
            StoryId = entity.StoryId,
            ImgPath = entity.ImgPath,
            Name = entity.StoryLocales.Find(locale => locale.LanguageCode == lanCode)?.Name ?? "Unknown",
            Description = entity.StoryLocales.Find(locale => locale.LanguageCode == lanCode)?.Description ?? "No description"
        });
    }

    public async Task<List<MainStoryItemDTO>> GetAllMainStoryItemsAsync(EnumTypes.LanguageType lanCode)
    {
        var response = await SupabaseClientProvider.Instance.ClientGameData
            .From<MainStoryItemEntity>()
            .Select("*, main_story_item_locale(*)")
            .Get();

        Debug.Log($"GetAllMainStoryItemsAsync: {response.Models.Count} items loaded");

        return response.Models.ConvertAll(entity => new MainStoryItemDTO {
            Id = entity.Id,
            ItemType = entity.ItemType,
            ImgPath = entity.ImgPath,
            ExtraData = entity.ExtraData,
            Name = entity.MainStoryItemLocales.Find(locale => locale.LanguageCode == lanCode)?.Name ?? "Unknown",
            Description = entity.MainStoryItemLocales.Find(locale => locale.LanguageCode == lanCode)?.Description ?? "No description"
        });
    }
}
