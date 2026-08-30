using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BagPopup : MonoBehaviour
{
    [SerializeField] private GameObject bagItemPrefab; // Bag 아이템 프리팹
    [SerializeField] private Canvas bagCanvas;
    [SerializeField] private GameObject bagMainPanel;
    [SerializeField] private GameObject mainCardObj,cardFrameObj, cardDecoObj;
    [SerializeField] private Transform bagItemContentPos;
    [SerializeField] private BagShowDetailPopup bagShowDetailPopup;
    public void ShowBagPopup(bool isFirst = false)
    {
        bagCanvas.gameObject.SetActive(true);
        // SFX
        AudioManager.Instance.ButtonClickSound1();
        // 팝업 애니메이션
        if (isFirst) { ButtonAnim.Instance.ButtonScaleIn(bagMainPanel, 0f, 1f); }
        // 배경 움직임 비활성화
        // mainManager.cambox.SetCanMove(false);

        // Bag 아이템 생성
        SetSelectFrame();
        StartCoroutine(ShowBagItemsCoroutine(EnumTypes.ShopItemType.frame));
    }

    private IEnumerator ShowBagItemsCoroutine(EnumTypes.ShopItemType itemType)
    {
        var itemDataByType = UserData.Instance.OwnedCardFrameList.FindAll(item => item.CardFrameType == itemType);

        // Bag 아이템이 contentPos의 자식 개수보다 많을 경우, 부족한 만큼 Bag 아이템 오브젝트를 생성 - 후에 비활성화
        if (itemDataByType.Count > bagItemContentPos.transform.childCount)
        {
            for (int i = bagItemContentPos.transform.childCount; i < itemDataByType.Count; i++)
            {
                var bagItem = Instantiate(bagItemPrefab, bagItemContentPos);
                bagItem.SetActive(false);
            }
        }
        // Bag 아이템이 contentPos의 자식 개수보다 적을 경우, 남는 Bag 아이템 오브젝트를 비활성화
        for (int i = itemDataByType.Count; i < bagItemContentPos.transform.childCount; i++)
        {
            bagItemContentPos.transform.GetChild(i).gameObject.SetActive(false);
        }
        // Bag 아이템 데이터에 따라 Bag 아이템 오브젝트를 활성화하고 설정
        for (int i = 0; i < itemDataByType.Count; i++)
        {
            var targetBagItem = bagItemContentPos.transform.GetChild(i).gameObject;
            targetBagItem.SetActive(true);

            // Bag 아이템 DTO를 오브젝트에 설정
            targetBagItem = BagDTOToObj.Instance.DTOToObj(targetBagItem, itemDataByType[i]);

            // Bag 아이템 클릭 이벤트 설정
            Button bagButton = targetBagItem.GetComponent<Button>();
            int index = i;
            bagButton.onClick.RemoveAllListeners();
            bagButton.onClick.AddListener(() => {
                // SFX
                AudioManager.Instance.ButtonClickSound2();
                // 선택된 Bag 아이템 상세 정보 표시
                bagShowDetailPopup.ShowBagDetail(itemDataByType[index], selectFrame: () => {
                    SelectFrame(itemDataByType[index]);
                });
            });
        }


        yield return null;
    }

    public void HideBagPopup()
    {
        // SFX
        AudioManager.Instance.ButtonClickSound2();

        bagCanvas.gameObject.SetActive(false);

        // 배경 움직임 활성화
        MainManager.Instance.cambox.SetCanMove(true);
    }

    public void RefreshBagItems(EnumTypes.ShopItemType itemType)
    {
        // Bag 아이템 목록 새로고침
        StartCoroutine(ShowBagItemsCoroutine(itemType));
    }

    private async void SelectFrame(UserOwnCardFrameDTO itemData)
    {
        // SFX
        AudioManager.Instance.ButtonClickSound2();
        // 선택된 카드 프레임 설정
        bagShowDetailPopup.HideBagDetail();

        var response = await SupabaseClientProvider.Instance.Client
                    .Rpc("equip_user_frame", new Dictionary<string, object>
                    {
                        { "p_frame_id", itemData.CardFrameId }
                    });

        bool result = bool.Parse(response.Content);
        if (result)
        {
            // 선택된 카드 프레임 ID 저장
            if (itemData.CardFrameType == EnumTypes.ShopItemType.frame)
            {
                UserData.Instance.SelectCardFrameId = itemData.CardFrameId;
            } else
            {
                UserData.Instance.SelectDecoId = itemData.CardFrameId;
            }
            SetSelectFrame();
        }

        RefreshBagItems(itemData.CardFrameType);
    }

    private void SetSelectFrame()
    {
        // frame 설정
        string frameImgPath = InGameData.Instance.ShopItems.FirstOrDefault(x => x.ItemId == UserData.Instance.SelectCardFrameId).ImgPath;
        cardFrameObj.transform.GetChild(1).GetComponent<Image>().sprite = Resources.Load<Sprite>(frameImgPath);
        Debug.Log($"선택된 카드 프레임 ID: {UserData.Instance.SelectCardFrameId}, 이미지 경로: {frameImgPath}");

        // deco 설정
        string decoImgPath = InGameData.Instance.ShopItems.FirstOrDefault(x => x.ItemId == UserData.Instance.SelectDecoId).ImgPath;
        cardDecoObj.transform.GetChild(1).GetComponent<Image>().sprite = Resources.Load<Sprite>(decoImgPath);

        // 메인 설정
        mainCardObj.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(frameImgPath);

        if (UserData.Instance.SelectDecoId == 2)
        {
            mainCardObj.transform.GetChild(1).gameObject.SetActive(false);
            cardDecoObj.transform.GetChild(2).gameObject.SetActive(true);
        } else
        {
            mainCardObj.transform.GetChild(1).gameObject.SetActive(true);
            cardDecoObj.transform.GetChild(2).gameObject.SetActive(false);
            mainCardObj.transform.GetChild(1).GetComponent<Image>().sprite = Resources.Load<Sprite>(decoImgPath);
        }

    }


    // 버튼 클릭
    public void OnClickFrameButton()
    {
        StartCoroutine(ShowBagItemsCoroutine(EnumTypes.ShopItemType.frame));
    }
    public void OnClickDecoButton()
    {
        StartCoroutine(ShowBagItemsCoroutine(EnumTypes.ShopItemType.deco));
    }
}
