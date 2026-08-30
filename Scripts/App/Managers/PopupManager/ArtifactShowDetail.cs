using UnityEngine;
using UnityEngine.UI;

public class ArtifactShowDetail : MonoBehaviour
{
    [SerializeField] private GameObject artifactCanvas;
    [SerializeField] private GameObject artifactDetailCanvas;
    [SerializeField] private GameObject artifactDetailPanel;
    [SerializeField] private GameObject artifactForm;
    [SerializeField] private GameObject artifactSelectButton; // 유물 선택 버튼 -> 단순히 보는 상황이면 false로 한다.
    [SerializeField] private GameObject artifactShowPanel; // 유물 전체 정보임 -> justDetail일때 false로 한다.

    /// justDetail: 상세정보만 보여주는것, 아니라면 전체 리스트로 유물 정보도 보여줌
    /// isSelect: 보물방에서 선택하려고 하는 상황인지 확인. 맞다면 관련된 버튼 보여줌.
    public void ShowArtifactDetail(ArtifactDTO artifactDTO, bool justDetail = false, bool isSelect = false)
    {
        // 유물 상세 정보를 보여주는 로직을 구현합니다.
        // 예: 유물 이름, 설명, 이미지 등을 UI에 표시
        // SFX
        AudioManager.Instance.ButtonClickSound1();
        // 캔버스 활성화
        if (!artifactCanvas.activeSelf) { artifactCanvas.SetActive(true); }
        if (!artifactDetailCanvas.activeSelf) { artifactDetailCanvas.SetActive(true); }
        artifactShowPanel.SetActive(!justDetail);
        artifactSelectButton.SetActive(isSelect);
        if (isSelect)
        {
            artifactSelectButton.GetComponent<Button>().onClick.RemoveAllListeners();
            artifactSelectButton.GetComponent<Button>().onClick.AddListener(() => {
                TreasureManager.Instance.SelectArtifact(artifactDTO);
                HideArtifactDetail();
            });
        }
        // 애니메이션
        ButtonAnim.Instance.ButtonScaleIn(artifactDetailPanel, 0.2f, 1f);
        // 유물 정보 설정
        GameObject targetArtifact = ArtifactDTOToObj.Instance.DTOToDetailObj(artifactForm, artifactDTO);
    }

    public void HideArtifactDetail()
    {
        // SFX
        AudioManager.Instance.ButtonClickSound1();
        // 캔버스 비활성화
        artifactDetailCanvas.SetActive(false);
    }
}
