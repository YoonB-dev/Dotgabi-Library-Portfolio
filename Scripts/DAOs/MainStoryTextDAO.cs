using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class MainStoryTextDAO : Singleton<MainStoryTextDAO>
{
    public async Task<List<MainStoryDTO>> GetAllMainStoryAsync(EnumTypes.LanguageType languageType)
    {
        var storyEntity = await SupabaseClientProvider.Instance.ClientGameData
            .From<MainStoryEntity>()
            .Select("*, main_story_text_locale(*)")
            .Order("text_id", Supabase.Postgrest.Constants.Ordering.Ascending)
            .Get();

        var chooseEntity = await SupabaseClientProvider.Instance.ClientGameData
            .From<MainStoryChooseEntity>()
            .Select("*, main_story_choose_locale(*)")
            .Order("id", Supabase.Postgrest.Constants.Ordering.Ascending)
            .Get();

        var resultEntity = await SupabaseClientProvider.Instance.ClientGameData
            .From<MainStoryResultEntity>()
            .Select("*, main_story_result_locale(*)")
            .Order("result_id", Supabase.Postgrest.Constants.Ordering.Ascending)
            .Get();


        var storyList = storyEntity.Models;
        var chooseList = chooseEntity.Models;
        var resultList = resultEntity.Models;

        var mainStoryDTOList = storyList.ConvertAll(story => new MainStoryDTO {
            TextId = story.TextId,
            TextTrigger = story.TextTrigger,
            TextTarget = story.TextTarget,
            NextTextId = story.NextTextId,
            StoryText = story.StoryTextLocal.FirstOrDefault(locale => locale.LanguageType == languageType)?.Text ?? "No text available",
            ExtraData = story.ExtraData,
            ChooseList = GetChooseListByTextId(story.TextId, chooseList, languageType, resultList),
        });

        return mainStoryDTOList;
    }

    public List<MainStoryChooseDTO> GetChooseListByTextId(int textId, List<MainStoryChooseEntity> chooseList, EnumTypes.LanguageType languageType, List<MainStoryResultEntity> resultList)
    {
        var filteredChooses = chooseList.Where(choose => choose.TextId == textId).ToList().OrderBy(choose => choose.ChooseOrder).ToList();

        return filteredChooses.ConvertAll(choose => new MainStoryChooseDTO {
            Id = choose.Id,
            ChooseText = choose.ChooseTextLocal.FirstOrDefault(locale => locale.LanguageType == languageType)?.Text ?? "No text available",
            NextTextId = choose.NextTextId,
            ResultList = GetResultListByChooseId(choose.Id, resultList, languageType),
            ExtraData = choose.ExtraData
        });
    }

    public List<MainStoryResultDTO> GetResultListByChooseId(int chooseId, List<MainStoryResultEntity> resultList, EnumTypes.LanguageType languageType)
    {
        var filteredResults = resultList.Where(result => result.ChooseId == chooseId).ToList();

        return filteredResults.ConvertAll(result => new MainStoryResultDTO {
            Id = result.Id,
            NextTextId = result.NextTextId,
            ResultText = result.ResultTextLocal.FirstOrDefault(locale => locale.LanguageType == languageType)?.Text ?? "No text available",
            ExtraData = result.ExtraData
        });
    }
}
