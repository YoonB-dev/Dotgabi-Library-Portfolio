using System.Collections.Generic;
using EnumTypes;
using UnityEngine;


// <summary>
// 특정 씬에서 팝업을 관리하는 매니저.
// 캐릭터, 도감, 카드 프레임등이 포함된다.
// </summary>

public class PopupSceneManager : SceneSingleton<PopupSceneManager>
{
    [SerializeField] private CharacterPopup characterPopup;
    [SerializeField] private ShopPopup shopPopup;
    [SerializeField] private BagPopup bagPopup;
    [SerializeField] private AchievePopup achievePopup;
    [SerializeField] private StoryPopup storyPopup;
    public void ShowPopup(PopupType popupType)
    {

        Debug.Log("팝업 실행됨");
        switch (popupType)
        {
            case PopupType.Character:
                characterPopup.ShowCharacterPopup();
                //로그
                var logText = LogManager.Instance?.GetMainLogText("main_character_list_show");
                LogManager.Instance?.AddLogMain(logText);
                break;
            case PopupType.ShopItem:
                shopPopup.ShowShopPopup();
                //로그
                var shopLogText = LogManager.Instance?.GetMainLogText("main_shop_item_show");
                LogManager.Instance?.AddLogMain(shopLogText);
                break;
            case PopupType.Bag:
                bagPopup.ShowBagPopup(isFirst: true);
                //로그
                var bagLogText = LogManager.Instance?.GetMainLogText("main_bag_show");
                LogManager.Instance?.AddLogMain(bagLogText);
                break;
            case PopupType.Achieve:
                Debug.Log("업적 팝업 실행됨");
                achievePopup.ShowAchievePopup();
                break;
            case PopupType.Story:
                storyPopup.ShowStoryPopup();
                break;
            default:
                break;
        }

        MainManager.Instance.cambox.SetCanMove(false); // 팝업이 열리면 카메라 이동 비활성화
    }
}

