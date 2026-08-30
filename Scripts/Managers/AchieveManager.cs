using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Localization.SmartFormat;
using DG.Tweening;
using System.Threading.Tasks;

public class AchieveManager : SceneSingleton<AchieveManager>
{
    [Header("Achieve UI")]
    [SerializeField] private Transform AchieveContent;
    [SerializeField] private GameObject AchievePrefab;
    [SerializeField] private TextMeshProUGUI AchieveCountText;
    [SerializeField] private TextMeshProUGUI AchieveCurrTxt;
    [SerializeField] private Image AchieveGageImg;
    [SerializeField] private Sprite[] AchieveImages = new Sprite[3];
    enum AchieveState { finish, clear, notclear }

    [Header("Big Achieve UI")]
    [SerializeField] private TextMeshProUGUI BigAchieveCountText;
    [SerializeField] private Image GaugeImg;
    [SerializeField] private GameObject[] BigAchieveRewardObjs;
    int finishedAchievesCount = 0;
    readonly int[] BigAchieveRewardInterval = new int[] { 10, 20, 30, 50 };

    [Header("Big Achieve Detail UI - GET")]
    [SerializeField] private GameObject BigAchieveDetailPopup;
    [SerializeField] private GameObject BigAchieveDetailBox;
    [SerializeField] private Image BigPriceImg;
    [SerializeField] private TextMeshProUGUI BigPriceNameTxt;
    [SerializeField] private TextMeshProUGUI BigPriceDescTxt;

    public void SetAchieveObjList()
    {
        var allAchieveGroups = InGameData.Instance.AchieveDTOLists; // 전체 업적 그룹 리스트
        Debug.Log("All Achieve Groups Count: " + allAchieveGroups.Count);
        var clearAchieveList = UserData.Instance.UserClearAchieveList;

        HashSet<int> finishedAchieveIds = new(clearAchieveList.Select(c => c.AchieveId));

        // 가장 위쪽에 표시할 '클리어 업적(보상은 아직 안받은)' 리스트
        List<AchieveDTO> clearedAchieves = new();
        // 중간에 표시할 '미클리어 최신 업적' 리스트
        List<AchieveDTO> notClearedLatestAchieves = new();
        // 아래쪽에 표시할 '끝난 업적(클리어 및 보상획득 완료)' 리스트
        List<AchieveDTO> finishedAchieves = new();

        foreach (var achieveGroup in allAchieveGroups)
        {
            EnumTypes.AchieveType groupType = achieveGroup.AchieveType;

            AchieveDTO nextUncleared = achieveGroup.Achieves
                .Where(a => !finishedAchieveIds.Contains(a.Id) && !IsAchieveCleared(a, groupType))
                .OrderBy(a => a.Level)
                .FirstOrDefault();

            // "다음 목표" 하나만 리스트에 추가
            if (nextUncleared != null)
            {
                notClearedLatestAchieves.Add(nextUncleared);
            }

            // 이미 목표치 충족(클리어, 보상 미수령) 업적 모으기
            var clearedInGroup = achieveGroup.Achieves
                .Where(a => !finishedAchieveIds.Contains(a.Id) && IsAchieveCleared(a, groupType))
                .OrderBy(a => a.Level)
                .ToList();
            clearedAchieves.AddRange(clearedInGroup);

            // 보상까지 받은 업적(완료) 리스트
            var finishedInGroup = achieveGroup.Achieves
                .Where(a => finishedAchieveIds.Contains(a.Id))
                .OrderBy(a => a.Level)
                .ToList();
            finishedAchieves.AddRange(finishedInGroup);
        }
        // -----UI 세팅-----

        // 모든 기존 자식 비활성화
        for (int i = 0; i < AchieveContent.childCount; i++)
        {
            AchieveContent.GetChild(i).gameObject.SetActive(false);
        }

        // 필요한 오브젝트 수 생성
        int totalCount = clearedAchieves.Count + notClearedLatestAchieves.Count + finishedAchieves.Count;
        for (int i = AchieveContent.childCount; i < totalCount; i++)
        {
            var obj = Instantiate(AchievePrefab, AchieveContent);
            obj.SetActive(false);
        }

        int idx = 0;
        // 클리어 O
        foreach (var achieve in clearedAchieves)
        {
            var obj = AchieveContent.GetChild(idx).gameObject;
            obj.SetActive(true);
            SetAchieveObj(obj, achieve, achieveState: AchieveState.clear, achieveType: achieve.AchieveType); // 클리어(보상 안받음)
            idx++;
        }
        // 클리어 X
        foreach (var achieve in notClearedLatestAchieves)
        {
            var obj = AchieveContent.GetChild(idx).gameObject;
            obj.SetActive(true);
            SetAchieveObj(obj, achieve, achieveState: AchieveState.notclear, achieveType: achieve.AchieveType); // 미클리어
            idx++;
        }
        // 아래쪽(끝난 업적) 표시
        foreach (var achieve in finishedAchieves)
        {
            var obj = AchieveContent.GetChild(idx).gameObject;
            obj.SetActive(true);
            SetAchieveObj(obj, achieve, achieveState: AchieveState.finish, achieveType: achieve.AchieveType); // 완료(보상까지 받음)
            idx++;
        }
        // 업적 달성도 UI 세팅
        finishedAchievesCount = finishedAchieves.Count;
        SetBigAchieve();

        // 대기 UI 비활성화
        MainManager.Instance.SetLoadingCanvas(false);
    }

