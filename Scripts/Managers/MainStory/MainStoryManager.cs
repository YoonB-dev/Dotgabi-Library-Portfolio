using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainStoryManager : SceneSingleton<MainStoryManager>
{
    [SerializeField] private GameObject MainStoryCanvas;
    public GameObject TextBox;
    [SerializeField] private List<MainStoryDTO> CurrentStoryList;
    public Button NextButton;
    [SerializeField] private Image NextButtonImage;
    public Image StoryImage;

    public void ShowMainStoryCanvas(EnumTypes.MainStoryTrigger? storyTrigger)
    {
        //SFX
        AudioManager.Instance.OpenMiniScrollSound();

        if (storyTrigger == null)
        {
            Debug.LogError("MainStoryTrigger is null");
            return;
        }
        MainStoryCanvas.SetActive(true);
        ButtonAnim.Instance.ButtonScaleIn(TextBox, 0.2f, 1f, 0.3f);
        CurrentStoryList = SetStory(storyTrigger.Value);

        StartCoroutine(SetMainStoryText(CurrentStoryList[0]));


        SetNextButtonActive(false);
    }

    public void CloseMainStoryCanvas()
    {
        //SFX
        AudioManager.Instance.ButtonClickSound2();

        MainStoryCanvas.SetActive(false);
    }

    private List<MainStoryDTO> SetStory(EnumTypes.MainStoryTrigger startTrigger)
    {
        var storyList = InGameData.Instance.MainStoryTexts;
        var result = new List<MainStoryDTO>();
        bool started = false;

        foreach (var story in storyList)
        {
            Debug.Log("스토리 트리거: " + story.TextTrigger);
            // 시작점 찾기
            if (!started)
            {
                if (story.TextTrigger == startTrigger)
                {
                    started = true;
                    result.Add(story);
                }
                continue;
            }

            // 트리거가 chain, end, 혹은 같은 트리거면 계속 추가
            if (story.TextTrigger == startTrigger || story.TextTrigger == EnumTypes.MainStoryTrigger.chain || story.TextTrigger == EnumTypes.MainStoryTrigger.end)
            {
                result.Add(story);
            }
            // 새로운 트리거가 나타나면 중단 (포함X)
            else
            {
                break;
            }
        }

        Debug.Log("스토리 개수: " + result.Count);
        return result;
    }

    public IEnumerator SetMainStoryText(MainStoryDTO StoryDTO)
    {
        //SFX
        AudioManager.Instance.ButtonClickSound1();

        SetNextButtonActive(false);
        StoryImage.gameObject.SetActive(false);

        Debug.Log("텍스트 ID: " + StoryDTO.TextId);

        if (StoryDTO.ExtraData != null)
        {
            SetMainStoryExtraData(StoryDTO.ExtraData);
        }

        var textPos = TextBox.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        yield return StartCoroutine(TextTypingUtils.PlayTypewriterEffect(textPos, StoryDTO.StoryText));

        if (StoryDTO.ChooseList != null && StoryDTO.ChooseList.Count > 0)
        {
            yield return new WaitForSeconds(0.5f);
            MainStoryChooseManager.Instance.SetMainChooseText(StoryDTO.ChooseList);
        }
        else
        {
            if (StoryDTO.TextTrigger == EnumTypes.MainStoryTrigger.end)
            {
                SetNextButtonActive(true);
                NextButton.onClick.RemoveAllListeners();
                NextButton.onClick.AddListener(() => {
                    CloseMainStoryCanvas();
                });
                yield break; // 다음 텍스트가 없으면 종료
            }

            if (StoryDTO.NextTextId != null)
            {
                int nextTextId = (int)StoryDTO.NextTextId;
                var nextStoryDTO = InGameData.Instance.MainStoryTexts.Find(x => x.TextId == nextTextId);
                SetNextButtonActive(true);
                NextButton.onClick.RemoveAllListeners();
                NextButton.onClick.AddListener(() => {
                    StartCoroutine(SetMainStoryText(nextStoryDTO));
                });
            }
        }
    }

    public void SetMainStoryExtraData(JObject extdaData)
    {
        if (extdaData.ContainsKey("show_image"))
        {
            string imgPath = extdaData["show_image"].ToString();
            ButtonAnim.Instance.ButtonScaleIn(StoryImage.gameObject, 0.2f, 1f, 0.3f);
            StoryImage.sprite = Resources.Load<Sprite>(imgPath);
            StoryImage.SetNativeSize();
        }
    }

    public void SetNextButtonActive(bool isActive)
    {
        NextButton.gameObject.SetActive(isActive);
        NextButtonImage.gameObject.SetActive(isActive);
    }
}
