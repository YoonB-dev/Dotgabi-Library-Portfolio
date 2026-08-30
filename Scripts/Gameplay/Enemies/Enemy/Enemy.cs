using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using Random = UnityEngine.Random;
using UnityEngine.Localization.SmartFormat;
using System.Linq;
using Spine.Unity;


public class Enemy : CharacterBase
{
    public EnemyDTO enemyDTO;
    [Header("----------[State]----------")]
    [SerializeField] private GameObject effectsPos;
    public Player player;
    public int enemyIndex; // 행동 순서용
    [SerializeField] private SkeletonAnimation enemyImage;
    [SerializeField] private TextMeshProUGUI enemyNameTxt;
    [SerializeField] private Image selectImage;
    [Header("----------[nextActions]----------")]
    [SerializeField]
    private Image[] nextActionShow = new Image[3];
    public List<(EnumTypes.enemyActionType actionType, int value, bool isAmountBlind, bool isBlindIcon, int abilityIndex)> nextActions = new(); // 타입, 수치, 공개 여부, 능력 인덱스(능력일 경우)
    [SerializeField] private Sprite questionMarkIcon;
    [SerializeField] private Sprite attackIcon, shieldIcon, healIcon;
    public bool isText;
    IEnumerator cor;
    public GameObject damageTxt, textGroup;
    public bool doubleAttack = false;
    public float ratio = 1;
    string[] flavorTexts;
    [SerializeField] private GameObject nextActionBox;
    public int enemyID;
    int count = 1;
    private Coroutine DamageColorCor;

    public Sequence moveTween;
    void Start()
    {
        SetStartData();
        SetStatText();
        for (int i = 0; i < nextActionShow.Length; i++)
        {
            nextActionShow[i].gameObject.SetActive(false);
        }
        SetFlavorTxt();
        cor = FlaverText();
        StartCoroutine(cor);
        textGroup = GameObject.FindWithTag("TextGroup");

        transform.GetChild(0).GetComponent<Canvas>().worldCamera = Camera.main;

        SetPassive();//패시브 버튼 생성
        nextActionBox.SetActive(false);
        // 적 대사 정보가 있는지 확인
        CheckText();

        // 애니메이션 완료 이벤트 등록
        enemyImage.AnimationState.Complete += OnAnimationComplete;
    }
    private void SetStartData()
    {
        var skeletonDataAsset = Resources.Load<SkeletonDataAsset>(enemyDTO.ImgSpinePath + "_SkeletonData");
        enemyImage.skeletonDataAsset = skeletonDataAsset;
        enemyImage.Initialize(true);  // 반드시 호출해줘야 바뀜
        Debug.Log($"적 스켈레톤 데이터: {skeletonDataAsset}");

        enemyImage.AnimationName = "idle";
        enemyNameTxt.text = enemyDTO.Name;

        enemyImage.transform.localScale = Vector2.one * ratio;

        characterName = enemyDTO.Name + enemyID;
    }
    private void SetPassive()
    {
        for (int i = 0; i < enemyDTO.Passive.Count; i++)
        {
            GetPassiveBase(enemyDTO.Passive[i]);
        }
    }

    void SetFlavorTxt()
    {
        flavorTexts = enemyDTO.FlavorText.Split("^");
    }
    public void SetSelectImage(bool isTrue)
    {
        selectImage.gameObject.SetActive(isTrue);
    }
    void Update()
    {
        //스탯 설명 비활성화
        if (Input.GetMouseButtonDown(0))
        {
            nextActionBox.SetActive(false);
        }
    }

