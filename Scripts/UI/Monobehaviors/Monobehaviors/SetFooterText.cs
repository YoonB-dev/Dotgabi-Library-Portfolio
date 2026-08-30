using System.Collections;
using DG.Tweening;
using Microsoft.Win32.SafeHandles;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// <summary>
// SetFooterText.cs
// This script manages the footer text in the game, including displaying
// move text for damage, healing, and money changes, as well as updating the HP bar.
// </summary>
public class SetFooterText : SceneSingleton<SetFooterText>
{
    [SerializeField] private GameObject amountText, moveTextGroup;
    [SerializeField] private TextMeshProUGUI gameMoneyTxt, currHpTxt, maxHpTxt, stageTxt, cardNumTxt;
    [SerializeField] private Image hpImg, hpShadowImg;

    private ScenarioDTO GetCurrData()
    {
        switch (GameData.Instance.CurrScenarioType)
        {
            case EnumMainType.ScenarioType.story:
                return UserData.Instance.MainScenarioData;
            case EnumMainType.ScenarioType.challenge:
                return UserData.Instance.ChallengeScenarioData;
            default:
                Debug.LogError("GetCurrData: Invalid ScenarioType");
                return null;
        }
    }
    public void SetAllText()
    {
        var SCENARIO_DATA = GetCurrData();
        if (SCENARIO_DATA == null)
        {
            Debug.LogError("SetAllText: SCENARIO_DATA is null");
            return;
        }

        gameMoneyTxt.text = SCENARIO_DATA.GameCoins.ToString();
        currHpTxt.text = SCENARIO_DATA.CurrHp.ToString();
        maxHpTxt.text = SCENARIO_DATA.MaxHp.ToString();
        stageTxt.text = SCENARIO_DATA.CurrStageLevel + " - " + (SCENARIO_DATA.SelectList.Count + 1);

        cardNumTxt.text = SCENARIO_DATA.OwnedCardList.Count.ToString();
    }

    public void SetMoveText(int amount, EnumTypes.MoveTextType Type)
    {
        SetMoveTextObj(amount, Type, null);
        SetAllText();
    }

    public void SetMoveTextObj(int amount, EnumTypes.MoveTextType Type, Transform textPos = null)
    {
        bool canUse = false;

        //풀링
        GameObject targetTxt = null;
        foreach (Transform child in moveTextGroup.transform)
        {
            if (!child.gameObject.activeInHierarchy)
            {
                child.gameObject.SetActive(true);
                targetTxt = child.gameObject;
                canUse = true;
                break;
            }
        }
        if (!canUse)
        {
            targetTxt = Instantiate(amountText);
            targetTxt.transform.SetParent(moveTextGroup.transform, false);
        }
        //텍스트 종류 확인
        switch (Type)
        {
            case EnumTypes.MoveTextType.damage:
                if (textPos == null && maxHpTxt != null)
                {
                    textPos = maxHpTxt.transform;
                }

                targetTxt.GetComponent<TextMeshProUGUI>().text = "<color=red>" + -amount + "</color>";
                targetTxt.transform.position = new Vector2(textPos.position.x + 2, textPos.position.y);
                targetTxt.transform.DOMoveY(targetTxt.transform.position.y + 1.5f, 1f).SetEase(Ease.OutQuad).OnComplete(() => {
                    targetTxt.SetActive(false);
                });
                break;
            case EnumTypes.MoveTextType.heal:
                if (textPos == null && maxHpTxt != null)
                {
                    textPos = maxHpTxt.transform;
                }

                targetTxt.GetComponent<TextMeshProUGUI>().text = "<color=green>" + amount + "</color>";
                targetTxt.transform.position = new Vector2(textPos.position.x + 2, textPos.position.y);
                targetTxt.transform.DOMoveY(targetTxt.transform.position.y + 1.5f, 1f).SetEase(Ease.OutQuad).OnComplete(() => {
                    targetTxt.SetActive(false);
                });
                break;
            case EnumTypes.MoveTextType.money:
                if (textPos == null && gameMoneyTxt != null)
                {
                    textPos = gameMoneyTxt.transform;
                }

                if (amount >= 0)
                {
                    targetTxt.GetComponent<TextMeshProUGUI>().text = "<color=green>" + amount + "</color>";
                }
                else
                {
                    targetTxt.GetComponent<TextMeshProUGUI>().text = "<color=red>" + amount + "</color>";
                }
                targetTxt.transform.position = new Vector2(textPos.position.x, textPos.position.y);
                targetTxt.transform.DOMoveY(targetTxt.transform.position.y - 1.5f, 1f).SetEase(Ease.OutQuad).OnComplete(() => {
                    targetTxt.SetActive(false);
                });
                break;
            default:
                targetTxt.GetComponent<TextMeshProUGUI>().color = Color.white;
                break;
        }
    }

    public void SetHpBar(EnumTypes.TextMotionType motion)
    {
        Debug.Log("current HP: " + UserData.Instance.MainScenarioData.CurrHp);
        var SCENARIO_DATA = GetCurrData();
        if (SCENARIO_DATA == null)
        {
            Debug.LogError("SetHpBar: SCENARIO_DATA is null");
            return;
        }
        switch (motion)
        {
            case EnumTypes.TextMotionType.up:
                StartCoroutine(HpUpMotion());
                break;
            case EnumTypes.TextMotionType.down:
                StartCoroutine(HpDownMotion());
                break;
            case EnumTypes.TextMotionType.direct:
                hpImg.fillAmount = (float)SCENARIO_DATA.CurrHp / (float)SCENARIO_DATA.MaxHp;
                hpShadowImg.fillAmount = (float)SCENARIO_DATA.CurrHp / (float)SCENARIO_DATA.MaxHp;
                break;
        }
        SetAllText();
    }
    IEnumerator HpDownMotion()
    {
        var SCENARIO_DATA = GetCurrData();
        if (SCENARIO_DATA == null)
        {
            Debug.LogError("HpDownMotion: SCENARIO_DATA is null");
            yield break;
        }
        hpImg.fillAmount = (float)SCENARIO_DATA.CurrHp / (float)SCENARIO_DATA.MaxHp;
        yield return new WaitForSeconds(0.5f);
        hpShadowImg.DOFillAmount((float)SCENARIO_DATA.CurrHp / (float)SCENARIO_DATA.MaxHp, 0.5f);
    }
    IEnumerator HpUpMotion()
    {
        var SCENARIO_DATA = GetCurrData();
        if (SCENARIO_DATA == null)
        {
            Debug.LogError("HpUpMotion: SCENARIO_DATA is null");
            yield break;
        }
        hpShadowImg.fillAmount = (float)SCENARIO_DATA.CurrHp / (float)SCENARIO_DATA.MaxHp;
        yield return new WaitForSeconds(0.5f);
        hpImg.DOFillAmount((float)SCENARIO_DATA.CurrHp / (float)SCENARIO_DATA.MaxHp, 0.5f);
    }
}
