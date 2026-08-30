using System.Collections;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class MaskFunction : SceneSingleton<MaskFunction>
{
    public int currMaskIndex = 0;//현재 탈 인덱스(장착 중인 탈)
    public int nextMaskIndex = 0;//매턴 바뀌는 마스크 인덱스(고정으로 1,2,3,4, 이동함)
    private int showMaskIndex = 0;//마스크 정보 보여줄 때 사용 -> 현재 보고있는 마스크 인덱스
    [SerializeField] private GameObject currMaskObj, selectMaskBtn;
    [SerializeField] private GameObject[] maskObjs;
    private bool detailShow = false;
    private bool isPopUp = false;
    public GameObject maskDetailObj;
    [SerializeField] private Sprite[] mask_back_imgs; // 마스크 뒷 배경 이미지들
    [SerializeField] private GameObject maskTipBox;
    [SerializeField] private Button backGroupdBtn;
    [SerializeField] private GameObject backBtn;
    [SerializeField] private GameObject popDownBtn;
    [SerializeField] private GameObject maskBox;


    private float ratio = 1;
    public void SwitchMaskNext()
    {
        StartCoroutine(SwitchMaskCo());
    }
    IEnumerator SwitchMaskCo()
    {
        //탈 번호 늘리기
        nextMaskIndex++;
        if (nextMaskIndex > 4) nextMaskIndex = 1;
        currMaskIndex = nextMaskIndex;
        SwitchMask(currMaskIndex);
        //모션 들어감
        yield return null;
    }

    public void SetMaskSelect()
    {
        SwitchMask(showMaskIndex);
        //SFX
        AudioManager.Instance.ButtonClickSound1();
        //마스크 상세 정보 닫기
        CloseMaskDetail();
    }

    //마스크를 변경했을 시
    private void SwitchMask(int maskId)
    {
        currMaskIndex = maskId;//현재 장착 중인 탈로 교체
        EquipmentFunction.Instance.SetMaskEquipmentAction(BattleManager.Instance.player);

        //CardFunction.cardF.UseEqAbility(11);
        currMaskObj.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(InGameData.Instance.Masks.Find(m => m.MaskId == maskId).ImgPath);

        //현재 탈 표시
        for (int i = 0; i < maskObjs.Length; i++)
        {
            if (i == currMaskIndex - 1) maskObjs[i].transform.GetChild(2).gameObject.SetActive(true);
            else maskObjs[i].transform.GetChild(2).gameObject.SetActive(false);
        }

        //다음 탈 표시
        int next = nextMaskIndex;
        if (next >= 4) next = 0;
        for (int i = 0; i < maskObjs.Length; i++)
        {
            if (i == next) maskObjs[i].transform.GetChild(1).gameObject.SetActive(true);
            else maskObjs[i].transform.GetChild(1).gameObject.SetActive(false);
        }
    }

    public void ShowMaskDetail(bool isChange = true)
    {
        if (currMaskIndex == 0) return;
        if (detailShow) return;

        maskDetailObj.SetActive(true);
        ButtonAnim.Instance.ButtonScaleIn(maskDetailObj.transform.GetChild(1).gameObject, 0f, 1f * ratio);
        //SFX
        AudioManager.Instance.ButtonClickSound1();
        SetCurrMask(currMaskIndex);
        selectMaskBtn.SetActive(isChange);
        maskDetailObj.transform.GetChild(1).GetChild(2).gameObject.SetActive(isChange);

        backGroupdBtn.interactable = !isChange;
        backBtn.SetActive(!isChange);
        popDownBtn.SetActive(isChange);
        isPopUp = true;
        detailShow = true;

        if (isChange)
        {
            popDownBtn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "숨기기";
        }
    }

    public void SetCurrMask(int index)
    {
        showMaskIndex = index;
        maskBox.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(InGameData.Instance.Masks.Find(m => m.MaskId == showMaskIndex).ImgPath);
        maskBox.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = InGameData.Instance.Masks.Find(m => m.MaskId == showMaskIndex).Name;
        maskBox.transform.GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>().text = InGameData.Instance.Masks.Find(m => m.MaskId == showMaskIndex).Description;
        //현재 보고있는 마스크 뒷 배경
        for (int i = 0; i < maskObjs.Length; i++)
        {
            if (i == index - 1) maskObjs[i].GetComponent<Image>().sprite = mask_back_imgs[0];
            else maskObjs[i].GetComponent<Image>().sprite = mask_back_imgs[1];
        }
    }

    public void PopDownButton()
    {
        //SFX
        AudioManager.Instance.ButtonClickSound2();
        isPopUp = !isPopUp;

        CardSystem.Instance.canActive = isPopUp;

        maskDetailObj.transform.GetChild(0).gameObject.SetActive(isPopUp);
        maskDetailObj.transform.GetChild(1).gameObject.SetActive(isPopUp);

        if (isPopUp)
        {
            popDownBtn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "숨기기"; // 숨기기
        }
        else
        {
            popDownBtn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "보기"; // 보기
        }
    }

    public void CloseMaskDetail()
    {
        //SFX
        AudioManager.Instance.ButtonClickSound2();
        maskDetailObj.SetActive(false);
        detailShow = false;
    }


    public void OpenTipDetailBox()
    {
        maskTipBox.SetActive(true);
        ButtonAnim.Instance.ButtonScaleIn(maskTipBox.transform.GetChild(1).gameObject, 0f, 1f * ratio);
        //SFX
        AudioManager.Instance.ButtonClickSound1();
    }

    public void CloseTipDetailBox()
    {
        maskTipBox.SetActive(false);
        //SFX
        AudioManager.Instance.ButtonClickSound2();
    }

    public void SetAttackMaskAbility(Enemy target, int damage)
    {
        if (currMaskIndex == (int)EnumTypes.PerformerMaskType.bong)
        {
            target.GetStatusEnemy(5, EnumTypes.Status.debuff, 1);
        }
        else if (currMaskIndex == (int)EnumTypes.PerformerMaskType.hahoe)
        {
            int amount = (int)math.round(damage * 0.25f);
            BattleManager.Instance.player.GetHealBase(amount);
        }
    }
}
