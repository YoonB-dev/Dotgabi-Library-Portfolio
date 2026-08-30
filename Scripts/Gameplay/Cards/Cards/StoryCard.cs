using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

public class StoryCard : MonoBehaviour
{
    public PRS originPRS;
    List<RaycastResult> result = new List<RaycastResult>(); //모바일 터치 사용하고 있는거임!
    [SerializeField] private SpriteRenderer cardImage;
    [SerializeField] private TextMeshPro cardName, cardDesc;
    public float ratio = 1f;
    public float cardUpPosY = -2f;
    public bool doDrag = false;
    private float startPosX;
    private float startPosY;
    private bool canTouch = true;
    int originOrder;
    public MainStoryItemDTO storyCardData;
    private IStoryCardUse storyCardUse;
    void Awake()
    {
        if (StoryCardUseBattle.Instance != null) storyCardUse = StoryCardUseBattle.Instance;
        else if (StoryCardUseOnu.Instance != null) storyCardUse = StoryCardUseOnu.Instance;
        else Debug.LogWarning("No StoryCardUse instance found.");
    }
    public void SetCardInfo(MainStoryItemDTO cardDTO)
    {
        storyCardData = cardDTO;
        DTOToObjModel(cardDTO);
    }

    public void DTOToObjModel(MainStoryItemDTO cardDTO)
    {
        storyCardData = cardDTO;
        cardName.text = cardDTO.Name;
        cardDesc.text = cardDTO.Description;
        cardImage.sprite = Resources.Load<Sprite>(cardDTO.ImgPath);
    }
    public void MoveTransform(PRS prs, int isDo, float doTime = 0)
    {
        if (isDo == 1) {// 카드 정렬 하는데 사용
            transform.DOMove(prs.pos, doTime);
            transform.DORotateQuaternion(prs.rot, doTime);
            transform.DOScale(prs.scale, doTime);
            doDrag = false;
        }
        else if (isDo == 0) { //바로 적용
            transform.localScale = prs.scale;
            transform.position = prs.pos;
            transform.rotation = prs.rot;

        }
        else if (isDo == 2) {//카드 클릭시, scale 값 바로 커짐
            transform.DOMove(prs.pos, doTime);
            transform.DORotateQuaternion(prs.rot, doTime);
            transform.localScale = prs.scale;
        }
    }
    void Update()
    {
        if (!canTouch) return;
#if UNITY_EDITOR

        // 마우스 포인터 기반 Raycast 검사 (UI 터치 체크)
        PointerEventData ep = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };
        result.Clear();
        EventSystem.current.RaycastAll(ep, result);

        if (doDrag)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            this.gameObject.transform.position = new Vector3(mousePos.x - startPosX, mousePos.y - startPosY, transform.position.z);
        }
#else
        if(Input.touchCount>0)
        {
            Touch touch = Input.GetTouch(0);
            PointerEventData ep = new PointerEventData(EventSystem.current)
            {
                position = touch.position
            };

            EventSystem.current.RaycastAll(ep,result);

            if (doDrag)
            {
                Vector2 touchPos = Camera.main.ScreenToWorldPoint(touch.position);
                this.gameObject.transform.position = new Vector3(touchPos.x - startPosX, touchPos.y - startPosY, transform.position.z);
            }
        }
