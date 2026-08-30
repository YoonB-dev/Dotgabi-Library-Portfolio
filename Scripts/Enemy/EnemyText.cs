using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.SmartFormat;
using UnityEngine.UI;

public class EnemyText : SceneSingleton<EnemyText>
{
    [Header("Canvas")]
    [SerializeField] private Canvas enemyTextCanvas;
    [SerializeField] private GameObject textBox;
    [SerializeField] private GameObject choiceGroup;
    [SerializeField] private Image nextButtonImage;

    public GameObject textEndButton;
    public Button[] selectText;
    private bool isHard = false;
    private int enemyIndex;
    private Action disableEnemyObjAction;
    private Coroutine textTypingCo = null;

    private bool isTouchNext = false; // 다음 텍스트로 넘어가는 플래그
    private EnemyTextDTO nextEnemyTextDTO = null;
    public void SetEnemyText(EnemyDTO enemyDTO, GameObject enemyObj)
    {
        SetCanvas();
        //타겟 몬스터 대사들 가져오기
        enemyIndex = enemyDTO.Id;
        if (enemyIndex == 14) { enemyIndex = 13; }

        // 몬스터 가운데로 이동시키기
        enemyObj.transform.DOMove(new Vector3(0, 0, 0), 0.3f);
        enemyObj.transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.3f);

        //메인 대사 출력
        EnemyTextDTO targetTextDTO = null;
        var targetTextDTOs = InGameData.Instance.EnemyTexts.FindAll(x => x.EnemyId == enemyIndex && x.TextType == EnumTypes.EnemyTextType.root);
        if (targetTextDTOs == null || targetTextDTOs.Count == 0)
        {
            Debug.LogError("EnemyTextDTO is null for EnemyId: " + enemyIndex);
            return;
        }

