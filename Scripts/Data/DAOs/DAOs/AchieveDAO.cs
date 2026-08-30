using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class AchieveDAO : Singleton<AchieveDAO>
{
    public async Task<List<AchieveDTOList>> GetAchievesAsync(EnumTypes.LanguageType languageType)
    {
        var achieveEntities = await SupabaseClientProvider.Instance.ClientGameData
            .From<AchieveEntity>()
            .Select("*, achieve_locales(*)")  // achieve_locales 테이블의 모든 열 포함
            .Filter("achieve_locales.lan_code", Supabase.Postgrest.Constants.Operator.Equals, languageType.ToString())
            .Order("achieve_id", Supabase.Postgrest.Constants.Ordering.Ascending) // 순서 정렬
            .Get();

        var achieveList = achieveEntities.Models;

        // AchieveType별로 그룹핑한 후 각각 AchieveDTOList 생성하여 리스트로 저장
        var achieveDTOLists = achieveList
            .GroupBy(entity => entity.AchieveType)
            .Select(g => new AchieveDTOList {
                AchieveType = g.Key,
                Achieves = g.Select(entity => new AchieveDTO {
                    Id = entity.AchieveId,
                    Level = entity.Level,
                    TargetValue = entity.TargetValue,
                    PriceType = entity.PriceType,
                    PriceAmount = entity.PriceAmount,
                    Description = entity.AchieveLocales?.FirstOrDefault(l => l.LanCode == languageType)?.Description ?? "",
                    AchieveType = entity.AchieveType
                }).ToList()
            })
            .ToList();

        return achieveDTOLists;
    }


    public async Task<List<UserClearAchieveDTO>> GetUserClearAchievesAsync(string userAuthId)
    {
        var userAchieveEntities = await SupabaseClientProvider.Instance.Client
            .From<UserAchieveEntity>()
            .Where(x => x.UserAuthId == userAuthId)
            .Get();

        var userAchieveList = userAchieveEntities.Models;

        // UserClearAchieveDTO 리스트로 변환
        var userClearAchieveDTOs = userAchieveList
            .Select(entity => new UserClearAchieveDTO {
                AchieveId = entity.AchieveId
            })
            .ToList();

        return userClearAchieveDTOs;
    }

    public async Task<UserAchieveCurrDataDTO> GetUserAchieveCurrDataAsync(string userAuthId)
    {
        var userAchieveCurrDataEntities = await SupabaseClientProvider.Instance.Client
            .From<UserAchieveCurrDataEntity>()
            .Where(x => x.UserAuthId == userAuthId)
            .Get();

        var userAchieveCurrDataEntity = userAchieveCurrDataEntities.Models.FirstOrDefault();

        if (userAchieveCurrDataEntity == null)
        {
            // 데이터가 없을 경우 기본값 반환
            return new UserAchieveCurrDataDTO {
                MoveForwardCount = 0,
                BattleCount = 0,
                ShopPurchaseCount = 0,
                RestCount = 0,
                ShowAdCount = 0,
                TotalUseCard = 0,
                TotalCoinUse = 0
            };
        }

        // UserAchieveCurrDataDTO로 변환
        var userAchieveCurrDataDTO = new UserAchieveCurrDataDTO {
            MoveForwardCount = userAchieveCurrDataEntity.MoveForwardCount,
            BattleCount = userAchieveCurrDataEntity.BattleCount,
            ShopPurchaseCount = userAchieveCurrDataEntity.ShopPurchaseCount,
            RestCount = userAchieveCurrDataEntity.RestCount,
            ShowAdCount = userAchieveCurrDataEntity.ShowAdCount,
            TotalUseCard = userAchieveCurrDataEntity.TotalUseCard
        };

        return userAchieveCurrDataDTO;
    }

    public async Task<UserAchievePriceGetDTO> GetUserAchievePriceGetAsync(string userAuthId)
    {
        var userAchievePriceGetEntities = await SupabaseClientProvider.Instance.Client
            .From<UserAchievePriceGetEntity>()
            .Where(x => x.AuthId == userAuthId)
            .Get();

        var userAchievePriceGetEntity = userAchievePriceGetEntities.Models.First();

        if (userAchievePriceGetEntity == null)
        {
            // 데이터가 없을 경우 기본값 반환
            Debug.LogWarning($"No UserAchievePriceGetEntity found for UserAuthId: {userAuthId}");
            return new UserAchievePriceGetDTO {
                BigPrice1 = false,
                BigPrice2 = false,
                BigPrice3 = false,
                BigPrice4 = false
            };
        }

        // UserAchievePriceGetDTO로 변환
        var userAchievePriceGetDTO = new UserAchievePriceGetDTO {
            BigPrice1 = userAchievePriceGetEntity.BigPrice1,
            BigPrice2 = userAchievePriceGetEntity.BigPrice2,
            BigPrice3 = userAchievePriceGetEntity.BigPrice3,
            BigPrice4 = userAchievePriceGetEntity.BigPrice4
        };

        return userAchievePriceGetDTO;
    }
}