    private bool IsAchieveCleared(AchieveDTO achieve, EnumTypes.AchieveType achieveType)
    {
        var currValue = GetAchieveCurrValue(achieve, achieveType);
        return currValue >= achieve.TargetValue ? true : false;
    }

    private int GetAchieveCurrValue(AchieveDTO achieve, EnumTypes.AchieveType achieveType)
    {
        var userCurrData = UserData.Instance.UserAchieveCurrData;
        var cardCollection = UserData.Instance.UserOwnedCardList;
        var artifactCollection = UserData.Instance.UserOwnedArtifactList;

        switch (achieveType)
        {
            // 카드 수집 개수
            case EnumTypes.AchieveType.card_collection_count:
                return cardCollection.Count();
            // 유물 수집 개수
            case EnumTypes.AchieveType.artifact_collection_count:
                return artifactCollection.Count();
            // 이동 거리
            case EnumTypes.AchieveType.move_forward_count:
                return userCurrData.MoveForwardCount;
            // 전투 횟수
            case EnumTypes.AchieveType.battle_count:
                return userCurrData.BattleCount;
            // 상점 구매 횟수
            case EnumTypes.AchieveType.shop_purchase_count:
                return userCurrData.ShopPurchaseCount;
            // 휴식 횟수
            case EnumTypes.AchieveType.rest_count:
                return userCurrData.RestCount;
            // 광고 시청 횟수
            case EnumTypes.AchieveType.show_ad_count:
                return userCurrData.ShowAdCount;
            // 카드 사용 횟수
            case EnumTypes.AchieveType.total_use_card:
                return userCurrData.TotalUseCard;
            // 사용한 코인의 총합
            case EnumTypes.AchieveType.total_coin_use:
                return userCurrData.TotalCoinUse;
        }
        return 0;
    }

    private void SetAchieveObj(GameObject obj, AchieveDTO achieveDTO, AchieveState achieveState, EnumTypes.AchieveType achieveType)
    {
        var achieveDesText = obj.transform.Find("AchieveDesText").GetComponent<TextMeshProUGUI>();

        // 설명 텍스트 세팅
        achieveDesText.text = achieveDTO.Description.FormatSmart(achieveDTO.TargetValue);
        var achieveBar = obj.transform.GetChild(2).gameObject;
        var completeButton = obj.transform.Find("CompleteBtn").GetComponent<Button>();

        // 보상 텍스트 세팅
        var achievePrice = obj.transform.GetChild(0).GetChild(1);
        achievePrice.GetComponent<TextMeshProUGUI>().text = $"{achieveDTO.PriceAmount}";

        // 진행도 텍스트 세팅
        var bar = obj.transform.GetChild(2).GetChild(0);
        var currValue = GetAchieveCurrValue(achieveDTO, achieveType);
        bar.GetComponent<Image>().fillAmount = (float)currValue / achieveDTO.TargetValue;

        var barText = obj.transform.GetChild(2).GetChild(1).GetComponent<TextMeshProUGUI>();
        barText.text = $"{currValue} / {achieveDTO.TargetValue}";


        switch (achieveState)
        {
            case AchieveState.clear:
                obj.transform.GetComponent<Image>().sprite = AchieveImages[1];
                achieveBar.SetActive(false);
                completeButton.gameObject.SetActive(true);
                completeButton.onClick.RemoveAllListeners();
                completeButton.onClick.AddListener(() => CompleteAchieve(achieveDTO));
                break;
            case AchieveState.notclear:
                obj.transform.GetComponent<Image>().sprite = AchieveImages[0];
                achieveBar.SetActive(true);
                completeButton.gameObject.SetActive(false);
                break;
            case AchieveState.finish:
                obj.transform.GetComponent<Image>().sprite = AchieveImages[2];
                achieveBar.SetActive(false);
                completeButton.gameObject.SetActive(false);
                break;
        }
    }

