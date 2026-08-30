using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

public class MiniGameShell : SceneSingleton<MiniGameShell>
{
    [SerializeField]
    private GameObject[] shells;
    List<ArtifactDTO> ShellArtifacts = new();
    private List<GameObject> selectedItems = new(); // 선택한 아이템
    private Dictionary<GameObject, ArtifactDTO> cardItemMap = new(); // 전체 아이템
    private int RestCount = 2;
    public TextMeshProUGUI countTxt;

    public void StartMinigame()
    {
        SetShellItem();
    }
    void SetShellItem()
    {
        //유물 넣어둘 데이터
        string restCountTxt = new LocalizedString("LocalTable", "RestCount").GetLocalizedString();
        countTxt.text = restCountTxt + ": " + RestCount;
        ShellArtifacts.Clear();
        shells[0].transform.parent.gameObject.SetActive(true);
        for (int i = 0; i < shells.Length; i++)
        {
            shells[i].GetComponent<Image>().sprite = Resources.Load<Sprite>("Image/Mystery/seashell_closed");
            shells[i].transform.GetChild(0).gameObject.SetActive(false);
            shells[i].transform.localScale = new Vector3(0, 0, 0);
            shells[i].transform.DOScale(new Vector3(0.8f, 0.8f, 0.8f), 0.5f);
        }
        List<ArtifactDTO> itemData = InGameData.Instance.Artifacts;
        List<ArtifactDTO> itemDataCommon = new List<ArtifactDTO>();
        List<ArtifactDTO> itemDataRare = new List<ArtifactDTO>();
        List<ArtifactDTO> itemDataEpic = new List<ArtifactDTO>();

        foreach (var item in itemData)
        {
            if (UserData.Instance.MainScenarioData.OwnedArtifactList.Find(x => x.ArtifactId == item.Id) == null &&
                (item.Place == "public" || item.Place == "shop"))
                {
                switch (item.Rarity)
                {
                    case EnumTypes.RarityType.common:
                        itemDataCommon.Add(item);
                        break;
                    case EnumTypes.RarityType.rare:
                        itemDataRare.Add(item);
                        break;
                    case EnumTypes.RarityType.epic:
                        itemDataEpic.Add(item);
                        break;
                }
            }
        }

        //유물 뽑기
        for (int i = 0; i < shells.Length / 2; i++)
        {
            var random = new System.Random(UserData.Instance.MainScenarioData.GenerateSeed * (i + 1) + UserData.Instance.MainScenarioData.SelectList.Count * 3);
            int ran = random.Next(1, 11);
            var ItemDataAll = itemDataCommon;
            int temp = i;
            if (ran >= 1 && ran <= 6) ItemDataAll = itemDataCommon;
            else if (ran > 6 && ran <= 9 && itemDataRare.Count > 0) ItemDataAll = itemDataRare;
            else if (ran >= 10 && itemDataEpic.Count > 0) ItemDataAll = itemDataEpic;

            if (itemDataEpic.Count == 0) { itemDataEpic.AddRange(itemDataCommon); }
            if (itemDataRare.Count == 0) { itemDataRare.AddRange(itemDataCommon); }
            if (itemDataCommon.Count == 0) { itemDataCommon.AddRange(itemDataRare); }
            if (itemDataCommon.Count == 0) { itemDataCommon.AddRange(itemDataEpic); }

            if (itemDataCommon.Count == 0 && itemDataRare.Count == 0 && itemDataEpic.Count == 0)
            {
                shells[temp].transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>("Image/Shop/icon_stamp_merchant");
                Debug.Log("유물이 없습니다.");
                continue;
            }

            int num = random.Next(0, ItemDataAll.Count);
            var targetItem = ItemDataAll[num];
            ShellArtifacts.Add(targetItem);
            ItemDataAll.RemoveAt(num);
        }

        //Item개수 2배로 늘리고 섞기
        var itemsCopy = new List<ArtifactDTO>(ShellArtifacts);
        ShellArtifacts.AddRange(itemsCopy);

        for (int i = 0; i < ShellArtifacts.Count; i++)
        {
            for (int k = 0; k < ShellArtifacts.Count; k++)
            {
                var temp = ShellArtifacts[i];
                int ran = Random.Range(0, ShellArtifacts.Count);
                ShellArtifacts[i] = ShellArtifacts[ran];
                ShellArtifacts[ran] = temp;
            }
        }
        cardItemMap.Clear();
        for (int i = 0; i < shells.Length; i++)
        {
            int index = i;
            var shell = shells[index].transform;
            shell.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(ShellArtifacts[index].ImageUrl);
            shell.GetChild(0).gameObject.SetActive(false);

            cardItemMap.Add(shell.GetChild(0).gameObject, ShellArtifacts[index]);

            shell.GetComponent<Button>().onClick.RemoveAllListeners();
            shell.GetComponent<Button>().onClick.AddListener(() => OnCardSelected(shell.transform.GetChild(0).gameObject, ShellArtifacts[index]));
        }
    }

