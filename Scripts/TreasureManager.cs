using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using DG.Tweening;
using Spine.Unity;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TreasureManager : SceneSingleton<TreasureManager>
{
    public GameObject[] products;
    public GameObject treasureCanvas,scrollImage,backImage;
    public GameObject forwardButton;
    [SerializeField] private GameObject ADButton;
    [SerializeField] private TextMeshProUGUI ADTimeText;
    private int canSeeAdTime = 2;
    [SerializeField] private GameObject backButton;
    private ScenarioDTO SCENARIO_DATA;
    // 유물방 호출--------------------
    public void CallArtifactRoom()
    {
        SCENARIO_DATA = MoveSystem.Instance?.SCENARIO_DATA;
        if (SCENARIO_DATA == null)
        {
            Debug.LogError("SCENARIO_DATA is null");
            SCENARIO_DATA = UserData.Instance.MainScenarioData;
        }
        StartCoroutine(SetArtifactCanvas());
    }

    // 유물방 입장 시 실행
    private IEnumerator SetArtifactCanvas()
    {
        Debug.Log("유물방 호출됨");
        treasureCanvas.SetActive(true);
        scrollImage.transform.parent.gameObject.SetActive(true);
        scrollImage.SetActive(true);
        scrollImage.transform.GetChild(0).gameObject.SetActive(false);
        scrollImage.transform.GetChild(1).gameObject.SetActive(false);
        scrollImage.transform.GetChild(2).gameObject.SetActive(false);
        backImage.SetActive(true);
        //canSeeAdTime = 2;
        canSeeAdTime = 0;

        for (int i = 0; i < products.Length; i++)
        {
            products[i].SetActive(false);
        }
        scrollImage.transform.GetComponent<SkeletonAnimation>().skeleton.SetToSetupPose();
        scrollImage.transform.GetComponent<SkeletonAnimation>().AnimationState.SetAnimation(0, "open_fadeIn", false);
        yield return new WaitForSecondsRealtime(1.5f);

        for (int i = 0; i < products.Length; i++)
        {
            ButtonAnim.Instance.ButtonScaleIn(products[i], 0f, 0.01f);
        }
        ButtonAnim.Instance.ButtonScaleIn(scrollImage.transform.GetChild(0).gameObject, 0, 0.01f);
        ButtonAnim.Instance.ButtonScaleIn(scrollImage.transform.GetChild(1).gameObject, 0, 0.01f);

        if (canSeeAdTime >= 1)
        {
            ButtonAnim.Instance.ButtonScaleIn(ADButton, 0, 0.01f);
        }

        SetTreasureItems();
    }

    // 유물방 입장 시
    public void SetTreasureItems(bool isRe = false)
    {
        List<ArtifactDTO> ItemDataCommon = new();
        List<ArtifactDTO> ItemDataRare = new();
        List<ArtifactDTO> ItemDataEpic = new();
        List<ArtifactDTO> ItemDataLegend = new();

        //유물 데이터 리스트에 넣기
        //이미 획득한 유물은 제외
        // 조건, 이벤트, 대화, 보스 유물 제외
        int count = 0;
        for (int i=0;i<InGameData.Instance.Artifacts.Count;i++)
        {
            if(SCENARIO_DATA.OwnedArtifactList.Exists(x => x.ArtifactId == InGameData.Instance.Artifacts[i].Id))
            {
                continue;
            }
            if (InGameData.Instance.Artifacts[i].Place == "condition" || InGameData.Instance.Artifacts[i].Place == "event"
            || InGameData.Instance.Artifacts[i].Place == "talk" || InGameData.Instance.Artifacts[i].Place == "boss")
            {
                continue;
            }

            switch (InGameData.Instance.Artifacts[i].Rarity)
            {
                case EnumTypes.RarityType.common:
                    ItemDataCommon.Add(InGameData.Instance.Artifacts[i]);
                    break;
                case EnumTypes.RarityType.rare:
                    ItemDataRare.Add(InGameData.Instance.Artifacts[i]);
                    break;
                case EnumTypes.RarityType.epic:
                    ItemDataEpic.Add(InGameData.Instance.Artifacts[i]);
                    break;
                case EnumTypes.RarityType.legendary:
                    ItemDataLegend.Add(InGameData.Instance.Artifacts[i]);
                    break;
            }
        }

        //순서 바꾸기
        System.Random rand = new System.Random(SCENARIO_DATA.GenerateSeed + SCENARIO_DATA.SelectList.Count * 3 + (count++ * 5));
        if(isRe) rand = new System.Random(Random.Range(0,10000));

        // 각 등급을 랜덤하게 섞기
        for(int i=0;i<ItemDataCommon.Count;i++)
        {
            var temp= ItemDataCommon[i];
            int ran = rand.Next(0, ItemDataCommon.Count);
            ItemDataCommon[i] = ItemDataCommon[ran];
            ItemDataCommon[ran] = temp;
        }
        for (int i = 0; i < ItemDataRare.Count; i++)
        {
            var temp = ItemDataRare[i];
            int ran = rand.Next(0, ItemDataRare.Count);
            ItemDataRare[i] = ItemDataRare[ran];
            ItemDataRare[ran] = temp;
        }
        for (int i = 0; i < ItemDataEpic.Count; i++)
        {
            var temp = ItemDataEpic[i];
            int ran = rand.Next(0, ItemDataEpic.Count);
            ItemDataEpic[i] = ItemDataEpic[ran];
            ItemDataEpic[ran] = temp;
        }
        for (int i = 0; i < ItemDataLegend.Count; i++)
        {
            var temp = ItemDataLegend[i];
            int ran = rand.Next(0, ItemDataLegend.Count);
            ItemDataLegend[i] = ItemDataLegend[ran];
            ItemDataLegend[ran] = temp;
        }

        for (int i = 0; i < products.Length; i++)
        {
            //도깨비 키 -> 스토리 모드에서만 나오게 조건
            if (SCENARIO_DATA.GetType() == typeof(UserMainScenarioDTO))
            {
                var gameData = (UserMainScenarioDTO)SCENARIO_DATA;
                if ((int)gameData.Difficulty >= 3 && !gameData.FirstPiece && i == 2)
                {
                    int keyRan = rand.Next(0, 2);
                    if (keyRan == 0)
                    {
                        var keyData = InGameData.Instance.DotgabiKeys.Find(x => x.KeyId == 1);
                        products[i].GetComponent<Button>().onClick.RemoveAllListeners();
                        products[i].GetComponent<Button>().onClick.AddListener(() => ShowDotgabiKeyInfo(keyData));
                        products[i].transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = keyData.KeyName;
                        products[i].transform.GetChild(2).GetComponent<Image>().sprite = Resources.Load<Sprite>(keyData.ImgPath);
                        products[i].transform.GetChild(1).gameObject.SetActive(false);
                        products[i].transform.GetChild(2).gameObject.SetActive(true);
                        continue;
                    }
                }
            }


            var rankData = ItemDataCommon;
            int ran = rand.Next(1, 11);

            //choose item
            if(ran>9 && ItemDataEpic.Count>0)rankData = ItemDataEpic;
            else if (ran > 6 && ran <= 9 && ItemDataRare.Count > 0) rankData = ItemDataRare;
            else if (ran >= 1 && ran <= 6 && ItemDataCommon.Count > 0) rankData = ItemDataCommon;


            if (ItemDataEpic.Count == 0) { ItemDataEpic.AddRange(ItemDataCommon); }
            if (ItemDataRare.Count == 0) { ItemDataRare.AddRange(ItemDataCommon); }
            if (ItemDataCommon.Count == 0) { ItemDataCommon.AddRange(ItemDataRare); }
            if (ItemDataCommon.Count == 0) { ItemDataCommon.AddRange(ItemDataEpic); }

            if(ItemDataRare.Count==0 && ItemDataEpic.Count==0 && ItemDataCommon.Count==0)
            {
                ERROR_NOITEM();
                break;
            }

            // 뽑히는 아이템 선택.
            int temp = i;
            var targetItem = rankData[0];
            products[temp].GetComponent<Button>().onClick.RemoveAllListeners();
            products[temp].GetComponent<Button>().onClick.AddListener(()=> ShowSelectArtifactInfo(targetItem));
            products[temp].transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = targetItem.Name;
            products[temp].transform.GetChild(2).GetComponent<Image>().sprite = Resources.Load<Sprite>(targetItem.ImageUrl);
            products[temp].transform.GetChild(2).GetComponent<Image>().color = new Color(1, 1, 1, 1);

            //이펙트 - 등급에 따라 다름
            if (targetItem.Rarity == EnumTypes.RarityType.common)
            {
                products[temp].transform.GetChild(1).gameObject.SetActive(false);
            }
            else if(targetItem.Rarity == EnumTypes.RarityType.rare)
            {
                products[temp].transform.GetChild(1).gameObject.SetActive(true);
                products[temp].transform.GetChild(1).GetChild(1).gameObject.SetActive(false);
            }
            else if(targetItem.Rarity == EnumTypes.RarityType.epic)
            {
                products[temp].transform.GetChild(1).gameObject.SetActive(true);
                products[temp].transform.GetChild(1).GetChild(1).gameObject.SetActive(true);
            }
            else if(targetItem.Rarity == EnumTypes.RarityType.legendary)
            {
                products[temp].transform.GetChild(1).gameObject.SetActive(true);
                products[temp].transform.GetChild(1).GetChild(1).gameObject.SetActive(true);
                // 특수 이펙트
            }

            rankData.RemoveAt(0);
        }
    }
    void ERROR_NOITEM()
    {
        Debug.Log("ERROR : NO ITEM");
        for (int i = 0; i < products.Length; i++)
        {
            products[i].SetActive(false);
        }
        string textAll = new LocalizedString("LocalTable", "Notification-AllTreasure").GetLocalizedString();
        scrollImage.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = textAll;
        ADButton.SetActive(false);
        backButton.SetActive(true);
    }
    public void ShowSelectArtifactInfo(ArtifactDTO artifact)
    {
        //SFX
        AudioManager.Instance.ButtonClickSound1();
        PopupManager.Instance.ShowJustArtifactDetail(artifact, isSelect: true);
    }

    public void ShowDotgabiKeyInfo(DotgabiKeyDTO data)
    {
        //SFX
        AudioManager.Instance.ButtonClickSound1();

        var ArtifactDTO = new ArtifactDTO {
            Id = 1000 + data.KeyId,
            Name = data.KeyName,
            Ability = data.KeyDescription,
            FlavorText = data.FlavorText,
            ImageUrl = data.ImgPath,
            Rarity = EnumTypes.RarityType.common,
            ArtifactEffects = null,
            Place = "dotgabi_key",
            IsIcon = false
        };
        PopupManager.Instance.ShowJustArtifactDetail(ArtifactDTO, isSelect: true);
    }

    // 유물방에서 아이템 선택 후
    public async void SelectArtifact(ArtifactDTO artifact)
    {
        for (int i = 0; i < products.Length; i++)
        {
            products[i].GetComponent<Button>().onClick.RemoveAllListeners();
            products[i].transform.GetChild(2).GetComponent<Image>().color = new Color(1, 1, 1, 0.8f);
        }
        backButton.SetActive(true);

        if (artifact.Place == "dotgabi_key")
        {
            await ScenarioArtifactUtils.Instance.GetDotgabiKey(1, SCENARIO_DATA);
            return;
        }
        await ScenarioArtifactUtils.Instance.GetArtifact(artifact, SCENARIO_DATA);
    }

    public void BacktoGame()
    {
        StartCoroutine(BacktoGameCo());
    }
    IEnumerator BacktoGameCo()
    {
        //SFX
        AudioManager.Instance.CloseScrollSound();
        scrollImage.transform.GetComponent<SkeletonAnimation>().AnimationState.SetAnimation(0, "closed_fadeOut", false);
        scrollImage.transform.GetChild(0).gameObject.SetActive(false);
        scrollImage.transform.GetChild(1).gameObject.SetActive(false);
        scrollImage.transform.GetChild(2).gameObject.SetActive(false);
        ADButton.SetActive(false);
        backButton.SetActive(false);
        for (int i=0;i<products.Length;i++)
        {
            products[i].SetActive(false);
        }
        yield return new WaitForSecondsRealtime(1.5f);
        scrollImage.transform.gameObject.SetActive(false);
        treasureCanvas.SetActive(false);
        forwardButton.SetActive(true);
        backImage.SetActive(false);
        yield return null;
    }
    //광고 시청시 유물 다시 돌리기
    public void ReSetItemsAD()
    {
        if (canSeeAdTime > 0)
        {
            //aDMAnager.ShowRewardedAd_Treasure();
        }
        else{ Debug.Log("광고 시청 불가"); }
    }
    public void SetReItemAD()
    {
        canSeeAdTime--;
        string ADSee = new LocalizedString("LocalTable", "Ad-See").GetLocalizedString();
        ADTimeText.text = ADSee + "(" + canSeeAdTime.ToString() + "/2)";
        if (canSeeAdTime == 0){ ADButton.SetActive(false); }

        SetTreasureItems(true);
    }
}