    public void GetDamage(CharacterBase fromCharacter, int damage, EnumTypes.EffectType effectType, bool isCard, Dictionary<string, object> extraData)
    {
        if (isDie) return;

        PlayHitEffect(effectType);

        int reDamage = fromCharacter != null ? DamageCal.AttackDamageCal(fromCharacter, this, damage, isCard, extraData) : damage;
        Debug.Log($"적에게 받은 데미지: {reDamage}");

        TakeDamageBase(reDamage, extraData, false);
        if (DamageColorCor != null) StopCoroutine(DamageColorCor);

        // 처형 동작
        ArtifactFunction.Instance.ArtifactAttackEnemy(player, this);

        if (!isDie)
        {
            // 패시브 적용
            StartCoroutine(PassiveFunction.Instance.PassiveAction(EnumTypes.EnemyPassiveTrigger.enemy_got_damage, this, player));

            // 피격 플래시 - 사망이 아니면
            DamageColorCor = StartCoroutine(HitFlash(enemyImage));
        }

        if (isDie)
        {
            DeleteObject();
        }
    }

    private void PlayHitEffect(EnumTypes.EffectType effectType)
    {
        switch (effectType)
        {
            case EnumTypes.EffectType.hit: AudioManager.Instance.HitSound(); break;
            case EnumTypes.EffectType.slash: AudioManager.Instance.SlashSound(); break;
            case EnumTypes.EffectType.smash: AudioManager.Instance.SmashSound(); break;
            case EnumTypes.EffectType.blood: AudioManager.Instance.BloodSound(); break;
            default:
                // 정의되지 않은 타입이 들어오면 기본 사운드만 출력
                AudioManager.Instance.HitSound();
                break;
        }
        // 오브젝트 풀링
        GameObject effect = EffectPoolManager.Instance.SpawnFromPool(effectType, effectsPos.transform.position, Quaternion.identity);

        if (effect != null)
        {
            var ps = effect.GetComponent<ParticleSystem>();
            if (ps != null) ps.Play();
        }
    }

    IEnumerator HitFlash(SkeletonAnimation enemy)
    {
        var skeleton = enemy.skeleton;
        skeleton.SetColor(new Color(0.9f, 0.2f, 0.3f));
        yield return new WaitForSeconds(0.3f); // 0.3초간 빨간색 유지
        skeleton.SetColor(Color.white);
    }

