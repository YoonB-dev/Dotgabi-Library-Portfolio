using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArtifactPopup : MonoBehaviour
{
    public PopUpSO popupSO { get; set; }
    [SerializeField] private Canvas artifactCanvas;
    [SerializeField] private GameObject artifactShowPanel;
    [SerializeField] private Transform contentPos;
    [SerializeField] private ArtifactShowDetail artifactDetail; // 유물 상세 정보를 보여주는 컴포넌트
    [Header("Artifact Popups - whole, detail")]
    [SerializeField] private GameObject artifactPopup; // 유물 팝업 (디테일X)
    [SerializeField] private GameObject artifactDetailPopup; // 유물 상세 팝업 (디테일O)


    public void ShowArtifactPopup(bool isFirst)
    {
        SetPopup();
        // SFX
        AudioManager.Instance.ButtonClickSound1();
        // 팝업 애니메이션
        if (isFirst) { ButtonAnim.Instance.ButtonScaleIn(artifactShowPanel, 0.3f, 1f); }
        // 배경 움직임 비활성화
        // mainManager.cambox.SetCanMove(false);

        // 유물 생성
        StartCoroutine(ShowArtifactPopupCoroutine());
    }

    private IEnumerator ShowArtifactPopupCoroutine()
    {
        // 유물 팝업 위치 설정
        var artifactData = InGameData.Instance.Artifacts;

        // 유물 데이터가 contentPos의 자식 개수보다 많을 경우, 부족한 만큼 유물 오브젝트를 생성 - 후에 비활성화
        if (artifactData.Count > contentPos.transform.childCount)
        {
            for (int i = contentPos.transform.childCount; i < artifactData.Count; i++)
            {
                var artifact = Instantiate(popupSO.content, contentPos);
                artifact.SetActive(false);
            }
        }

        // 유물 데이터가 contentPos의 자식 개수보다 적을 경우, 남는 유물 오브젝트를 비활성화
        for (int i = artifactData.Count; i < contentPos.transform.childCount; i++)
        {
            contentPos.transform.GetChild(i).gameObject.SetActive(false);
        }

        // 유물 데이터에 따라 유물 오브젝트를 활성화하고 설정
        for (int i = 0; i < artifactData.Count; i++)
        {
            var targetArtifact = contentPos.transform.GetChild(i).gameObject;
            targetArtifact.SetActive(true);

            // 유물 DTO를 오브젝트에 설정
            targetArtifact = ArtifactDTOToObj.Instance.DTOToObj(targetArtifact, artifactData[i]);

            // 유물 클릭 이벤트 설정
            Button artifactButton = targetArtifact.GetComponent<Button>();
            artifactButton.onClick.RemoveAllListeners();
            int index = i;
            artifactButton.onClick.AddListener(() => artifactDetail.ShowArtifactDetail(artifactData[index]));
        }

        yield return null;
    }
    public void ShowMainOwnedArtifactPopup()
    {
        SetPopup();
        // SFX
        AudioManager.Instance.ButtonClickSound1();
        ButtonAnim.Instance.ButtonScaleIn(artifactShowPanel, 0.3f, 1f);
        // 배경 움직임 비활성화
        // mainManager.cambox.SetCanMove(false);
        StartCoroutine(ShowOwnedArtifactPopupCoroutine());
    }

    private IEnumerator ShowOwnedArtifactPopupCoroutine()
    {
        // 유물 팝업 위치 설정
        var artifactData = GetOwnedArtifactList();
        if (artifactData == null)
        {
            Debug.LogError("ShowOwnedArtifactPopup: artifactData is null");
            yield break;
        }

        // 유물 데이터가 contentPos의 자식 개수보다 많을 경우, 부족한 만큼 유물 오브젝트를 생성 - 후에 비활성화
        if (artifactData.Count > contentPos.transform.childCount)
        {
            for (int i = contentPos.transform.childCount; i < artifactData.Count; i++)
            {
                var artifact = Instantiate(popupSO.content, contentPos);
                artifact.SetActive(false);
            }
        }

        // 유물 데이터가 contentPos의 자식 개수보다 적을 경우, 남는 유물 오브젝트를 비활성화
        for (int i = artifactData.Count; i < contentPos.transform.childCount; i++)
        {
            contentPos.transform.GetChild(i).gameObject.SetActive(false);
        }

        // 유물 데이터에 따라 유물 오브젝트를 활성화하고 설정
        for (int i = 0; i < artifactData.Count; i++)
        {
            var targetArtifact = contentPos.transform.GetChild(i).gameObject;
            targetArtifact.SetActive(true);

            // 유물 DTO를 오브젝트에 설정
            var artifact = InGameData.Instance.Artifacts.Find(a => a.Id == artifactData[i].ArtifactId);
            targetArtifact = ArtifactDTOToObj.Instance.DTOToObj(targetArtifact, artifact);

            // 유물 클릭 이벤트 설정
            Button artifactButton = targetArtifact.GetComponent<Button>();
            artifactButton.onClick.RemoveAllListeners();
            int index = i;
            artifactButton.onClick.AddListener(() => artifactDetail.ShowArtifactDetail(artifact));
        }

        yield return null;
    }

    public void SetPopup()
    {
        artifactCanvas.gameObject.SetActive(true);
        artifactPopup.SetActive(true);
        artifactDetailPopup.SetActive(false);
    }

    public void HideArtifactPopup()
    {
        artifactCanvas.gameObject.SetActive(false);
        // SFX
        AudioManager.Instance.ButtonClickSound1();
        // 배경 움직임 활성화
        MainManager.Instance?.cambox.SetCanMove(true);
    }
    public void ShowArtifactJustDetail(ArtifactDTO artifactDTO, bool isSelect = false)
    {
        // 유물 상세 정보만 보여주는 로직
        artifactDetail.ShowArtifactDetail(artifactDTO, justDetail: true, isSelect: isSelect);
    }

    public List<UserScenarioOwnedArtifactDTO> GetOwnedArtifactList()
    {
        switch (GameData.Instance.CurrScenarioType)
        {
            case EnumMainType.ScenarioType.story:
                return UserData.Instance.MainScenarioData.OwnedArtifactList;
            case EnumMainType.ScenarioType.challenge:
                return UserData.Instance.ChallengeScenarioData.OwnedArtifactList;
            default:
                Debug.LogError("GetOwnedCardList: Invalid ScenarioType");
                return null;
        }
    }
}