    private async void CompleteAchieve(AchieveDTO achieve)
    {
        // 처리 중 배경 활성화
        MainManager.Instance.SetLoadingCanvas(true);
        // 업적 보상 처리
        var client = SupabaseClientProvider.Instance.Client;
        var response = await client.Rpc("user_clear_check_achieve_rpc", new Dictionary<string, object> {
            { "p_achieve_id", achieve.Id },
            { "p_level", achieve.Level },
        });

        bool result = bool.TryParse(response.Content, out result);

        // 업적 완료 처리
        if (result)
        {
            UserData.Instance.UserClearAchieveList.Add(new UserClearAchieveDTO { AchieveId = achieve.Id });
        }
        else
        {
            Debug.LogError("error on complete achieve");
        }

        UserData.Instance.UserClearAchieveList = await AchieveDAO.Instance.GetUserClearAchievesAsync(UserData.Instance.UserAuthId);

        // 처리 중 배경 비활성화
        MainManager.Instance.SetLoadingCanvas(false);

        // UI 업데이트
        SetAchieveObjList();
        MainManager.Instance?.GetPoint("Achieve", achieve.PriceAmount);
    }


    // 큰 업적 UI 세팅
    private void SetBigAchieve()
    {
        BigAchieveCountText.text = $"{finishedAchievesCount}";
        int totalAchieveCount = InGameData.Instance.AchieveDTOLists.Sum(group => group.Achieves.Count);
        GaugeImg.fillAmount = (float)finishedAchievesCount / totalAchieveCount;

        SetBigAchievePosition();
        SetBigAchieveRewardObj();
    }

    public void SetBigAchievePosition()
    {
        float gaugeWidth = AchieveGageImg.rectTransform.rect.width;
        float halfWidth = gaugeWidth / 2f;
        int totalAchieveCount = InGameData.Instance.AchieveDTOLists.Sum(group => group.Achieves.Count);
        var getData = UserData.Instance.UserAchievePriceGetData;
        for (int i = 0; i < BigAchieveRewardObjs.Length && i < BigAchieveRewardInterval.Length; i++)
        {
            var obj = BigAchieveRewardObjs[i];

            // 전체 대비 비율 계산
            float ratio = (float)BigAchieveRewardInterval[i] / totalAchieveCount;

            // 왼쪽 기준에서 중앙 기준으로 맞춰서 위치 계산
            float xPos = (gaugeWidth * ratio) - halfWidth;
            obj.transform.localPosition = new Vector3(
                x: xPos,
                y: obj.transform.localPosition.y,
                z: obj.transform.localPosition.z
            );
        }
    }

