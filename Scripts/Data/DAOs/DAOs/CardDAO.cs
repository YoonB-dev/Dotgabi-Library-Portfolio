using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class CardDAO : Singleton<CardDAO>
{
    public async Task<List<CardDTO>> GetAllCardsAsync(EnumTypes.LanguageType language)
    {
        var response = await SupabaseClientProvider.Instance.ClientGameData
            .From<CardEntity>()
            .Where(x => x.LanguageCode == language)
            .Get();

        return response.Models.Select(view => new CardDTO{
            Id = view.Id,
            Name = view.CardName,
            Description = view.CardDescription,
            CardType = view.CardType,
            ImageUrl = view.ImgPath,
            CardJob = view.CardJob.ToList(),
            Cost = new List<int> { view.CardCost, view.CardCost2, view.CardCost3 },
            CardActions = ParseCardAction(view.CardActions)
        }).ToList();
    }

    public List<CardActionDTO> ParseCardAction(List<JsonCardAction> jsonActions)
    {
        List<CardActionDTO> cardActions = new();

        if (jsonActions == null) return cardActions;


        foreach (var jsonAction in jsonActions)
        {
            if (jsonAction == null) continue;
            try
            {
                cardActions.Add(new CardActionDTO
                {
                    OrderIndex = jsonAction.order_index,
                    ActionType = (EnumTypes.Action)System.Enum.Parse(typeof(EnumTypes.Action), jsonAction.action),
                    Target = (EnumTypes.Target)System.Enum.Parse(typeof(EnumTypes.Target), jsonAction.target),
                    Value = new int[] { jsonAction.value, jsonAction.value_upgrade, jsonAction.value_upgrade2 },
                    Effect = (EnumTypes.EffectType)System.Enum.Parse(typeof(EnumTypes.EffectType), jsonAction.effect),
                    ExtraData = jsonAction.extra_data
                });
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error parsing card action: {e.Message}");
            }
        }
        return cardActions;
    }
}
