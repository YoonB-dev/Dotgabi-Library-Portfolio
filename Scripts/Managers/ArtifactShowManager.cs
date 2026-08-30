using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArtifactShowManager : SceneSingleton<ArtifactShowManager>
{
    private ScenarioDTO SCENARIO_DATA;
    [SerializeField] private GameObject flowItemSize; // 유물 넘칠 때 표시
    public Transform treasureIconPos;
    public GameObject ItemIcon;
    public int itemShowIndex = 0;
    public void SetItems(ScenarioDTO data)
    {
        SCENARIO_DATA = data;
        SetArtifactIcon();
    }
    public void SetArtifactIcon()
    {
        // 유물 넘치면 text 활성화
        int flowItem = SCENARIO_DATA.OwnedArtifactList.Count - 14;
        if (flowItem > 0)
        {
            flowItemSize.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"+{flowItem}";
            flowItemSize.SetActive(true);

            flowItemSize.GetComponent<Button>().onClick.RemoveAllListeners();
            flowItemSize.GetComponent<Button>().onClick.AddListener(() => {
                PopupManager.Instance.ShowPopup(EnumTypes.PopupType.Artifact, isCollection: false, isDetail: true);
            });
        }
        else
        {
            flowItemSize.SetActive(false);
        }

        Debug.Log($"SetArtifactIcon: {SCENARIO_DATA.OwnedArtifactList.Count} artifacts owned.");

        // 유물 아이콘 부족하면 생성
        if (treasureIconPos.childCount < SCENARIO_DATA.OwnedArtifactList.Count)
        {
            for (int i = treasureIconPos.childCount; i < SCENARIO_DATA.OwnedArtifactList.Count; i++)
            {
                GameObject iconButton = Instantiate(ItemIcon, treasureIconPos);
                iconButton.SetActive(false);
            }
        }
        // 일단 전부 비활성화
        foreach (Transform child in treasureIconPos)
        {
            child.gameObject.SetActive(false);
        }
        // 유물 아이콘 설정
        for (int i = 0; i < SCENARIO_DATA.OwnedArtifactList.Count; i++)
        {
            var iconButton = treasureIconPos.GetChild(i).gameObject;
            iconButton.SetActive(true);
            var artifact = InGameData.Instance.Artifacts.Find(x => x.Id == SCENARIO_DATA.OwnedArtifactList[i].ArtifactId);
            iconButton.GetComponent<Image>().sprite = Resources.Load<Sprite>(artifact.ImageUrl);
            iconButton.GetComponent<Button>().onClick.RemoveAllListeners();
            iconButton.GetComponent<Button>().onClick.AddListener(() => ShowArtifactDetail(artifact));
        }
    }

    private void ShowArtifactDetail(ArtifactDTO artifactDTO)
    {
        // 유물 상세 정보를 보여주는 로직
        PopupManager.Instance.ShowJustArtifactDetail(artifactDTO);
    }
}
