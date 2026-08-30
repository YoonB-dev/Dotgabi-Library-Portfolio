using System;
using UnityEngine;
using UnityEngine.UI;

public class CardShowDetail : SceneSingleton<CardShowDetail>
{
    // <summary>
    // 카드 상세 정보를 보여주는 클래스입니다.
    // </summary>
    [SerializeField] private Canvas cardCanvase;
    [SerializeField] private GameObject cardDetailView;

    [Header("Card Detail Show Type")]
    [SerializeField] private GameObject cardDetailShowView; // 카드 상세 정보를 보여주는 뷰
    [SerializeField] private GameObject cardDetailForm; // 카드 상세 폼

    [Header("Card Delete Show Type")]
    [SerializeField] private GameObject cardDeleteShowView; // 카드 삭제 정보를 보여주는 뷰
    [SerializeField] private GameObject cardDeleteForm; // 카드 삭제 폼
    [SerializeField] private GameObject cardDeleteButton; // 카드 삭제 버튼
    [Header("Card Upgrade Show Type")]
    [SerializeField] private GameObject cardUpgradeShowView; // 카드 업그레이드 정보를 보여주는 뷰
    [SerializeField] private GameObject cardUpgradeForm; // 카드 업그레이드 폼
    [SerializeField] private GameObject cardUpgradeObjBefore; // 카드 업그레이드 폼
    [SerializeField] private GameObject cardUpgradeObjAfter; // 카드 업그레이드 폼
    [SerializeField] private GameObject cardUpgradeButton; // 카드 업그레이드 버튼
    public void ShowCardDetail(CardDTO cardDTO)
    {
        // 카드 상세 정보를 보여주는 로직을 구현합니다.
        // 예: 카드 이름, 설명, 이미지 등을 UI에 표시
        // SFX
        AudioManager.Instance.ButtonClickSound1();
        // 캔버스 활성화
        if (cardCanvase.isActiveAndEnabled == false) { cardCanvase.gameObject.SetActive(true); }
        cardDetailView.SetActive(true);
        cardDetailShowView.SetActive(true);
        cardDeleteShowView.SetActive(false);
        cardUpgradeShowView.SetActive(false);

        // 애니메이션
        ButtonAnim.Instance.ButtonScaleIn(cardDetailForm.transform.parent.gameObject, 0.2f, 1f);

        // 카드 정보 설정
        cardDetailForm = CardDTOToObj.DTOToObj(cardDetailForm, cardDTO);
        CardDTOToObj.SetCardAbilitys(cardDetailForm, cardDTO);
    }

    public void HideCardDetail()
    {
        // 카드 상세 정보를 숨기는 로직을 구현합니다.
        // SFX
        AudioManager.Instance.ButtonClickSound1();
        // 캔버스 비활성화
        cardDetailView.SetActive(false);
        cardDeleteShowView.SetActive(false);
        cardUpgradeShowView.SetActive(false);
        // 애니메이션
    }
    // 강화
    public void ShowCardUpgradeDetail(CardDTO cardDTO, int ownedId, Action action = null)
    {
        AudioManager.Instance.ButtonClickSound1();
        // 캔버스 활성화
        if (cardCanvase.isActiveAndEnabled == false) { cardCanvase.gameObject.SetActive(true); }
        cardDetailView.SetActive(true);
        cardDetailShowView.SetActive(false);
        cardDeleteShowView.SetActive(false);
        cardUpgradeShowView.SetActive(true);
        // 애니메이션
        ButtonAnim.Instance.ButtonScaleIn(cardUpgradeForm, 0.2f, 1f);

        // 카드 정보 설정
        cardUpgradeObjBefore = CardDTOToObj.DTOToObj(cardUpgradeObjBefore, cardDTO);
        //CardDTOToObj.SetCardAbilitys(cardUpgradeObjBefore, cardDTO);

        var cardUpgradeData = CardUpgradeUtils.Instance.ShowUpgradeCard(cardDTO);
        cardUpgradeObjAfter = CardDTOToObj.DTOToObj(cardUpgradeObjAfter, cardUpgradeData);

        //CardDTOToObj.SetCardAbilitys(cardUpgradeObjAfter, cardDTO);

        // 카드 버튼 설정
        var ownedCardList = CardPopup.Instance.GetOwnedCardList();
        if (ownedCardList == null)
        {
            Debug.LogError("ShowCardUpgradeDetail: ownedCardList is null");
            return;
        }
        int currUpgradeLevel = ownedCardList.Find(c => c.OwnedId == ownedId).UpgradeTime;
        cardUpgradeButton.GetComponent<Button>().onClick.RemoveAllListeners();
        cardUpgradeButton.GetComponent<Button>().onClick.AddListener(() => {
            // 카드 업그레이드 로직을 구현합니다.
            SupabaseCard.Instance.UpgradeCard(MoveSystem.Instance.SCENARIO_DATA, ownedId, cardDTO.Name);
            HideCardDetail();
            CardPopup.Instance.HideCardPopup();
            action?.Invoke();
        });
    }

    // 삭제
    public void ShowCardDeleteDetail(CardDTO cardDTO, int ownedId, Action action = null)
    {
        AudioManager.Instance.ButtonClickSound1();
        // 캔버스 활성화
        if (cardCanvase.isActiveAndEnabled == false) { cardCanvase.gameObject.SetActive(true); }
        cardDetailView.SetActive(true);
        cardDetailShowView.SetActive(false);
        cardDeleteShowView.SetActive(true);
        cardUpgradeShowView.SetActive(false);
        // 애니메이션
        ButtonAnim.Instance.ButtonScaleIn(cardDeleteForm.transform.parent.gameObject, 0.2f, 1f);

        // 카드 정보 설정
        cardDeleteForm = CardDTOToObj.DTOToObj(cardDeleteForm, cardDTO);
        CardDTOToObj.SetCardAbilitys(cardDeleteForm, cardDTO);

        // 카드 버튼 설정
        var ownedCardList = CardPopup.Instance.GetOwnedCardList();
        if (ownedCardList == null)
        {
            Debug.LogError("ShowCardUpgradeDetail: ownedCardList is null");
            return;
        }
        int currUpgradeLevel = ownedCardList.Find(c => c.OwnedId == ownedId).UpgradeTime;
        cardDeleteButton.GetComponent<Button>().onClick.RemoveAllListeners();
        cardDeleteButton.GetComponent<Button>().onClick.AddListener(() => {
            // 카드 삭제 로직을 구현합니다.
            MysteryResult.Instance.DeleteCardById(ownedId, cardDTO.Name);
            HideCardDetail();
            CardPopup.Instance.HideCardPopup();
            CardPopup.Instance.SetBackgroundBackButton();
            action?.Invoke();
        });
    }
}
