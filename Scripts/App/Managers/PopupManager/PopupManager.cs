using System;
using System.Collections.Generic;
using EnumTypes;
using UnityEngine;

// <summary>
// 게임 씬이 바뀌어도 팝업을 관리하는 매니저.
// 카드와 유물 데이터가 포함된다.
// </summary>

public class PopupManager : MonoBehaviorSingleton<PopupManager>
{
    [SerializeField] private List<PopUpSO> popUpSOs;
    [SerializeField] private CardPopup cardPopup;
    [SerializeField] private ArtifactPopup artifactPopup;
    [SerializeField] private Canvas cardShowPopupCanvas, artifactShowPopupCanvas;

    public void SetCanvasCamera(Camera camera)
    {
        cardShowPopupCanvas.worldCamera = camera;
        artifactShowPopupCanvas.worldCamera = camera;
    }

    public void ShowPopup(PopupType popupType, bool isCollection = true, bool isDetail = false)
    {
        PopUpSO popUpSO = popUpSOs.Find(p => p.popupType == popupType);
        if (popUpSO != null)
        {
            Debug.Log("팝업 실행됨");

            switch (popupType)
            {
                case PopupType.Card:
                    cardPopup.popupSO = popUpSO;
                    if (isCollection)
                    {
                        cardPopup.ShowCardPopup(isFirst: true);
                    }
                    else
                    {
                        cardPopup.ShowMainOwnedCardPopup();
                    }

                    break;
                case PopupType.Artifact:
                    Debug.Log("Artifact 팝업 실행됨");
                    artifactPopup.popupSO = popUpSO;
                    if (isCollection)
                    {
                        artifactPopup.ShowArtifactPopup(isFirst: true);
                    }
                    else
                    {
                        Debug.Log("ShowMainOwnedArtifactPopup 실행됨");
                        artifactPopup.ShowMainOwnedArtifactPopup();
                    }

                    break;
                default:

                    break;
            }
        }
        else
        {
            Debug.LogWarning($"Popup of type {popupType} not found.");
        }

        MainManager.InstanceOrNull?.cambox?.SetCanMove(false); // 팝업이 열리면 카메라 이동 비활성화
    }

    public void ShowCardUpgradePopup(System.Action action = null)
    {
        PopUpSO popUpSO = popUpSOs.Find(p => p.popupType == PopupType.Card);
        if (popUpSO != null)
        {
            cardPopup.popupSO = popUpSO;
            cardPopup.ShowCardUpgradePopup(action);
        }
        else
        {
            Debug.LogWarning("Card popup not found.");
        }
    }

    public void ShowCardDeletePopup(bool canBack, System.Action action = null)
    {
        PopUpSO popUpSO = popUpSOs.Find(p => p.popupType == PopupType.Card);
        if (popUpSO != null)
        {
            cardPopup.popupSO = popUpSO;
            cardPopup.ShowCardDeletePopup(canBack, action: action);
        }
        else
        {
            Debug.LogWarning("Card popup not found.");
        }
    }

    public void ShowJustArtifactDetail(ArtifactDTO artifactDTO, bool isSelect = false)
    {
        // 유물 상세 정보만 보여주는 로직
        artifactPopup.ShowArtifactJustDetail(artifactDTO, isSelect: isSelect);
    }

    // 전투중 카드 사용 & 핸드 카드 보여주기
    public void ShowUsedCardPopup()
    {
        PopUpSO popUpSO = popUpSOs.Find(p => p.popupType == PopupType.Card);
        if (popUpSO != null)
        {
            cardPopup.popupSO = popUpSO;
            cardPopup.ShowUsedCardPopup();
        }
        else
        {
            Debug.LogWarning("Card popup not found.");
        }
    }
    public void ShowCanCardPopup()
    {
        PopUpSO popUpSO = popUpSOs.Find(p => p.popupType == PopupType.Card);
        if (popUpSO != null)
        {
            cardPopup.popupSO = popUpSO;
            cardPopup.ShowCanCardPopup();
        }
        else
        {
            Debug.LogWarning("Card popup not found.");
        }
    }

    // 카드 상세 정보만 보여주기
    public void ShowCardDetail(CardDTO cardDTO)
    {
        cardPopup.ShowCardJustDetail(cardDTO);
    }

    // 카드 전체 정보 보여주기
    public void ShowAllCard(List<CardDTO> cardDTOs)
    {
        PopUpSO popUpSO = popUpSOs.Find(p => p.popupType == PopupType.Card);
        if (popUpSO != null)
        {
            cardPopup.popupSO = popUpSO;
        }
        cardPopup.ShowCardPopupList(cardDTOs);
    }
}
