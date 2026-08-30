using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class EnemyDAO : Singleton<EnemyDAO>
{
    public async Task<List<EnemyDTO>> GetAllEnemysAsync(EnumTypes.LanguageType lanCode)
    {
        var response = await SupabaseClientProvider.Instance.ClientGameData
            .From<EnemyEntity>()
            .Where(x => x.LanguageCode == lanCode)
            .Get();

        return response.Models.ConvertAll(view => new EnemyDTO {
            Id = view.Id,
            Name = view.EnemyName,
            Description = view.Description,
            FlavorText = view.FlavorText,
            ImgPath = view.ImgPath,
            ImgSpinePath = view.ImgSpinePath,
            Count = view.Count,
            HealthMin = view.HealthMin,
            HealthMax = view.HealthMax,
            AttackMin = view.AttackMin,
            AttackMax = view.AttackMax,
            DefenseMin = view.DefenseMin,
            DefenseMax = view.DefenseMax,
            HealMin = view.HealMin,
            HealMax = view.HealMax,
            Stage = view.Stage,
            ImgFacePath = view.ImgFacePath,
            // EnemyAbilities
            EnemyAbilities = view.EnemyAbilities,
            // PassiveAbilities
            Passive = view.PassiveAbilities
        });
    }


    /// <summary>
    /// 적 대화 정보
    /// </summary>
    public async Task<List<EnemyTextDTO>> GetEnemyTextsAsync(EnumTypes.LanguageType lanCode)
    {
        var response = await SupabaseClientProvider.Instance.ClientGameData
            .From<EnemyTextEntity>()
            .Get();

        var responseLocale = await SupabaseClientProvider.Instance.ClientGameData
            .From<EnemyTextLocaleEntity>()
            .Where(x => x.LanguageCode == lanCode)
            .Get();

        var responseChoices = await SupabaseClientProvider.Instance.ClientGameData
            .From<EnemyTextChoiceEntity>()
            .Get();

        var responseChoiceLocale = await SupabaseClientProvider.Instance.ClientGameData
            .From<EnemyTextChoiceLocaleEntity>()
            .Where(x => x.LanguageCode == lanCode)
            .Get();

        var responseChoiceEntity = responseChoices.Models;
        var responseChoiceLocaleEntity = responseChoiceLocale.Models;

        var Choices = GetEnemyTextChoices(responseChoiceEntity, responseChoiceLocaleEntity, lanCode);

        return response.Models.ConvertAll(text => {
            var localeText = responseLocale.Models.Find(x => x.TextId == text.Id);
            return new EnemyTextDTO {
                Id = text.Id,
                EnemyId = text.EnemyId,
                TextType = text.TextType,
                TextValue = localeText?.TextValue ?? string.Empty,
                ExtraData = text.ExtraData ?? new Dictionary<string, object>(),
                Choices = Choices.FindAll(choice => choice.TextId == text.Id)
            };
        });
    }

    public List<EnemyTextChoiceDTO> GetEnemyTextChoices(List<EnemyTextChoiceEntity> choices, List<EnemyTextChoiceLocaleEntity> locales, EnumTypes.LanguageType lanCode)
    {
        return choices.ConvertAll(choice => {
            var locale = locales.Find(x => x.ChoiceId == choice.Id && x.LanguageCode == lanCode);
            return new EnemyTextChoiceDTO {
                Id = choice.Id,
                TextId = choice.TextId,
                ChoiceOrder = choice.ChoiceOrder,
                NextIndex = choice.NextIndex,
                ChoiceText = locale?.ChoiceText ?? string.Empty,
                ExtraData = choice.ExtraData ?? new Dictionary<string, object>()
            };
        });
    }
}
