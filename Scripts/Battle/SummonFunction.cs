using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class SummonFunction : SceneSingleton<SummonFunction>
{
    public GameObject summonUnion;
    public int[,] summonData = new int[4, 2]; //0-소환수 번호 1- 소환수 amount
    public int[] summonMax = new int[4];
    public GameObject showSummon;
    public GameObject summonBox;
    //소환한 소환수의 수
    public int summonCount = 0;
    int showNum = 0;
    //소환수 좌우 버튼
    public GameObject btnL, btnR, btnCheck;
    [SerializeField] private GameObject changeGroup;
    private bool isChange = false;
    private int changeIndex = 0;
    private int summonTime = 0;
    void Start()
    {
        SetSummon();
    }

    public int GetSummonCount(int summonId, bool isWhole = false)
    {

        int count = 0;

        if (isWhole)
        {
            for (int i = 0; i < summonData.Length / 2; i++)
            {
                if (summonData[i, 0] > 0) count++;
            }
        }
        else
        {
            for (int i = 0; i < summonData.Length / 2; i++)
            {
                if (summonData[i, 0] == summonId) count++;
            }
        }

        return count;
    }

    public void Summon(int num, int state, int time = 1)
    {
        summonTime = time;
        for (int k = 0; k < time; k++)
        {
            for (int i = 0; i < summonData.Length / 2; i++)
            {
                if (summonData[i, 0] == 0)
                {
                    summonData[i, 0] = num;
                    summonData[i, 1] = state;
                    summonMax[i] = state;
                    summonCount++;

                    int temp = i;
                    summonUnion.transform.GetChild(i).GetComponent<Button>().onClick.RemoveAllListeners();
                    summonUnion.transform.GetChild(i).GetComponent<Button>().onClick.AddListener(() => ShowSummonData(num, temp));
                    summonTime -= 1;
                    break;
                }
            }
        }


        if (summonTime > 0)
        {
            isChange = true;
            ShowSummonData(num, 0, state);
            summonTime -= 1;
        }

        SetSummon();
        //장비 능력 발동
        EquipmentFunction.Instance.SetSummonEqipmentAction(BattleManager.Instance.player);
        BattleManager.Instance.player.GetComponent<CharacterBase>().SetStatusIcon();
    }
    public int SummonsAbility(int index,int pos,string type, int DamageAmount=0)
    {
        // index : 소환수 번호 , pos : 소환수 위치
        int returnNum = 0;
        var enemys = EnemyManager.Instance.enemies;
        if (type == "StartTurn")
        {
            switch (index)
            {
                case 2:
                    int randomNum2 = Random.Range(0, enemys.Count);
                    enemys[randomNum2].GetComponent<Enemy>().GetDamage(BattleManager.Instance.player, summonData[pos, 1], EnumTypes.EffectType.hit, false, null);
                    UseAbility(summonUnion.transform.GetChild(pos).gameObject);
                    break;
                case 4:
                    int randomNum4 = Random.Range(0, enemys.Count);
                    enemys[randomNum4].GetComponent<Enemy>().GetStatusEnemy(4, EnumTypes.Status.debuff, summonData[pos, 1]);
                    UseAbility(summonUnion.transform.GetChild(pos).gameObject);
                    break;
                case 5:
                    int randomNum5 = Random.Range(0, enemys.Count);
                    enemys[randomNum5].GetComponent<Enemy>().GetStatusEnemy(15, EnumTypes.Status.debuff, summonData[pos, 1]);
                    UseAbility(summonUnion.transform.GetChild(pos).gameObject);
                    break;
            }
        }
        else if (type == "GetDamage")
        {
            int calDam = DamageAmount;
            switch (index)
            {
                case 1:
                    summonData[pos, 0] = 0;
                    summonData[pos, 1] = 0;
                    calDam = 0;
                    UseAbility(summonUnion.transform.GetChild(pos).gameObject);
                    break;
                case 3:
                    if (summonData[pos, 1] > DamageAmount)
                    {
                        calDam = 0;
                        summonData[pos, 1] -= DamageAmount;
                    }
                    else
                    {
                        calDam = DamageAmount - summonData[pos, 1];
                        summonData[pos, 0] = 0;
                        summonData[pos, 1] = 0;
                    }
                    UseAbility(summonUnion.transform.GetChild(pos).gameObject);
                    break;
            }
            SetSummon();
            return calDam;
        }

        SetSummon();
        return returnNum;
    }
    public void SetSummon()
    {
        for (int i = 0; i < summonData.Length / 2; i++)
        {
            if (summonData[i, 0] == 0)
            {
                summonUnion.transform.GetChild(i).GetChild(1).GetComponent<TextMeshProUGUI>().text = "";
                summonData[i,1] = 0;
                summonUnion.transform.GetChild(i).GetComponent<Button>().onClick.RemoveAllListeners();

                summonUnion.transform.GetChild(i).GetChild(0).gameObject.SetActive(false);
            }
            else if (summonData[i, 0] > 0)
            {
                string st = summonData[i, 1].ToString();
                summonUnion.transform.GetChild(i).GetChild(1).GetComponent<TextMeshProUGUI>().text = st;

                summonUnion.transform.GetChild(i).GetChild(0).gameObject.SetActive(true);
                string path = InGameData.Instance.Summons.Find(s => s.Id == summonData[i, 0])?.ImgPath;
                summonUnion.transform.GetChild(i).GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(path);
            }
        }
    }

    public void ShowSummonData(int index, int btnindex, int state = 0)
    {
        //index = 소환수의 인덱스
        //btnindex = 버튼의 인덱스
        //SFX
        AudioManager.Instance.ButtonClickSound1();

        summonBox.transform.GetChild(0).gameObject.SetActive(!isChange);
        changeGroup.SetActive(isChange);
        //만약 소환 교체일시 실행
        if (isChange)
        {
            showNum = 0;

            changeGroup.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(InGameData.Instance.Summons.Find(s => s.Id == summonData[showNum, 0])?.ImgPath);
            changeGroup.transform.GetChild(1).GetComponent<Image>().sprite = Resources.Load<Sprite>(InGameData.Instance.Summons.Find(s => s.Id == index)?.ImgPath);

            changeIndex = index;
            //버튼 교체하기로 수정
            btnCheck.SetActive(true);
            btnCheck.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = btnCheck.transform.GetChild(0).GetComponent<LocalizeStringEvent>().StringReference.TableEntryReference = "Change";
            btnCheck.GetComponent<Button>().onClick.RemoveAllListeners();

            btnCheck.GetComponent<Button>().onClick.AddListener(() => ChangeSummonData(index, state));

            //배경 터치 비활성화
            showSummon.transform.GetChild(0).GetComponent<Button>().interactable = false;
        }
        else
        {
            showNum = btnindex;

            string path = InGameData.Instance.Summons.Find(s => s.Id == index)?.ImgPath;
            summonBox.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(path);

            //버튼 뒤로가기로 수정
            btnCheck.SetActive(true);
            btnCheck.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = btnCheck.transform.GetChild(0).GetComponent<LocalizeStringEvent>().StringReference.TableEntryReference = "Back";
            btnCheck.GetComponent<Button>().onClick.RemoveAllListeners();
            btnCheck.GetComponent<Button>().onClick.AddListener(CloseSummonData);
        }

        summonBox.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = InGameData.Instance.Summons.Find(s => s.Id == index)?.Name;
        summonBox.transform.GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>().text = InGameData.Instance.Summons.Find(s => s.Id == index)?.Description;

        showSummon.SetActive(true);
        ButtonAnim.Instance.ButtonScaleIn(summonBox.gameObject,0f,1f);




        Debug.Log(btnindex);
        SetRL();
    }

    //좌우 이동
    public void MoveSummonData(bool isLeft)
    {
        var show = new List<int>();
        for (int i = 0; i < summonData.Length / 2; i++)
        {
            if (summonData[i, 0] != 0)
            {
                show.Add(summonData[i, 0]);
            }
        }

        if (isLeft)showNum++;
        else showNum--;


        if(isChange)
        {
            changeGroup.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(InGameData.Instance.Summons.Find(s => s.Id == show[showNum])?.ImgPath);
            changeGroup.transform.GetChild(1).GetComponent<Image>().sprite = Resources.Load<Sprite>(InGameData.Instance.Summons.Find(s => s.Id == changeIndex)?.ImgPath);
        }
        else
        {
            summonBox.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(InGameData.Instance.Summons.Find(s => s.Id == show[showNum])?.ImgPath);
        }

        summonBox.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = InGameData.Instance.Summons.Find(s => s.Id == show[showNum])?.Name;
        summonBox.transform.GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>().text = InGameData.Instance.Summons.Find(s => s.Id == show[showNum])?.Description;

        SetRL();
    }

    //소환수 교체
    public void ChangeSummonData(int summonIndex, int state)
    {
        summonData[showNum, 0] = summonIndex;
        summonData[showNum, 1] = state;
        summonMax[showNum] = state;
        summonUnion.transform.GetChild(showNum).GetComponent<Button>().onClick.RemoveAllListeners();
        int temp = showNum;
        summonUnion.transform.GetChild(showNum).GetComponent<Button>().onClick.AddListener(() => ShowSummonData(summonIndex, temp));

        showNum = 0;
        SetSummon();

        if (summonTime > 0)
        {
            isChange = true;
            ShowSummonData(summonIndex, 0, state);
            summonTime -= 1;
        }
        else
        {
            //배경 비활성화 다시 풀어주기
            showSummon.transform.GetChild(0).GetComponent<Button>().interactable = true;
            CloseSummonData();
            isChange = false;
            changeIndex = 0;
        }
    }
    public void SetRL()
    {
        //SFX
        AudioManager.Instance.ButtonClickSound1();
        //오른쪽 버튼 set
        if (showNum >= 1) btnR.SetActive(true);
        else btnR.SetActive(false);
        //왼쪽 버튼 set
        int max = 0;
        for (int i = 0; i < summonData.Length / 2; i++) if (summonData[i, 0] != 0) max++;
        if (showNum < max - 1) btnL.SetActive(true);
        else btnL.SetActive(false);
    }
    public void CloseSummonData()
    {
        //SFX
        AudioManager.Instance.ButtonClickSound1();
        showSummon.SetActive(false);
    }

    private void UseAbility(GameObject targetObj)
    {
        targetObj.transform.DOKill();
        targetObj.transform.DOScale(new Vector3(1.5f, 1.5f, 1.5f), 0.3f).SetLoops(2, LoopType.Yoyo);
    }

    public void PlayAllSummonAbility()
    {
        StartCoroutine(PlaySummonAbilityCo());
    }
    private IEnumerator PlaySummonAbilityCo()
    {
        for (int i = summonData.Length / 2 - 1; i >= 0; i--)
        {
            if (summonData[i, 0] > 0)
            {
                SummonsAbility(summonData[i, 0], i, "StartTurn");
                if (summonData[i, 0] == 1 || summonData[i, 0] == 3)
                {
                    continue;
                }
                yield return new WaitForSecondsRealtime(0.5f);
            }
        }
    }
}
