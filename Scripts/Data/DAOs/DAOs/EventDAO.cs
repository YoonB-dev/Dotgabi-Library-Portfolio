using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class EventDAO : Singleton<EventDAO>
{
    private List<EventChoiceEntity> eventChoices;
    private List<EventResultEntity> eventResults;
    public async Task<List<EventDTOList>> GetAllEventsAsync(EnumTypes.LanguageType language)
    {
        // 1. 전체 이벤트 데이터 조회
        var response = await SupabaseClientProvider.Instance.ClientGameData
            .From<EventEntity>()
            .Get();
        // 2. Choice 데이터 조회
        var responseChoice = await SupabaseClientProvider.Instance.ClientGameData
            .From<EventChoiceEntity>()
            .Get();
        eventChoices = responseChoice.Models;
        // 3. Result 데이터 조회
        var responseResult = await SupabaseClientProvider.Instance.ClientGameData
            .From<EventResultEntity>()
            .Get();
        eventResults = responseResult.Models;


        // 4. event_num별로 그룹화
        var grouped = response.Models
            .GroupBy(e => new { e.EventNum })
            .Select(g => new EventDTOList {
                EventNum = g.Key.EventNum,
                Place = g.FirstOrDefault()?.Place ?? "public",
                EventList = g
                    .OrderBy(e => e.EventOrder)
                    .Select(e => new EventDTO {
                        ImgPath = e.ImgPath,
                        EventText = e.EventLocales
                            .Find(l => l.LanguageCode == language)?.EventText ?? "No description",
                        EventChoices = GetAllEventChoices(e.eventId, language)
                    })
                    .ToList()

            })
            .ToList();

        return grouped;
    }

    public List<EventChoiceDTO> GetAllEventChoices(int eventId, EnumTypes.LanguageType language)
    {
        // 1. 전체 이벤트 선택지 데이터 조회
        var choices = eventChoices
            .Where(c => c.EventId == eventId)
            .OrderBy(c => c.OrderIndex)
            .Select(c => {
                var resultList = eventResults
                    .Where(r => r.ChoiceId == c.EventChoiceId)
                    .OrderBy(r => r.ResultId)
                    .Select(r => new EventResultDTO {
                            ResultId = r.ResultId,
                            ResultText = r.EventResultLocale.Find(l => l.LanguageCode == language)?.ResultText ?? "No result text",
                            Weight = r.Weight,
                            ResultType = r.ResultType,
                            ResultAction = r.ResultAction,
                            ExtraData = r.ExtraData
                        })
                    .ToList();

                var resultBundles = GroupResultBundles(resultList);

                return new EventChoiceDTO {
                    EventId = c.EventId,
                    ChoiceText = c.EventChoiceLocale.Find(l => l.LanguageCode == language)?.ChoiceText ?? "No choice text",
                    EventResult = GetRandomResultByWeight(resultBundles),
                };
            }).ToList();

        return choices;
    }

    public List<EventResultBundle> GroupResultBundles(List<EventResultDTO> results)
    {
        var bundles = new List<EventResultBundle>();
        int i = 0;

        while (i < results.Count)
        {
            var bundleResults = new List<EventResultDTO> { results[i] };

            // chain이면 다음 end/continue까지 묶기

            var weight = results[i].Weight;
            var bundleText = results[i].ResultText;

            if (results[i].ResultType == "chain")
            {
                int j = i+1;

                while (j < results.Count && results[j].ResultType == "chain")
                {
                    bundleResults.Add(results[j]);
                    j++;
                }
                if (j < results.Count && (results[j].ResultType == "end" || results[j].ResultType == "continue"))
                {
                    bundleResults.Add(results[j]);
                    j++;
                }
                i = j;
            }
            else
            {
                i++;
            }

            bundles.Add(new EventResultBundle
            {
                ResultDTOs = bundleResults,
                ResultText = bundleText,
                Weight = weight
            });
        }
        return bundles;
    }

    public EventResultBundle GetRandomResultByWeight(List<EventResultBundle> bundles)
    {
        int totalWeight = bundles.Sum(r => r.Weight);
        int randomValue = Random.Range(1, totalWeight + 1); // inclusive

        foreach (var result in bundles)
        {
            randomValue -= result.Weight;
            if (randomValue <= 0) return result;
        }

        return bundles.First(); // fallback, should not happen
    }
}
