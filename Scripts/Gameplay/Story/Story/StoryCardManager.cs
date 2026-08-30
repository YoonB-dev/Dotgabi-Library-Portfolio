using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class StoryCardManager : SceneSingleton<StoryCardManager>
{
    [SerializeField] private GameObject cardPrefab;
    [Header("----------[Card Tranform]----------")]
    [SerializeField] Transform cardSpownPoint;
    [SerializeField] Transform cardLeft, cardRight;
    [SerializeField] Transform cardGroupPos;
    [SerializeField] Transform cardUIPos;
    public Transform cardDeletePos;
    [Header("----------[Card List]----------")]
    public List<GameObject> cards; // 카드 오브젝트 리스트
    float ratio = 1f;
    public bool canDrag = true;
    public bool canCardUse = false; // 카드 사용 여부
    void Start()
    {
        ratio = ButtonAnim.Instance.ratio;
    }
    public StoryCard AddCard(MainStoryItemDTO itemDTO)
    {
        //SFX
        AudioManager.Instance.DrawCardSound(false);

        var card = Instantiate(cardPrefab, cardSpownPoint.position, Quaternion.identity, cardGroupPos);
        cards.Add(card);
        CardAlignment(1);
        Debug.Log("Add Card: " + itemDTO.Name);
        var storyCard = card.GetComponent<StoryCard>();
        if (storyCard == null)
        {
            Debug.LogError("StoryCard component not found on the instantiated card prefab.");
            return null;
        }

        storyCard.SetCardInfo(itemDTO);
        return storyCard;
    }
    public void CardAlignment(int type)
    {
        List<PRS> cardPRSs;
        float newScale = 1.5f * ratio;
        cardPRSs = RoundAlignment(cards.Count, 0.5f, new Vector3(newScale, newScale, newScale));
        for (int i = 0; i < cards.Count; i++)
        {
            var card = cards[i].GetComponent<StoryCard>();
            card.originPRS = cardPRSs[i];
            card.ratio = ratio;
            if (ratio > 0) card.cardUpPosY = -2 * (1 / ratio);
            card.MoveTransform(cardPRSs[i], type, 0.7f);//Card Move
            int num = i * 10;
            cards[i].GetComponent<SortingGroup>().sortingOrder = num + 1;
        }
    }
    List<PRS> RoundAlignment(int objCount, float height, Vector3 scale)
    {
        float[] objLerps = new float[objCount];
        List<PRS> result = new List<PRS>(objCount);

        switch (objCount)
        {
            case 1: objLerps = new float[] { 0.5f }; break;
            case 2: objLerps = new float[] { 0.33f, 0.66f }; break;
            case 3: objLerps = new float[] { 0.2f, 0.5f, 0.8f }; break;
            default:
                float interval = 1f / (objCount - 1);
                for (int i = 0; i < objCount; i++)
                    objLerps[i] = interval * i;
                break;
        }

        for (int i = 0; i < objCount; i++)
        {
            var targetPos = Vector3.Lerp(cardLeft.position, cardRight.position, objLerps[i]);
            var targetRot = Utils.QI;
            if (objCount >= 4)
            {
                float curve = Mathf.Sqrt(Mathf.Pow(height, 2) - Mathf.Pow(objLerps[i] - 0.5f, 2));
                if (curve == 0) curve = 0.2f;
                targetPos.y += curve;
                targetRot = Quaternion.Slerp(cardLeft.rotation, cardRight.rotation, objLerps[i]);
            }
            result.Add(new PRS(targetPos, targetRot, scale));
        }
        return result;
    }

    public void DeleteAllCards()
    {
        cardGroupPos.gameObject.SetActive(false);
    }
}
