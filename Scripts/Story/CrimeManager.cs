using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CrimeManager : SceneSingleton<CrimeManager>
{
    [Header("Crime Main UI")]
    [SerializeField] private GameObject CrimeCanvas;
    [Header("Text")]
    [SerializeField] private TextMeshProUGUI ActiveText;
    private Coroutine textCo = null;
    [SerializeField] private GameObject BlueFragmentObject;
    [Header("GetCanvas")]
    [SerializeField] private Canvas GetCanvas;
    [SerializeField] private Image GetImage;
    private List<int> getCardIndex = new List<int>();
    private List<int> obtainedArtifactIds = new List<int>();

    // 클릭 여부 확인
    private bool isClickedTree = false;
    private bool isClickedFootprint = false;
    private bool isClickedBlueFragment = false;
    void Start()
    {
        SetText("find_crime");
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

    public void ClickCrimeTree()
    {
        SetText("click_crime_tree");

        if (isClickedTree) return;
        isClickedTree = true;
        var itemDTO = InGameData.Instance.MainStoryItems.Find(x => x.Id == 7); // 나무 자국
        StartCoroutine(ShowGetCanvas(itemDTO));
    }

    public void ClickCrimeFootprint()
    {
        SetText("click_crime_footprint");

        if (isClickedFootprint) return;
        isClickedFootprint = true;
        var itemDTO = InGameData.Instance.MainStoryItems.Find(x => x.Id == 6); // 발자국
        StartCoroutine(ShowGetCanvas(itemDTO));
    }

    public void ClickCrimeBlueFragment()
    {
        SetText("click_crime_blue_fragment");
        BlueFragmentObject.SetActive(false);

        if (isClickedBlueFragment) return;
        isClickedBlueFragment = true;
        var itemDTO = InGameData.Instance.MainStoryItems.Find(x => x.Id == 1); // 파편
        GetArtifact(itemDTO);
    }
    public void GetArtifact(MainStoryItemDTO artifact)
    {
        if (artifact.ExtraData == null || !artifact.ExtraData.ContainsKey("artifact_id"))
        {
            Debug.LogError("Artifact ExtraData is null or does not contain artifact_id");
            return;
        }

        obtainedArtifactIds.Add((int)artifact.ExtraData["artifact_id"]);
        StartCoroutine(ShowGetCanvas(artifact));
    }

    public void ClickCrimeSilhouette()
    {
        SetText("click_crime_silhouette");
    }

    private IEnumerator ShowGetCanvas(MainStoryItemDTO itemDTO)
    {
        GetCanvas.gameObject.SetActive(true);
        ButtonAnim.Instance.ButtonScaleIn(GetImage.gameObject, 0.2f, 1f, 0.3f);
        GetImage.sprite = Resources.Load<Sprite>(itemDTO.ImgPath);
        yield return new WaitForSeconds(1.5f);
        GetCanvas.gameObject.SetActive(false);

        // Add to inventory
        var cardDTO = StoryCardManager.Instance?.AddCard(itemDTO);
        if (cardDTO != null)
        {
            getCardIndex.Add(itemDTO.Id);
        }
    }

    public void ClickBackButton()
    {
        //SFX
        AudioManager.Instance.ButtonClickSound1();
        _ = ClickBackButtonAsync();
    }

    public async Task ClickBackButtonAsync()
    {
        // 클리어 데이터 저장
        SupabaseMainScenarioStoryUpdate.Instance.UpdateMainScenarioStoryClearData(EnumTypes.MainStoryType.crime_scene_clear, true);
        // 얻은 카드 저장하기
        SupabaseMainScenarioStoryUpdate.Instance.InsertMainScenarioStoryOwnedCard(getCardIndex);
        // 얻은 유물 저장하기
        await GetArtifactEnd();
        SceneManager.LoadScene("GameScene");
    }

    private async Task GetArtifactEnd()
    {
        // 종료 후 유물 얻기
        foreach (var artifactId in obtainedArtifactIds)
        {
            var item = InGameData.Instance.Artifacts.Find(a => a.Id == artifactId);
            if (item != null)
            {
                await SupabaseArtifact.Instance.GetArtifact(artifactId, UserData.Instance.MainScenarioData);
            }
        }
    }
}
