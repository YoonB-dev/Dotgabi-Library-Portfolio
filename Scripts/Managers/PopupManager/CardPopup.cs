using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardPopup : MonoBehaviorSingleton<CardPopup>
{
    [SerializeField] private Canvas cardCanvase;
    [SerializeField] private GameObject cardShowView;
    [SerializeField] private Transform contentPos;

    [SerializeField] private CardShowDetail cardShowDetail; // 카드 상세 정보를 보여주는 컴포넌트
    [SerializeField] private GameObject jobButtonGroup;
    [SerializeField] private GameObject backButton;
    [SerializeField] private GameObject backgoundButton;
    public PopUpSO popupSO { get; set; }
    public static int cardUpDePrice = 0;

    [Header("card Popups - whole, detail")]
    [SerializeField] private GameObject cardPopup; // 카드 팝업 (디테일X)
    [SerializeField] private GameObject cardDetailPopup; // 카드 상세 팝업 (디테일O)

    // <summary>
    // 카드 팝업을 보여줍니다. - 메인 화면
    // </summary>
    public void ShowCardPopup(EnumTypes.JobType job = EnumTypes.JobType.Public, bool isFirst = false)
    {
        SetPopUp();
        // SFX
        AudioManager.Instance.ButtonClickSound1();
        // 팝업 애니메이션
        if (isFirst) { if (!jobButtonGroup.activeSelf) { jobButtonGroup.SetActive(true); } }
        // 배경 움직임 비활성화
        // mainManager.cambox.SetCanMove(false);
        // 카드 생성
        var cardData = InGameData.Instance.Cards.FindAll(card => card.CardJob != null && card.CardJob.Contains((int)job));
        StartCoroutine(ShowCardPopupCoroutine(cardData, isFirst));

    }

    public void ShowCardPopupList(List<CardDTO> cardData)
    {
        SetPopUp();
        // SFX
        AudioManager.Instance.ButtonClickSound1();
        // 팝업 애니메이션
        if (jobButtonGroup.activeSelf) { jobButtonGroup.SetActive(false); }
        // 배경 움직임 비활성화
        // mainManager.cambox.SetCanMove(false);
        // 카드 생성
        StartCoroutine(ShowCardPopupCoroutine(cardData, false));

    }

    private IEnumerator ShowCardPopupCoroutine(List<CardDTO> cardData, bool isFirst)
    {
        if (isFirst) { ButtonAnim.Instance.ButtonScaleIn(cardShowView, 0.3f, 1f); }
        backButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 100);

        // 카드 팝업 위치 설정
        //var cardData = InGameData.Instance.Cards.FindAll(card => card.CardJob != null && card.CardJob.Contains((int)job));
        // 카드 데이터가 contentPos의 자식 개수보다 많을 경우, 부족한 만큼 카드 오브젝트를 생성 - 후에 비활성화
        if (cardData.Count > contentPos.transform.childCount)
        {
            for (int i = contentPos.transform.childCount; i < cardData.Count; i++)
            {
                var card = Instantiate(popupSO.content, contentPos);
                card.SetActive(false);
            }
        }

        // 카드 데이터가 contentPos의 자식 개수보다 적을 경우, 남는 카드 오브젝트를 비활성화
        for (int i = cardData.Count; i < contentPos.transform.childCount; i++)
        {
            contentPos.transform.GetChild(i).gameObject.SetActive(false);
        }

        // 카드 데이터에 따라 카드 오브젝트를 활성화하고 설정
        for (int i = 0; i < cardData.Count; i++)
        {
            var targetCard = contentPos.transform.GetChild(i).gameObject;
            targetCard.SetActive(true);

            // 카드 DTO를 오브젝트에 설정
            targetCard = CardDTOToObj.DTOToObj(targetCard, cardData[i]);

            // 카드 클릭 이벤트 설정
            Button cardButton = targetCard.GetComponent<Button>();
            cardButton.onClick.RemoveAllListeners();
            int index = i;
            cardButton.onClick.AddListener(() => {
                // 카드 상세 정보 보여주기
                cardShowDetail.ShowCardDetail(cardData[index]);
            });
            if (i % 5 == 0) yield return null;
        }

        // 카드 팝업 위치 초기화
        contentPos.GetComponent<RectTransform>().anchoredPosition = new Vector2(contentPos.GetComponent<RectTransform>().anchoredPosition.x, 0);
    }

    public void ShowMainOwnedCardPopup()
    {
        SetPopUp();
        // SFX
        AudioManager.Instance.ButtonClickSound1();
        // 팝업 애니메이션
        backButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 120);
        ButtonAnim.Instance.ButtonScaleIn(cardShowView, 0.3f, 1f);

        jobButtonGroup.SetActive(false);

        // 배경 움직임 비활성화
        // mainManager.cambox.SetCanMove(false);
        // 카드 생성
        StartCoroutine(ShowMainOwnedCardPopupCoroutine());
    }
    private IEnumerator ShowMainOwnedCardPopupCoroutine()
    {
        List<UserScenarioOwnedCardDTO> cardData = GetOwnedCardList();
        if (cardData == null)
        {
            Debug.LogError("ShowMainOwnedCardPopup: cardData is null");
            yield break;
        }


        if (cardData.Count > contentPos.transform.childCount)
        {
            for (int i = contentPos.transform.childCount; i < cardData.Count; i++)
            {
                var card = Instantiate(popupSO.content, contentPos);
                card.SetActive(false);
            }
        }

        // 카드 데이터가 contentPos의 자식 개수보다 적을 경우, 남는 카드 오브젝트를 비활성화
        for (int i = cardData.Count; i < contentPos.transform.childCount; i++)
        {
            contentPos.transform.GetChild(i).gameObject.SetActive(false);
        }

        // 카드 데이터에 따라 카드 오브젝트를 활성화하고 설정
        for (int i = 0; i < cardData.Count; i++)
        {
            var targetCard = contentPos.transform.GetChild(i).gameObject;
            targetCard.SetActive(true);

            // 카드 DTO를 오브젝트에 설정
            var cardDTO = InGameData.Instance.Cards.Find(card => card.Id == cardData[i].CardId).Copy();
            cardDTO.CardUpgrade = cardData[i].UpgradeTime;
            Debug.Log("Owned Card - CardUpgrade: " + cardDTO.CardUpgrade);

            targetCard = CardDTOToObj.DTOToObj(targetCard, cardDTO);

            // 카드 클릭 이벤트 설정
            Button cardButton = targetCard.GetComponent<Button>();
            cardButton.onClick.RemoveAllListeners();
            int index = i;
            cardButton.onClick.AddListener(() => {
                // 카드 상세 정보 보여주기
                cardShowDetail.ShowCardDetail(cardDTO);
            });
            if (i % 5 == 0) yield return null;
        }

        // 카드 팝업 위치 초기화
        contentPos.GetComponent<RectTransform>().anchoredPosition = new Vector2(contentPos.GetComponent<RectTransform>().anchoredPosition.x, 0);
    }

    // 카드 강화 팝업을 보여준다.
    public void ShowCardUpgradePopup(Action action = null)
    {
        SetPopUp();
        // SFX
        AudioManager.Instance.ButtonClickSound1();
        // 팝업 애니메이션
        ButtonAnim.Instance.ButtonScaleIn(cardShowView, 0.3f, 1f);
        jobButtonGroup.SetActive(false);
        backButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 120);

        // 배경 움직임 비활성화
        // mainManager.cambox.SetCanMove(false);
        // 카드 생성
        StartCoroutine(ShowCardUpgradePopupCoroutine(action));
    }

    private IEnumerator ShowCardUpgradePopupCoroutine(Action action = null)
    {
        var ownedCardList = GetOwnedCardList();
        if (ownedCardList == null)
        {
            Debug.LogError("ShowCardUpgradePopup: ownedCardList is null");
            yield break;
        }
        var canUpgradeCardList = CardCheckUtils.Instance.GetCanUpgradeCardDTO(ownedCardList, false);

        if (canUpgradeCardList.Count > contentPos.transform.childCount)
        {
            for (int i = contentPos.transform.childCount; i < canUpgradeCardList.Count; i++)
            {
                var card = Instantiate(popupSO.content, contentPos);
                card.SetActive(false);
            }
        }

        // 카드 데이터가 contentPos의 자식 개수보다 적을 경우, 남는 카드 오브젝트를 비활성화
        for (int i = canUpgradeCardList.Count; i < contentPos.transform.childCount; i++)
        {
            contentPos.transform.GetChild(i).gameObject.SetActive(false);
        }

        // 카드 데이터에 따라 카드 오브젝트를 활성화하고 설정
        for (int i = 0; i < canUpgradeCardList.Count; i++)
        {
            var targetCard = contentPos.transform.GetChild(i).gameObject;
            targetCard.SetActive(true);

            // 카드 DTO를 오브젝트에 설정
            var cardDTO = InGameData.Instance.Cards.Find(card => card.Id == canUpgradeCardList[i].CardId).Copy();
            cardDTO.CardUpgrade = canUpgradeCardList[i].UpgradeTime;

            targetCard = CardDTOToObj.DTOToObj(targetCard, cardDTO);

            // 카드 클릭 이벤트 설정
            Button cardButton = targetCard.GetComponent<Button>();

            int index = i;
            cardButton.onClick.RemoveAllListeners();
            cardButton.onClick.AddListener(() => {
                // 카드 상세 정보 보여주기
                cardShowDetail.ShowCardUpgradeDetail(cardDTO, ownedId: canUpgradeCardList[index].OwnedId, action: action);
            });
            if (i % 5 == 0) yield return null;
        }
        // 카드 팝업 위치 초기화
        contentPos.GetComponent<RectTransform>().anchoredPosition = new Vector2(contentPos.GetComponent<RectTransform>().anchoredPosition.x, 0);
    }

    // 카드 삭제 팝업을 보여준다.
    public void ShowCardDeletePopup(bool canBack, Action action = null)
    {
        SetPopUp();
        // SFX
        AudioManager.Instance.ButtonClickSound1();
        // 팝업 애니메이션
        ButtonAnim.Instance.ButtonScaleIn(cardShowView, 0.3f, 1f);
        jobButtonGroup.SetActive(false);
        backButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 120);

        backButton.SetActive(canBack);
        backgoundButton.GetComponent<Button>().interactable = canBack;


        // 배경 움직임 비활성화
        // mainManager.cambox.SetCanMove(false);
        // 카드 생성
        StartCoroutine(ShowDeleteMainOwnedCardPopupCoroutine(action));
    }

    // 카드 삭제
    private IEnumerator ShowDeleteMainOwnedCardPopupCoroutine(Action action = null)
    {
        var cardData = new List<UserScenarioOwnedCardDTO>();
        var ownedCardList = GetOwnedCardList();
        if (ownedCardList == null)
        {
            Debug.LogError("ShowDeleteMainOwnedCardPopup: ownedCardList is null");
            yield break;
        }

        foreach (var card in ownedCardList)
        {
            if (card.CardId == 54 || card.CardId == 55) continue;
            cardData.Add(card);
        }

        if (cardData.Count > contentPos.transform.childCount)
        {
            for (int i = contentPos.transform.childCount; i < cardData.Count; i++)
            {
                var card = Instantiate(popupSO.content, contentPos);
                card.SetActive(false);
            }
        }

        // 카드 데이터가 contentPos의 자식 개수보다 적을 경우, 남는 카드 오브젝트를 비활성화
        for (int i = cardData.Count; i < contentPos.transform.childCount; i++)
        {
            contentPos.transform.GetChild(i).gameObject.SetActive(false);
        }

        // 카드 데이터에 따라 카드 오브젝트를 활성화하고 설정
        for (int i = 0; i < cardData.Count; i++)
        {
            var targetCard = contentPos.transform.GetChild(i).gameObject;
            targetCard.SetActive(true);

            // 카드 DTO를 오브젝트에 설정
            var cardDTO = InGameData.Instance.Cards.Find(card => card.Id == cardData[i].CardId).Copy();
            cardDTO.CardUpgrade = cardData[i].UpgradeTime;
            Debug.Log("Delete Card - CardUpgrade: " + cardDTO.CardUpgrade);

            targetCard = CardDTOToObj.DTOToObj(targetCard, cardDTO);

            // 카드 클릭 이벤트 설정
            Button cardButton = targetCard.GetComponent<Button>();
            cardButton.onClick.RemoveAllListeners();
            int index = i;
            cardButton.onClick.AddListener(() => {
                // 카드 상세 정보 보여주기
                cardShowDetail.ShowCardDeleteDetail(cardDTO, ownedId: cardData[index].OwnedId, action: action);
            });
            if (i % 5 == 0) yield return null;
        }
        // 카드 팝업 위치 초기화
        contentPos.GetComponent<RectTransform>().anchoredPosition = new Vector2(contentPos.GetComponent<RectTransform>().anchoredPosition.x, 0);
    }

    public void HideCardPopup()
    {
        cardCanvase.gameObject.SetActive(false);
        // SFX
        AudioManager.Instance.ButtonClickSound1();
        // 배경 움직임 활성화 -> mainmanager존재하면
        MainManager.InstanceOrNull?.cambox?.SetCanMove(true);
    }

    // <summary>
    // 각 직업별 카드 팝업을 보여준다. -> 버튼 이벤트로 사용
    // </summary>
    public void ShowCardPopupByJob(string jobName)
    {
        EnumTypes.JobType jobType = (EnumTypes.JobType)System.Enum.Parse(typeof(EnumTypes.JobType), jobName);
        ShowCardPopup(jobType);
    }
    // 캐릭터에서 버튼 클릭 시 호출되는 메서드
    public void ShowCardPopupByJob(EnumTypes.JobType jobType, bool isFirst = false)
    {
        ShowCardPopup(jobType, isFirst);
        if (jobButtonGroup.activeSelf) { jobButtonGroup.SetActive(false); }
    }

    public void ShowUsedCardPopup()
    {
        if (CardSystem.Instance == null)
        {
            Debug.LogWarning("CardSystem.Instance is null. Cannot show used card popup.");
            return;
        }
        SetPopUp();
        jobButtonGroup.SetActive(false);
        var usedCardData = CardSystem.Instance.usedCards;
        StartCoroutine(ShowCardPopupCoroutine(usedCardData, true));
    }

    public void ShowCanCardPopup()
    {
        if (CardSystem.Instance == null)
        {
            Debug.LogWarning("CardSystem.Instance is null. Cannot show deck card popup.");
            return;
        }

        SetPopUp();
        jobButtonGroup.SetActive(false);
        var canCardData = CardSystem.Instance.canCards;
        StartCoroutine(ShowCardPopupCoroutine(canCardData, true));
    }

    public void ShowCardJustDetail(CardDTO cardDTO)
    {
        SetDetailPopUp();
        // SFX
        AudioManager.Instance.ButtonClickSound1();
        // 카드 상세 정보 보여주기
        cardShowDetail.ShowCardDetail(cardDTO);
    }



    // 뒤로가기 배경이 클릭 이벤트가 없을 시 추가
    public void SetBackgroundBackButton()
    {
        backgoundButton.GetComponent<Button>().interactable = true;
    }

    private void SetPopUp()
    {
        cardCanvase.gameObject.SetActive(true);
        cardPopup.SetActive(true);
        cardDetailPopup.SetActive(false);
    }

    private void SetDetailPopUp()
    {
        cardCanvase.gameObject.SetActive(true);
        cardPopup.SetActive(false);
        cardDetailPopup.SetActive(true);
    }

    public List<UserScenarioOwnedCardDTO> GetOwnedCardList()
    {
        switch (GameData.Instance.CurrScenarioType)
        {
            case EnumMainType.ScenarioType.story:
                return UserData.Instance.MainScenarioData.OwnedCardList;
            case EnumMainType.ScenarioType.challenge:
                return UserData.Instance.ChallengeScenarioData.OwnedCardList;
            default:
                Debug.LogError("GetOwnedCardList: Invalid ScenarioType");
                return null;
        }
    }
}
