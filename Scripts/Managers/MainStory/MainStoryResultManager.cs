using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainStoryResultManager : SceneSingleton<MainStoryResultManager>
{
    public void SetMainResultText(List<MainStoryResultDTO> ResultDTOs)
    {
        Debug.Log("선택지 결과 개수: " + ResultDTOs.Count);
        MainStoryChooseManager.Instance.CloseChooseBox();

        var textPos = MainStoryManager.Instance.TextBox.transform.GetChild(0).GetComponent<TextMeshProUGUI>();

        textPos.text = ResultDTOs[0].ResultText; // 일단 첫번째 결과만 보여주기

        if (ResultDTOs[0].NextTextId != null)
        {
            var nextButton = MainStoryManager.Instance.NextButton;
            MainStoryManager.Instance.SetNextButtonActive(true);
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(() => {
                int nextTextId = (int)ResultDTOs[0].NextTextId;
                var nextStoryDTO = InGameData.Instance.MainStoryTexts.Find(x => x.TextId == nextTextId);
                nextButton.gameObject.SetActive(false);
                StartCoroutine(MainStoryManager.Instance.SetMainStoryText(nextStoryDTO));
            });
        }
    }



}
