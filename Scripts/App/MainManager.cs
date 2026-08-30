using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Spine.Unity;
using System.IO;
using UnityEngine.Localization.Components;
using UnityEngine.Localization;
using System.Threading.Tasks;


public class MainManager : SceneSingleton<MainManager>
{
    public CamBox cambox;

    [Header("Canvas")]
    public Canvas MainCanvas;
    [SerializeField] private Canvas WebtoonCanvas;
    [SerializeField] private GameObject recordButton;
    public GameObject[] Headers;
    [Header("coins text")]
    [SerializeField] private TextMeshProUGUI AchievePointText;
    [SerializeField] private TextMeshProUGUI AdPointText;
    [Header("Achieve")]
    [SerializeField] private GameObject RightHeightElements;
    [SerializeField] private GameObject LoadingCanvas;
    [SerializeField] private GameObject moveTextPrefab;
    [SerializeField] private GameObject moveTextGroup;
    public void Start()
    {
        AudioManager.Instance.StartMainBGM();
        string enterTxt = LogManager.Instance.GetMainLogText("enter_main");
        LogManager.Instance.AddLogMain(enterTxt);
        SetLoadingCanvas(false);

        GetClearData();
    }

    private async void GetClearData()
    {
        UserData.Instance.UserMainClearRecordList = await UserMainClearRecordDAO.Instance.GetUserMainClearRecord(UserData.Instance.UserAuthId);
    }

    IEnumerator ShowStartWebtoonCo()
    {
        WebtoonCanvas.gameObject.SetActive(true);
        for (int i = 2; i < WebtoonCanvas.transform.childCount; i++)
        {
            WebtoonCanvas.transform.GetChild(i).gameObject.SetActive(false);
        }
        ButtonAnim.Instance.ButtonFadeOut(WebtoonCanvas.transform.GetChild(1).gameObject, 1.5f);


        WebtoonCanvas.transform.GetChild(2).gameObject.SetActive(true);
        WebtoonCanvas.transform.GetChild(2).GetChild(0).GetComponent<SkeletonAnimation>().AnimationState.SetAnimation(0, "play", false);
        WebtoonCanvas.transform.GetChild(2).GetChild(0).GetComponent<SkeletonAnimation>().timeScale = 0;

        WebtoonCanvas.transform.GetChild(3).gameObject.SetActive(false);
        WebtoonCanvas.transform.GetChild(2).GetChild(1).gameObject.SetActive(false);

        yield return new WaitForSecondsRealtime(1f);

        WebtoonCanvas.transform.GetChild(2).GetChild(0).GetComponent<SkeletonAnimation>().timeScale = 1;
        yield return new WaitForSecondsRealtime(4f);
        WebtoonCanvas.transform.GetChild(2).GetChild(1).gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(9f);

        WebtoonCanvas.transform.GetChild(3).gameObject.SetActive(true);
        WebtoonCanvas.transform.GetChild(2).GetChild(1).gameObject.SetActive(false);
        ButtonAnim.Instance.ButtonScaleIn(WebtoonCanvas.transform.GetChild(3).gameObject, 0f, 1f);
        yield return null;
    }

    public void SetTextAll()
    {
        AchievePointText.text = UserData.Instance.AchievePoint.ToString();
        AdPointText.text = UserData.Instance.AdPoint.ToString();
    }

    public void GetPoint(string type, int amount)
    {
        // SFX
        AudioManager.Instance.MoneySound();

        if (type == "Ad")
        {
            UserData.Instance.AdPoint += amount;
            AdPointText.text = UserData.Instance.AdPoint.ToString();
            SetFooterText.Instance.SetMoveTextObj(amount, EnumTypes.MoveTextType.money, AdPointText.transform);
        }
        else if (type == "Achieve")
        {
            UserData.Instance.AchievePoint += amount;
            AchievePointText.text = UserData.Instance.AchievePoint.ToString();
            SetFooterText.Instance.SetMoveTextObj(amount, EnumTypes.MoveTextType.money, AchievePointText.transform);
        }

        SetTextAll();
    }
    public void SetAchieveButton(bool isActive)
    {
        RightHeightElements.SetActive(isActive);
        recordButton.SetActive(isActive);
    }

    public void SetLoadingCanvas(bool isActive)
    {
        LoadingCanvas.SetActive(isActive);
    }
}
