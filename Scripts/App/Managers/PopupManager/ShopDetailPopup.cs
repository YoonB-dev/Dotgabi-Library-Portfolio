using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Localization.SmartFormat;
using UnityEngine.Localization.Tables;

public class ShopDetailPopup : SceneSingleton<ShopDetailPopup>
{
    [SerializeField] private Canvas shopDetailCanvas;
    [SerializeField] private GameObject shopDetailBox;
    [SerializeField] private GameObject targetItem;

    public void ShowShopItemDetail(ShopItemDTO shopItemDTO, Action refreshFunc)
    {
        // SFX
        AudioManager.Instance.ButtonClickSound1();
        // 캔버스 활성화
        if (!shopDetailCanvas.isActiveAndEnabled) { shopDetailCanvas.gameObject.SetActive(true); }
        // 애니메이션
        ButtonAnim.Instance.ButtonScaleIn(shopDetailBox, 0.2f, 1f);
        // Shop 아이템 정보 설정
        targetItem = ShopItemDTOToObj.Instance.DTOToObj(targetItem, shopItemDTO, true, () => OnBuyButtonClick(shopItemDTO, refreshFunc));
    }

    public void HideShopItemDetail()
    {
        // SFX
        AudioManager.Instance.ButtonClickSound1();
        // 캔버스 비활성화
        shopDetailCanvas.gameObject.SetActive(false);
    }

    // 구매 버튼 클릭 시 호출되는 메서드
    public async void OnBuyButtonClick(ShopItemDTO shopItemDTO , Action refreshFunc)
    {
        // 로딩 팝업 활성화
        MainManager.Instance.SetLoadingCanvas(true);
        // SFX
        AudioManager.Instance.ButtonClickSound1();

        // 구매 로직 추가
        try
        {
            var response = await SupabaseClientProvider.Instance.Client
                    .Rpc("buy_product", new Dictionary<string, object>
                    {
                        { "t_product_id", shopItemDTO.ItemId }
                    });

            // response.Content는 string 타입의 JSON 배열임
            bool result = bool.Parse(response.Content);

            if (result)
            {
                var text = LogManager.Instance?.GetDBLogText(EnumTypes.LogActionType.shop_buy).FormatSmart(shopItemDTO.ItemName);
                NotificationManager.Instance.SetShownNotification(text);
                // UserData 업데이트
                int amount = -shopItemDTO.ItemPrice ?? 0;
                if (shopItemDTO.PriceType == "achieve")
                {
                    MainManager.Instance?.GetPoint("Achieve", amount);
                }
                else if (shopItemDTO.PriceType == "ad")
                {
                    MainManager.Instance?.GetPoint("Ad", amount);
                }
                Debug.Log($"구매 성공: {shopItemDTO.ItemName}, 잔액 - Achieve: {UserData.Instance.AchievePoint}, Ad: {UserData.Instance.AdPoint}");

                // 캐릭터 보유 현황 업데이트
                if (shopItemDTO.ItemType == EnumTypes.ShopItemType.character)
                {
                    UserData.Instance.OwnedCharacter = await UserOwnedCharacterDAO.Instance.GetUserOwnedCharacterAsync(UserData.Instance.UserAuthId);
                }

                // UserData에 소유 카드 프레임 목록 업데이트
                UpdateUserOwnedFrameList(shopItemDTO);
                refreshFunc?.Invoke(); // 구매 후 Shop 아이템 목록 새로고침
            }
            else
            {
                var text = LogManager.Instance?.GetLocalText("fail_to_buy");
                NotificationManager.Instance.SetShownNotification(text);
                AudioManager.Instance.GetShieldSound();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"구매 중 오류 발생: {e.Message}");
            var text = LogManager.Instance?.GetLocalText("fail_to_buy");
            NotificationManager.Instance.SetShownNotification(text);
            AudioManager.Instance.GetShieldSound();
        }

        // 로딩 팝업 비활성화
        MainManager.Instance.SetLoadingCanvas(false);

        HideShopItemDetail(); // 구매 후 팝업 닫기
    }

    public void UpdateUserOwnedFrameList(ShopItemDTO shopItemDTO)
    {
        // UserData에 소유 카드 프레임 목록 업데이트
        var existingFrame = UserData.Instance.OwnedCardFrameList
            .Find(frame => frame.CardFrameId == shopItemDTO.ItemId);

        if (existingFrame != null)
        {
            existingFrame.Count += shopItemDTO.Count;
        } else
        {
            UserData.Instance.OwnedCardFrameList.Add(new UserOwnCardFrameDTO
            {
                CardFrameId = shopItemDTO.ItemId,
                Count = shopItemDTO.Count,
                CardFrameType = shopItemDTO.ItemType
            });
        }
    }
}
