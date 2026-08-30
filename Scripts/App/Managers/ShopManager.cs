using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;


class Product
{
    public int price;
    public bool isBuy = false;
    public bool isNone = false;
    public Product(int price, bool isBuy)
    {
        this.price = price;
        this.isBuy = isBuy;
    }
}

public class ShopManager : MonoBehaviour
{
    public GameObject back;
    public GameObject[] buttons_cards;
    public GameObject[] buttons_relics;
    [SerializeField] GameObject UpgradeCardButton;
    [SerializeField] GameObject DeleteCardButton;

    private List<Product> relicsPrice = new();
    private List<Product> cardPrice = new();
    public GameObject info;
    public GameObject shopCanvas;
    //public GameObject forwaredButton;
    [SerializeField]
    private GameObject ShopBackObj;
    int price = 0;

    //할인 진행 유무
    private int salePercent = 0;

    private int priceOfUpgradeAndDelete = 40;

    // target ScenarioData
    private ScenarioDTO SCENARIO_DATA;

    public void ShopStart()
    {
        StartCoroutine(StartCo());

        //강화 삭제 버튼 초기화
        UpgradeCardButton.GetComponent<Button>().onClick.RemoveAllListeners();
        DeleteCardButton.GetComponent<Button>().onClick.RemoveAllListeners();
        UpgradeCardButton.GetComponent<Button>().onClick.RemoveAllListeners();
        UpgradeCardButton.GetComponent<Button>().onClick.AddListener(() => UpgradeCardButtonClick());
        DeleteCardButton.GetComponent<Button>().onClick.RemoveAllListeners();
        DeleteCardButton.GetComponent<Button>().onClick.AddListener(() => DeleteCardButtonClick());
        priceOfUpgradeAndDelete = 40; // 초기화

        // 유물 효과 적용
        ArtifactFunction.Instance.ArtifactEnterShop();
    }

