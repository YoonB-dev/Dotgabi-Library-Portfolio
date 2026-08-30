using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArtifactDTOToObj : Singleton<ArtifactDTOToObj>
{
    public GameObject DTOToObj(GameObject targetArtifact, ArtifactDTO artifactDTO)
    {
        targetArtifact.GetComponent<Image>().sprite = Resources.Load<Sprite>(artifactDTO.ImageUrl);

        return targetArtifact;
    }


    public GameObject DTOToDetailObj(GameObject targetArtifact, ArtifactDTO artifactDTO)
    {
        // 유물 상세 정보를 설정하는 로직
        targetArtifact.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(artifactDTO.ImageUrl);
        targetArtifact.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = artifactDTO.Name;
        targetArtifact.transform.GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>().text = artifactDTO.Ability;
        targetArtifact.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = artifactDTO.FlavorText;

        return targetArtifact;
    }
}
