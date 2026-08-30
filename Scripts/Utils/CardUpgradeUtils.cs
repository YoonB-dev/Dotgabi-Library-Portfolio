using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// CardUpgradeUtils는 카드 업그레이드 관련 유틸리티 클래스입니다.
/// 카드 업그레이드에 필요한 기능들을 제공합니다.
/// 카드 업그레이드 로직을 담당합니다.
/// </summary>
public class CardUpgradeUtils : Singleton<CardUpgradeUtils>
{
    public CardDTO ShowUpgradeCard(CardDTO targetCardDTO)
    {
        // 카드 강화 시 보여지는 수치를 반환합니다. (실제 강화는 아님)

        // 카드 강화 전 카드 DTO 복사

        // 예외 처리: 특정 카드가 강화 시 다른 카드로 변환되는 경우
        // 찢어진 동화 조각
        if (targetCardDTO.Id == 55)
        {
            CardDTO newUpgradeCardDTO = InGameData.Instance.Cards.Find(c => c.Id == 56).Copy();
            return newUpgradeCardDTO;
        }

        // 기본 강화 로직: 카드의 강화 레벨을 1 증가시킴
        CardDTO upgradeCardDTO = targetCardDTO.Copy();
        upgradeCardDTO.CardUpgrade++;
        return upgradeCardDTO;
    }

    public async void UpgradeRandomUpgradeableCard(int count, ScenarioDTO targetData)
    {
        for (int i = 0; i < count; i++)
        {
            // 강화 가능한 카드 리스트 얻기
            var upgradeableCards = UserData.Instance.MainScenarioData.OwnedCardList
                .Where(card => CardCheckUtils.Instance.CheckCardCanUpgrade(card, false))
                .ToList();

            if (upgradeableCards.Count == 0)
            {
                var text = LogManager.Instance?.GetLocalText("no_card_to_upgrade");
                NotificationManager.Instance.SetShownNotification(text);
                return;
            }

            // 랜덤으로 하나 선택
            int randomIndex = Random.Range(0, upgradeableCards.Count);
            var randomCard = upgradeableCards[randomIndex];

            var cardName = InGameData.Instance.Cards.Find(a => a.Id == randomCard.CardId).Name;
            // 강화 실행
            SupabaseCard.Instance.UpgradeCard(MoveSystem.Instance.SCENARIO_DATA, randomCard.OwnedId, cardName, false);
            await Task.Delay(500); // 0.5초 대기
        }
    }
}
