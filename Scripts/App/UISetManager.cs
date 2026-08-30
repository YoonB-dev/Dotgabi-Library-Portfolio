using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UISetManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField]
    private CanvasScaler canvasScaler;
    [Header("UI Elements - size (button...)")]
    [SerializeField]
    private RectTransform[] uiElements;
    [Header("UI Elements - scale (panel...)")]
    [SerializeField]
    private RectTransform[] uiElements_scale;
    [Header("UI Elements - scale (background...) // can be used for scaling up")]
    [SerializeField]
    private RectTransform[] uiElements_scale_up;
    [Header("UI Elements - grids (content...)")]
    [SerializeField]
    private RectTransform[] uiGrids;

    [SerializeField]
    private bool isCal = false;
    private void OnEnable()
    {
        if(isCal)
        {
            return;
        }
        // UI 요소들의 크기 조정
        foreach (RectTransform uiElement in uiElements)
        {
            ScaleUIElement(uiElement);
        }
        // UI 요소들의 스케일 조정
        foreach (RectTransform uiElement in uiElements_scale)
        {
            ScaleSet(uiElement,false);
        }
        // 그리드의 크기 조정
        foreach (RectTransform gridParent in uiGrids)
        {
            StartCoroutine(ScaleGrid(gridParent));
            //ScaleGrid2(gridParent);
        }
        // 배경의 크기 조정
        foreach (RectTransform uiElement in uiElements_scale_up)
        {
            ScaleSet(uiElement,true);
        }

        isCal = true;
    }


    //UI 사이즈 관련 코드 - 버튼이나 패널 등 UI 요소의 크기 조정
    public void ScaleUIElement(RectTransform uiElement)
    {
        if (uiElement == null)
        {
            return;
        }

        // 해상도 비율 계산
        float screenRatio = (float)Screen.width / Screen.height;
        float referenceRatio = canvasScaler.referenceResolution.x / canvasScaler.referenceResolution.y;

        // 가로와 세로 비율 중 작은 값을 기준으로 스케일링
        float scaleFactor = Mathf.Min(screenRatio / referenceRatio, 1f); // 최대 1배로 제한

        // UI 요소의 크기 조정
        uiElement.sizeDelta = new Vector2(
            uiElement.sizeDelta.x * scaleFactor,
            uiElement.sizeDelta.y
        );
    }

    //UI 사이즈 관련 코드 - 스케일
    public void ScaleSet(RectTransform targetRectTransform, bool canScaleUp)
    {
        // 기존 scale 값
        float scaleX = targetRectTransform.localScale.x;
        // 해상도 비율 계산
        float screenRatio = (float)Screen.width / Screen.height;
        float referenceRatio = canvasScaler.referenceResolution.x / canvasScaler.referenceResolution.y;

        // 가로와 세로 비율 중 작은 값을 기준으로 스케일링
        float scaleFactor;
        if (!canScaleUp)
        {
            scaleFactor = Mathf.Min(screenRatio / referenceRatio, 1f); // 최대 1배로 제한
        }else
        {
            scaleFactor = screenRatio / referenceRatio;
        }


        // 스케일 조정
        targetRectTransform.localScale = new Vector3(scaleX * scaleFactor, scaleX * scaleFactor, 1f);
    }

    //UI 사이즈 관련 코드 - 스케일
    public IEnumerator ScaleGrid(RectTransform gridParent)
    {
        yield return null;
        // 해상도 비율 계산
        float screenRatio = (float)Screen.width / Screen.height;
        float referenceRatio = canvasScaler.referenceResolution.x / canvasScaler.referenceResolution.y;

        // 그리드의 스케일 조정
        float scaleFactor = screenRatio / referenceRatio;

        //기존 데이터
        float gridX = gridParent.rect.width;
        int contentCount = gridParent.gameObject.GetComponent<GridLayoutGroup>().constraintCount;
        Vector2 cellsize = gridParent.gameObject.GetComponent<GridLayoutGroup>().cellSize;
        float spacing = gridParent.gameObject.GetComponent<GridLayoutGroup>().spacing.x;

        //계산 데이터
        Vector2 newCellSize = new Vector2(cellsize.x * scaleFactor, cellsize.y * scaleFactor);

        float newSpacing;
        if(contentCount<=1)
        {
            newSpacing = spacing;
        }
        else
        {
            newSpacing = (gridX - newCellSize.x * contentCount) / (contentCount - 1);
        }
        //대입
        gridParent.gameObject.GetComponent<GridLayoutGroup>().cellSize = newCellSize;
        gridParent.gameObject.GetComponent<GridLayoutGroup>().spacing = new Vector2(newSpacing, gridParent.gameObject.GetComponent<GridLayoutGroup>().spacing.y);
    }
    //UI 사이즈 관련 코드 - 스케일
    public float GetScale(RectTransform gridParent)
    {
        // 해상도 비율 계산
        float screenRatio = (float)Screen.width / Screen.height;
        float referenceRatio = canvasScaler.referenceResolution.x / canvasScaler.referenceResolution.y;

        // 그리드의 스케일 조정
        float scaleFactor = screenRatio / referenceRatio * gridParent.localScale.x;

        return scaleFactor;
    }

}
