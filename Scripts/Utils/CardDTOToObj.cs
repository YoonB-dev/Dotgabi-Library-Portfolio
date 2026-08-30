using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class CardDTOToObj
{
    public static GameObject DTOToObj(GameObject card, CardDTO cardDTO)
    {
        // 카드 오브젝트에 데이터 설정 - UI용
        card.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(cardDTO.ImageUrl);
        card.transform.GetChild(4).GetComponent<TextMeshProUGUI>().text = SetName(cardDTO);
        card.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = SetDescription(cardDTO);
        card.transform.GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>().text = cardDTO.CardType.ToString();

        int cost = cardDTO.CardUpgrade < 2 ? cardDTO.Cost[cardDTO.CardUpgrade] : cardDTO.Cost[2];
        card.transform.GetChild(5).GetChild(0).GetComponent<TextMeshProUGUI>().text = cost.ToString();

        // 카드 프레임 설정 & 데코 설정
        card.transform.GetChild(1).GetComponent<Image>().sprite = Resources.Load<Sprite>(GetFramePath(UserData.Instance.SelectCardFrameId));

        var decoPath = GetDecoPath(UserData.Instance.SelectDecoId);
        if (decoPath == "none")
        {
            card.transform.GetChild(1).GetChild(0).gameObject.SetActive(false);
        }
        else
        {
            card.transform.GetChild(1).GetChild(0).gameObject.SetActive(true);
            card.transform.GetChild(1).GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(decoPath);
        }
        return card;
    }

    public static GameObject DTOToObjModel(GameObject card, CardDTO cardDTO)
    {
        // 카드 오브젝트에 데이터 설정 - 모델용
        card.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(cardDTO.ImageUrl);
        card.transform.GetChild(2).GetChild(0).GetComponent<TextMeshPro>().text = cardDTO.CardType.ToString();
        card.transform.GetChild(3).GetComponent<TextMeshPro>().text = SetDescription(cardDTO);
        card.transform.GetChild(4).GetComponent<TextMeshPro>().text = SetName(cardDTO);

        int cost = cardDTO.CardUpgrade < 2 ? cardDTO.Cost[cardDTO.CardUpgrade] : cardDTO.Cost[2];
        card.transform.GetChild(5).GetChild(0).GetComponent<TextMeshPro>().text = cost.ToString();

        // 카드 프레임 설정 & 데코 설정
        card.transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(GetFramePath(UserData.Instance.SelectCardFrameId));

        var decoPath = GetDecoPath(UserData.Instance.SelectDecoId);
        if (decoPath == "none")
        {
            card.transform.GetChild(1).GetChild(0).gameObject.SetActive(false);
        }
        else
        {
            card.transform.GetChild(1).GetChild(0).gameObject.SetActive(true);
            card.transform.GetChild(1).GetChild(0).GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(decoPath);
        }

        return card;
    }

    private static string SetName(CardDTO cardDTO)
    {
        // 카드 이름 설정
        string cardName = cardDTO.Name;
        if (cardDTO.CardUpgrade > 0 && cardDTO.CardUpgrade < 3)
        {
            cardName = "<color=green>" + cardName; // 이름에 색상 추가
            for (int i = 0; i < cardDTO.CardUpgrade; i++)
            {
                cardName += "+"; // 업그레이드 레벨에 따라 이름에 + 추가
            }
            cardName += "</color>"; // 색상 닫기
        }
        else if (cardDTO.CardUpgrade >= 3)
        {
            cardName = "<color=green>" + cardName + "+" + cardDTO.CardUpgrade + "</color>"; // 업그레이드 레벨이 3 이상일 경우 + 숫자로 표시
        }
        return cardName;
    }

    private static string SetDescription(CardDTO cardDTO)
    {
        var sb = new StringBuilder();
        int valueIndex = 0;
        int lastIndex = 0;

        // 모든 ^ 위치를 순차적으로 처리
        var originDTO = InGameData.Instance.Cards.Find(x => x.Id == cardDTO.Id);
        for (int i = 0; i < cardDTO.Description.Length; i++)
        {
            if (cardDTO.Description[i] == '^')
            {
                // ^ 이전 문자열 추가
                sb.Append(cardDTO.Description.Substring(lastIndex, i - lastIndex));
                // 값이 남아있으면 삽입
                if (valueIndex < cardDTO.CardActions.Count)
                {
                    if (originDTO == null)
                    {
                        return cardDTO.Description; // 원본 설명 반환
                    }
                    if ((cardDTO.CardActions[valueIndex].ExtraData != null && cardDTO.CardActions[valueIndex].ExtraData.ContainsKey("play_instrument")) || cardDTO.CardActions[valueIndex].ActionType == EnumTypes.Action.equip)
                    {
                        valueIndex++;
                    }
                    var action = cardDTO.CardActions[valueIndex];
                    int actionValue = DamageCal.GetUnLimitcard(action, cardDTO.CardUpgrade);
                    actionValue = GetExtraDataValue(action, actionValue);
                    // 값이 더 클 경우
                    if (originDTO.CardActions[valueIndex].Value[0] < actionValue)
                    {
                        var actionType = action.ActionType;

                        if (actionType == EnumTypes.Action.attack || actionType == EnumTypes.Action.debuff)
                        {
                            if (action.Target == EnumTypes.Target.self)
                            {
                                sb.Append("<color=red>");
                                sb.Append(actionValue);
                                sb.Append("</color>");
                            }
                            else if (action.Target == EnumTypes.Target.enemy || action.Target == EnumTypes.Target.enemys)
                            {
                                sb.Append("<color=green>");
                                sb.Append(actionValue);
                                sb.Append("</color>");
                            }
                            else
                            {
                                sb.Append(actionValue);
                            }
                        }
                        else if (actionType == EnumTypes.Action.shield || actionType == EnumTypes.Action.heal || actionType == EnumTypes.Action.buff || actionType == EnumTypes.Action.draw || actionType == EnumTypes.Action.action)
                        {
                            if (action.Target == EnumTypes.Target.self)
                            {
                                sb.Append("<color=green>");
                                sb.Append(actionValue);
                                sb.Append("</color>");
                            }
                            else if (action.Target == EnumTypes.Target.enemy || action.Target == EnumTypes.Target.enemys)
                            {
                                sb.Append("<color=red>");
                                sb.Append(actionValue);
                                sb.Append("</color>");
                            }
                            else
                            {
                                sb.Append(actionValue);
                            }
                        }
                    }
                    // 값이 더 작을 경우
                    else if (originDTO.CardActions[valueIndex].Value[0] > actionValue)
                    {
                        var actionType = action.ActionType;

                        if (actionType == EnumTypes.Action.attack || actionType == EnumTypes.Action.debuff)
                        {
                            if (action.Target == EnumTypes.Target.self)
                            {
                                sb.Append("<color=green>");
                                sb.Append(actionValue);
                                sb.Append("</color>");
                            }
                            else if (action.Target == EnumTypes.Target.enemy || action.Target == EnumTypes.Target.enemys)
                            {
                                sb.Append("<color=red>");
                                sb.Append(actionValue);
                                sb.Append("</color>");
                            }
                            else
                            {
                                sb.Append(actionValue);
                            }
                        }
                        else if (actionType == EnumTypes.Action.shield || actionType == EnumTypes.Action.heal || actionType == EnumTypes.Action.buff || actionType == EnumTypes.Action.draw || actionType == EnumTypes.Action.action)
                        {
                            if (action.Target == EnumTypes.Target.self)
                            {
                                sb.Append("<color=red>");
                                sb.Append(actionValue);
                                sb.Append("</color>");
                            }
                            else if (action.Target == EnumTypes.Target.enemy || action.Target == EnumTypes.Target.enemys)
                            {
                                sb.Append("<color=green>");
                                sb.Append(actionValue);
                                sb.Append("</color>");
                            }
                            else
                            {
                                sb.Append(actionValue);
                            }
                        }
                    }
                    // 값이 같으면 그냥 값 삽입
                    else
                    {
                        sb.Append(actionValue);
                    }
                    valueIndex++;
                }
                else
                {
                    // 값이 없으면 빈 문자열 삽입
                    sb.Append('?');
                }
                lastIndex = i + 1; // 처리 위치 업데이트
            }
        }

        // 마지막 ^ 이후 문자열 추가
        if (lastIndex < cardDTO.Description.Length)
        {
            sb.Append(cardDTO.Description.Substring(lastIndex));
        }

        return sb.ToString();
    }


    public static int GetExtraDataValue(CardActionDTO cardAction, int amount)
    {
        if (cardAction.ExtraData == null) return amount;

        if (cardAction.ExtraData.ContainsKey("value"))
        {
            var valueToken = cardAction.ExtraData["value"] as JObject ?? cardAction.ExtraData["value"];
            if (valueToken.ToString() == "play_instrument_time")
            {
                amount = PlayFunction.Instance ? PlayFunction.Instance.playTime : amount;
            }

        }

        return amount;
    }

    public static void SetCardAbilitys(GameObject cardModel, CardDTO cardDTO, bool isShowAbility = true, bool isLeft = true)
    {
        cardModel.transform.GetChild(6).gameObject.SetActive(isShowAbility);
        if (!isShowAbility) return;

        List<AbilityBoxData> abilityBoxList = new();

        // 맨처음 초기화
        for (int i = 0; i < cardModel.transform.GetChild(6).childCount; i++)
        {
            cardModel.transform.GetChild(6).GetChild(i).gameObject.SetActive(false);
        }

        // 카드 액션에 따라 능력 박스 데이터 설정
        for (int i = 0; i < cardDTO.CardActions.Count; i++)
        {
            var action = cardDTO.CardActions[i];

            // 소멸 관련 처리
            if (action.ExtraData != null && action.ExtraData.ContainsKey("ethereal"))
            {
                if (action.ExtraData["ethereal"].ToString() == "true")
                {
                    //
                    var ethereal = InGameData.Instance.Debuffs.Find(x => x.Id == 16);
                    AbilityBoxData abilityBoxData = new AbilityBoxData(ethereal.Name, ethereal.Description);
                    abilityBoxList.Add(abilityBoxData);
                }
            }

            // 악기 연주
            if (action.ExtraData != null && action.ExtraData.ContainsKey("play_instrument"))
            {
                var playInstrument = InGameData.Instance.Buffs.Find(x => x.Id == 17);
                AbilityBoxData abilityBoxData = new AbilityBoxData(playInstrument.Name, playInstrument.Description);
                abilityBoxList.Add(abilityBoxData);
            }

            // 버프, 디버프 관련 처리
            if (action.ActionType == EnumTypes.Action.buff || action.ActionType == EnumTypes.Action.debuff)
            {
                if (action.ExtraData != null && action.ExtraData.ContainsKey("get_status"))
                {
                    int statusId = int.Parse(action.ExtraData["get_status"].ToString());
                    var status = action.ActionType == EnumTypes.Action.buff ? InGameData.Instance.Buffs.Find(x => x.Id == statusId) : InGameData.Instance.Debuffs.Find(x => x.Id == statusId);

                    AbilityBoxData abilityBoxData = new AbilityBoxData(status.Name, status.Description);
                    abilityBoxList.Add(abilityBoxData);
                }

                else if (action.ExtraData != null && action.ExtraData.ContainsKey("condition") && action.ExtraData.ContainsKey("action"))
                {
                    var actionData = action.ExtraData["action"] as JObject;
                    if (actionData != null && actionData.ContainsKey("get_status"))
                    {
                        int statusId = int.Parse(actionData["get_status"].ToString());
                        var status = action.ActionType == EnumTypes.Action.buff ? InGameData.Instance.Buffs.Find(x => x.Id == statusId) : InGameData.Instance.Debuffs.Find(x => x.Id == statusId);

                        AbilityBoxData abilityBoxData = new AbilityBoxData(status.Name, status.Description);
                        abilityBoxList.Add(abilityBoxData);
                    }
                }
            }
        }

        // 카드 모델에 능력 박스 데이터 설정
        for (int i = 0; i < abilityBoxList.Count; i++)
        {
            cardModel.transform.GetChild(6).GetChild(i).gameObject.SetActive(true);
            string desc = abilityBoxList[i].abilityName + "\n" + abilityBoxList[i].abilityDesc;

            var tmp3D = cardModel.transform.GetChild(6).GetChild(i).GetChild(0).GetComponent<TextMeshPro>();
            var tmpUI = cardModel.transform.GetChild(6).GetChild(i).GetChild(0).GetComponent<TextMeshProUGUI>();

            if (tmp3D != null)
            {
                tmp3D.text = desc;
            }
            else if (tmpUI != null)
            {
                tmpUI.text = desc;
            }
            else
            {
                Debug.LogError("No TextMeshPro or TextMeshProUGUI component found in the card model.");
            }
        }
    }

    static string GetFramePath(int frameId)
    {
        var frame = InGameData.Instance.ShopItems.Find(x => x.ItemId == frameId);
        if (frame == null)
        {
            return "Image/Card/Frame/frame_card_public"; // 못 찾으면 기본 경로
        }
        return frame.ImgPath;
    }

    static string GetDecoPath(int decoId)
    {
        var deco = InGameData.Instance.ShopItems.Find(x => x.ItemId == decoId);
        if (deco == null || UserData.Instance.SelectDecoId == 2)
        {
            return "none"; // 못 찾으면 없음.
        }
        return deco.ImgPath;
    }
}

public class AbilityBoxData
{
    public string abilityName;
    public string abilityDesc;
    public AbilityBoxData(string name, string desc)
    {
        abilityName = name;
        abilityDesc = desc;
    }
}