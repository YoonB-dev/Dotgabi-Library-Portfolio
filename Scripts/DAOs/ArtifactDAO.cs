using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ArtifactDAO : Singleton<ArtifactDAO>
{
    public async Task<List<ArtifactDTO>> GetAllArtifactsAsync(EnumTypes.LanguageType language)
    {
        var response = await SupabaseClientProvider.Instance.ClientGameData
            .From<ArtifactEntity>()
            .Where(x => x.LanguageCode == language)
            .Get();

        Debug.Log($"GetAllCardsAsync: {response.Models.Count} artifacts loaded");

        return response.Models.ConvertAll(view => new ArtifactDTO {
            Id = view.Id,
            Name = view.ArtifactName,
            Ability = view.ArtifactAbility,
            Rarity = view.Rarity,
            Place = view.Place,
            ImageUrl = view.ImgPath,
            FlavorText = view.FlavorText,
            IsIcon = view.IsIcon,
            // ArtifactEffects
            ArtifactEffects = ParseArtifactEffects(view.ArtifactEffects),
        });
    }

    private List<ArtifactEffectDTO> ParseArtifactEffects(List<JsonArtifactEffect> jsonEffects)
    {
        List<ArtifactEffectDTO> artifactEffects = new();

        if (jsonEffects == null) return artifactEffects;

        foreach (var jsonEffect in jsonEffects)
        {
            if (jsonEffect == null) continue;
            try
            {
                artifactEffects.Add(new ArtifactEffectDTO
                {
                    ItemTrigger = (EnumTypes.ArtifactTriggerType)System.Enum.Parse(typeof(EnumTypes.ArtifactTriggerType), jsonEffect.item_trigger),
                    ItemEffectType = (EnumTypes.ArtifaceEffectType)System.Enum.Parse(typeof(EnumTypes.ArtifaceEffectType), jsonEffect.item_effect_type),
                    Target = (EnumTypes.Target)System.Enum.Parse(typeof(EnumTypes.Target), jsonEffect.target),
                    Value = jsonEffect.value,
                    ValueType = jsonEffect.value_type,
                    ExtraData = jsonEffect.extra_data
                });
            } catch (System.Exception e)
            {
                Debug.LogError($"Error parsing artifact effect: {e.Message}");
            }
        }
        return artifactEffects;
    }
}
