using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class SummonDAO : Singleton<SummonDAO>
{
    public async Task<List<SummonDTO>> GetAllSummonsAsync(EnumTypes.LanguageType lanCode)
    {
        var summonEntities = await SupabaseClientProvider.Instance.ClientGameData
            .From<SummonEntity>()
            .Get();

        var summonLocaleEntities = await SupabaseClientProvider.Instance.ClientGameData
            .From<SummonLocalesEntity>()
            .Where(x => x.LanguageCode == lanCode)
            .Get();

        var summonList = summonEntities.Models;
        var summonLocalesList = summonLocaleEntities.Models;

        var list = summonList.Select(summon => {
            var locale = summonLocalesList.FirstOrDefault(l => l.SummonId == summon.SummonId);

            return new SummonDTO
            {
                Id = summon.SummonId,
                Name = locale?.Name ?? "Unknown",
                Description = locale?.Description ?? "No description",
                ImgPath = summon.ImgPath
            };
        }).ToList();

        return list;
    }
}