    public IEnumerator StartTurn()
    {
        if (isDie) yield break;

        // 로그 출력
        var text = LogManager.Instance?.GetLocalizedText("enemy_turn").FormatSmart(characterName);
        LogManager.Instance.AddLogBattle(text);

        //버프 삭제
        SetBuffStartTurn();
        //턴 시작 패시브 발동
        yield return PassiveFunction.Instance.PassiveAction(EnumTypes.EnemyPassiveTrigger.enemy_start_turn, this, player);

        for (int i = 0; i < nextActions.Count; i++)
        {
            if (isDie || player.isDie) break;

            yield return new WaitForSecondsRealtime(0.8f);

            transform.DOKill();
            nextActionShow[i].gameObject.transform.DOScale(new Vector3(2.5f, 2.5f, 2.5f) * ratio, 0.3f).SetLoops(2, LoopType.Yoyo);
            switch (nextActions[i].actionType)
            {
                case EnumTypes.enemyActionType.attack:
                    yield return StartCoroutine(AttackPlayer(nextActions[i].value, null));
                    if (doubleAttack)
                    {
                        yield return new WaitForSecondsRealtime(0.5f);
                        yield return StartCoroutine(AttackPlayer(nextActions[i].value, null));
                    }
                    break;
                case EnumTypes.enemyActionType.shield:
                    yield return StartCoroutine(GetShieldEnemy(nextActions[i].value));
                    break;
                case EnumTypes.enemyActionType.heal:
                    yield return StartCoroutine(GetHealEnemy(nextActions[i].value));
                    break;
                case EnumTypes.enemyActionType.action:
                    int abilityIndex = nextActions[i].abilityIndex;
                    yield return EnemyAbilityFunction.Instance.EnemyAbilityAction(this, player, abilityIndex);
                    break;
            }
        }
        //end of turn
        yield return new WaitForSecondsRealtime(0.5f);
        StartCoroutine(EndTurn());
        yield return null;
    }
    public IEnumerator EndTurn()
    {
        //디버프
        SetDebuffEndTurn();
        yield return null;
    }
    public IEnumerator AttackTutorial(int amount)
    {
        CardSystem.Instance.tuBattle.GetComponent<TutorialBattle>().GetDamage(amount);
        var tween = this.transform.GetChild(1).DOLocalMove(new Vector2(0, -2f), 0.1f).SetLoops(2, LoopType.Yoyo);

        CardSystem.Instance.tuBattle.GetComponent<TutorialBattle>().SetGetDamageAnimation();

        yield return tween.WaitForCompletion();
    }
    private void OnAnimationComplete(Spine.TrackEntry trackEntry)
    {
        if (trackEntry.Animation.Name == "attack")
        {
            // attack이 끝난 경우만
            enemyImage.AnimationState.SetAnimation(0, "idle", true); // idle, loop on
        }
    }
    private void OnDestroy()
    {
        // 이벤트 해제
        enemyImage.AnimationState.Complete -= OnAnimationComplete;
    }
    public IEnumerator AttackPlayer(int amount, Dictionary<string, object> extraData)
    {
        yield return null;
        if (isDie) yield break;

        int calAmount = DamageCal.AttackDamageCal(this, player, amount, true, extraData);

        if (CheckHaveBuffOrDebuff(EnumTypes.Status.debuff, 12))
        {
            //시야차단 디버프
            int ran = Random.Range(0, 10);
            if (ran < 3)
            {
                // 빗나감 로그 표시
                var missText = LogManager.Instance?.GetLocalizedText("character_attack_miss").FormatSmart(characterName);
                LogManager.Instance?.AddLogBattle(missText);
                yield break;
            }
        }

        if (CheckHaveBuffOrDebuff(EnumTypes.Status.debuff, 15))
        {
            //환술 디버프
            int ran = Random.Range(0, 10);
            if (ran < 3)
            {
                TakeDamageBase(amount, extraData, false);
                // 환술 로그 표시
                var selfText = LogManager.Instance.GetLocalizedText("character_self_attack").FormatSmart(characterName, calAmount);
                LogManager.Instance.AddLogBattle(selfText);
                yield break;
            }
        }


        //var tween = enemyImage.transform.DOLocalMove(new Vector2(0, -3f), 0.2f).SetLoops(2, LoopType.Yoyo);
        // enemyImage.transform.DOScale(new Vector2(1.5f, 1.5f) * ratio, 0.2f).SetLoops(2, LoopType.Yoyo).WaitForCompletion();
        // yield return new WaitForSecondsRealtime(0.2f);
        enemyImage.AnimationState.SetAnimation(0, "attack", false); // attack, loop off
        //yield return tween.WaitForCompletion();

        // 데미지 로그 표시
        var textDamage = LogManager.Instance.GetLocalizedText("character_attack").FormatSmart(characterName, player.characterName, calAmount);
        LogManager.Instance.AddLogBattle(textDamage);

        // 유물 효과 적용
        // 첫 피해 확인
        if (!player.isFirstDamage)
        {
            player.isFirstDamage = true;
            var damageResultByArtifact = ArtifactFunction.Instance.ArtifactGetDamageFirst(player, this);

            if (damageResultByArtifact != null && damageResultByArtifact.IsDamageNullified)
            {
                var text = LogManager.Instance.GetLocalizedText("character_attack_null").FormatSmart(characterName);
                LogManager.Instance.AddLogBattle(text);
                calAmount = 0;
            }
            if (damageResultByArtifact != null && damageResultByArtifact.DrawCard > 0)
            {
                // 카드 드로우
                StartCoroutine(CardSystem.Instance.DrawCard(damageResultByArtifact.DrawCard));
            }
        }

        // 플레이어가 피해를 입을때 동작 유물
        ArtifactFunction.Instance.ArtifactGetDamage(player, this);

        // 플레이어에게 피해 적용
        player.GetDamagePlayer(calAmount, extraData);
        player.SetGetDamageAnimation(1);

        //혼란
        if (CheckHaveBuffOrDebuff(EnumTypes.Status.debuff, 9))
        {
            //GetDamage(null ,GetBuffOrDebuffValue(EnumTypes.Status.debuff, 9), EnumTypes.EffectType.hit, false, null);
        }

        //만약 플레이어가 가시 버프 있으면
        if (player.CheckHaveBuffOrDebuff(EnumTypes.Status.buff, 6))
        {
            int amountSpire = player.GetBuffOrDebuffValue(EnumTypes.Status.buff, 6);
            GetDamage(null, amountSpire, EnumTypes.EffectType.hit, false, null);

            var reflectText = LogManager.Instance.GetLocalizedText("character_get_reflect_damage").FormatSmart(characterName, amountSpire);
            LogManager.Instance.AddLogBattle(reflectText);
        }
        yield return new WaitForSecondsRealtime(0.5f);
        //패시브 적용 - 되는 애들만 함
        yield return PassiveFunction.Instance.PassiveAction(EnumTypes.EnemyPassiveTrigger.attack_to_player, this, player);
        yield return null;
    }