    public IEnumerator StartCo()
    {
        // ScenarioData 세팅
        SCENARIO_DATA = MoveSystem.Instance.SCENARIO_DATA;
        //유물 할인 초기화
        SetArtifactSale();

        relicsPrice.Clear();
        cardPrice.Clear();

        shopCanvas.SetActive(true);
        back.SetActive(true);
        // 유물 세팅
        SetRelics();
        // 카드 세팅
        SetCards();
        SetProductsMoney();//상품 가격 세팅
        ButtonAnim.Instance.ButtonFadeInScale(ShopBackObj, 0.3f, false);
        yield return new WaitForSecondsRealtime(0.6f);
        yield return null;
    }
    public void SetRelics()
    {
        List<ArtifactDTO> ItemData = InGameData.Instance.Artifacts;
        List<ArtifactDTO> ItemDataCommon = new ();
        List<ArtifactDTO> ItemDataRare = new ();
        List<ArtifactDTO> ItemDataHigh = new ();

        //유물 아이템 분류
        for (int i = 0; i < ItemData.Count; i++)
        {
            if (SCENARIO_DATA.OwnedArtifactList.Any(x => x.ArtifactId == ItemData[i].Id))
            {
                continue; // 이미 소유한 유물은 제외
            }

            switch (ItemData[i].Rarity)
            {
                case EnumTypes.RarityType.common:
                    ItemDataCommon.Add(ItemData[i]);
                    break;
                case EnumTypes.RarityType.rare:
                    ItemDataRare.Add(ItemData[i]);
                    break;
                case EnumTypes.RarityType.epic:
                    ItemDataHigh.Add(ItemData[i]);
                    break;
                case EnumTypes.RarityType.legendary:
                    // 전설 등급은 현재 상점에 포함되지 않음
                    break;
            }
        }
        //유물 뽑기
        for (int i = 0; i < buttons_relics.Length; i++)
        {
            System.Random rand = new System.Random(SCENARIO_DATA.MapSeed * (i + 1) + SCENARIO_DATA.StageList.Count * 11);
            //도깨비 키 뽑기
            if (SCENARIO_DATA is UserMainScenarioDTO)
            {
                var data = (UserMainScenarioDTO)SCENARIO_DATA;
                if ((int)data.Difficulty >= 3 && !data.SecondPiece && i == 2)
                {
                    int keyRan = rand.Next(0, 2);
                    if (keyRan == 0)
                    {
                        var key = InGameData.Instance.DotgabiKeys.Find(x => x.KeyId == 2);
                        buttons_relics[i].GetComponent<Image>().sprite = Resources.Load<Sprite>(key.ImgPath);
                        buttons_relics[i].transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = "39";
                        buttons_relics[i].transform.GetChild(1).gameObject.SetActive(false);

                        int temp3 = i;
                        Product productKey = new Product(39, false);
                        relicsPrice.Add(productKey);
                        // 도깨비 키를 아티팩트로 변환
                        var tempKeyArtifact = new ArtifactDTO {
                            Id = 1000 + key.KeyId,
                            Name = key.KeyName,
                            Ability = key.KeyDescription,
                            FlavorText = key.FlavorText,
                            Rarity = EnumTypes.RarityType.common,
                            ImageUrl = key.ImgPath,
                            ArtifactEffects = null,
                            Place = "dotgabi_key",
                            IsIcon = false
                        };

                        // 클릭 정보 -> 상세 보기
                        buttons_relics[i].GetComponent<Button>().onClick.RemoveAllListeners();
                        buttons_relics[i].GetComponent<Button>().onClick.AddListener(() => ShowArtifactInfo(tempKeyArtifact, temp3, true));
                        continue;
                    }
                }
            }

            int ran = rand.Next(1, 11);
            var ItemDataAll = ItemDataCommon;
            int temp = i;
            if (ran >= 1 && ran <= 6) ItemDataAll = ItemDataCommon;
            else if (ran > 6 && ran <= 9 && ItemDataRare.Count > 0) ItemDataAll = ItemDataRare;
            else if (ran >= 10 && ItemDataHigh.Count > 0) ItemDataAll = ItemDataHigh;

            if (ItemDataHigh.Count == 0) { ItemDataHigh.AddRange(ItemDataCommon); }
            if (ItemDataRare.Count == 0) { ItemDataRare.AddRange(ItemDataCommon); }
            if (ItemDataCommon.Count == 0) { ItemDataCommon.AddRange(ItemDataRare); }
            if (ItemDataCommon.Count == 0) { ItemDataCommon.AddRange(ItemDataHigh); }

            if (ItemDataCommon.Count == 0 && ItemDataRare.Count == 0 && ItemDataHigh.Count == 0)
            {
                buttons_relics[temp].GetComponent<Image>().sprite = Resources.Load<Sprite>("Image/Shop/icon_stamp_merchant");
                buttons_relics[temp].transform.GetChild(0).gameObject.SetActive(false);
                Product noProduct = new Product(price, false);
                noProduct.isNone = true;
                relicsPrice.Add(noProduct);
                Debug.Log("유물이 없습니다.");
                continue;
            }

            int num = rand.Next(0, ItemDataAll.Count);

            var targetItem = ItemDataAll[num];
            switch (targetItem.Rarity)
            {
                case EnumTypes.RarityType.common:
                    price = rand.Next(70, 111);
                    break;
                case EnumTypes.RarityType.rare:
                    price = rand.Next(90, 121);
                    break;
                case EnumTypes.RarityType.epic:
                    price = rand.Next(130, 161);
                    break;
            }
            //난이도 상점 가격 상승
            if (SCENARIO_DATA is UserMainScenarioDTO scData && (int)scData.Difficulty >= 5) { price = Mathf.RoundToInt(price * 1.5f); }

            //유물 처음 세팅
            buttons_relics[temp].GetComponent<Image>().color = Color.white;
            buttons_relics[temp].GetComponent<Image>().sprite = Resources.Load<Sprite>(targetItem.ImageUrl);
            buttons_relics[temp].transform.GetChild(0).gameObject.SetActive(true);
            buttons_relics[temp].transform.GetChild(1).gameObject.SetActive(false);

            ItemDataAll.RemoveAt(num);
            buttons_relics[temp].GetComponent<Button>().onClick.RemoveAllListeners();

            int newPrice = CalculatePrice(price);
            Product newProduct = new (newPrice, false);

            relicsPrice.Add(newProduct);
            buttons_relics[temp].GetComponent<Button>().onClick.AddListener(() => ShowArtifactInfo(targetItem, temp));
        }
    }
    //유물 가격 세팅
    public void SetProductsMoney()
    {
        bool isSale = salePercent > 0;
        //유물 돈 계산
        for (int i = 0; i < buttons_relics.Length; i++)
        {
            if (relicsPrice[i].isBuy)
            {
                buttons_relics[i].transform.GetChild(0).gameObject.SetActive(false);
                //이미 구매한 유물은 가격 표시 안함
                continue;
            }

            //유물 - 상점 가격 할인
            string text;
            if (isSale) { text = "<color=green>" + relicsPrice[i].price.ToString() + "</color>"; }
            else { text = "<color=white>" + relicsPrice[i].price.ToString() + "</color>"; }
            if (SCENARIO_DATA.GameCoins < relicsPrice[i].price) { text = "<color=red>" + relicsPrice[i].price.ToString() + "</color>"; }
            buttons_relics[i].transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = text;
        }
        //카드 돈 계산
        for (int i = 0; i < buttons_cards.Length; i++)
        {
            if (cardPrice[i].isBuy)
            {
                //이미 구매한 카드는 가격 표시 안함
                buttons_cards[i].transform.GetChild(6).gameObject.SetActive(false);
                continue;
            }

            //유물 - 상점 가격 할인
            string text;
            if (isSale) { text = "<color=green>" + cardPrice[i].price + "</color>"; }
            else { text = "<color=white>" + cardPrice[i].price + "</color>"; }
            //돈이 부족하면 빨간색 표시
            if (SCENARIO_DATA.GameCoins < cardPrice[i].price) { text = "<color=red>" + cardPrice[i].price + "</color>"; }
            buttons_cards[i].transform.GetChild(6).GetChild(0).GetComponent<TextMeshProUGUI>().text = text;
        }

        //강화 삭제 돈 계산
        if (SCENARIO_DATA.GameCoins < priceOfUpgradeAndDelete)
        {
            UpgradeCardButton.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "<color=red>" + priceOfUpgradeAndDelete + "</color>";
            DeleteCardButton.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "<color=red>" + priceOfUpgradeAndDelete + "</color>";
            UpgradeCardButton.GetComponent<Button>().interactable = false;
            DeleteCardButton.GetComponent<Button>().interactable = false;
        }
        else
        {
            UpgradeCardButton.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "<color=white>" + priceOfUpgradeAndDelete + "</color>";
            DeleteCardButton.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "<color=white>" + priceOfUpgradeAndDelete + "</color>";
            UpgradeCardButton.GetComponent<Button>().interactable = true;
            DeleteCardButton.GetComponent<Button>().interactable = true;
        }

        SetFooterText.Instance.SetAllText();
    }
    private int CalculatePrice(int price)
    {
        int newPrice = Mathf.RoundToInt(price * (1f - salePercent / 100f));
        return newPrice;
    }

