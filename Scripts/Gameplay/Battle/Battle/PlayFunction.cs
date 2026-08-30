using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayFunction : SceneSingleton<PlayFunction>
{
    public EnumTypes.PerformerPlayType currPlay = EnumTypes.PerformerPlayType.none;
    public int playTime = 0;
    public bool isPlay = false; // 연주 중인지 여부
    public CardDTO playCardDTO = new ();
    [SerializeField]
    private Sprite basicPlayImg;
    public GameObject playObj;//현재 플레이 중인 오브젝트

    public void Play(EnumTypes.PerformerPlayType playType, CardDTO cardInfo)
    {
        //string name = cardInfo.cardName.ToString();
        //string des = "[" + name + "]\n" + GameManager.gameManager.SplitDescription(cardInfo);
        PlayNone(); // 이전 연주 중인 오브젝트 초기화
        currPlay = playType;
        playTime++;
        playCardDTO = cardInfo;
        //연주중인 오브젝트 데이터 세팅
        playObj.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>("Image/Icon/instrument/"+ playType.ToString());
        playObj.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>().text = cardInfo.Description;

        isPlay = true;
    }
    public void ShowPlay()
    {
        if (currPlay == EnumTypes.PerformerPlayType.none)
        {
            string text = LogManager.Instance?.GetLocalizedText("no_performer_play");
            playObj.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>().text = text;
        }

        playObj.transform.GetChild(1).gameObject.SetActive(true);
        //SFX
        AudioManager.Instance.ButtonClickSound1();
    }
    public void UsePlay()
    {
        StartCoroutine(UsePlayCo());
    }
    IEnumerator UsePlayCo()
    {
        if(currPlay == EnumTypes.PerformerPlayType.none) yield break;

        for (int i = 1; i < playCardDTO.CardActions.Count; i++)
        {
            CardFunction.Instance.AbilityFunction(playCardDTO, playCardDTO.CardActions[i], i);
        }

        playObj.GetComponent<RectTransform>().DOScale(new Vector3(1.2f,1.2f,1.2f),0.3f).SetLoops(2,LoopType.Yoyo);
        yield return new WaitForSeconds(0.3f);

        yield return null;
    }

    public void PlayNone()
    {
        if (currPlay == EnumTypes.PerformerPlayType.none) return;

        // CardFunction.cardF.UseEqAbility(12);
        currPlay = EnumTypes.PerformerPlayType.none;

        //이미지 삭제
        playObj.transform.GetChild(0).GetComponent<Image>().sprite = basicPlayImg;
        isPlay = false;

        // 장착 효과 적용 - 연주 종료 시
        EquipmentFunction.Instance.SetInstrumentEndEquipmentAction(BattleManager.Instance.player);
        BattleManager.Instance?.player?.SetStatusIcon();
    }
}
