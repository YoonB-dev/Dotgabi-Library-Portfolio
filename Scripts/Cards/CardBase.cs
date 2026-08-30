using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

/// <summary>
/// 카드 기본 클래스
/// - 터치 및 드래그 기능 구현
/// - 카드 이동 및 회전 애니메이션
/// </summary>
public class CardBase : MonoBehaviour
{
    public PRS originPRS;
    private float startPosX;
    private float startPosY;
    public bool doDrag = false;
    private bool canTouch = true;
    public float ratio = 1f;
    public bool isChange = false; // 카드가 변이 카드인지 여부
    List<RaycastResult> result = new List<RaycastResult>();//모바일 터치 사용하고 있는거임!
    [SerializeField] private SpriteRenderer cardImage;
    [SerializeField] private TextMeshPro cardName, cardDesc;
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

            GetComponent<SortingGroup>().sortingLayerName = "CardUp";
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
        }
    }

    public void MoveTransform(PRS prs, int isDo, float doTime = 0)
    {
        if (isDo == 1) {// 카드 정렬 하는데 사용
            transform.DOMove(prs.pos, doTime);
            transform.DORotateQuaternion(prs.rot, doTime);
            transform.DOScale(prs.scale, doTime);
            doDrag = false;
            SetAlpha(1f);
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
    void SetAlpha(float amount)
    {
        // Color color = cardImage.color;
        // color.a = amount;

        // Color color2 = transform.GetChild(1).GetComponent<SpriteRenderer>().color;
        // color2.a = amount;

        // Color color3 = transform.GetChild(5).GetComponent<SpriteRenderer>().color;
        // color3.a = amount;

        // transform.GetChild(0).GetComponent<SpriteRenderer>().color = color;
        // transform.GetChild(1).GetComponent<SpriteRenderer>().color = color2;
        // transform.GetChild(5).GetComponent<SpriteRenderer>().color = color3;
    }
}
