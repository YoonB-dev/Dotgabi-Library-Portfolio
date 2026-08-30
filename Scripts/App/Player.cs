using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : CharacterBase
{
    private ScenarioDTO SCENARIO_DATA;
    [Header("----------[Player]----------")]
    public int maxAction = 4;
    public int currAction = 0;
    public EnumTypes.JobType Job;
    public GameObject[] actions; // 행동력 gameObject
    public bool isFirstDamage = false; // 첫 피해 여부
    [Header("----------[ETC]----------")]

    public GameObject[] Animations, effects;
    public Transform effectPos;
    [SerializeField] private ADMAnager adManager;
    public float ratio = 1;
    [Header("----------[Dosa]----------")]
    public EnumTypes.DosaModeType DosaMod = EnumTypes.DosaModeType.none; //nomal, change
    public ChangeMode changeMode;
    public void SetPlayerStats()
    {
        SCENARIO_DATA = BattleManager.Instance.SCENARIO_DATA;
        Stat stat = new Stat {
            maxHp = SCENARIO_DATA.MaxHp,
            currHp = SCENARIO_DATA.CurrHp,
            currShield = 0,
            isChange = false,
            changeMaxHp = 0,
            changeCurrHp = 0
        };
        this.Stats = stat;
        ArtifactList = SCENARIO_DATA.OwnedArtifactList;

        characterName = LogManager.Instance?.GetLocalizedText("player");
    }

    void Update()
    {
        if (Input.GetKeyDown("a"))
        {
            StartCoroutine(TurnManager.Instance.StartTurn());
        }
        if (Input.GetKeyDown("s"))
        {
            SetGetDamageAnimation(1);
        }
        if (Input.GetKeyDown("d"))
        {
            GetStatusBase(12, EnumTypes.Status.debuff, 20);
        }
    }
    public void GetShield(int amount)
    {
        Debug.Log($"GetShield: {amount}");
        GetShieldBase(amount);
        // 패시브 동작 -> 플레이어가 보호막을 얻었을 때 패시브 능력 발동
        for (int i = 0; i < EnemyManager.Instance.enemies.Count; i++)
        {
            if (!EnemyManager.Instance.enemies[i].isDie)
            {
                StartCoroutine(PassiveFunction.Instance?.PassiveAction(EnumTypes.EnemyPassiveTrigger.player_get_shield, EnemyManager.Instance.enemies[i], this));
                Debug.Log("fuck!!");
            }
        }
    }

    public void GetDamagePlayer(int amount, Dictionary<string, object> extraData)
    {
        if (isDie) return;
        //SFX
        AudioManager.Instance.GetDamageSound();
        TakeDamageBase(amount, extraData, false);
        //데미지 피격 이펙트
        Instantiate(effects[0], effectPos);
        //죽음 체크
        if (Stats.currHp <= 0)
        {
            Stats.currHp = 0;
            var reviveResult = ArtifactFunction.Instance.ArtifactRevive(this, null);
            if (reviveResult != null && reviveResult.IsRevive)
            {
                Debug.Log("부활!");
                StartCoroutine(HpbarMotion(EnumTypes.TextMotionType.up));
                SetStatText();
                SetStatusIcon();
            }
            else
            {
                Debug.Log("사망");
                StartCoroutine(DieCo());
            }
            return;
        }

        //도사 변신 데미지
        if (Job == EnumTypes.JobType.Dosa && DosaMod != EnumTypes.DosaModeType.none && Stats.changeCurrHp <= 0)
        {
            changeMode.BackToNomal();
        }

        // 연주 시 연주 종료
        PlayFunction.Instance?.PlayNone();

        //버프 정리
        //피해 받으면 카드 뽑기
        if (CheckHaveBuffOrDebuff(EnumTypes.Status.buff, 14))
        {
            //StartCoroutine(CardSystem.Inst.DrawCard(playerState.buff[14]));
        }
        if (CheckHaveBuffOrDebuff(EnumTypes.Status.buff, 17))
        {
            //사용 후 0으로 만들기 나중에 구현해야함
            StartCoroutine(CardSystem.Instance.DrawCard(2));
        }

        SetStatusIcon();
        SetActionText();
    }

    public void GetDamagePure(int amount)
    {
        //SFX
        AudioManager.Instance.GetDamageSound();

        TakeDamagePure(amount);
    }
    public void GetAction(int amount, bool isStart = false)
    {
        if (isDie) return;
        //SFX
        if (!isStart) AudioManager.Instance.GetActionSound();

        currAction += amount;
        if (currAction < 0)
        {
            currAction = 0;
        }
        if (currAction > maxAction)
        {
            currAction = maxAction;
        }
        SetActionText();
    }
    public void GetHeal(int amount)
    {
        //회복량 증가
        if (SCENARIO_DATA.OwnedArtifactList.Find(x => x.ArtifactId == 43) != null)
        {
            amount += 2;
        }

        //난이도 회복량 조절
        float magnification = 1;

        if (SCENARIO_DATA is UserMainScenarioDTO)
        {
            var data = SCENARIO_DATA as UserMainScenarioDTO;
            if ((int)data.Difficulty >= 3)
            {
                magnification = 0.8f;
            }
        }

        amount = Mathf.RoundToInt(amount * magnification);

        if (Job == EnumTypes.JobType.Dosa && DosaMod != EnumTypes.DosaModeType.none)
        {
            if (Stats.changeCurrHp + amount > Stats.changeMaxHp) amount = Stats.changeMaxHp - Stats.changeCurrHp;
        }
        else
        {
            if (Stats.currHp + amount > Stats.maxHp) amount = Stats.maxHp - Stats.currHp;
        }

        //battleLog.AddTxtString(GameData.Text_Log[36][DamageCal.logLan].ToString(),amount.ToString());
        GetHealBase(amount);
        //SetMoveTextBase(amount.ToString(), EnumTypes.TextMotionType.up, EnumTypes.MoveTextType.heal, 1);
    }


    public void SetActionText()
    {
        //액션 세팅
        for (int i = 0; i < 4; i++)
        {
            actions[i].GetComponent<Image>().sprite = (i < currAction) ? Resources.Load<Sprite>("Image/Player/ActionOn") : Resources.Load<Sprite>("Image/Player/ActionOff");
        }
    }

    public void SetGetDamageAnimation(int index)
    {
        int realIndex = index - 1;

        Animations[realIndex].SetActive(true);
        StartCoroutine(AnimationFinish(Animations[realIndex]));
    }
    IEnumerator AnimationFinish(GameObject animator)
    {
        while (true)
        {
            AnimatorStateInfo stateInfo = animator.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0);

            //끝났는지 체크.
            if (stateInfo.normalizedTime >= 1.0f)
            {
                animator.SetActive(false);
                yield break; // 코루틴 종료
            }

            yield return null;
        }
    }

    IEnumerator DieCo()
    {
        isDie = true;
        yield return new WaitForSecondsRealtime(1f);
        VictoryManager.Instance.GameOverButtonActive();
    }
}
