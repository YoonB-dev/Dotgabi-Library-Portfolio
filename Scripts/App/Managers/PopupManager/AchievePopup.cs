using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.SmartFormat;
using UnityEngine.UI;

public class AchievePopup : MonoBehaviour
{
    [SerializeField] private Canvas AchieveCanvas;
    [SerializeField] private GameObject AchieveBox;
    public void ShowAchievePopup()
    {
        //SFX
        AudioManager.Instance.ButtonClickSound1();
        //Camera 움직임 비활성화
        MainManager.Instance.cambox.SetCanMove(false);
        //Canvas 활성화
        AchieveCanvas.gameObject.SetActive(true);
        ButtonAnim.Instance.ButtonScaleIn(AchieveBox, 0f, 1f);
        //업적 오브젝트 설정
        AchieveManager.Instance.SetAchieveObjList();
    }
    public void CloseAchieve()
    {
        //SFX
        AudioManager.Instance.ButtonClickSound3();
        //카메라 움직임 활성화
        MainManager.Instance.cambox.SetCanMove(true);
        //Canvas 비활성화
        AchieveCanvas.gameObject.SetActive(false);
    }
}
