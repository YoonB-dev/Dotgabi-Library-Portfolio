using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class EventSmallDAO : Singleton<EventSmallDAO>
{
    public async Task<List<EventSmallDTO>> GetAllEventSmallsAsync(EnumTypes.LanguageType language)
    {
        var response = await SupabaseClientProvider.Instance.ClientGameData
            .From<EventSmallEntity>()
            .Select("*, event_small_locale(*)")
            .Filter("event_small_locale.lan_code", Supabase.Postgrest.Constants.Operator.Equals, language.ToString())
            .Get();

        var eventList = response.Models;
        return eventList.ConvertAll(log => new EventSmallDTO {
            Id = log.Id,
            AmountMin = log.AmountMin,
            AmountMax = log.AmountMax,
            EventType = log.EventType,
            Text = log.SmallEventLocal.Find(locale => locale.EventSmallId == log.Id)?.Text ?? "No text available"
        });
    }
}