    public void SetCards()
    {
        for (int i = 0; i < buttons_cards.Length; i++)
        {
            buttons_cards[i].transform.GetChild(6).gameObject.SetActive(true);
            System.Random rand = new System.Random(SCENARIO_DATA.MapSeed * (i + 2) + SCENARIO_DATA.SelectList.Count * i);
            int temp = i;
            int index = 0;
            int ran = rand.Next(0, 2);
            if (ran == 0)
            {
                //공용 카드
                //index = random.Next(0,50); //-> 이게 맞음 테스트용으로 제한 걸어둔 상태
                index = rand.Next(0, 25);
                price = rand.Next(50, 61);
            }
            else if (ran == 1)
            {
                //직업 카드

                var cards = InGameData.Instance.Cards.FindAll(x => x.CardJob.Contains(SCENARIO_DATA.JobId)
                && x.Id != 57 && x.Id != 58);

                index = cards[rand.Next(0, cards.Count)].Id;
                price = rand.Next(60, 71);
            }

            //난이도 상점 가격 상승
            if (SCENARIO_DATA is UserMainScenarioDTO data && (int)data.Difficulty >= 5) { price = Mathf.RoundToInt(price * 1.5f); }

            //카드 업그레이드
            int upCard = 0;
            int upCardRan = rand.Next(0, 10);
            if (upCardRan == 0)
            {
                upCard = 2;
            }
            else if (upCardRan <= 2)
            {
                upCard = 1;
            }



            CardDTO cardTemp = InGameData.Instance.Cards.Find(x => x.Id == index);

            if (upCard == 1)
            {
                price = Mathf.RoundToInt(price * 1.5f);
            }
            if (upCard == 2)
            {
                price = Mathf.RoundToInt(price * 2f);
            }

            //카드 정보 나타내기
            CardDTOToObj.DTOToObj(buttons_cards[temp], cardTemp);
            //가격 측정
            int newPrice = CalculatePrice(price);
            Product newProduct = new Product(newPrice, false);
            cardPrice.Add(newProduct);
            //버튼 클릭 정보 넘겨주기
            buttons_cards[temp].GetComponent<Button>().onClick.RemoveAllListeners();
            buttons_cards[temp].GetComponent<Button>().onClick.AddListener(() => ShowCardInfo(cardTemp, temp));

            buttons_cards[temp].transform.GetChild(0).GetComponent<Image>().color = Color.white;
            buttons_cards[temp].transform.GetChild(1).GetComponent<Image>().color = Color.white;
            buttons_cards[temp].transform.GetChild(3).GetComponent<TextMeshProUGUI>().color = Color.white;
            buttons_cards[temp].transform.GetChild(7).gameObject.SetActive(false);
        }
    }
    void ShowCardInfo(CardDTO cardData, int buttonIndex)
    {
        int cost = cardPrice[buttonIndex].price;
        //SFX
        AudioManager.Instance.ButtonClickSound1();

        info.SetActive(true);
        //카드 show 활성화, 아이템 show 비활성화
        info.transform.GetChild(0).GetChild(0).gameObject.SetActive(true); info.transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
        var pricePos = info.transform.GetChild(0).GetChild(0).GetChild(1).GetChild(1);
        //가격 색상 변경
        if (SCENARIO_DATA.GameCoins < cost)
            pricePos.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = "<color=red>" + cost.ToString() + "</color>";
        else
            pricePos.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = "<color=white>" + cost.ToString() + "</color>";

        var cardPos = info.transform.GetChild(0).GetChild(0).GetChild(1).GetChild(0);
        ButtonAnim.Instance.ButtonFadeInScale(info.transform.GetChild(0).GetChild(0).GetChild(1).gameObject);

        //카드 정보 세팅
        CardDTOToObj.DTOToObj(cardPos.gameObject, cardData);

        //버튼 클릭 세팅
        pricePos.GetComponent<Button>().onClick.RemoveAllListeners();
        pricePos.GetComponent<Button>().onClick.AddListener(() => BuyCard(cardData, cost, buttonIndex));
    }
    public void ShowArtifactInfo(ArtifactDTO artifact, int temp, bool isKey = false)
    {
        int cost = relicsPrice[temp].price;
        //SFX
        AudioManager.Instance.ButtonClickSound1();
        info.SetActive(true);
        var targetItem = info.transform.GetChild(0).GetChild(1).GetChild(1);
        info.transform.GetChild(0).GetChild(1).gameObject.SetActive(true); info.transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
        info.transform.GetChild(0).GetChild(1).GetChild(0).gameObject.SetActive(true);

        targetItem.transform.GetChild(0).gameObject.SetActive(true);
        ButtonAnim.Instance.ButtonScaleIn(targetItem.gameObject, 0f, 1f);

        if (SCENARIO_DATA.GameCoins < cost)
            targetItem.GetChild(1).GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = "<color=red>" + cost.ToString() + "</color>";
        else
            targetItem.GetChild(1).GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = "<color=white>" + cost.ToString() + "</color>";

        Transform itemPos = targetItem.GetChild(0);
        itemPos.gameObject.SetActive(true);
        if (!isKey)
        {
            ArtifactDTOToObj.Instance.DTOToDetailObj(itemPos.gameObject, artifact);
            //버튼 클릭 세팅
            targetItem.GetChild(1).GetComponent<Button>().onClick.RemoveAllListeners();
            targetItem.GetChild(1).GetComponent<Button>().onClick.AddListener(() => BuyArtifact(artifact, cost, temp));
        }
        else
        {
            //도깨비 키
            ArtifactDTOToObj.Instance.DTOToDetailObj(itemPos.gameObject, artifact);

            targetItem.GetChild(1).GetComponent<Button>().onClick.RemoveAllListeners();
            targetItem.GetChild(1).GetComponent<Button>().onClick.AddListener(() => BuyArtifact(artifact, cost, temp, true));
        }
    }
    public void ShowInfoBack()
    {
        //SFX
        AudioManager.Instance.ButtonClickSound2();
        info.SetActive(false);
        //카드
        info.transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
        //아이템
        info.transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
    }
    async void BuyCard(CardDTO cardData, int price, int temp)
    {
        if (SCENARIO_DATA.GameCoins >= price)
        {
            info.SetActive(false);
            buttons_cards[temp].GetComponent<Button>().onClick.RemoveAllListeners();

            //버튼 구매후 색생 및 구매완료 이미지 생성
            buttons_cards[temp].transform.GetChild(0).GetComponent<Image>().color = Color.gray;
            buttons_cards[temp].transform.GetChild(1).GetComponent<Image>().color = Color.gray;
            buttons_cards[temp].transform.GetChild(3).GetComponent<TextMeshProUGUI>().color = Color.gray;
            buttons_cards[temp].transform.GetChild(7).gameObject.SetActive(true);

            //카드 구매 데이터 세팅
            bool success = await SupabaseCard.Instance.GetCard(SCENARIO_DATA, cardData);
            if (!success) return;
            await GetCoin(-price);
            SetFooterText.Instance.SetAllText();

            //카드 구매 로그
            var logData = new UserScenarioLogDTO {
                CardId = cardData.Id,
            };
            LogManager.Instance.SetLogMainScene(EnumTypes.LogActionType.shop_buy, logData, SCENARIO_DATA);
            //구매 업적
            SupabaseAchieve.Instance.AchieveCurrData(EnumTypes.AchieveType.shop_purchase_count, 1);
            //구매후 돈 세팅
            cardPrice[temp].isBuy = true;
            SetProductsMoney();
        }
        else
        {
            string textMoney = new LocalizedString("LocalTable", "Money-Less").GetLocalizedString();
            NotificationManager.Instance.SetShownNotification(textMoney);
        }
    }
    public async void BuyArtifact(ArtifactDTO artifact, int price, int temp, bool isKey = false)
    {
        if (SCENARIO_DATA.GameCoins < price)
        {
            string textMoney = new LocalizedString("LocalTable", "Money-Less").GetLocalizedString();
            NotificationManager.Instance.SetShownNotification(textMoney);
            return;
        }

        info.SetActive(false);
        buttons_relics[temp].GetComponent<Button>().onClick.RemoveAllListeners();

        if (!isKey)
        {
            bool success = await ScenarioArtifactUtils.Instance.GetArtifact(artifact, MoveSystem.Instance.SCENARIO_DATA, isLog: false);
            if (!success) return;
        }
        else
        {
            //도깨비 키 구매
            bool success = await ScenarioArtifactUtils.Instance.GetDotgabiKey(artifact.Id - 1000, MoveSystem.Instance.SCENARIO_DATA);
            if (!success) return;
        }

        await GetCoin(-price);

        //유물 구매시 구매 완료 이미지 생성
        buttons_relics[temp].GetComponent<Image>().color = Color.gray;
        buttons_relics[temp].transform.GetChild(1).gameObject.SetActive(true);
        //업적
        SupabaseAchieve.Instance.AchieveCurrData(EnumTypes.AchieveType.shop_purchase_count, 1);
        //구매후 돈 세팅
        relicsPrice[temp].isBuy = true;
        SetProductsMoney();

        //유물 구매 로그
        if (!isKey)
        {
            var logData = new UserScenarioLogDTO {
                ArtifactId = artifact.Id,
            };
            LogManager.Instance.SetLogMainScene(EnumTypes.LogActionType.shop_buy, logData, SCENARIO_DATA);
        }
    }

