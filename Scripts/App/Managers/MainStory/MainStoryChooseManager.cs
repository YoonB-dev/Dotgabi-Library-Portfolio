using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainStoryChooseManager : SceneSingleton<MainStoryChooseManager>
{
    [SerializeField] private Transform ChooseBoxGroup;
    public void SetMainChooseText(List<MainStoryChooseDTO> ChooseDTO)
    {
        // 선택지 UI 활성화 및 텍스트 설정
        ChooseBoxGroup.gameObject.SetActive(true);

        int chooseCount = ChooseDTO.Count;
        foreach (Transform child in ChooseBoxGroup)
        {
            child.gameObject.SetActive(false);
        }

        for (int i = 0; i < chooseCount; i++)
        {
            var chooseBox = ChooseBoxGroup.GetChild(i);
            ButtonAnim.Instance.ButtonScaleIn(chooseBox.gameObject, 0.2f, 1f, 0.3f);
            SetChooseBoxPos(chooseBox.gameObject, i, chooseCount);

            var chooseText = chooseBox.GetChild(0).GetComponent<TextMeshProUGUI>();
            chooseText.text = ChooseDTO[i].ChooseText;

            int index = i;
            var chooseBoxButton = chooseBox.GetComponent<Button>();
            chooseBoxButton.onClick.RemoveAllListeners();
            chooseBoxButton.onClick.AddListener(() => {
                //SFX
                AudioManager.Instance.ButtonClickSound1();

                if (ChooseDTO[index].ResultList == null || ChooseDTO[index].ResultList.Count == 0)
                {
                    if (ChooseDTO[index].NextTextId == null)
                    {
                        CloseChooseBox();

                        if (ChooseDTO[index].ExtraData != null)
                        {
                            SetMainStoryChooseExtraData(ChooseDTO[index].ExtraData);
                        }
                        return;
                    }
                    int nextTextId = (int)ChooseDTO[index].NextTextId;
                    var nextStoryDTO = InGameData.Instance.MainStoryTexts.Find(x => x.TextId == nextTextId);
                    StartCoroutine(MainStoryManager.Instance.SetMainStoryText(nextStoryDTO));
                }
                else
                {
                    MainStoryResultManager.Instance.SetMainResultText(ChooseDTO[index].ResultList);
                }
                CloseChooseBox();
            });
        }
    }

    private void SetMainStoryChooseExtraData(JObject extraData)
    {
        if (extraData.ContainsKey("scene_move"))
        {
            string sceneName = extraData["scene_move"].ToString();
            SceneManager.LoadScene(sceneName);
        }
    }



    // 선택지 박스 위치 설정
    private void SetChooseBoxPos(GameObject chooseBox, int index, int totalCount)
    {
        if (totalCount == 1)
        {
            chooseBox.GetComponent<RectTransform>().localPosition = new Vector2(0, 0);
        }
        else if (totalCount == 2)
        {
            if (index == 0)
            {
                chooseBox.GetComponent<RectTransform>().localPosition = new Vector2(0, 350);
            }
            else if (index == 1)
            {
                chooseBox.GetComponent<RectTransform>().localPosition = new Vector2(0, 50);
            }
        }
        else if (totalCount == 3)
        {
            if (index == 0)
            {
                chooseBox.GetComponent<RectTransform>().localPosition = new Vector2(0, 400);
            }
            else if (index == 1)
            {
                chooseBox.GetComponent<RectTransform>().localPosition = new Vector2(0, 100);
            }
            else if (index == 2)
            {
                chooseBox.GetComponent<RectTransform>().localPosition = new Vector2(0, -200);
            }
        }

    }


    public void CloseChooseBox()
    {
        ChooseBoxGroup.gameObject.SetActive(false);
    }
}
