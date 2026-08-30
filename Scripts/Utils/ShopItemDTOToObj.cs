using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemDTOToObj : Singleton<ShopItemDTOToObj>
{
    // ShopItemDTO를 오브젝트에 설정하는 메서드
    private readonly string _achievePath = "Image/Icon/Money/AchievePointIcon";
    private readonly string _adPath = "Image/Icon/Money/AdPointIcon";
    public GameObject DTOToObj(GameObject targetItem, ShopItemDTO shopItemDTO, bool isDetail = false, Action onBuy = null)
    {
        var item = targetItem.transform.GetChild(0);

        item.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(shopItemDTO.ImgPath);
        item.GetChild(1).GetComponent<TextMeshProUGUI>().text = shopItemDTO.Count.ToString();
        item.GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>().text = shopItemDTO.ItemName;


        bool canBuy = false;

        if (shopItemDTO.PriceType == "achieve")
        {
            item.GetChild(3).GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(_achievePath);
            canBuy = UserData.Instance.AchievePoint >= shopItemDTO.ItemPrice;
        }
        else if (shopItemDTO.PriceType == "ad")
        {
            item.GetChild(3).GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(_adPath);
            canBuy = UserData.Instance.AdPoint >= shopItemDTO.ItemPrice;
        }

        item.GetChild(3).GetChild(1).GetComponent<TextMeshProUGUI>().text = shopItemDTO.ItemPrice.ToString();
        item.GetChild(3).GetChild(1).GetComponent<TextMeshProUGUI>().color = canBuy ? Color.white : Color.red;

        // 해당 아이템을 모두 찾기 (리스트 반환)
        var ownedList = UserData.Instance.OwnedCardFrameList
            .FindAll(x => x.CardFrameId == shopItemDTO.ItemId);

        bool isOwned = ownedList.Count > 0; // 하나라도 있으면 이미 소유

        // 캐릭터일 경우 추가 검사
        bool isCharacter = CheckCharacter(shopItemDTO.ItemType, shopItemDTO.ItemValue);
        if (isCharacter)
        {
            isOwned = true;
        }

        item.GetChild(3).GetChild(0).gameObject.SetActive(!isOwned);
        item.GetChild(3).GetChild(1).gameObject.SetActive(!isOwned);
        item.GetChild(3).GetChild(2).gameObject.SetActive(isOwned);

        item.GetChild(3).GetChild(2).GetComponent<TextMeshProUGUI>().text = isOwned ? "소유" : "구매";
        if (isDetail && item.childCount > 4)
        {
            item.GetChild(4).GetChild(0).GetComponent<TextMeshProUGUI>().text = shopItemDTO.ItemDescription;
            var buyButton = item.GetChild(3).GetComponent<Button>();
            buyButton.onClick.RemoveAllListeners();

            if (!isOwned)
            {
                buyButton.onClick.AddListener(() => {
                    onBuy?.Invoke();
                });
            }

        }
        return targetItem;
    }

    // 캐릭터 소유 여부 확인 메서드
    public bool CheckCharacter(EnumTypes.ShopItemType characterType, int characterId)
    {
        if (characterType != EnumTypes.ShopItemType.character)
        {
            return false;
        }

        switch (characterId)
        {
            case 2:
                return UserData.Instance.OwnedCharacter.OwnedDosa;
            case 3:
                return UserData.Instance.OwnedCharacter.OwnedPerformer;
        }

        return false;
    }

}
