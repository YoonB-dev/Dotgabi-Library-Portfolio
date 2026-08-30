using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BagDTOToObj : Singleton<BagDTOToObj>
{
    public GameObject DTOToObj(GameObject item, UserOwnCardFrameDTO itemData)
    {
        // 1. ShopItems에서 itemId가 일치하는 ShopItemDTO 찾기
        var shopItem = InGameData.Instance.ShopItems
            .FirstOrDefault(x => x.ItemId == itemData.CardFrameId);

        if (shopItem != null)
        {
            item.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(shopItem.ImgPath);
            if ((itemData.CardFrameType == EnumTypes.ShopItemType.frame && UserData.Instance.SelectCardFrameId == itemData.CardFrameId)
            || (itemData.CardFrameType == EnumTypes.ShopItemType.deco && UserData.Instance.SelectDecoId == itemData.CardFrameId))
            {

                item.transform.GetChild(1).gameObject.SetActive(true);
            }else
            {
                item.transform.GetChild(1).gameObject.SetActive(false);
            }
        }

        return item;
    }

    public GameObject DetailToObj(GameObject item, UserOwnCardFrameDTO itemData, Action selectFrame)
    {
        // 1. ShopItems에서 itemId가 일치하는 ShopItemDTO 찾기
        var shopItem = InGameData.Instance.ShopItems.FirstOrDefault(x => x.ItemId == itemData.CardFrameId);

        var product = item.transform.GetChild(0);

        if (shopItem != null)
        {
            product.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(shopItem.ImgPath);
        }

        if (itemData.Count > 1)
        {
            product.GetChild(1).gameObject.SetActive(true);
            product.GetChild(1).GetComponent<TextMeshProUGUI>().text = itemData.Count.ToString();
        } else
        {
            product.GetChild(1).gameObject.SetActive(false);
        }

        product.GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>().text = shopItem.ItemName;
        product.GetChild(4).GetChild(0).GetComponent<TextMeshProUGUI>().text = shopItem.ItemDescription;

        product.GetChild(3).GetComponent<Button>().onClick.RemoveAllListeners();
        product.GetChild(3).GetComponent<Button>().onClick.AddListener(() => {
            selectFrame?.Invoke();
        });


        return item;
    }
}
