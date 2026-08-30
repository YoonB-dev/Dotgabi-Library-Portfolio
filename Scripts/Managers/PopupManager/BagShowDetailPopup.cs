using System;
using UnityEngine;

public class BagShowDetailPopup : MonoBehaviour
{
    [SerializeField] private GameObject bagDetailCanvas;
    [SerializeField] private GameObject bagDetailPanel;
    [SerializeField] private GameObject product;
    public void ShowBagDetail(UserOwnCardFrameDTO itemData, Action selectFrame)
    {
        bagDetailCanvas.SetActive(true);
        // SFX
        AudioManager.Instance.ButtonClickSound1();
        // 팝업 애니메이션
        ButtonAnim.Instance.ButtonScaleIn(bagDetailPanel, 0f, 1f);

        // Bag 아이템 상세 정보 설정
        product = BagDTOToObj.Instance.DetailToObj(product, itemData, selectFrame);
    }
    public void HideBagDetail()
    {
        bagDetailCanvas.SetActive(false);
        // SFX
        AudioManager.Instance.ButtonClickSound2();
    }
}
