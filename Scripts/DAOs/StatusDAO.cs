using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class StatusDAO : Singleton<StatusDAO>
{
    public async Task<List<StatusDTO>> GetAllStatusesAsync(EnumTypes.LanguageType language)
    {
        var responseStatus = await SupabaseClientProvider.Instance.ClientGameData
            .From<StatusEntity>()
            .Get();

        var responseStatusLocales = await SupabaseClientProvider.Instance.ClientGameData
            .From<StatusLocaleEntity>()
            .Where(x => x.LanguageCode == language)
            .Get();

        var statusList = responseStatus.Models;
        var statusLocalesList = responseStatusLocales.Models;

        // 2. 조인 후 DTO 작성
        var list = statusList.Select(status => {
            var locale = statusLocalesList.FirstOrDefault(l => l.Id == status.Id && l.StatusType == status.StatusType);
            if (locale == null)
            {

            }
            return new StatusDTO {
                Id = status.Id,
                StatusType = status.StatusType,
                Name = locale?.Name ?? "Unknown",
                Description = locale?.Description ?? "No description",
                ImgPath = status.ImgPath
            };
        }).ToList();

        return list;
    }
}
