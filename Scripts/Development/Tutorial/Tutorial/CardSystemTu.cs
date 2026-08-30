using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class CardSystemTu : SceneSingleton<CardSystemTu>
{
    [SerializeField] private GameObject cardPrefab, cardPrefabUI;
    [Header("----------[Card Tranform]----------")]
    [SerializeField] Transform cardSpownPoint;
    [SerializeField] Transform cardLeft, cardRight;
    [SerializeField] Transform cardUIPos;
    [SerializeField] Transform cardGroupPos;
    [Header("----------[Manage]----------")]
    public bool isFinish = false;
    public bool canActive = true; // 행동 가능 관리
    public bool canDrag = false; // 드래그 가능 관리
    public int maxCardNum = 6;
    [Header("----------[Card List]----------")]
    public List<GameObject> cards; // 카드 오브젝트 리스트
    public List<CardDTO> canCards; // 덱 리스트
    public List<CardDTO> handCards; // 핸드 리스트
    public List<CardDTO> usedCards; // 사용 리스트
    [Header("----------[Card Txt]----------")]
    public TMP_Text useCardsTxt;
    public TMP_Text canCardsTxt;
    [Header("---------[Canvas]---------")]
    public float ratio = 1;
    [Header("----------[ETC]----------")]
    private ScenarioDTO SCENARIO_DATA;
    [SerializeField] private Player player;
    [SerializeField] public TutorialBattle tuBattle;

    void Start()
    {
        ratio = ButtonAnim.Instance.ratio;
    }
    public void SetCard(ScenarioDTO scenarioData)
    {
        SCENARIO_DATA = scenarioData;
        cards = new List<GameObject>();
        canCards = new();
        handCards = new();
        usedCards = new();

        // 핸드 카드 개수
        maxCardNum += SCENARIO_DATA.OwnedArtifactList.Find(x => x.ArtifactId == 10) != null ? 1 : 0;

        //Set the canCard
        for (int i = 0; i < SCENARIO_DATA.OwnedCardList.Count; i++)
        {
            var cardData = InGameData.Instance.Cards.Find(c => c.Id == SCENARIO_DATA.OwnedCardList[i].CardId).Copy();

            if (cardData == null) { Debug.LogError("Card not found: " + SCENARIO_DATA.OwnedCardList[i].CardId); continue; }
            cardData.CardUpgrade = SCENARIO_DATA.OwnedCardList[i].UpgradeTime;

            canCards.Add(cardData);
        }

        useCardsTxt.text = "0";
        canCardsTxt.text = canCards.Count.ToString();

        SuffleDeck();
    }

    public void AddCard(int upgrade = 0)
    {

        if (canCards.Count <= 0) return;
        // SFX
        AudioManager.Instance.DrawCardSound(false);

        var cardObject = Instantiate(cardPrefab, cardSpownPoint.position, Utils.QI, cardGroupPos);
        cardObject.GetComponent<CardTu>().ratio = ratio;
        cards.Add(cardObject);
        cards[cards.Count - 1].GetComponent<CardTu>().cardData = canCards[0].Copy();


        //카드 데이터 설정
        handCards.Add(canCards[0]);

        //카드 손 갯수 확인 코드
        if (handCards.Count <= maxCardNum && canCards[0].CardType != EnumTypes.CardType.curse)
        {
            CardAlignment(1);
        }
        else
        {
            CardMax(cardObject);
        }

        canCards.RemoveAt(0);
        canCardsTxt.text = canCards.Count.ToString();
    }
    public void CardAlignment(int type)
    {
        List<PRS> cardPRSs;
        float newScale = 1.5f * ratio;
        cardPRSs = RoundAlignment(cardLeft, cardRight, cards.Count, 0.5f, new Vector3(newScale, newScale, newScale));
        for (int i = 0; i < cards.Count; i++)
        {
            var card = cards[i].GetComponent<CardTu>();
            card.originPRS = cardPRSs[i];
            card.ratio = ratio;
            if (ratio > 0) card.cardUpPosY = -2 * (1 / ratio);
            card.MoveTransform(cardPRSs[i], type, 0.7f);//Card Move
            int num = i * 10;
            cards[i].GetComponent<SortingGroup>().sortingOrder = num;

            card.SetDescription();
        }
    }
    List<PRS> RoundAlignment(Transform left, Transform right, int objCount, float height, Vector3 scale)
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
    public void CardMax(GameObject card)
    {
        card.GetComponent<SortingGroup>().sortingLayerName = "CardUp";
        card.GetComponent<SortingGroup>().sortingOrder = 10;
        card.transform.DOMove(Vector3.zero, 0.5f);
        card.transform.DOScale(new Vector3(2f, 2f, 2f), 0.5f);
        StartCoroutine(card.GetComponent<CardTu>().DeleteObject(true));
        cards.Remove(card);
        CurseCardCheck(card.GetComponent<CardTu>().cardData);
    }

    private void CurseCardCheck(CardDTO card)
    {
        if (card == null || card.CardType != EnumTypes.CardType.curse) return;

        if (card.CardActions[0] != null && card.CardActions[0].ExtraData != null && card.CardActions[0].ExtraData.ContainsKey("curse_condition") && card.CardActions[0].ExtraData["curse_condition"].ToString() == "on_draw")
        {
            // 저주 카드의 조건이 curse이고 draw조건 일 경우
            Debug.Log("저주 카드가 드로우 조건으로 사용되었습니다: " + card.Name);
            for (int i = 0; i < card.CardActions.Count; i++)
            {
                var blockCurse = ArtifactFunction.Instance.ArtifactBlockCurse(player, null);
                if (blockCurse != null && blockCurse.IsBlockCurse)
                {
                    Debug.Log("저주 카드가 막혔습니다: " + card.Name);
                    break; // 저주 카드가 막혔으므로 더 이상 처리하지 않음
                }
                CardFunction.Instance.AbilityFunction(card, card.CardActions[i], i);

            }
            // 저주 카드 드로우 시 효과 적용
            ArtifactFunction.Instance.ArtifactCardDrawCurse(player, null);
        }

    }

    public IEnumerator DrawCard(int drawCount, int upgrade = 0)
    {
        if (isFinish || player.isDie) yield break;

        canDrag = false;
        canActive = false;

        for (int i = 0; i < drawCount; i++)
        {
            if (canCards.Count == 0) { yield return StartCoroutine(UseToCan()); }

            //장착카드 효과 (뽑는 카드 업그레이드 확률)
            upgrade = Mathf.Max(EquipmentFunction.Instance.UpgradeCardWhenDraw(player), upgrade);

            AddCard(upgrade);
            upgrade = 0;
            yield return new WaitForSecondsRealtime(0.5f);
            if (handCards.Count >= maxCardNum) { yield return new WaitForSecondsRealtime(1f); }
        }
        yield return new WaitForSecondsRealtime(0.1f);
        canDrag = true;
        canActive = true;
    }

    //손에 들고있는 카드 원위치
    public void CardReSetAll()
    {
        for (int i = 0; i < handCards.Count; i++)
        {
            cards[i].GetComponent<CardTu>().doDrag = false;
            cards[i].GetComponent<CardTu>().MoveTransform(cards[i].GetComponent<CardTu>().originPRS, 1, 0.2f);
        }
    }

    public IEnumerator UseToCan()
    {
        List<CardDTO> tmp = new(usedCards);
        usedCards = new();
        useCardsTxt.text = usedCards.Count.ToString();
        canCards = new(tmp);
        canCardsTxt.text = canCards.Count.ToString();
        //모션 넣기
        for (int i = 0; i < canCards.Count; i++)
        {
            int ran1 = Random.Range(0, canCards.Count);
            int ran2 = Random.Range(0, canCards.Count);

            var temp = canCards[ran1];
            canCards[ran1] = canCards[ran2];
            canCards[ran2] = temp;
        }
        //yield return new WaitForSecondsRealtime(0.7f);
        //모션
        Debug.Log("카드 들어가는 모션");
        yield return null;
    }
    public void SuffleDeck(int sd = 0)
    {
        int seed = SCENARIO_DATA.GenerateSeed * (SCENARIO_DATA.SelectList.Count + SCENARIO_DATA.CurrStageLevel) + sd;
        System.Random random = new(seed);
        //처음 카드 셔플
        for (int i = 0; i < canCards.Count; i++)
        {
            int ran1 = random.Next(0, canCards.Count);
            int ran2 = random.Next(0, canCards.Count);

            (canCards[ran2], canCards[ran1]) = (canCards[ran1], canCards[ran2]);
        }
    }
    public IEnumerator SetEnemySelectImg()
    {
        yield return null;
        yield return new WaitForSecondsRealtime(0.01f);
        for (int i = 0; i < EnemyManagerTu.Instance.enemies.Count; i++)
        {
            EnemyManagerTu.Instance.enemies[i].GetComponent<EnemyTu>().SetSelectImage(false);
        }
    }


    // 카드 복사
    public void CopyCard(CardDTO cardData, int count)
    {
        if (cardData == null) return;

        canCards.Insert(0, cardData.Copy());
        StartCoroutine(DrawCard(count));
    }

    // 카드 전체 비활성화
    public void CardGroupSetActive(bool isActive)
    {
        cardGroupPos.gameObject.SetActive(isActive);
    }
}
