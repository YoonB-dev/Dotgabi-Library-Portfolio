using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CardCheckUtils는 카드 관련 유효성을 검사하는 클래스입니다.
/// 카드 강화, 삭제등의 작업을 수행하기 전에 카드의 유효성을 검사합니다.
/// </summary>
public class CardCheckUtils : Singleton<CardCheckUtils>
{
    readonly int MAX_UPGRADE_TIME = 2;
    public List<UserScenarioOwnedCardDTO> GetCanUpgradeCardDTO(List<UserScenarioOwnedCardDTO> ownedCardList, bool isBattle)
    {
        // 카드 강화 가능 여부를 검사합니다.
        var canUpgradeCardList = new List<UserScenarioOwnedCardDTO>();
        foreach (var ownedCard in ownedCardList)
        {
            if (CheckCardCanUpgrade(ownedCard, isBattle))
            {
                canUpgradeCardList.Add(ownedCard);
            }
        }

        return canUpgradeCardList;
    }

    public bool CheckCardCanUpgrade(UserScenarioOwnedCardDTO card, bool isBattle)
    {
        var cardDTO = InGameData.Instance.Cards.Find(c => c.Id == card.CardId).Copy();
        cardDTO.CardUpgrade = card.UpgradeTime;
        return checkCardCanUpgradeDTO(cardDTO, isBattle);
    }

    public bool checkCardCanUpgradeDTO(CardDTO cardDTO, bool isBattle)
    {
        bool canUpgrade = false;
        // 카드가 강화 가능한지 검사
        // 1. 카드 강화 제외 카드 - 특정 카드
        if (cardDTO.Id == 57 || cardDTO.Id == 58)
        {
            return false;
        }
        // 2. 카드 강화 제외 카드 - 저주 카드
        if (InGameData.Instance.Cards.Find(c => c.Id == cardDTO.Id).CardType == EnumTypes.CardType.curse)
        {
            return false;
        }
        // 3. 카드 강화 제외 카드 - 특수 카드
        if (InGameData.Instance.Cards.Find(c => c.Id == cardDTO.Id).CardType == EnumTypes.CardType.special)
        {
            return false;
        }
        // 4. 전투중에는 강화 불가능 카드 제외
        if (isBattle)
        {
            if (cardDTO.Id == 55)
            {
                return false; ;
            }
        }
        // 5. 카드 강화 횟수 검사
        if (cardDTO.CardUpgrade < MAX_UPGRADE_TIME)
        {
            canUpgrade = true;
        }
        else if (cardDTO.Id == 64)
        {
            // 카드의 강화에 제한이 없는 경우
            canUpgrade = true;
        }

        return canUpgrade;
    }

    public bool CheckCardCanDelete(UserScenarioOwnedCardDTO card, bool isBattle)
    {
        var cardDTO = InGameData.Instance.Cards.Find(c => c.Id == card.CardId).Copy();
        cardDTO.CardUpgrade = card.UpgradeTime;
        return checkCardCanDeleteDTO(cardDTO, isBattle);
    }

    public bool checkCardCanDeleteDTO(CardDTO cardDTO, bool isBattle)
    {
        // 카드가 삭제 가능한지 검사
        // 1. 카드 삭제 제외 카드 - 특정 카드
        if (cardDTO.Id == 55)
        {
            return false;
        }

        return true;
    }
}