    //튜토리얼 사망
    IEnumerator TutorialDie()
    {
        var enemyImg = transform.GetChild(1).GetComponent<SpriteRenderer>();
        var tween = enemyImg.DOFade(0, 0.5f).SetEase(Ease.InCirc);
        yield return tween.WaitForCompletion();
        yield return new WaitForSecondsRealtime(0.2f);
        CardSystem.Instance.tuBattle.GetComponent<TutorialBattle>().SetVictoryBox();
    }
    public void DeleteObject()
    {
        //몬스터 사망 이펙트
        var spawnPosition = effectsPos.transform.position + new Vector3(0, -1.5f, 0);

        GameObject effectObj = EffectPoolManager.Instance.SpawnFromPool(EnumTypes.EffectType.monster_die, spawnPosition, Quaternion.identity);

        if (effectObj != null && effectObj.TryGetComponent<ParticleSystem>(out var ps))
        {
            ps.Play();
        }

        //업적
        //AchieveManager.instance.AchieveCheck("Battle_Fight", 1);

        if (EnemyManager.Instance.enemies.Count <= 1) { BattleManager.Instance.isVictory = true; BattleManager.Instance.SetCardButton(false); }

        //적 데이터 삭제하기
        EnemyManager.Instance.EnemyDie(this);
    }
    public void SetNextAction(bool canAction = true, int seed = 1)
    {
        nextActions.Clear();
        for (int i = 0; i < nextActionShow.Length; i++)
        {
            nextActionShow[i].gameObject.SetActive(false);
        }

        if (canAction)
        {
            EnumTypes.enemyActionType[] actions = new EnumTypes.enemyActionType[] {
                EnumTypes.enemyActionType.attack, EnumTypes.enemyActionType.attack,
                EnumTypes.enemyActionType.shield, EnumTypes.enemyActionType.shield,
                EnumTypes.enemyActionType.heal,
                EnumTypes.enemyActionType.action, EnumTypes.enemyActionType.action
            };
            var data = UserData.Instance.MainScenarioData;
            int ranSeed = data.GenerateSeed % 100007 + data.MapSeed % 9907 + seed + count * 71 + enemyIndex * 37;
            if (ranSeed >= 100000) ranSeed = ranSeed / 1000;
            System.Random random = new(ranSeed);
            count++;
            if (enemyDTO.EnemyAbilities.Count == 0)
            {
                actions = new EnumTypes.enemyActionType[] {
                    EnumTypes.enemyActionType.attack, EnumTypes.enemyActionType.attack,
                    EnumTypes.enemyActionType.shield, EnumTypes.enemyActionType.shield,
                    EnumTypes.enemyActionType.heal
                };
                Debug.Log("능력 없음");
            }

            //섞기
            for (int i = 0; i < actions.Length; i++)
            {
                int ran = random.Next(0, actions.Length);
                (actions[i], actions[ran]) = (actions[ran], actions[i]);
            }

            int actionCount = random.Next(0, 5); // 1 2 2 3 3
            if (actionCount == 0) actionCount = 2;
            else if (actionCount == 4) actionCount = 3;

            //다음 행동 세팅
            for (int k = 0; k < actionCount; k++)
            {
                bool amountBlind = random.Next(0, 3) == 0; // 0-안보임 나머지-보임
                bool isBlindIcon = random.Next(0, 4) == 0; // 0-안보임 나머지-보임
                if (player != null && player.CheckHaveBuffOrDebuff(EnumTypes.Status.buff, 25)) { amountBlind = false; }

                int amount = 0;
                if (actions[k] == EnumTypes.enemyActionType.attack)
                {
                    amount = random.Next(enemyDTO.AttackMin, enemyDTO.AttackMax + 1);
                }
                else if (actions[k] == EnumTypes.enemyActionType.shield)
                {
                    amount = random.Next(enemyDTO.DefenseMin, enemyDTO.DefenseMax + 1);
                }
                else if (actions[k] == EnumTypes.enemyActionType.heal)
                {
                    amount = random.Next(enemyDTO.HealMin, enemyDTO.HealMax + 1);
                }

                nextActions.Add(new()
                {
                    actionType = actions[k],
                    value = amount,
                    isAmountBlind = amountBlind,
                    isBlindIcon = isBlindIcon,
                    abilityIndex = actions[k] == EnumTypes.enemyActionType.action ? random.Next(0, enemyDTO.EnemyAbilities.Count) : 0
                });

                //
                nextActionShow[k].gameObject.SetActive(true);
                if (actionCount == 1) nextActionShow[k].transform.localPosition = new Vector3(0, -1.8f, 0);
                else if (actionCount == 2) nextActionShow[k].transform.localPosition = new Vector3(-0.8f + (k * 1.6f), -1.8f, 0);
                else if (actionCount == 3) nextActionShow[k].transform.localPosition = new Vector3(-1.6f + (k * 1.6f), -1.8f, 0);
            }
        }
        else
        {
            Debug.Log("봉인");
        }
        var artifactAction = ArtifactFunction.Instance.ArtifactPredictEnemyAction(player, this);

        if (artifactAction != null && artifactAction.PredictEnemyACtion)
        {
            ReSetAmount(true);
        }
        else
        {
            ReSetAmount();
        }

    }

