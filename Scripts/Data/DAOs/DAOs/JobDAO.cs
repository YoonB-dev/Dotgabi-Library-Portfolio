using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class JobDAO : Singleton<JobDAO>
{
    public async Task<List<JobDTO>> GetAllJobAsync(EnumTypes.LanguageType language)
    {
        var response = await SupabaseClientProvider.Instance.ClientGameData
        .From<JobEntity>()
        .Get();

        var jobList = response.Models;

        return jobList.Select(view => new JobDTO {
            Id = view.Id,
            Name = view.JobLocales.FirstOrDefault(locale => locale.LanguageCode == language)?.Name ?? "Unknown",
            Description = view.JobLocales.FirstOrDefault(locale => locale.LanguageCode == language)?.Description ?? "No description",
            ImgPath = view.ImgPath,
            ImgFacePath = view.ImageFacePath,
            StartHP = view.StartHP,
            StartCoin = view.StartCoin
        }).ToList();
    }
}