        if (targetTextDTOs.Count > 1)
        {
            for (int i = 0; i < targetTextDTOs.Count; i++)
            {
                var textDTO = targetTextDTOs[i];
                // 조건 체크
                if (EnemyTextConditionChecker.CheckCondition(textDTO, UserData.Instance))
                {
                    targetTextDTO = textDTO;
                    break;
                }
            }
        }
        else
        {
            targetTextDTO = targetTextDTOs[0];
        }
        SetTextBox(targetTextDTO);
        disableEnemyObjAction = () => {
            enemyObj.SetActive(false);
            enemyObj.transform.localScale = Vector3.one;
        };
    }

    void Update()
    {
        if (isTouchNext && Input.GetMouseButtonDown(0))
        {
            Debug.Log($"isTouchNext: {isTouchNext}, nextEnemyTextDTO: {nextEnemyTextDTO}");
            SetNextButtonActive(false);
            StartContinueText(nextEnemyTextDTO);
        }
    }

    private void SetCanvas()
    {
        enemyTextCanvas.gameObject.SetActive(true);
        VictoryManager.Instance.SetHeaderCoinCanvasUp();
        textBox.SetActive(false);
    }

    public void SetTextBox(EnemyTextDTO enemyTextDTO)
    {
        var textBoxTransform = textBox.transform.GetChild(0);
        if (!textBox.activeSelf)
        {
            textBox.SetActive(true);
            textBoxTransform.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            textBoxTransform.transform.DOScale(new Vector3(1f, 1f, 1f), 0.3f);
        }

        //선택지 비활성화
        SetChoiceGroupActive(false);

        if (textTypingCo != null)
        {
            StopCoroutine(textTypingCo);
            textTypingCo = null;
        }

        //텍스트 설정
        string text = GetFormattedText(enemyTextDTO);
        textTypingCo = StartCoroutine(TextTypingUtils.PlayTypewriterEffect(textBoxTransform.transform.GetChild(0).GetComponent<TextMeshProUGUI>(), text));

        //선택 결과
        EnemyTextChoiceResult.Instance.SetChoiceResult(enemyTextDTO);

        // 만약 특수 행동일 경우 다른 처리
        if (enemyTextDTO.TextType == EnumTypes.EnemyTextType.action)
        {
            // 행동 처리
            EnemyTextActionHandler.Instance.HandleAction(enemyTextDTO);
        }

        // 만약 continue일 경우 다음 텍스트로 넘어가기
        if (enemyTextDTO.TextType == EnumTypes.EnemyTextType.continue_)
        {
            StartCoroutine(SetTouchNext());
            nextEnemyTextDTO = enemyTextDTO;
        }
        else
        {
            SetNextButtonActive(false);
            nextEnemyTextDTO = null;
        }

        //끝난 경우 종료
        if (enemyTextDTO.TextType == EnumTypes.EnemyTextType.leaf)
        {
            if (enemyTextDTO.ExtraData != null && enemyTextDTO.ExtraData.ContainsKey("is_final"))
            {
                bool isFinal = bool.Parse(enemyTextDTO.ExtraData["is_final"].ToString());
                if (isFinal)
                {
                    //최종 대사인 경우
                    VictoryManager.Instance?.CallVictoryManager(EnumTypes.EnemyType.boss, true);
                    return;
                }
            }
            StartCoroutine(SetCloseButton());
        }

        //선택지가 없는 경우 종료
        var isBranch = enemyTextDTO.Choices != null && enemyTextDTO.Choices.Count > 0;
        if (!isBranch) return;

        StartCoroutine(ShowChoice(enemyTextDTO.Choices));
    }

    private string GetFormattedText(EnemyTextDTO enemyTextDTO)
    {
        string text = enemyTextDTO.TextValue;
        if (enemyTextDTO.ExtraData != null && enemyTextDTO.ExtraData.ContainsKey("lose_coin"))
        {
            var loseCoinObj = enemyTextDTO.ExtraData["lose_coin"] as JObject;
            int coinValue = 0;
            if (loseCoinObj.ContainsKey("value"))
            {
                coinValue = int.Parse(loseCoinObj["value"].ToString());
            }
            else if (loseCoinObj.ContainsKey("percent"))
            {
                float coinPercent = float.Parse(loseCoinObj["percent"].ToString());
                coinValue = (int)(UserData.Instance.MainScenarioData.GameCoins * coinPercent);
            }
            text = Smart.Format(text, coinValue);
        }

        return text;
    }

    public void SetTextBoxText(string text)
    {
        var textBoxTransform = textBox.transform.GetChild(0);
        if (textTypingCo != null)
        {
            StopCoroutine(textTypingCo);
            textTypingCo = null;
        }
        textTypingCo = StartCoroutine(TextTypingUtils.PlayTypewriterEffect(textBoxTransform.transform.GetChild(0).GetComponent<TextMeshProUGUI>(), text));
    }

    IEnumerator SetTouchNext()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        SetNextButtonActive(true);
        yield return null;
    }

    public void StartContinueText(EnemyTextDTO enemyTextDTO)
    {
        if (enemyTextDTO.TextType == EnumTypes.EnemyTextType.continue_)
        {
            if (enemyTextDTO.ExtraData != null && enemyTextDTO.ExtraData.ContainsKey("text_id"))
            {
                int textId = int.Parse(enemyTextDTO.ExtraData["text_id"].ToString());
                Debug.Log($"Continue to Text ID: {textId}");
                var nextEnemyTextDTO = InGameData.Instance.EnemyTexts.Find(x => x.Id == textId);
                if (nextEnemyTextDTO != null)
                {
                    // 대사 박스 텍스트 변경
                    SetTextBox(nextEnemyTextDTO);
                }
            }
        }
    }

    IEnumerator ShowChoice(List<EnemyTextChoiceDTO> enemyTextChoiceDTO)
    {
        choiceGroup.SetActive(true);

        //일단 비활성화 - 텍스트 박스
        SetChoiceGroupActive(false);
        yield return new WaitForSecondsRealtime(1f);

        //선택지들 출력
        for (int i = 0; i < enemyTextChoiceDTO.Count; i++)
        {
            selectText[i].gameObject.SetActive(true);
            selectText[i].GetComponent<RectTransform>().localScale = Vector2.zero;
            selectText[i].GetComponent<RectTransform>().DOScale(Vector2.one, 0.3f);
            selectText[i].GetComponentInChildren<TextMeshProUGUI>().text = enemyTextChoiceDTO[i].ChoiceText;
            int temp = i;
            selectText[i].onClick.RemoveAllListeners();
            selectText[i].onClick.AddListener(() => SetChoiceBox(enemyTextChoiceDTO[temp]));
            yield return new WaitForSecondsRealtime(0.2f);
        }
        yield return null;
    }

    public void SetChoiceBox(EnemyTextChoiceDTO enemyTextChoiceDTO)
    {
        //SFX
        AudioManager.Instance.ButtonClickSound1();

        int nextIndex = enemyTextChoiceDTO.NextIndex;
        var nextEnemyTextDTO = InGameData.Instance.EnemyTexts.Find(x => x.Id == nextIndex);
        Debug.Log($"Choice Next Text ID: {nextIndex}");
        // 대사 박스 텍스트 변경
        SetTextBox(nextEnemyTextDTO);
    }
    IEnumerator SetCloseButton()
    {
        SetChoiceGroupActive(false);
        yield return new WaitForSecondsRealtime(1f);
        textEndButton.SetActive(true);
        yield return null;
    }

    private void SetChoiceGroupActive(bool isActive)
    {
        for (int i = 0; i < selectText.Length; i++)
        {
            selectText[i].gameObject.SetActive(isActive);
        }
    }

    public void EndEnemyText()
    {
        textBox.SetActive(false);
        choiceGroup.SetActive(false);
        textEndButton.SetActive(false);
        enemyTextCanvas.gameObject.SetActive(false);
        disableEnemyObjAction?.Invoke();
    }

    public void SetNextButtonActive(bool isActive)
    {
        isTouchNext = isActive;
        nextButtonImage.gameObject.SetActive(isActive);
    }
}
