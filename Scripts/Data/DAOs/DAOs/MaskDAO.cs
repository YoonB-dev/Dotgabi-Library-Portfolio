using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class MaskDAO : Singleton<MaskDAO>
{
    public async Task<List<MaskDTO>> GetAllMasksAsync(EnumTypes.LanguageType lanCode)
    {
        var response = await SupabaseClientProvider.Instance.ClientGameData
            .From<MaskEntity>()
            .Get();

        var maskLocaleEntity = await SupabaseClientProvider.Instance.ClientGameData
            .From<MaskLocaleEntity>()
            .Where(x => x.LanguageCode == lanCode)
            .Get();

        var maskList = response.Models;
        var maskLocalesList = maskLocaleEntity.Models;


        var list = maskList.Select(mask => {
            var locale = maskLocalesList.FirstOrDefault(l => l.Id == mask.Id);

            return new MaskDTO {
                MaskId = mask.Id,
                Name = locale?.Name ?? "Unknown",
                Description = locale?.Description ?? "No description",
                ImgPath = mask.ImgPath
            };
        }).ToList();

        return list;
    }
}