    private void UpgradeCardButtonClick()
    {
        if (SCENARIO_DATA.GameCoins < priceOfUpgradeAndDelete)
        {
            // 돈이 부족할 때
            string textMoney = new LocalizedString("LocalTable", "Money-Less").GetLocalizedString();
            NotificationManager.Instance.SetShownNotification(textMoney);
            //SFX
            AudioManager.Instance.GetShieldSound();
            return;
        }
        else
        {
            // 돈이 충분할 때
            PopupManager.Instance.ShowCardUpgradePopup(action: async () => {
                await GetCoin(-priceOfUpgradeAndDelete);
                priceOfUpgradeAndDelete += 20; // 다음 업그레이드 및 삭제 가격 증가
                SetProductsMoney();
            });
        }
    }
    private void DeleteCardButtonClick()
    {
        if (SCENARIO_DATA.GameCoins < priceOfUpgradeAndDelete)
        {
            string textMoney = new LocalizedString("LocalTable", "Money-Less").GetLocalizedString();
            NotificationManager.Instance.SetShownNotification(textMoney);
            //SFX
            AudioManager.Instance.GetShieldSound();
            return;
        }
        else
        {
            PopupManager.Instance.ShowCardDeletePopup(true, action: async () => {
                await GetCoin(-priceOfUpgradeAndDelete);
                priceOfUpgradeAndDelete += 20; // 다음 업그레이드 및 삭제 가격 증가
                SetProductsMoney();
            });
        }
    }

    //상점 뒤로가기 버튼에 연결 되어있음.
    public void BacktoGame()
    {
        back.SetActive(false);
        shopCanvas.SetActive(false);
        MoveSystem.Instance.SetForwardButtonActive(true);
        //SFX
        AudioManager.Instance.ButtonClickSound2();
        //SFX BGM
        AudioManager.Instance.StartinGameBGM();
        //로그
        UserScenarioLogDTO logData = new();
        LogManager.Instance.SetLogMainScene(EnumTypes.LogActionType.shop_exit, logData, SCENARIO_DATA);
    }

    private async Task GetCoin(int amount)
    {
        await SupabaseGetScenarioCoin.Instance.GetCoin(amount, SCENARIO_DATA);
        SetFooterText.Instance.SetMoveText(amount, EnumTypes.MoveTextType.money);
    }

    private void SetArtifactSale()
    {
        var artifactSale = ArtifactFunction.Instance.ArtifactShopDiscount();
        if (artifactSale != null && artifactSale.SalePercent > 0)
        {
            salePercent = artifactSale.SalePercent;
        }
    }
}