#endif
    }
    private void OnMouseDown()
    {
        if (!canTouch) return;
        if (!StoryCardManager.Instance.canDrag) return;

#if UNITY_EDITOR
        if (EventSystem.current.IsPointerOverGameObject())
            return;
#else
            if (result.Count > 0) return;
#endif

        //터치
        if (Input.GetMouseButtonDown(0) && doDrag == false)
        {
            StoryCardManager.Instance.canDrag = false;
            doDrag = true;

            //SFX
            AudioManager.Instance.ShowCardSound();

            Vector3 upPRS = new Vector3(originPRS.pos.x, originPRS.pos.y + 4.5f * ratio, 0);
            Vector2 upScale = new Vector2(3.5f, 3.5f) * ratio;

            transform.DOKill();
            MoveTransform(new PRS(upPRS, Utils.QI, upScale), 0);

            GetComponent<SortingGroup>().sortingLayerName = "CardUp";
            GetComponent<SortingGroup>().sortingOrder = 100;
            originOrder = GetComponent<SortingGroup>().sortingOrder;

            Vector3 mousePos;
            mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.rotation = Utils.QI;

            startPosX = mousePos.x - this.transform.position.x;
            startPosY = mousePos.y - this.transform.position.y;
            //this.transform.position = new Vector3(0, 0, -5f);
        }
    }
    void OnMouseDrag()
    {
        if (!canTouch) return;
        if (!doDrag) return;
        if (Camera.main.ScreenToWorldPoint(Input.mousePosition).y >= cardUpPosY)
        {
            SetAlpha(0.7f);
            transform.DOKill();
            transform.DOScale(new Vector2(2f, 2f) * ratio, 0.1f);
        }
        else
        {
            SetAlpha(1f);
            transform.DOKill();
            transform.DOScale(new Vector2(3.5f, 3.5f) * ratio, 0.1f);
        }

    }
    void OnMouseUp()
    {
        if (!canTouch) return;
        if (!doDrag) return;
        doDrag = false;
        StoryCardManager.Instance.canDrag = true;
        //var cardDataDTO = isChange ? this.changeCardData : this.cardData;

        if (Camera.main.ScreenToWorldPoint(Input.mousePosition).y >= cardUpPosY && StoryCardManager.Instance.canCardUse)
        {
            // MoveTransform(originPRS, 1, 0.2f);
            // this.GetComponent<SortingGroup>().sortingLayerName = "Card";
            //CardDTOToObj.SetCardAbilitys(this.gameObject, cardDataDTO, false);
            UseCard();
            return;
        }

        MoveOrigin();
    }

    void SetAlpha(float amount)
    {
        if (!StoryCardManager.Instance.canCardUse) return;

        Color color = transform.GetChild(0).GetComponent<SpriteRenderer>().color;
        color.a = amount;

        Color color2 = transform.GetChild(1).GetComponent<SpriteRenderer>().color;
        color2.a = amount;

        Color color3 = transform.GetChild(1).GetChild(0).GetComponent<SpriteRenderer>().color;
        color3.a = amount;

        transform.GetChild(0).GetComponent<SpriteRenderer>().color = color;
        transform.GetChild(1).GetComponent<SpriteRenderer>().color = color2;
        transform.GetChild(1).GetChild(0).GetComponent<SpriteRenderer>().color = color3;
    }

    //카드 사용
    void UseCard()
    {
        if (storyCardData == null)
        {
            Debug.LogError("No storyCardData found.");
            MoveOrigin();
            return;
        }
        //카드 능력 발동
        var success = storyCardUse?.OnCardUse(this);
        if (success != true)
        {
            if (success == null)
            {
                Debug.LogWarning("No StoryCardUseBattle instance found.");
            }
            else
            {
                Debug.Log("Card use was not successful.");
            }
            MoveOrigin();
            return;
        }
        //코루틴 시작 - 삭제
        StartCoroutine(DeleteObject());
        //SFX
        AudioManager.Instance.UseCardSound();
    }

    private void MoveOrigin()
    {
        MoveTransform(originPRS, 1, 0.2f);
        GetComponent<SortingGroup>().sortingLayerName = "Ui_Victory";
        GetComponent<SortingGroup>().sortingOrder = originOrder;
        SetAlpha(1f);
    }
    public IEnumerator DeleteObject()
    {
        canTouch = false;
        int index = StoryCardManager.Instance.cards.FindIndex(a => a.gameObject == this.gameObject);

        StoryCardManager.Instance.cards.Remove(this.gameObject);
        StoryCardManager.Instance.CardAlignment(2);

        transform.DOKill();
        transform.DOJump(StoryCardManager.Instance.cardDeletePos.transform.position, 10f, 1, 0.5f);
        transform.DORotate(new Vector3(0, 0, 180), 0.5f);
        transform.DOScale(Vector2.zero, 0.5f);
        yield return new WaitForSecondsRealtime(0.5f);

        Destroy(gameObject);
        yield return null;
    }
}
