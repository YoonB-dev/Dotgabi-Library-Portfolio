using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

public class RestManager : SceneSingleton<RestManager>
{
    private ScenarioDTO SCENARIO_DATA;
    [SerializeField] private LocalizedString[] localizedString;
    [Header("Canvas")]
    public int cardShowType=0;
    public GameObject[] buttons; //1 = 업그레이드 2= 삭제 3 = 휴식
    [SerializeField]
    private Sprite[] fireSprites;
    float fireTime = 0.2f;
    private int currentIndex = 0;

    [SerializeField]
    private GameObject hpDetail;
    [SerializeField]
    private GameObject backButton;
    public GameObject infoText;

    [SerializeField]
    private GameObject backBaseImgs;
    public GameObject restCanvas;
    public bool canClick;
    private Coroutine fireCo;
    public void OpenRestManager()
    {
        SCENARIO_DATA = MoveSystem.Instance.SCENARIO_DATA;
        infoText.transform.GetComponent<TextMeshProUGUI>().text = localizedString[0].GetLocalizedString();
        infoText.transform.GetComponent<RectTransform>().DOScale(Vector2.one * 1.1f, 0.5f).SetLoops(-1, LoopType.Yoyo);
        //뒤로가기 버튼 비활성화
        backButton.SetActive(false);
        currentIndex = 0;
        //뒤 배경 끄기
        backBaseImgs.SetActive(false);
        //강화, 삭제, 회복 버튼 세팅
        for (int i = 0; i < buttons.Length; i++)
        {
            int temp = i;
            buttons[temp].GetComponent<Button>().onClick.RemoveAllListeners();
            buttons[temp].GetComponent<Button>().onClick.AddListener(() => SetButton(temp));

            buttons[temp].GetComponent<Image>().color = Color.white;
            buttons[temp].transform.GetChild(0).gameObject.SetActive(true);

            ButtonAnim.Instance.ButtonFadeInScale(buttons[temp],0.3f);

        }
        //회복 버튼 수치 표시
        string text = new LocalizedString("LocalTable", "Heal").GetLocalizedString();
        buttons[2].transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = $"{text}({Mathf.RoundToInt(SCENARIO_DATA.MaxHp * 0.3f)})";
        if(fireCo!=null)StopCoroutine(fireCo);
        fireCo = StartCoroutine(PlayFire());
        canClick = true;
        restCanvas.SetActive(true);
    }
    IEnumerator PlayFire()
    {
        while (true)
        {
            if (currentIndex >= 4) currentIndex = 0;
            buttons[2].GetComponent<Image>().sprite = fireSprites[currentIndex];
            currentIndex++;
            yield return new WaitForSecondsRealtime(fireTime);
        }
    }
    public void SetButton(int num)
    {
        // SFX
        AudioManager.Instance.ButtonClickSound1();
        switch (num)
        {
            case 0:
                //강화
                cardShowType = 1;
                PopupManager.Instance.ShowCardUpgradePopup(action: () => RestButtonSelect(0));
                break;
            case 1:
                //삭제
                cardShowType = 2;
                PopupManager.Instance.ShowCardDeletePopup(true, action: () => RestButtonSelect(1));
                break;
            case 2:
                //휴식
                int amount = Mathf.RoundToInt(SCENARIO_DATA.MaxHp * 0.3f);
                ShowHealDetail(amount);
                break;
        }
    }
    public void ShowHealDetail(int amount)
    {
        //SFX
        AudioManager.Instance.ButtonClickSound1();

        hpDetail.SetActive(true);
        ButtonAnim.Instance.ButtonScaleIn(hpDetail.transform.GetChild(1).gameObject,0f,1f);

        var amountTxt = hpDetail.transform.GetChild(1).GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>();
        var checkButton = hpDetail.transform.GetChild(1).GetChild(4).GetComponent<Button>();

        amountTxt.text =  "+" + amount.ToString();
        checkButton.onClick.RemoveAllListeners();
        checkButton.onClick.AddListener(()=> GetHeal(amount));
    }
    public void CloseHealDetail()
    {
        //SFX
        AudioManager.Instance.ButtonClickSound2();

        hpDetail.SetActive(false);
    }
    public async void GetHeal(int amount)
    {
        await SupabaseGetScenarioCoin.Instance.GetHp(amount, SCENARIO_DATA);
        RestButtonSelect(2);
        //string text = GameData.Text_Log[36]["Text" + GameManager.gameManager.totalGameData.Language].ToString().Replace("^",amount.ToString());
        //SetUseText(text);
        //NotificationManager.Instance.SetShownNotification(text);
        //업적
        SupabaseAchieve.Instance.AchieveCurrData(EnumTypes.AchieveType.rest_count, 1);
        CloseHealDetail();
    }

    void RestButtonSelect(int index)
    {
        for(int i=0;i<buttons.Length;i++)
        {
            if(i!=index)buttons[i].GetComponent<Image>().color = Color.gray;
            buttons[i].transform.GetChild(0).gameObject.SetActive(false);
            buttons[i].GetComponent<Button>().onClick.RemoveAllListeners();
        }

        infoText.transform.GetComponent<TextMeshProUGUI>().text = localizedString[1].GetLocalizedString();
        SetBackButton();
    }
    void SetBackButton()
    {
        backButton.SetActive(true);
    }
    //버튼 연결
    public void GoToMain()
    {
        restCanvas.SetActive(false);
        MoveSystem.Instance.forwardButton.SetActive(true);
        //뒤 배경 키기
        backBaseImgs.SetActive(true);
        //SFX
        AudioManager.Instance.ButtonClickSound2();
    }
}
