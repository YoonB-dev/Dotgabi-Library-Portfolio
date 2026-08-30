using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class UserMapper : Singleton<UserMapper>
{
    public async Task<UserDTO> ToDTO(UserEntity entity)
    {
        var response = await SupabaseClientProvider.Instance.Client
            .From<UserGoodsEntity>()
            .Select(x => new object[] { x.AchievePoint, x.AdPoint })
            .Where(x => x.UserAuthId == entity.AuthId)
            .Get();

        var user_good = response.Models.FirstOrDefault();

        return new UserDTO {
            AuthId = entity.AuthId,
            UserGoods = new UserGoodsDTO {
                AchievePoint = user_good?.AchievePoint ?? 0,
                AdPoint = user_good?.AdPoint ?? 0,
            },
            Email = entity.Email,
            SelectCardFrameId = entity.SelectCardFrameId,
            SelectCardDecoId = entity.SelectDecoId,
            CurrScenarioType = string.IsNullOrEmpty(entity.CurrScenarioType) ? EnumMainType.ScenarioType.story : Enum.Parse<EnumMainType.ScenarioType>(entity.CurrScenarioType),
            IsTutorial = entity.IsTutorial
        };
    }
}
