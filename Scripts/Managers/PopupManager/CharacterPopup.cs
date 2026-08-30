using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterPopup : MonoBehaviour
{
    [SerializeField] private Canvas CharacterCanvas;
    [SerializeField] private Transform CharacterBox;
    [SerializeField] private Transform JobButtonContentPos;
    [SerializeField] private GameObject JobButtonPrefab;
    [SerializeField] private Sprite[] JobButtonImg = new Sprite[3]; //0-일반 1-선택 2-비활성화
    private static int CurrJobId;
    [SerializeField] private PopUpSO popUpSO; // 카드 팝업에 사용될 SO
    void Start()
    {
        Init();
    }

    private void Init()
    {
        SetJobButton();
    }

    public void ShowCharacterPopup()
    {
        CharacterCanvas.gameObject.SetActive(true);
        ButtonAnim.Instance.ButtonScaleIn(CharacterCanvas.transform.GetChild(1).gameObject, 0f, 1f);
        var jobDTO = InGameData.Instance.Jobs.Find(job => job.Id == 1); //id가 1인 직업을 기본으로 선택
        SelectCharacterShow(jobDTO); // 기본적으로 첫 번째 직업 선택
        // 잠금 업데이트
        SetJobButtonLock();
        //카메라 움직임 비활성화
        MainManager.Instance.cambox.SetCanMove(false);
    }

    private void SetJobButton()
    {
        var jobList = InGameData.Instance.Jobs.OrderBy(job => job.Id).ToList();

        // 1부터 시작하는 이유: 0은 public 이기 때문에
        for (int i = 1; i < InGameData.Instance.Jobs.Count; i++)
        {
            GameObject btn = Instantiate(JobButtonPrefab, JobButtonContentPos);
            btn.transform.GetComponent<Image>().sprite = JobButtonImg[0];
            int temp = i;
            btn.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(jobList[temp].ImgFacePath);

            // 잠금 설정
            SetJobButtonLock();

            btn.GetComponent<Button>().onClick.AddListener(() => SelectCharacterShow(jobList[temp]));
        }
    }

    private void SetJobButtonLock()
    {
        var jobList = InGameData.Instance.Jobs.OrderBy(job => job.Id).ToList();
        for (int i = 0; i < JobButtonContentPos.childCount; i++)
        {
            var btn = JobButtonContentPos.GetChild(i);
            int temp = i+1;
            if (JobUtils.Instance.CheckJobUnlockSync(jobList[temp].Id))
            {
                btn.GetChild(1).gameObject.SetActive(false);
            }
            else
            {
                btn.GetChild(1).gameObject.SetActive(true);
            }
        }
    }

    private void SelectCharacterShow(JobDTO jobDTO)
    {
        Debug.Log($"선택된 캐릭터: {jobDTO.Name}");
        // 캐릭터 박스 정보 설정
        CharacterBox.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(jobDTO.ImgPath);
        CharacterBox.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>().text = jobDTO.StartHP.ToString();
        CharacterBox.GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>().text = jobDTO.StartCoin.ToString();
        CharacterBox.GetChild(3).GetChild(0).GetComponent<TextMeshProUGUI>().text = jobDTO.Description;
        CharacterBox.GetChild(4).GetChild(0).GetComponent<TextMeshProUGUI>().text = jobDTO.Name;
        // 버튼 이미지 업데이트 (선택된 캐릭터 표시)
        StartCoroutine(SelectCharacterShowCoroutine(jobDTO));

        CurrJobId = jobDTO.Id; // 현재 선택된 직업 ID 저장
    }
    private IEnumerator SelectCharacterShowCoroutine(JobDTO jobDTO)
    {
        yield return null; // 약간의 딜레이를 주어 UI 업데이트가 자연스럽게 보이도록 함
        for (int i = 0; i < JobButtonContentPos.childCount; i++)
        {
            JobButtonContentPos.GetChild(i).GetComponent<Image>().sprite = ((i + 1) == jobDTO.Id) ? JobButtonImg[1] : JobButtonImg[0];
        }
    }

    public void JobCardShow()
    {
        EnumTypes.JobType jobType = (EnumTypes.JobType)CurrJobId;
        CardPopup.Instance.popupSO = popUpSO;
        CardPopup.Instance.ShowCardPopupByJob(jobType, isFirst: true);
    }

    public void HideCharacterPopup()
    {
        // SFX
        AudioManager.Instance.ButtonClickSound2();
        CharacterCanvas.gameObject.SetActive(false);

        // 배경 움직임 활성화
        MainManager.Instance.cambox.SetCanMove(true);
    }
}