    private void SetBigAchieveRewardObj()
    {
        var getPriceData = UserData.Instance.UserAchievePriceGetData;
        bool[] bigPrices = new bool[] {
                getPriceData.BigPrice1,
                getPriceData.BigPrice2,
                getPriceData.BigPrice3,
                getPriceData.BigPrice4
            };

        var achievePriceData = InGameData.Instance.ShopItems
            .Where(item => item.ItemSource == EnumMainType.ItemSourceType.achieve_price)
            .OrderBy(item => item.ItemId)
            .ToList();

        for (int i = 0; i < BigAchieveRewardObjs.Length; i++)
        {
            bool getBool = bigPrices[i];
            var rewardObj = BigAchieveRewardObjs[i];
            var data = achievePriceData[i];
            // 보상 이미지 설정
            rewardObj.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(data.ImgPath);
            // 보상 버튼 크기 및 색상 초기화
            rewardObj.transform.DOKill();
            rewardObj.transform.localScale = Vector2.one;
            rewardObj.GetComponent<Image>().color = Color.gray; // 회색톤으로 변경
            // 보상 버튼 설정
            rewardObj.GetComponent<Button>().onClick.RemoveAllListeners();
            int index = i;

            // 획득 가능한 상태인지 확인 - 업적 달성 했고, 보상 아직 안받은 상태
            if (finishedAchievesCount >= BigAchieveRewardInterval[i])
            {
                rewardObj.transform.GetChild(2).gameObject.SetActive(false); // 잠금 아이콘 숨기기
                if (!getBool)
                {
                    rewardObj.GetComponent<Button>().interactable = true;
                    rewardObj.transform.DOScale(Vector2.one * 1.2f, 0.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine); // 획득 가능 애니메이션
                    rewardObj.GetComponent<Image>().color = Color.white; // 원래 색상으로 변경
                    rewardObj.GetComponent<Button>().onClick.AddListener(() => {
                        // 보상 획득
                        GetAchieveReward(data, index + 1);
                    });
                }
                else
                {
                    // 이미 보상 받은 상태

                    rewardObj.GetComponent<Button>().onClick.AddListener(() => {
                        OpenBigAchieveDetailPopup(data);
                    });
                }
                continue;
            }
            // 획득 불가능한 상태 - 업적 달성 못함
            else
            {
                rewardObj.transform.GetChild(2).gameObject.SetActive(true); // 잠금 아이콘 보이기
                rewardObj.GetComponent<Button>().onClick.AddListener(() => {
                    OpenBigAchieveDetailPopup(data);
                });
            }

        }
    }

    private void OpenBigAchieveDetailPopup(ShopItemDTO itemDTO)
    {
        BigPriceImg.sprite = Resources.Load<Sprite>(itemDTO.ImgPath);
        BigPriceNameTxt.text = itemDTO.ItemName;
        BigPriceDescTxt.text = itemDTO.ItemDescription;

        BigAchieveDetailPopup.SetActive(true);
        ButtonAnim.Instance.ButtonScaleIn(BigAchieveDetailBox, 0f, 1f);

        //SFX
        AudioManager.Instance.ButtonClickSound1();
    }

    public void CloseBigAchieveDetailPopup()
    {
        //SFX
        AudioManager.Instance.ButtonClickSound3();
        BigAchieveDetailPopup.SetActive(false);
    }

    // ------------------업적 보상 획득-------------------
    private async void GetAchieveReward(ShopItemDTO itemDTO, int rewardIndex)
    {
        // 로딩 팝업 활성화
        MainManager.Instance.SetLoadingCanvas(true);
        var success = await GetAchieveRewardSupabase(itemDTO, rewardIndex);
        if (success)
        {
            OpenBigAchieveDetailPopup(itemDTO);
            var text = LogManager.Instance?.GetLocalText("item_get").FormatSmart(itemDTO.ItemName);
            BigPriceDescTxt.text = text;
        }
        else
        {
            var text = LogManager.Instance?.GetLocalText("item_get_fail");
            NotificationManager.Instance.SetShownNotification(text);
        }

        // 보상 오브젝트 업데이트
        SetAchieveObjList();
        // 로딩 팝업 비활성화
        MainManager.Instance.SetLoadingCanvas(false);
    }

    private async Task<bool> GetAchieveRewardSupabase(ShopItemDTO itemDTO, int rewardIndex)
    {
        var client = SupabaseClientProvider.Instance.Client;

        var response = await SupabaseWrap.ExecuteWithRefresh(() => client
            .Rpc("update_user_achieve_price_get", new Dictionary<string, object>
            {
                { "p_price_number", rewardIndex },
                { "p_item_id", itemDTO.ItemId }}
            ));

        bool result = bool.Parse(response.Content);

        if (!result)
        {
            Debug.LogError("Error in GetAchieveRewardSupabase: Failed to get achieve reward");
            return false;
        }

        try
        {
            // 최신화
            UserData.Instance.UserAchievePriceGetData = await AchieveDAO.Instance.GetUserAchievePriceGetAsync(UserData.Instance.UserAuthId);
            ShopDetailPopup.Instance?.UpdateUserOwnedFrameList(itemDTO);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in GetAchieveRewardSupabase: {e.Message}");
            result = false;
        }

        return result;
    }
}
