using System.Collections.Generic;
using UnityEngine;

public class UserData : Singleton<UserData>
{
    public string UserAuthId { get; set; }
    public int AchievePoint { get; set; }
    public int AdPoint { get; set; }
    public int UserCoinUseTotal { get; set; } = 0; // 사용한 코인의 총합
    public List<UserOwnCardFrameDTO> OwnedCardFrameList { get; set; } = new List<UserOwnCardFrameDTO>();
    public UserOwnCharacterDTO OwnedCharacter { get; set; } = new UserOwnCharacterDTO();
    public int SelectCardFrameId { get; set; } = 1;
    public int SelectDecoId { get; set; } = 2;
    public EnumTypes.LanguageType LanguageType { get; set; } = EnumTypes.LanguageType.en;

    // 튜토리얼 진행 여부
    public bool istutorialCompleted { get; set; } = false;

    // 업적 관련 데이터
    public List<UserClearAchieveDTO> UserClearAchieveList { get; set; } = new List<UserClearAchieveDTO>();
    public UserAchieveCurrDataDTO UserAchieveCurrData { get; set; } = new UserAchieveCurrDataDTO();
    public UserAchievePriceGetDTO UserAchievePriceGetData { get; set; } = new UserAchievePriceGetDTO();

    public List<UserOwnedCardDataDTO> UserOwnedCardList { get; set; } = new List<UserOwnedCardDataDTO>();
    public List<UserOwnedArtifactDataDTO> UserOwnedArtifactList { get; set; } = new List<UserOwnedArtifactDataDTO>();
    // 클리어 데이터
    public List<UserMainClearRecordDTO> UserMainClearRecordList { get; set; } = new List<UserMainClearRecordDTO>();

    // 메인 시나리오 관련 데이터
    public UserMainScenarioDTO MainScenarioData { get; set; } = null;
    public UserScenarioClearDTO MainScenarioClear { get; set; } = null;
    // 도전 시나리오 관련 데이터
    public UserChallengeScenarioDTO ChallengeScenarioData { get; set; } = null;

    public float ratio = 1.0f; // 화면 비율 조정용, 기본값은 1.0f
    public int totalLevelCount = 0; // 전체 레벨 수

    public void GetCoin(int amount, ScenarioDTO dataType)
    {
        //SFX
        AudioManager.Instance.MoneySound();
        if (dataType is UserMainScenarioDTO)
        {
            MainScenarioData.GameCoins += amount;
            MainScenarioData.TotalGameCoins += amount; // 총 코인 수 업데이트
        }
        else if (dataType is UserChallengeScenarioDTO)
        {
            ChallengeScenarioData.GameCoins += amount;
            ChallengeScenarioData.TotalGameCoins += amount; // 총 코인 수 업데이트
        }
    }
    public void UseCoin(int amount, ScenarioDTO dataType)
    {
        //SFX
        AudioManager.Instance.MoneySound();
        if (dataType is UserMainScenarioDTO)
        {
            MainScenarioData.GameCoins -= amount;
            if (MainScenarioData.GameCoins < 0)
            {
                MainScenarioData.GameCoins = 0; // 코인이 음수가 되지 않도록 보장
            }
        }
        else if (dataType is UserChallengeScenarioDTO)
        {
            ChallengeScenarioData.GameCoins -= amount;
            if (ChallengeScenarioData.GameCoins < 0)
            {
                ChallengeScenarioData.GameCoins = 0; // 코인이 음수가 되지 않도록 보장
            }
        }

        //돈 사용 업적
        if (amount < 0)
        {
            SupabaseAchieve.Instance.AchieveCurrData(EnumTypes.AchieveType.total_coin_use, amount);
        }
    }
    public void GetHp(int amount, ScenarioDTO dataType)
    {
        //SFX
        if (amount > 0)
        {
            AudioManager.Instance.HealSound();
        }
        else
        {
            AudioManager.Instance.DamageSound();
        }

        if (dataType is UserMainScenarioDTO)
        {
            MainScenarioData.CurrHp += amount;
            if (MainScenarioData.CurrHp > MainScenarioData.MaxHp)
            {
                MainScenarioData.CurrHp = MainScenarioData.MaxHp;
            }
        }
        else if (dataType is UserChallengeScenarioDTO)
        {
            ChallengeScenarioData.CurrHp += amount;
            if (ChallengeScenarioData.CurrHp > ChallengeScenarioData.MaxHp)
            {
                ChallengeScenarioData.CurrHp = ChallengeScenarioData.MaxHp;
            }
        }
    }
    public void GetMaxHp(int amount, ScenarioDTO dataType)
    {
        //SFX
        AudioManager.Instance.HealSound();

        if (dataType is UserMainScenarioDTO)
        {
            MainScenarioData.MaxHp += amount;
            if (MainScenarioData.MaxHp < 1)
            {
                MainScenarioData.MaxHp = 1; // 최소 HP를 1로 설정
            }
            if (MainScenarioData.CurrHp > MainScenarioData.MaxHp)
            {
                MainScenarioData.CurrHp = MainScenarioData.MaxHp; // 현재 HP가 최대 HP를 초과하지 않도록 조정
            }
        }
        else if (dataType is UserChallengeScenarioDTO)
        {
            ChallengeScenarioData.MaxHp += amount;
            if (ChallengeScenarioData.MaxHp < 1)
            {
                ChallengeScenarioData.MaxHp = 1; // 최소 HP를 1로 설정
            }
            if (ChallengeScenarioData.CurrHp > ChallengeScenarioData.MaxHp)
            {
                ChallengeScenarioData.CurrHp = ChallengeScenarioData.MaxHp; // 현재 HP가 최대 HP를 초과하지 않도록 조정
            }
        }
    }
}
