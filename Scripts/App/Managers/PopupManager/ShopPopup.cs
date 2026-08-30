using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopPopup : MonoBehaviour
{
    [SerializeField] private GameObject shopItemPrefab; // Shop 아이템 프리팹
    [SerializeField] private Canvas shopCanvas;
    [SerializeField] private Transform shopItemContentPos;
    [SerializeField] private GameObject ShopMainBox;
    [SerializeField] private ShopDetailPopup shopDetailPopup;
    private EnumTypes.ShopItemType currentShopItemType = EnumTypes.ShopItemType.frame;
    public void ShowShopPopup()
    {
        shopCanvas.gameObject.SetActive(true);
        // SFX
        AudioManager.Instance.ButtonClickSound1();
        // 팝업 애니메이션
        ButtonAnim.Instance.ButtonScaleIn(ShopMainBox, 0f, 1f);

        // 배경 움직임 비활성화
        // mainManager.cambox.SetCanMove(false);

        // Shop 아이템 생성
        StartCoroutine(ShowShopItemsCoroutine(EnumTypes.ShopItemType.frame));
    }

    private IEnumerator ShowShopItemsCoroutine(EnumTypes.ShopItemType shopItemType)
    {
        // 현재 Shop 아이템 타입을 설정
        currentShopItemType = shopItemType;
        Debug.Log("유저 돈: " + UserData.Instance.AchievePoint);
        // Shop 아이템 데이터 가져오기
        List<ShopItemDTO> shopItems = new ();
        switch (shopItemType)
        {
            case EnumTypes.ShopItemType.frame:
                shopItems = InGameData.Instance.FrameShopItems;
                break;
            case EnumTypes.ShopItemType.deco:
                shopItems = InGameData.Instance.DecoShopItems;
                break;
            case EnumTypes.ShopItemType.character:
                shopItems = InGameData.Instance.CharacterShopItems;
                break;
        }


        // Shop 아이템 데이터가 contentPos의 자식 개수보다 많을 경우, 부족한 만큼 Shop 아이템 오브젝트를 생성 - 후에 비활성화
        if (shopItems.Count > shopItemContentPos.childCount)
        {
            for (int i = shopItemContentPos.childCount; i < shopItems.Count; i++)
            {
                var item = Instantiate(shopItemPrefab, shopItemContentPos);
                item.SetActive(false);
            }
        }

        // Shop 아이템 데이터가 contentPos의 자식 개수보다 적을 경우, 남는 Shop 아이템 오브젝트를 비활성화
        for (int i = shopItems.Count; i < shopItemContentPos.childCount; i++)
        {
            shopItemContentPos.GetChild(i).gameObject.SetActive(false);
        }

        // Shop 아이템 데이터에 따라 Shop 아이템 오브젝트를 활성화하고 설정
        for (int i = 0; i < shopItems.Count; i++)
        {
            var targetItem = shopItemContentPos.GetChild(i).gameObject;
            targetItem.SetActive(true);

            // Shop 아이템 DTO를 오브젝트에 설정
            targetItem = ShopItemDTOToObj.Instance.DTOToObj(targetItem, shopItems[i]);

            // Shop 아이템 클릭 이벤트 설정
            Button itemButton = targetItem.transform.GetChild(0).GetComponent<Button>();
            itemButton.onClick.RemoveAllListeners();
            int index = i;
            itemButton.onClick.AddListener(() => {
                shopDetailPopup.ShowShopItemDetail(shopItems[index], RefreshShopItems);
            });
        }

        yield return null;
    }

    public void RefreshShopItems()
    {
        // 현재 Shop 아이템 타입에 따라 Shop 아이템을 다시 생성
        Debug.Log($"Refreshing Shop Items for type: {currentShopItemType}");
        StartCoroutine(ShowShopItemsCoroutine(currentShopItemType));
    }

    public void HideShopPopup()
    {
        shopCanvas.gameObject.SetActive(false);
        // SFX
        AudioManager.Instance.ButtonClickSound1();
        // 배경 움직임 활성화
        MainManager.Instance.cambox.SetCanMove(true);
    }


    // 버튼 이벤트로 사용되는 메서드들
    public void ShowFrameContent()
    {
        StartCoroutine(ShowShopItemsCoroutine(EnumTypes.ShopItemType.frame));
    }
    public void ShowDecoContent()
    {
        StartCoroutine(ShowShopItemsCoroutine(EnumTypes.ShopItemType.deco));
    }
    public void ShowCharacterContent()
    {
        StartCoroutine(ShowShopItemsCoroutine(EnumTypes.ShopItemType.character));
    }
}
