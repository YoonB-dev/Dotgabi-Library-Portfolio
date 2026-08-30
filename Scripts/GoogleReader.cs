using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class GoogleReader : MonoBehaviour
{
    public class SheetData
    {
        public readonly string ADDRESS;
        public readonly string Range;
        public readonly long SHEET_ID_PEN;
        public SheetData(string address, string range, long ID)
        {
            this.ADDRESS = address;
            this.Range = range;
            this.SHEET_ID_PEN = ID;
        }
    }
    readonly SheetData publicCardPath = new("https://docs.google.com/spreadsheets/d/1ZyqX2TvNLPGfCYuND9uda4neTtvDdSAWDIvr3MDWWTE", "A1:K", 0);
    readonly SheetData blacksmithCardPath = new("https://docs.google.com/spreadsheets/d/1ZyqX2TvNLPGfCYuND9uda4neTtvDdSAWDIvr3MDWWTE", "A1:K", 1901089729);
    readonly SheetData dosaCardPath = new("https://docs.google.com/spreadsheets/d/1ZyqX2TvNLPGfCYuND9uda4neTtvDdSAWDIvr3MDWWTE", "A1:K", 1935081013);
    readonly SheetData enemyPath = new("https://docs.google.com/spreadsheets/d/1PCrwfZJDZ0k0fG0R9T4Ujn34oiB_Kk1ygpQolZv3QN0", "A1:S", 0);
    readonly SheetData itemPath = new("https://docs.google.com/spreadsheets/d/1Tjs8UFgEePytTlb24_Pbyck4jQ4HNy0GSxVDJmxzsQc", "A1:J", 0);
    readonly SheetData eventPath = new("https://docs.google.com/spreadsheets/d/1asKyPsdMXx7_i-0luLhAVmHgaIpt-V1Ijt-7YIqz3pU", "A1:O", 0);
    readonly SheetData buffPath = new("https://docs.google.com/spreadsheets/d/1CMrGv0rMmQHLvl8hl8FJS1ZdZItjTpjQhaGpe6O_1W8", "A1:J", 0);
    readonly SheetData chfPath = new("https://docs.google.com/spreadsheets/d/1mBexKwQTWzIUg1TcoJfPRrOePs19wtJyzEtw7SRXves", "A1:I", 0);
    readonly SheetData eventSmallPath = new("https://docs.google.com/spreadsheets/d/1asKyPsdMXx7_i-0luLhAVmHgaIpt-V1Ijt-7YIqz3pU", "A1:C", 1137996530);
    readonly SheetData storyPath = new("https://docs.google.com/spreadsheets/d/1cZ5-PT0pOpinDAqdQpXekzFbBhmBT5rkDPoewxLyR0c", "A1:F", 0);
    readonly SheetData enemyText = new("https://docs.google.com/spreadsheets/d/1PCrwfZJDZ0k0fG0R9T4Ujn34oiB_Kk1ygpQolZv3QN0", "A1:I", 1680176973);
    readonly SheetData performerCardPath = new("https://docs.google.com/spreadsheets/d/1ZyqX2TvNLPGfCYuND9uda4neTtvDdSAWDIvr3MDWWTE", "A1:K", 788293903);
    readonly SheetData achievePath = new("https://docs.google.com/spreadsheets/d/1B-VfN7_SftLQIeiib28q09XGIGVih4jDdvUF_TjB0OU", "A1:F", 0);
    readonly SheetData enemyText2 = new("https://docs.google.com/spreadsheets/d/1PCrwfZJDZ0k0fG0R9T4Ujn34oiB_Kk1ygpQolZv3QN0", "A1:I", 137837996);
    readonly SheetData frameList = new("https://docs.google.com/spreadsheets/d/1S3epw5ZAAg4HheapIwFwINNUa_gfGjVgZj9lHgNtIgU", "A1:K", 0);
    readonly SheetData textLog = new("https://docs.google.com/spreadsheets/d/1XcARZQmmLJOAg1vz3MsXbzN78B8OWMG3c97u7-qEVSY", "A1:E", 0);
    readonly SheetData keyList = new("https://docs.google.com/spreadsheets/d/1Tjs8UFgEePytTlb24_Pbyck4jQ4HNy0GSxVDJmxzsQc","A1:H",84640161);
    readonly SheetData performData = new("https://docs.google.com/spreadsheets/d/1mBexKwQTWzIUg1TcoJfPRrOePs19wtJyzEtw7SRXves", "A1:D", 260476133);
    readonly SheetData summonData = new("https://docs.google.com/spreadsheets/d/1mBexKwQTWzIUg1TcoJfPRrOePs19wtJyzEtw7SRXves", "A1:E", 2046688829);
    readonly SheetData promoData = new("https://docs.google.com/spreadsheets/d/1aXMkJkaDPkoGX9M4TJAc9C9L4GgGjJVdYB9J7sLUW7U", "A1:I", 0);
    readonly List<SheetData> DATA_PATH = new();



    [Header("Loading Data")]
    public GameObject loadingObj;
    public Image Title,loadingBar;
    public TextMeshProUGUI loadingText;

    [Header("Start")]
    public GameObject startButton;
    [SerializeField]private GameObject errorText;
    [Header("Canvas")]
    [SerializeField]
    Canvas BlackCanvas;

    private bool[] loadSuccess;
    void Awake()
    {
        StartCoroutine(InitializeLocalizationAndLoadData());
    }

    private IEnumerator InitializeLocalizationAndLoadData()
    {
        // 로컬라이제이션 초기화가 완료될 때까지 대기
        yield return LocalizationSettings.InitializationOperation;

        // 로컬라이제이션 초기화가 완료된 후 데이터 로드 시작
        SetAllData();
        errorText.SetActive(false);
    }

    //----------------------------------------------

    public async void SetAllData()
    {
        DATA_PATH.Add(publicCardPath);
        DATA_PATH.Add(blacksmithCardPath);
        DATA_PATH.Add(dosaCardPath);
        DATA_PATH.Add(enemyPath);
        DATA_PATH.Add(itemPath);
        DATA_PATH.Add(eventPath);
        DATA_PATH.Add(buffPath);
        DATA_PATH.Add(chfPath);
        DATA_PATH.Add(eventSmallPath);
        DATA_PATH.Add(storyPath);
        DATA_PATH.Add(enemyText);
        DATA_PATH.Add(performerCardPath);
        DATA_PATH.Add(achievePath);
        DATA_PATH.Add(enemyText2);
        DATA_PATH.Add(frameList);
        DATA_PATH.Add(textLog);
        DATA_PATH.Add(keyList);
        DATA_PATH.Add(performData);
        DATA_PATH.Add(summonData);
        DATA_PATH.Add(promoData);


        loadSuccess = new bool[DATA_PATH.Count];

        await LoadDataAsync();
    }
    private async Task LoadDataAsync()
    {
        Debug.Log("****LOADING****");
        startButton.SetActive(false);
        loadingBar.fillAmount = 0;
        SetTitle();
        StartCoroutine(SetLoadingText(0));

        List<Task> tasks = new List<Task>();
        for (int i = 0; i < DATA_PATH.Count; i++)
        {
            int index = i;
            tasks.Add(LoadData(index));
        }

        await Task.WhenAll(tasks);

        if (AllDataLoadedSuccessfully())
        {
            Debug.Log("****COMPLETE****");
            loadingObj.SetActive(false);
            startButton.SetActive(true);
            startButton.transform.GetChild(0).GetComponent<RectTransform>().DOScale(Vector2.one * 1.2f, 0.5f).SetLoops(-1, LoopType.Yoyo);
        }
        else
        {
            Debug.LogError("****ERROR****");
            loadingObj.SetActive(false);
            errorText.SetActive(true);
            errorText.transform.GetComponent<RectTransform>().DOScale(Vector2.one * 1.1f, 0.6f).SetLoops(-1, LoopType.Yoyo);
        }
    }

    private bool AllDataLoadedSuccessfully()
    {
        foreach (bool success in loadSuccess)
        {
            if (!success)
            {
                return false;
            }
        }
        return true;
    }

    private async Task LoadData(int index)
    {
        // try {
        //     UnityWebRequest www = UnityWebRequest.Get(GetTSVAddress(DATA_PATH[index]));
        //     await www.SendWebRequest();

        //     if (www.isDone)
        //{
        //         string path = www.downloadHandler.text;
        //         loadSuccess[index] = true;
        //         switch (index)
        //{
        //             case 0:
        //                 GameData.CardDataPublic = Read(path);
        //                 Debug.Log(GameData.CardDataPublic.Count);
        //                 if (GameData.CardDataPublic.Count == 0) loadSuccess[index] = false;
        //                 break;
        //             case 1:
        //                 GameData.CardDataBlacksmith = Read(path);
        //                 Debug.Log(GameData.CardDataBlacksmith.Count);
        //                 if (GameData.CardDataBlacksmith.Count == 0) loadSuccess[index] = false;
        //                 break;
        //             case 2:
        //                 GameData.CardDataDosa = Read(path);
        //                 Debug.Log(GameData.CardDataDosa.Count);
        //                 if (GameData.CardDataDosa.Count == 0) loadSuccess[index] = false;
        //                 break;
        //             case 3:
        //                 GameData.EnemyData = Read(path);
        //                 Debug.Log(GameData.EnemyData.Count);
        //                 if (GameData.EnemyData.Count == 0) loadSuccess[index] = false;
        //                 break;
        //             case 4:
        //                 GameData.ItemData = Read(path);
        //                 Debug.Log("이거 왜 안나옴??:" + GameData.ItemData.Count);
        //                 if (GameData.ItemData.Count == 0) loadSuccess[index] = false;
        //                 break;
        //             case 5:
        //                 GameData.EventData = Read(path);
        //                 Debug.Log(GameData.EventData.Count);
        //                 if (GameData.EventData.Count == 0) loadSuccess[index] = false;
        //                 break;
        //             case 6:
        //                 GameData.BuffData = Read(path);
        //                 Debug.Log(GameData.BuffData.Count);
        //                 if (GameData.BuffData.Count == 0) loadSuccess[index] = false;
        //                 break;
        //             case 7:
        //                 GameData.ChData = Read(path);
        //                 Debug.Log(GameData.ChData.Count);
        //                 if (GameData.ChData.Count == 0) loadSuccess[index] = false;
        //                 break;
        //             case 8:
        //                 GameData.EventSmallData = Read(path);
        //                 Debug.Log(GameData.EventSmallData.Count);
        //                 if (GameData.EventSmallData.Count == 0) loadSuccess[index] = false;
        //                 break;
        //             case 9:
        //                 GameData.StoryData = Read(path);
        //                 Debug.Log(GameData.StoryData.Count);
        //                 if (GameData.StoryData.Count == 0) loadSuccess[index] = false;
        //                 break;
        //             case 10:
        //                 GameData.EnemyTextData = Read(path);
        //                 Debug.Log(GameData.EnemyTextData.Count);
        //                 if (GameData.EnemyTextData.Count == 0) loadSuccess[index] = false;
        //                 break;
        //             case 11:
        //                 GameData.CardDataPerformer = Read(path);
        //                 Debug.Log(GameData.CardDataPerformer.Count);
        //                 if (GameData.CardDataPerformer.Count == 0) loadSuccess[index] = false;
        //                 break;
        //             case 12:
        //                 GameData.AchieveData = Read(path);
        //                 Debug.Log(GameData.AchieveData.Count);
        //                 if (GameData.AchieveData.Count == 0) loadSuccess[index] = false;
        //                 break;
        //             case 13:
        //                 GameData.EnemyTextData2 = Read(path);
        //                 Debug.Log(GameData.EnemyTextData2.Count);
        //                 if (GameData.EnemyTextData2.Count == 0) loadSuccess[index] = false;
        //                 break;
        //             case 14:
        //                 GameData.FrameData = Read(path);
        //                 Debug.Log(GameData.FrameData.Count);
        //                 if (GameData.FrameData.Count == 0) loadSuccess[index] = false;
        //                 break;
        //             case 15:
        //                 // GameData.Text_Log = Read(path);
        //                 // Debug.Log(GameData.Text_Log.Count);
        //                 // if (GameData.Text_Log.Count == 0) loadSuccess[index] = false;
        //                 break;
        //             case 16:
        //                 GameData.keyData = Read(path);
        //                 Debug.Log(GameData.keyData.Count);
        //                 if (GameData.keyData.Count == 0) loadSuccess[index] = false;
        //                 break;
        //             case 17:
        //                 GameData.performerData = Read(path);
        //                 Debug.Log(GameData.performerData.Count);
        //                 if (GameData.performerData.Count == 0) loadSuccess[index] = false;
        //                 break;
        //             case 18:
        //                 GameData.summonData = Read(path);
        //                 Debug.Log(GameData.summonData.Count);
        //                 if (GameData.summonData.Count == 0) loadSuccess[index] = false;
        //                 break;
        //             case 19:
        //                 GameData.promoData = Read(path);
        //                 Debug.Log(GameData.promoData.Count);
        //                 if (GameData.promoData.Count == 0) loadSuccess[index] = false;
        //                 break;
        //         }
        //         float targetFillAmount = (index + 1) / (float)DATA_PATH.Count;
        //         loadingBar.DOFillAmount(targetFillAmount, 0.5f);
        //     }
        // }
        // catch (System.Exception e)
        //{
        //     Debug.LogError(e);
        //     loadSuccess[index] = false;
        // }
    }

    public static List<Dictionary<string, object>> Read(string sheet)
    {
        var list = new List<Dictionary<string, object>>();

        var datas = sheet.Split("\r\n");
        var header = datas[0].Split('\t');


        for (int i = 1; i < datas.Length; i++)
        {
            var entry = new Dictionary<string, object>();
            var values = datas[i].Split('\t');
            if (values.Length == 0 || values[0] == "") continue;

            for (int j = 0; j < header.Length; j++)
            {
                entry.Add(header[j], values[j]);
            }
            list.Add(entry);
        }
        return list;
    }

    public string GetTSVAddress(SheetData s)
    {
        return $"{s.ADDRESS}/export?format=tsv&range={s.Range}&gid={s.SHEET_ID_PEN}";
    }


    IEnumerator SetLoadingText(int time=0)
    {
        string[] originLoadingText = new LocalizedString("LocalTable", "Loading-Text").GetLocalizedString().ToString().Split('^');

        yield return new WaitForSecondsRealtime(0.5f);
        int index = time/4;
        if(index>=originLoadingText.Length)index = originLoadingText.Length-1;

        if(time%4==0)loadingText.text = originLoadingText[index];
        else {loadingText.text +=".";}
        int num = time+1;
        if(num<originLoadingText.Length * 4){StartCoroutine(SetLoadingText(num));}
        else{StartCoroutine(SetLoadingText(0));}
    }
    public void SetTitle()
    {
        Color color = Title.color;
        color.a = 0;
        Title.color = color;
        Title.DOFade(1, 0.8f);
        Title.gameObject.GetComponent<RectTransform>().localScale = Vector2.zero;
        Title.gameObject.GetComponent<RectTransform>().DOScale(Vector2.one,0.8f);
    }
}