    IEnumerator FlaverText()
    {
        if (isDie) yield break;
        int ranNum = Random.Range(0, flavorTexts.Length);
        transform.GetChild(0).GetChild(6).GetChild(1).GetComponent<TextMeshProUGUI>().text = flavorTexts[ranNum];
        int ranTime = Random.Range(3, 5);
        yield return new WaitForSecondsRealtime(ranTime);
        cor = FlaverText();
        StartCoroutine(cor);
    }
    public IEnumerator GetHealEnemy(int num)
    {
        GetHealBase(num);

        // 적 몬스터는 회복 모션 넣기
        transform.DOKill();
        transform.GetChild(1).DOScale(new Vector2(1.5f, 1.5f) * ratio, 0.2f).SetLoops(2, LoopType.Yoyo).WaitForCompletion();
        yield return new WaitForSecondsRealtime(0.2f);
    }
    public IEnumerator GetShieldEnemy(int amount)
    {
        GetShieldBase(amount);

        // 적 몬스터는 방어 모션 넣기
        transform.DOKill();
        transform.GetChild(1).DOScale(new Vector2(1.5f, 1.5f) * ratio, 0.2f).SetLoops(2, LoopType.Yoyo).WaitForCompletion();
        yield return new WaitForSecondsRealtime(0.2f);
    }
    public void ReSetAmount(bool showAll = false)
    {
        for (int i = 0; i < nextActions.Count; i++)
        {
            var actionType = nextActions[i].actionType;
            int originAmount = nextActions[i].value;
            int reAmount = originAmount;
            bool isBlind = showAll ? false : nextActions[i].isAmountBlind;
            bool isBlindIcon = showAll ? false : nextActions[i].isBlindIcon;

            if (isBlindIcon)
            {
                nextActionShow[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().gameObject.SetActive(false);
                nextActionShow[i].transform.GetChild(0).GetComponent<Image>().sprite = questionMarkIcon;
                continue;
            }
            else
            {
                nextActionShow[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().gameObject.SetActive(true);
            }


            var amountText = nextActionShow[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>();

            if (actionType == EnumTypes.enemyActionType.attack)
            {
                reAmount = DamageCal.AttackDamageCal(this, player, originAmount, true, null);
                nextActionShow[i].transform.GetChild(0).GetComponent<Image>().sprite = attackIcon;
                //text
                if (!isBlind)
                {
                    if (reAmount < originAmount)
                    {
                        amountText.text = "<color=green>" + reAmount.ToString() + "</color>";
                    }
                    else if (reAmount > originAmount)
                    {
                        amountText.text = "<color=red>" + reAmount.ToString() + "</color>";
                    }
                    else
                    {
                        amountText.text = reAmount.ToString();
                    }
                }
                else
                {
                    amountText.text = "?";
                }

            }
            else if (actionType == EnumTypes.enemyActionType.shield)
            {
                nextActionShow[i].transform.GetChild(0).GetComponent<Image>().sprite = shieldIcon;
                amountText.text = !isBlind ? reAmount.ToString() : "?";
            }
            else if (actionType == EnumTypes.enemyActionType.heal)
            {
                nextActionShow[i].transform.GetChild(0).GetComponent<Image>().sprite = healIcon;
                amountText.text = !isBlind ? reAmount.ToString() : "?";
            }
            else if (actionType == EnumTypes.enemyActionType.action)
            {
                amountText.gameObject.SetActive(false);

                string enemyTier = enemyDTO.Stage.Split("_")[0];
                if (enemyTier == "elite")
                {
                    nextActionShow[i].transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>("Image/Icon/icon_EnemyAttack02_purple");
                }
                else if (enemyTier == "boss")
                {
                    nextActionShow[i].transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>("Image/Icon/icon_EnemyAttack03_purple");
                }
                else
                {
                    nextActionShow[i].transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>("Image/Icon/icon_EnemyAttack01_purple");
                }
            }
        }
    }
    public void SetShowActionDetail(int index)
    {
        nextActionBox.SetActive(true);
        //SFX
        AudioManager.Instance.ButtonClickSound1();

        string type = "";

        if (nextActions[index].isBlindIcon)
        {
            type = "enemy_action_unknown_detail";
            nextActionBox.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = LogManager.Instance.GetLocalizedText(type);
        }
        else
        {
            if (nextActions[index].actionType == EnumTypes.enemyActionType.action)
            {
                int abilityIndex = nextActions[index].abilityIndex;
                List<int> abilityValues = new();
                foreach (var ability in enemyDTO.EnemyAbilities[abilityIndex].Abilities)
                {
                    abilityValues.Add(ability.Value);
                }

                type = enemyDTO.EnemyAbilities[abilityIndex].Text.FormatSmart(args: abilityValues.Cast<object>().ToArray());
                nextActionBox.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = type;
            }
            else
            {
                switch (nextActions[index].actionType)
                {
                    case EnumTypes.enemyActionType.attack:
                        type = "enemy_action_attack_detail";
                        break;
                    case EnumTypes.enemyActionType.shield:
                        type = "enemy_action_shield_detail";
                        break;
                    case EnumTypes.enemyActionType.heal:
                        type = "enemy_action_heal_detail";
                        break;
                }
                nextActionBox.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = LogManager.Instance.GetLocalizedText(type);
            }
        }
    }

    public void GetStatusEnemy(int statusId, EnumTypes.Status type, int value = 1)
    {
        GetStatusBase(statusId, type, value);
        EquipmentFunction.Instance.SetStatusEquipmentAction(statusId, type, this);
        //var statusText = LogManager.Instance.GetLocalizedText("character_status_get").FormatSmart(characterName, statusId, value);
    }

    public void StopCoroutine()
    {
        if (cor != null)
        {
            StopCoroutine(cor);
            cor = null;
        }
    }

    private void CheckText()
    {
        isText = InGameData.Instance.EnemyTexts.Find(x => x.EnemyId == enemyDTO.Id) != null;

        if (enemyDTO.Id == 14)
        {
            isText = InGameData.Instance.EnemyTexts.Find(x => x.EnemyId == 13) != null;
        }

        Debug.Log($"적 텍스트 존재 여부: {isText}");
    }
}
