using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.SmartFormat;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OnuManager : SceneSingleton<OnuManager>
{
    [Header("Main UI - Out")]
    [SerializeField] private Canvas MainCanvas;
    [SerializeField] private GameObject MainUI;
    [SerializeField] private GameObject LeftButton;
    [SerializeField] private GameObject RightButton;
    public int currIndex = 0; // -1: 나무, 0: 집, 1: 우물
    private List<int> getCardIndex = new List<int>();
    [Header("House In UI")]
    [SerializeField] private Canvas HouseInCanvas;
    // Window
    [SerializeField] private Image WindowImage;
    [SerializeField] private Sprite[] WindowSprites; // 0: 닫힘, 1: 열림
    private bool isWindowOpen = false;
    // Drawer
    [SerializeField] private Image DrawerImage;
    [SerializeField] private Sprite[] DrawerSprites; // 0: 닫힘, 1: 열림, 2: 도끼
    private bool isDrawerOpen = false;
    private bool isGetAxe = false;
    // Box
    [SerializeField] private Image BoxImage;
    [SerializeField] private Sprite[] BoxSprites; // 0: 닫힘, 1: 열림
    private bool isBoxOpen = false;
    // Cloth
    private bool isClothOn = false;
    // Bedding
    [SerializeField] private Image BeddingImage;
    [SerializeField] private Sprite[] BeddingSprites; // 0: 기본, 1: 움직임
    private bool isBeddingOn = false;

    // CobWeb
    [SerializeField] private GameObject CobWebObj;

    // Bucket
    [SerializeField] private GameObject BucketObj;


    [Header("WellDetail UI")]
    [SerializeField] private Canvas WellDetailCanvas;
    public bool isWellEmpty = false; // 우물에 물이 있는지 여부
    public bool isItemGet = false; // 우물에서 아이템을 얻었는지 여부
    [SerializeField] private Sprite WellWaterUpSprite;
    [SerializeField] private GameObject WellWaterObj;

    [SerializeField] private Sprite WellWaterDetailUpSprite;
    [SerializeField] private GameObject WellWaterDetailObj;
    [SerializeField] private Button WellItemButton;

    [Header("Tree")]
    [SerializeField] private GameObject TreeObj;
    private bool isTreeAxeUsed = false; // 나무 도끼 사용 여부
    [SerializeField] private Sprite TreeAxeSprite;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI ActiveText;
    private Coroutine textCo = null;

    [Header("GetCanvas")]
    [SerializeField] private Canvas GetCanvas;
    [SerializeField] private Image GetImage;

    private List<int> obtainedArtifactIds = new List<int>();
    void Start()
    {
        SetText("show_house");
    }

    public void MoveButtonClick(bool isLeft)
    {
        //SFX
        AudioManager.Instance.ButtonClickSound1();

        Debug.Log("MoveButtonClick: " + (isLeft ? "Left" : "Right"));
        if (isLeft)
        {
            if (currIndex > -1)
            {
                currIndex--;
                UpdateUI();
            }
        }
        else
        {
            if (currIndex < 1)
            {
                currIndex++;
                UpdateUI();
            }
        }
    }

    private void UpdateUI()
    {
        LeftButton.SetActive(false);
        RightButton.SetActive(false);
        if (currIndex == -1)
        {
            MainUI.transform.DOLocalMove(new Vector3(1500 * ButtonAnim.Instance.ratio, 0, 0), 0.5f).onComplete = () => {
                RightButton.SetActive(true);
            };

            SetText("show_tree");
        }
        else if (currIndex == 0)
        {
            MainUI.transform.DOLocalMove(new Vector3(0, 0, 0), 0.5f).onComplete = () => {
                LeftButton.SetActive(true);
                RightButton.SetActive(true);
            };

            SetText("show_house");
        }
        else if (currIndex == 1)
        {
            MainUI.transform.DOLocalMove(new Vector3(-1500 * ButtonAnim.Instance.ratio, 0, 0), 0.5f).onComplete = () => {
                LeftButton.SetActive(true);
            };

            SetText("show_well");
        }
    }

    public void EnterHouse()
    {
        //SFX
        AudioManager.Instance.ButtonClickSound1();

        Debug.Log("EnterHouse");
        MainCanvas.gameObject.SetActive(false);
        HouseInCanvas.gameObject.SetActive(true);
        WellDetailCanvas.gameObject.SetActive(false);

        SetText("enter_house");
    }

    public void ExitHouse()
    {
        //SFX
        AudioManager.Instance.ButtonClickSound2();

        Debug.Log("ExitHouse");
        HouseInCanvas.gameObject.SetActive(false);
        MainCanvas.gameObject.SetActive(true);
        WellDetailCanvas.gameObject.SetActive(false);
    }

    public void EnterWellDetail()
    {
        //SFX
        AudioManager.Instance.ButtonClickSound1();

        MainCanvas.gameObject.SetActive(false);
        HouseInCanvas.gameObject.SetActive(false);
        WellDetailCanvas.gameObject.SetActive(true);

        SetText("show_well_detail");
    }

    public void ExitWellDetail()
    {
        //SFX
        AudioManager.Instance.ButtonClickSound2();

        MainCanvas.gameObject.SetActive(true);
        HouseInCanvas.gameObject.SetActive(false);
        WellDetailCanvas.gameObject.SetActive(false);
    }

    public void ClickTree()
    {
        //SFX
        AudioManager.Instance.ButtonClickSound1();

        if (isTreeAxeUsed)
        {
            SetText("click_tree2");
        }
        else
        {
            SetText("click_tree");
        }

    }

    private void SetText(string text_key)
    {
        Debug.Log("SetText: " + text_key);
        string text = new LocalizedString { TableReference = "StoryTable", TableEntryReference = text_key }.GetLocalizedString();
        if (textCo != null)
        {
            StopCoroutine(textCo);
        }
        textCo = StartCoroutine(TextTypingUtils.PlayTypewriterEffect(ActiveText, text));
    }


    // ---------- 내부 ----------
    public void ClickWindow()
    {
        //SFX
        AudioManager.Instance.ButtonClickSound1();

        isWindowOpen = !isWindowOpen;
        if (isWindowOpen && WindowSprites.Length > 1)
        {
            WindowImage.sprite = WindowSprites[1];
        }
        else
        {
            WindowImage.sprite = WindowSprites[0];
        }
    }

    public void ClickDrawer()
    {
        //SFX
        AudioManager.Instance.GetItemSound();

        isDrawerOpen = !isDrawerOpen;
        if (isDrawerOpen && DrawerSprites.Length > 1)
        {
            if (!isGetAxe && DrawerSprites.Length > 2)
            {
                DrawerImage.sprite = DrawerSprites[2];
            }
            else
            {
                DrawerImage.sprite = DrawerSprites[1];
            }
        }
        else
        {
            if (!isGetAxe)
            {
                isGetAxe = true;
                var itemDTO = InGameData.Instance.MainStoryItems.Find(x => x.Id == 2); // 도끼
                StartCoroutine(ShowGetCanvas(itemDTO));
                DrawerImage.sprite = DrawerSprites[1];
                isDrawerOpen = true;
            }
            else
            {
                DrawerImage.sprite = DrawerSprites[0];
            }
        }
    }

    public void ClickBox()
    {
        //SFX
        AudioManager.Instance.GetItemSound();

        isBoxOpen = !isBoxOpen;
        if (isBoxOpen && BoxSprites.Length > 1)
        {
            BoxImage.sprite = BoxSprites[1];
        }
        else
        {
            BoxImage.sprite = BoxSprites[0];
        }
    }

    public void ClickCloth()
    {
        //SFX
        AudioManager.Instance.GetItemSound();

        if (!isClothOn)
        {
            isClothOn = true;
            var itemDTO = InGameData.Instance.MainStoryItems.Find(x => x.Id == 5); // 편지
            StartCoroutine(ShowGetCanvas(itemDTO));
        }
    }

    public void ClickBedding()
    {
        //SFX
        AudioManager.Instance.GetItemSound();

        isBeddingOn = !isBeddingOn;
        if (isBeddingOn && BeddingSprites.Length > 1)
        {
            BeddingImage.sprite = BeddingSprites[1];
        }
        else
        {
            BeddingImage.sprite = BeddingSprites[0];
        }
    }

    public void ClickWeb()
    {
        var itemDTO = InGameData.Instance.MainStoryItems.Find(x => x.Id == 4); // 거미줄
        StartCoroutine(ShowGetCanvas(itemDTO));
        CobWebObj.SetActive(false);
    }

    public void ClickBucket()
    {
        var itemDTO = InGameData.Instance.MainStoryItems.Find(x => x.Id == 3); // 양동이
        StartCoroutine(ShowGetCanvas(itemDTO));
        BucketObj.SetActive(false);
    }

    private IEnumerator ShowGetCanvas(MainStoryItemDTO itemDTO)
    {
        //SFX
        AudioManager.Instance.GetItemSound();

        GetCanvas.gameObject.SetActive(true);
        ButtonAnim.Instance.ButtonScaleIn(GetImage.gameObject, 0.2f, 1f, 0.3f);
        GetImage.sprite = Resources.Load<Sprite>(itemDTO.ImgPath);
        yield return new WaitForSeconds(1.5f);
        GetCanvas.gameObject.SetActive(false);

        var cardDTO = StoryCardManager.Instance?.AddCard(itemDTO);
        if (cardDTO != null)
        {
            getCardIndex.Add(itemDTO.Id);
        }
    }

    public void ClickButtonBack()
    {
        //SFX
        AudioManager.Instance.ButtonClickSound1();

        // 비동기 메서드 별도로 호출 (void라서 예외 신경써야 함)
        _ = ClickButtonBackAsync();
    }

    private async Task ClickButtonBackAsync()
    {
        // 스토리 카드 저장
        SupabaseMainScenarioStoryUpdate.Instance.InsertMainScenarioStoryOwnedCard(getCardIndex);
        // 클리어 데이터 저장
        SupabaseMainScenarioStoryUpdate.Instance.UpdateMainScenarioStoryClearData(EnumTypes.MainStoryType.onu_house_clear, true);
        Debug.Log("OnuManager: ClickButtonBackAsync - 스토리 카드 및 클리어 데이터 저장 완료");
        // 얻은 유물 저장
        await GetArtifactEnd();
        Debug.Log("OnuManager: ClickButtonBackAsync - 유물 저장 완료");
        SceneManager.LoadScene("GameScene");
    }

    private async Task GetArtifactEnd()
    {
        // 종료 후 유물 얻기
        Debug.Log(InGameData.Instance.Artifacts.Count);
        Debug.Log(UserData.Instance.MainScenarioData);
        foreach (var artifactId in obtainedArtifactIds)
        {
            var item = InGameData.Instance.Artifacts.Find(a => a.Id == artifactId);
            if (item != null)
            {
                await SupabaseArtifact.Instance.GetArtifact(artifactId, UserData.Instance.MainScenarioData);
            }
        }
    }


    // 나무 도끼 사용
    public void SetTreeAxe()
    {
        //SFX
        AudioManager.Instance.GetItemSound();

        TreeObj.GetComponent<Button>().onClick.RemoveAllListeners();
        isTreeAxeUsed = true;

        obtainedArtifactIds.Add(19); // 기름 얻기
        var artifact = InGameData.Instance.Artifacts.Find(x => x.Id == 19);
        string artifactText = LogManager.Instance.GetDBLogText(EnumTypes.LogActionType.player_get_something).FormatSmart($"<color=green>{artifact.Name}</color>");
        NotificationManager.Instance.SetShownNotification(artifactText);

        SetText("click_tree2");

        // image change
        TreeObj.GetComponent<Image>().sprite = TreeAxeSprite;
    }

    // 우물 물 길기
    public void SetWellEmpty()
    {
        //SFX
        AudioManager.Instance.MissSound();

        isWellEmpty = true;
        SetText("click_well2");

        // image change
        WellWaterObj.GetComponent<Image>().sprite = WellWaterUpSprite;
        WellWaterDetailObj.GetComponent<Image>().sprite = WellWaterDetailUpSprite;

        WellItemButton.interactable = true;
        WellItemButton.onClick.RemoveAllListeners();
        WellItemButton.onClick.AddListener(SetWellItemGet);
    }

    // 우물 아이템 얻기
    public void SetWellItemGet()
    {
        //SFX
        AudioManager.Instance.GetItemSound();

        if (!isItemGet && isWellEmpty)
        {
            isItemGet = true;
            SetText("click_item_well");
            obtainedArtifactIds.Add(40); // 일단 임시 유물 얻음.

            var artifact = InGameData.Instance.Artifacts.Find(x => x.Id == 40);
            string artifactText = LogManager.Instance.GetDBLogText(EnumTypes.LogActionType.player_get_something).FormatSmart($"<color=green>{artifact.Name}</color>");
            NotificationManager.Instance.SetShownNotification(artifactText);

            WellItemButton.gameObject.SetActive(false);
        }
    }
}
