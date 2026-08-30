using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;
using System.Linq;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.Localization;

public class Card : MonoBehaviour
{
    public PRS originPRS;
    private float startPosX;
    private float startPosY;
    public bool doDrag = false;
    private bool canTouch = true;
    public CardDTO cardData;
    int originOrder;
    List<RaycastResult> result = new List<RaycastResult>();//모바일 터치 사용하고 있는거임!
    public GameObject cardAbs; // 카드 설명들 부모 오브젝트

    //카드 변경되면 true, 원래 카드 데이터에는 변화 주지 않음.
    public bool isChange = false;
    public CardDTO changeCardData = null;
    List<string> cardAbilitys = new();
    List<string> newCardAbilitys = new();
    public float ratio = 1;
    public float cardUpPosY = -2f;
    void Start()
    {
        for (int i = 0; i < this.cardData.CardUpgrade; i++)
        {
            this.transform.GetChild(4).GetComponent<TextMeshPro>().text += "+";
            this.transform.GetChild(4).GetComponent<TextMeshPro>().color = Color.green;
        }
        SetCardInfo();
        SetDescription();
        //cardAbilitys = GameManager.gameManager.SetCardAbs(this.cardData.cardActions);
    }
    void Update()
    {
        if(!canTouch)return;
#if UNITY_EDITOR
        if (doDrag && CardSystem.Instance.canActive && BattleManager.Instance.player.isTurn)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            this.gameObject.transform.position = new Vector3(mousePos.x - startPosX,mousePos.y-startPosY,transform.position.z);
        }
# else
        if(Input.touchCount>0)
        {
            Touch touch = Input.GetTouch(0);
            PointerEventData ep = new PointerEventData(EventSystem.current)
            {
                position = touch.position
            };

            EventSystem.current.RaycastAll(ep,result);

            if (doDrag && CardSystem.Instance.canActive && BattleManager.Instance.player.isTurn)
            {
                Vector2 touchPos = Camera.main.ScreenToWorldPoint(touch.position);
                this.gameObject.transform.position = new Vector3(touchPos.x - startPosX, touchPos.y - startPosY, transform.position.z);
            }
        }
#endif
    }

    public void MoveTransform(PRS prs, int isDo, float doTime = 0)
    {
        if (isDo==1){// 카드 정렬 하는데 사용
            transform.DOMove(prs.pos,doTime);
            transform.DORotateQuaternion(prs.rot,doTime);
            transform.DOScale(prs.scale,doTime);
            doDrag = false;
            SetAlpha(1f);
        }
        else if(isDo==0){ //바로 적용
            transform.localScale = prs.scale;
            transform.position = prs.pos;
            transform.rotation = prs.rot;

        }
        else if(isDo==2){//카드 클릭시, scale 값 바로 커짐
            transform.DOMove(prs.pos, doTime);
            transform.DORotateQuaternion(prs.rot, doTime);
            transform.localScale = prs.scale;
        }
    }
    private void OnMouseDown()
    {
        if (!canTouch) return;
        if (!CardSystem.Instance.canDrag || !CardSystem.Instance.canActive) return;

#if UNITY_EDITOR
        if (EventSystem.current.IsPointerOverGameObject())
                return;
#else
            if (result.Count > 0) return;
#endif

        //터치
        if (Input.GetMouseButtonDown(0) && doDrag == false)
        {
            CardSystem.Instance.canDrag = false;
            doDrag = true;

            //SFX
            AudioManager.Instance.ShowCardSound();

            Vector3 upPRS = new Vector3(originPRS.pos.x, originPRS.pos.y + 4.5f * ratio, 0);
            Vector2 upScale = new Vector2(3.5f, 3.5f) * ratio;

            transform.DOKill();
            MoveTransform(new PRS(upPRS, Utils.QI, upScale), 0);

            this.GetComponent<SortingGroup>().sortingLayerName = "CardUp";
            originOrder = this.GetComponent<SortingGroup>().sortingOrder;
            GetComponent<SortingGroup>().sortingOrder = 100;

            if (BattleManager.Instance.player.isTurn)
            {
                Vector3 mousePos;
                mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                transform.rotation = Utils.QI;

                startPosX = mousePos.x - this.transform.position.x;
                startPosY = mousePos.y - this.transform.position.y;
                this.transform.position = new Vector3(0, 0, -5f);
            }
            var cardDataDTO = isChange ? this.changeCardData : this.cardData;
            CardDTOToObj.SetCardAbilitys(this.gameObject, cardDataDTO);
        }
    }
    void OnMouseDrag()
    {
        if (!canTouch) return;
        if (!doDrag || !CardSystem.Instance.canActive) return;
        if (Camera.main.ScreenToWorldPoint(Input.mousePosition).y >= cardUpPosY && BattleManager.Instance.player.isTurn)
        {
            SetAlpha(0.7f);
            switch (EnemyManager.Instance.enemies.Count())
            {
                //적 타겟하는 코드들 왜 이렇게 짜뒀냐.... 암튼 작동함
                case 1:
                    if (CardFunction.Instance.selectEnemyObj != EnemyManager.Instance.enemies[0].gameObject)
                    {
                        CardFunction.Instance.selectEnemyObj = EnemyManager.Instance.enemies[0].gameObject;
                    }
                    break;
                case 2:
                    if (Camera.main.ScreenToWorldPoint(Input.mousePosition).x < 0 && CardFunction.Instance.selectEnemyObj != EnemyManager.Instance.enemies[0].gameObject)
                    {
                        CardFunction.Instance.selectEnemyObj = EnemyManager.Instance.enemies[0].gameObject;
                    }
                    else if (Camera.main.ScreenToWorldPoint(Input.mousePosition).x >= 0 && CardFunction.Instance.selectEnemyObj != EnemyManager.Instance.enemies[1].gameObject)
                    {
                        CardFunction.Instance.selectEnemyObj = EnemyManager.Instance.enemies[1].gameObject;
                    }
                    break;
                case 3:
                    if (Camera.main.ScreenToWorldPoint(Input.mousePosition).x < -2 && CardFunction.Instance.selectEnemyObj != EnemyManager.Instance.enemies[0].gameObject)
                    {
                        CardFunction.Instance.selectEnemyObj = EnemyManager.Instance.enemies[0].gameObject;
                    }
                    else if (Camera.main.ScreenToWorldPoint(Input.mousePosition).x >= -2 && Camera.main.ScreenToWorldPoint(Input.mousePosition).x <= 2 && CardFunction.Instance.selectEnemyObj != EnemyManager.Instance.enemies[1].gameObject)
                    {
                        CardFunction.Instance.selectEnemyObj = EnemyManager.Instance.enemies[1].gameObject;
                    }
                    else if (Camera.main.ScreenToWorldPoint(Input.mousePosition).x > 2 && CardFunction.Instance.selectEnemyObj != EnemyManager.Instance.enemies[2].gameObject)
                    {
                        CardFunction.Instance.selectEnemyObj = EnemyManager.Instance.enemies[2].gameObject;
                    }
                    break;
            }
            SetDescription(true);
            //적 머리위에 화살표
            for (int i = 0; i < EnemyManager.Instance.enemies.Count; i++)
            {
                if (CardFunction.Instance.selectEnemyObj == EnemyManager.Instance.enemies[i].gameObject && (cardData.CardType == EnumTypes.CardType.attack || cardData.CardType == EnumTypes.CardType.action))
                {
                    EnemyManager.Instance.enemies[i].GetComponent<Enemy>().SetSelectImage(true);
                }
                else
                {
                    EnemyManager.Instance.enemies[i].GetComponent<Enemy>().SetSelectImage(false);
                }
            }
            transform.DOKill();
            transform.DOScale(new Vector2(2f, 2f) * ratio, 0.1f);
        }
        else
        {
            SetAlpha(1f);
            if (CardFunction.Instance.selectEnemyObj != null)
            {
                transform.DOKill();
                transform.DOScale(new Vector2(3.5f, 3.5f) * ratio, 0.1f);
                CardFunction.Instance.selectEnemyObj = null;
                SetDescription();
                Debug.Log("카드 설명 닫힘");
            }
            for (int i = 0; i < EnemyManager.Instance.enemies.Count; i++)
            {
                EnemyManager.Instance.enemies[i].GetComponent<Enemy>().SetSelectImage(false);
            }
        }

    }
    void OnMouseUp()
    {
        if (!canTouch) return;
        if (!CardSystem.Instance.canActive || !doDrag) return;
        doDrag = false;
        for (int i = 0; i < EnemyManager.Instance.enemies.Count; i++)
        {
            EnemyManager.Instance.enemies[i].GetComponent<Enemy>().SetSelectImage(false);
        }

        var cardDataDTO = isChange ? this.changeCardData : this.cardData;

        if (Camera.main.ScreenToWorldPoint(Input.mousePosition).y >= cardUpPosY && BattleManager.Instance.player.isTurn)
        {
            //튜토리얼 체크
            if (CardSystem.Instance.GetIsTutorial())
            {
                CardDTO cardDataRe = cardDataDTO.Copy();
                UseCard(cardDataRe);
                return;
            }

            if (BattleManager.Instance.player.Job == EnumTypes.JobType.Performer && MaskFunction.Instance.currMaskIndex == (int)EnumTypes.PerformerMaskType.bang)
            {
                int amount = 3; // 체력대비 코스트
                if (BattleManager.Instance.player.Stats.currHp > cardDataDTO.Cost[cardDataDTO.CardUpgrade] * amount)
                {
                    CardDTO cardDataRe = cardDataDTO.Copy();
                    UseCard(cardDataRe);
                    BattleManager.Instance.player.GetDamagePure(cardDataDTO.Cost[cardDataDTO.CardUpgrade] * amount);
                }
                else
                {
                    string str = new LocalizedString("LocalTable", "Hp-Less").GetLocalizedString();
                    NotificationManager.Instance.SetCheckNotification(str);
                    MoveTransform(originPRS, 1, 0.2f);
                    this.GetComponent<SortingGroup>().sortingLayerName = "Card";
                    AfterUserCard();
                    SetDescription(false);
                }
                CardDTOToObj.SetCardAbilitys(this.gameObject, cardDataDTO, false);
                return;
            }
            //코스트 체크
            int cost = cardDataDTO.CardUpgrade < 2 ? cardDataDTO.Cost[cardDataDTO.CardUpgrade] : cardDataDTO.Cost[2];
            if (cost <= BattleManager.Instance.player.currAction)
            {
                CardDTO cardDataRe = cardDataDTO.Copy();
                BattleManager.Instance.player.currAction -= cost;
                UseCard(cardDataRe);
            }
            else
            {
                string str = new LocalizedString("LocalTable", "Action-Less").GetLocalizedString();
                NotificationManager.Instance.SetCheckNotification(str);
                MoveTransform(originPRS, 1, 0.2f);
                this.GetComponent<SortingGroup>().sortingLayerName = "Card";
                SetDescription(false);
            }
        }
        else
        {
            MoveTransform(originPRS, 1, 0.2f);
            this.GetComponent<SortingGroup>().sortingLayerName = "Card";
            SetDescription(false);
        }
        CardDTOToObj.SetCardAbilitys(this.gameObject, cardDataDTO, false);
        AfterUserCard();
    }
    private void AfterUserCard()
    {
        CardFunction.Instance.selectEnemyObj = null;
        GetComponent<SortingGroup>().sortingOrder = originOrder;
        SetAlpha(1f);
        CardSystem.Instance.canDrag = true;
        CardSystem.Instance.StartCoroutine(CardSystem.Instance.SetEnemySelectImg());

        for (int i = 0; i < cardAbilitys.Count; i++)
        {
            var targetObj = cardAbs.transform.GetChild(i).gameObject;
            targetObj.SetActive(false);
        }
    }

    //카드 사용
    void UseCard(CardDTO cardDataRe)
    {
        //코루틴 시작 - 삭제
        StartCoroutine(DeleteObject());
        //카드 능력 발동
        CardFunction.Instance.CardAbility(cardDataRe);

        if (CardSystem.Instance.GetIsTutorial())
        {
            if (CardSystem.Instance.tuBattle.tutorialIndex == 4)
            {
                CardSystem.Instance.tuBattle.tutorialIndex++;
                CardSystem.Instance.tuBattle.SetTutorial();
            }
            return;
        }

        var player = BattleManager.Instance.player;
        player.GetComponent<Player>().SetActionText();
        //SFX
        AudioManager.Instance.UseCardSound();

        if (player.Job == EnumTypes.JobType.Performer && MaskFunction.Instance.currMaskIndex == 1)
        {
            player.GetShield(3);
        }
        if (player != null && player.CheckHaveBuffOrDebuff(EnumTypes.Status.debuff, 13))
        {
            // 밥풀 디버프
            player.SetBuffOrDebuffValue(EnumTypes.Status.debuff, 13, -1);
        }

        // 카드 사용 업적 수치 올리기
        BattleManager.Instance.AddUseCardCount(1);
    }

    public IEnumerator DeleteObject(bool isMax = false)
    {
        canTouch = false;
        int index = CardSystem.Instance.cards.FindIndex(a=>a.gameObject==this.gameObject);
        //int index = CardSystem.Inst.handCards.FindIndex(a=>a==cardData);
        if((this.cardData.CardType != EnumTypes.CardType.equip && this.cardData.CardType != EnumTypes.CardType.ethreal ) || (this.cardData.CardType == EnumTypes.CardType.equip && isMax) || (this.cardData.CardType == EnumTypes.CardType.ethreal && isMax))
        {
            //CardSystem.Inst.usedCards.Add(this.cardData); //강화 수치 유지
            if (this.cardData.CardActions[0]?.ExtraData != null && this.cardData.CardActions[0].ExtraData.ContainsKey("ethereal"))
            {
                Debug.Log("Ethereal Card Deleted");
            }
            else
            {
                CardSystem.Instance.usedCards.Add(CardSystem.Instance.handCards[index]); //강화 수치 삭제
            }
        }
        CardSystem.Instance.cards.Remove(this.gameObject);
        CardSystem.Instance.CardAlignment(2);
        CardSystem.Instance.handCards.RemoveAt(index);

        if (isMax) yield return new WaitForSecondsRealtime(1f);
        transform.DOKill();
        transform.DOJump(CardSystem.Instance.useCardsTxt.transform.position, 10f, 1, 0.5f);
        transform.DORotate(new Vector3(0, 0, 180), 0.5f);
        transform.DOScale(Vector2.zero, 0.5f);
        yield return new WaitForSecondsRealtime(0.5f);
        CardSystem.Instance.useCardsTxt.text = CardSystem.Instance.usedCards.Count.ToString();

        if(!isMax)
        {
            CardSystem.Instance.canDrag = true;
            CardSystem.Instance.canActive = true;
        }
        Destroy(gameObject);
        yield return null;
    }
    public IEnumerator UpgradeCard()
    {
        //수치 적용
        cardData = CardUpgradeUtils.Instance.ShowUpgradeCard(cardData);
        this.transform.DOScale(new Vector3(2,2,2),0.2f).SetLoops(2,LoopType.Yoyo);
        SetDescription();
        yield return null;
    }

    public IEnumerator ChangeCard(string type, int cardIndex = 0)
    {
        if (type == "Origin")
        {
            isChange = false;
        }else
        {
            isChange = true;

            if (type == "Dosa")
            {
                this.changeCardData = InGameData.Instance.Cards.Find(c => c.Id == cardIndex).Copy();
            }
            //newCardAbilitys = GameManager.gameManager.SetCardAbs(changeCardData.cardActions);
            //Debug.Log(newCardAbilitys.Count);
        }

        Debug.Log("fuck");

        this.transform.DOScale(new Vector3(2, 2, 2), 0.2f).SetLoops(2, LoopType.Yoyo);
        SetDescription();
        yield return null;
    }
    public void SetDescription(bool enemyTarget = false)
    {
        if (enemyTarget && CardFunction.Instance.selectEnemyObj != null)
        {
            SetNewDescription();
        }
        else
        {
            if (isChange)
            {
                CardDTOToObj.DTOToObjModel(this.gameObject, changeCardData);
            }
            else
            {
                CardDTOToObj.DTOToObjModel(this.gameObject, cardData);
            }

        }
    }
    public void SetCardInfo()
    {
        var targetData = isChange ? changeCardData : cardData;
        CardDTOToObj.DTOToObjModel(this.gameObject, targetData);
    }
    void SetAlpha(float amount)
    {
        Color color = transform.GetChild(0).GetComponent<SpriteRenderer>().color;
        color.a = amount;

        Color color2 = transform.GetChild(1).GetComponent<SpriteRenderer>().color;
        color2.a = amount;

        Color color3 = transform.GetChild(5).GetComponent<SpriteRenderer>().color;
        color3.a = amount;

        transform.GetChild(0).GetComponent<SpriteRenderer>().color = color;
        transform.GetChild(1).GetComponent<SpriteRenderer>().color = color2;
        transform.GetChild(5).GetComponent<SpriteRenderer>().color = color3;
    }


    // 적 타겟 시 Status에 따라 설명 변경
    private void SetNewDescription()
    {
        var newCardDTO = isChange ? this.changeCardData : this.cardData.Copy();
        var origin = InGameData.Instance.Cards.Find(x => x.Id == newCardDTO.Id);
        for (int i = 0; i < newCardDTO.CardActions.Count; i++)
        {
            if (newCardDTO.CardActions[i].ActionType == EnumTypes.Action.attack)
            {
                int originDamage = DamageCal.GetUnLimitcard(newCardDTO.CardActions[i], newCardDTO.CardUpgrade);
                int newDamage = DamageCal.AttackDamageCal(BattleManager.Instance.player, CardFunction.Instance.selectEnemyObj.GetComponent<CharacterBase>(), originDamage, true, null);
                // 강화 제한 없는 카드 때문에 이렇게 나눔
                if (newCardDTO.CardUpgrade > 2 )
                {
                    newCardDTO.CardActions[i].Value[2] = newDamage;
                }else
                {
                    newCardDTO.CardActions[i].Value[newCardDTO.CardUpgrade] = newDamage;
                }
                CardDTOToObj.DTOToObjModel(this.gameObject, newCardDTO);
            }
        }
    }
}