    void OnCardSelected(GameObject item, ArtifactDTO artifact)
    {
        if (selectedItems.Contains(item) || RestCount <= 0) return;

        selectedItems.Add(item);

        item.transform.parent.GetComponent<Image>().sprite = Resources.Load<Sprite>("Image/Mystery/seashell_open");

        item.SetActive(true);
        item.transform.localScale = new Vector3(0, 0, 0);
        item.transform.DOScale(new Vector3(1f, 1f, 1f), 0.5f);

        if (selectedItems.Count == 2)
        {
            CheckMatch(artifact);
            RestCount--;
            string restCountTxt = new LocalizedString("LocalTable", "RestCount").GetLocalizedString();
            countTxt.text = restCountTxt + ": " + RestCount;
        }
    }

    public async void CheckMatch(ArtifactDTO artifact)
    {
        for (int i = 0; i < shells.Length; i++)
        {
            shells[i].GetComponent<Button>().interactable = false;
        }

        await Task.Delay(1000);

        var item1 = selectedItems[0];
        var item2 = selectedItems[1];

        if (cardItemMap[item1].Id == cardItemMap[item2].Id)
        {
            Debug.Log("유물이 일치합니다!");
            //유물 획득 로직 추가
            item1.transform.parent.GetComponent<Button>().interactable = false;
            item2.transform.parent.GetComponent<Button>().interactable = false;

            await ScenarioArtifactUtils.Instance.GetArtifact(artifact, MoveSystem.Instance.SCENARIO_DATA);

        }
        else
        {
            Debug.Log("유물이 일치하지 않습니다.");
            //유물 뒤집기
            item1.SetActive(false);
            item2.SetActive(false);

            item1.transform.parent.GetComponent<Image>().sprite = Resources.Load<Sprite>("Image/Mystery/seashell_closed");
            item2.transform.parent.GetComponent<Image>().sprite = Resources.Load<Sprite>("Image/Mystery/seashell_closed");
        }

        selectedItems.Clear();

        if (RestCount <= 0)
        {
            string restText = new LocalizedString("LocalTable", "NoMore-Chance").GetLocalizedString();
            MysteryManager.Instance.eventText.GetComponent<TextMeshProUGUI>().text = restText;

            for (int i = 0; i < shells.Length; i++)
            {
                shells[i].GetComponent<Button>().interactable = false;
                shells[i].transform.DOScale(Vector2.zero, 0.5f);
            }
            await Task.Delay(500); // 0.5초 대기
            for (int i = 0; i < shells.Length; i++)
            {
                shells[i].SetActive(false);
            }
            StartCoroutine(MysteryManager.Instance.SetBackButtons());
            countTxt.text = string.Empty;
        }

        for (int i = 0; i < shells.Length; i++)
        {
            shells[i].GetComponent<Button>().interactable = true;
        }
    }

}
