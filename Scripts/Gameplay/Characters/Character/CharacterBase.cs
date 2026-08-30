using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DG.Tweening;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Localization.SmartFormat;
using UnityEngine.UI;

public class CharacterBase : MonoBehaviour
{
    public Stat Stats;
    public string characterName;
    public List<StatusData> StatusList { get; set; } = new();
    public List<EquipDTO> EquipList { get; set; } = new();
    public List<UserScenarioOwnedArtifactDTO> ArtifactList { get; set; } = new();
    public List<EnemyPassiveDTO> PassiveList { get; set; } = new();
    public bool isDie = false;
    public bool isTurn { get; set; } = false;

    [Header("Character Base Stat Text")]
    [SerializeField] private TextMeshProUGUI maxHpTxt;
    [SerializeField] private TextMeshProUGUI currHpTxt;
    [SerializeField] private TextMeshProUGUI currShieldTxt;

    [Header("Character Base Stat Image")]
    [SerializeField] private Image hpBarImg;
    [SerializeField] private Image hpBarShadowImg;
    [SerializeField] private Image shieldImg;
    [SerializeField] Sprite originHpBarSprite, changeHpBarSprite;

    [Header("Character Status Icon and Pos")]
    [SerializeField] private GameObject statusIconPrefab;
    [SerializeField] private Transform statusIconPos;

    [Header("Character Move Text")]
    [SerializeField] private Transform moveTextPos;
    [SerializeField] private GameObject moveTextPrefab;

    private List<TextMeshProUGUI> moveTextList = new();

    // Tween
    Tween hpBarTween;
    Sequence moveTextTween;
    public virtual void TakeDamageBase(int damage, Dictionary<string, object> extraData, bool isLog = true)
    {
        if (isDie) return;

        int shieldDamageRate = 1;
        // 방어력 배수
        if (extraData != null && extraData.ContainsKey("shield_attack"))
        {
            shieldDamageRate = int.Parse(extraData["shield_attack"].ToString());
            Debug.Log("Shield Damage Rate: " + shieldDamageRate);
        }
        if (extraData != null && extraData.ContainsKey("pure_damage"))
        {
            TakeDamagePure(damage);
            Debug.Log("관통 데미지: " + damage);
            if (isLog)
            {
                var damageText = LogManager.Instance?.GetLocalizedText("character_get_pure_damage").FormatSmart(characterName, damage);
                LogManager.Instance?.AddLogBattle(damageText);
            }
            return;
        }

        int shield = Stats.currShield;
        if (Stats.currShield > 0)
        {
            if (damage * shieldDamageRate < Stats.currShield)
            {
                Stats.currShield -= damage * shieldDamageRate;
                damage = 0;
            }
            else
            {
                int newDam = damage * shieldDamageRate - Stats.currShield;
                Debug.Log(newDam + " - " + shieldDamageRate);
                damage = newDam / shieldDamageRate;
                Stats.currShield = 0;
            }
        }

        if (isLog)
        {
            var damageText = LogManager.Instance?.GetLocalizedText("character_get_damage").FormatSmart(characterName, damage + shield);
            LogManager.Instance?.AddLogBattle(damageText);
        }


        TakeDamagePure(damage);
    }

    public void TakeDamagePure(int damage)
    {
        if (isDie) return;

        if (Stats.isChange)
        {
            Stats.changeCurrHp -= damage;
            if (Stats.changeCurrHp < 0)
            {
                Stats.changeCurrHp = 0;
                Stats.isChange = false;
            }
        }
        else
        {
            Stats.currHp -= damage;

            if (Stats.currHp <= 0)
            {
                // 버티기 효과 확인
                if (StatusList.Find(x => x.statusDTO.Id == 10 && x.statusDTO.StatusType == EnumTypes.Status.buff) != null)
                {
                    Stats.currHp = 1;
                }
                else
                {
                    Debug.Log("Character is dead");
                    Stats.currHp = 0;
                    isDie = true;
                }
            }
        }
        StartCoroutine(HpbarMotion(EnumTypes.TextMotionType.down));
        SetMoveTextBase(damage.ToString(), EnumTypes.TextMotionType.up, EnumTypes.MoveTextType.damage, 1);
    }

    public void GetShieldBase(int shieldAmount)
    {
        if (isDie) return;

        // 방어불가가 있는 경우
        if (CheckHaveBuffOrDebuff(EnumTypes.Status.debuff, 14))
        {
            //SFX
            AudioManager.Instance.MissSound();
            return;
        }
        //SFX
        AudioManager.Instance.GetShieldSound();
        // 방어 강화 버프가 있는 경우 2배 방어도
        shieldAmount = CheckHaveBuffOrDebuff(EnumTypes.Status.buff, 14) ? shieldAmount * 2 : shieldAmount;

        Stats.currShield += shieldAmount;
        if (Stats.currShield < 0)
        {
            Stats.currShield = 0;
            shieldImg.transform.gameObject.SetActive(false);
        }
        else
        {
            shieldImg.transform.gameObject.SetActive(true);
            shieldImg.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = Stats.currShield.ToString();
        }

        // 로그
        var shieldText = LogManager.Instance?.GetLocalizedText("character_get_shield").FormatSmart(characterName, shieldAmount);
        LogManager.Instance?.AddLogBattle(shieldText);
    }
    public void ShieldBreakBase()
    {
        if (isDie) return;
        Stats.currShield = 0;
        SetStatText();
    }

    public void GetHealBase(int amount)
    {
        if (isDie) return;
        //SFX
        AudioManager.Instance.HealSound();

        if (ArtifactList.Find(x => x.ArtifactId == 44) != null)
        {
            amount += 2; // 유물 효과로 회복량 증가
        }

        Stats.currHp += amount;
        if (Stats.currHp > Stats.maxHp)
        {
            Stats.currHp = Stats.maxHp;
        }

        // 로그
        var healText = LogManager.Instance?.GetLocalizedText("character_get_heal").FormatSmart(characterName, amount);
        LogManager.Instance?.AddLogBattle(healText);

        // Update UI
        SetStatText();
        StartCoroutine(HpbarMotion(EnumTypes.TextMotionType.up));
        SetMoveTextBase(amount.ToString(), EnumTypes.TextMotionType.up, EnumTypes.MoveTextType.heal, 1);
    }
    // Sets the text for the character's stats
    public void SetStatText()
    {
        maxHpTxt.text = Stats.maxHp.ToString();
        currHpTxt.text = Stats.currHp.ToString();

        currShieldTxt.transform.parent.gameObject.SetActive(Stats.currShield > 0);
        currShieldTxt.text = Stats.currShield.ToString();

        hpBarImg.fillAmount = (float)Stats.currHp / Stats.maxHp;
        hpBarShadowImg.fillAmount = (float)Stats.currHp / Stats.maxHp;
    }
    // Hp의 바 모션을 위한 코루틴
    public IEnumerator HpbarMotion(EnumTypes.TextMotionType motionType)
    {
        //쉴드 세팅
        if (Stats.currShield != 0) currShieldTxt.text = Stats.currShield.ToString();
        else { currShieldTxt.text = ""; currShieldTxt.transform.parent.gameObject.SetActive(false); }
        // 변신인지 그냥인지
        int MaxHpInt = Stats.isChange ? Stats.changeMaxHp : Stats.maxHp;
        int CurrHpInt = Stats.isChange ? Stats.changeCurrHp : Stats.currHp;

        //체력 세팅
        maxHpTxt.text = MaxHpInt.ToString();
        currHpTxt.text = CurrHpInt.ToString();

        switch (motionType)
        {
            case EnumTypes.TextMotionType.direct:
                hpBarImg.fillAmount = (float)CurrHpInt / MaxHpInt; // 체력바 세팅
                hpBarShadowImg.fillAmount = (float)CurrHpInt / MaxHpInt; // 체력바 shadow 세팅
                break;
            case EnumTypes.TextMotionType.up:
                hpBarShadowImg.fillAmount = (float)CurrHpInt / MaxHpInt; // 체력바 shadow 세팅
                hpBarTween = hpBarImg.DOFillAmount((float)CurrHpInt / (float)MaxHpInt, 0.5f); // 체력바 세팅
                yield return new WaitForSecondsRealtime(0.5f);
                break;
            case EnumTypes.TextMotionType.down:
                hpBarImg.fillAmount = (float)CurrHpInt / MaxHpInt; // 체력바 세팅
                hpBarTween =hpBarShadowImg.DOFillAmount((float)CurrHpInt / (float)MaxHpInt, 0.5f); // 체력바 shadow 세팅
                yield return new WaitForSecondsRealtime(0.5f);
                break;
        }
        yield return null;
    }
    public void SetCharacterChangeBase(int changeCurrHp, int changeMaxHp)
    {
        if (isDie) return;

        Stats.isChange = true;
        Stats.changeCurrHp = changeCurrHp;
        Stats.changeMaxHp = changeMaxHp;
        SetHpBarChangeBase(true);
    }
    public void SetHpBarChangeBase(bool isChange)
    {
        hpBarImg.sprite = isChange ? changeHpBarSprite : originHpBarSprite;
        hpBarShadowImg.sprite = isChange ? changeHpBarSprite : originHpBarSprite;
        StartCoroutine(HpbarMotion(EnumTypes.TextMotionType.direct));
    }

    public void GetMaxHpBase(int amount)
    {
        if (isDie) return;

        Stats.maxHp += amount;
        Stats.currHp += amount;
        if (Stats.currHp > Stats.maxHp)
        {
            Stats.currHp = Stats.maxHp;
        }
        SetStatText();
        StartCoroutine(HpbarMotion(EnumTypes.TextMotionType.up));
        SetMoveTextBase(amount.ToString(), EnumTypes.TextMotionType.up, EnumTypes.MoveTextType.heal, 1);
    }

    /// <summary>
    /// 버프 디버프 관련 메서드
    /// </summary>

    public void GetStatusBase(int statusId, EnumTypes.Status type, int value = 1)
    {
        if (isDie) return;

        Debug.Log("GetStatusBase: " + statusId + ", Type: " + type + ", Value: " + value);

        var statusDTO = type == EnumTypes.Status.buff ? InGameData.Instance.Buffs.Find(x => x.Id == statusId) : InGameData.Instance.Debuffs.Find(x => x.Id == statusId);
        if (statusDTO != null)
        {
            if (StatusList.Find(x => x.statusDTO.Id == statusId && x.statusDTO.StatusType == type) != null)
            {
                // 이미 있는 디버프는 값만 증가
                SetBuffOrDebuffValue(type, statusId, value);
                return;
            }

            StatusList.Add(new StatusData(statusDTO, value));
            SetStatusIcon();
        }
        else
        {
            Debug.LogWarning("Status not found: " + statusId);
        }

        // 로그
        string statusName = type == EnumTypes.Status.buff ? $"<color=green>{InGameData.Instance?.Buffs.Find(x => x.Id == statusId).Name}</color>" : $"<color=red>{InGameData.Instance?.Debuffs.Find(x => x.Id == statusId).Name}</color>";
        var statusText = LogManager.Instance?.GetLocalizedText("character_get_status").FormatSmart(characterName, statusName, value);
        LogManager.Instance?.AddLogBattle(statusText);
    }

    public void GetEquipBase(int equipId, int upgradeTime)
    {
        if (isDie) return;

        var cardDTO = InGameData.Instance.Cards.Find(x => x.Id == equipId).Copy();
        cardDTO.CardUpgrade = upgradeTime;
        EquipDTO equipDTO = new EquipDTO {
            cardDTO = cardDTO,
            equipAmount = 0 // 기본값 0 설정
        };
        EquipList.Add(equipDTO);
        SetStatusIcon();
    }
    public void GetPassiveBase(EnemyPassiveDTO passiveDTO)
    {
        if (isDie) return;

        // 이미 있는 패시브는 추가하지 않음
        if (PassiveList.Find(x => x.PassiveId == passiveDTO.PassiveId) != null) return;

        PassiveList.Add(passiveDTO);
        SetStatusIcon();
    }

    public bool CheckHaveBuffOrDebuff(EnumTypes.Status type, int statusId)
    {
        return StatusList.Exists(x => x.statusDTO.Id == statusId && x.statusDTO.StatusType == type);
    }
    public int GetBuffOrDebuffValue(EnumTypes.Status type, int statusId)
    {
        int amount = 0;

        var status = StatusList.FindAll(x => x.statusDTO.Id == statusId && x.statusDTO.StatusType == type).ToList();
        foreach (var s in status)
        {
            amount += s.statusValue;
        }

        return amount;
    }
    public void SetBuffOrDebuffValue(EnumTypes.Status type, int statusId, int amount)
    {
        var status = StatusList.FindAll(x => x.statusDTO.Id == statusId && x.statusDTO.StatusType == type);
        foreach (var s in status)
        {
            s.statusValue += amount;
            if (s.statusValue <= 0)
            {
                StatusList.Remove(s);
            }
        }
        SetStatusIcon();
    }
    public void DeleteStatusBase(int statusId, EnumTypes.Status type, int amount)
    {
        if (isDie) return;

        var status = StatusList.FindAll(x => x.statusDTO.Id == statusId && x.statusDTO.StatusType == type);
        foreach (var s in status)
        {
            s.statusValue -= amount;
            if (s.statusValue <= 0)
            {
                StatusList.Remove(s);
            }
        }
        SetStatusIcon();
    }
    public void SetStatusIcon()
    {
        // Clear existing icons
        foreach (Transform child in statusIconPos)
        {
            child.gameObject.SetActive(false);
        }

        // Add Buffs and Debuffs
        int count = StatusList.Count + EquipList.Count + ArtifactList.Count + PassiveList.Count;
        if (count > statusIconPos.childCount)
        {
            for (int i = statusIconPos.childCount; i < count; i++)
            {
                var icon = Instantiate(statusIconPrefab, statusIconPos);
                icon.SetActive(false);
            }
        }

        // Passive 아이콘 세팅
        int passiveCount = 0;
        for (int i = 0; i < PassiveList.Count; i++)
        {
            var Icon = statusIconPos.GetChild(passiveCount);
            Icon.gameObject.SetActive(true);
            Icon.GetComponent<Image>().sprite = Resources.Load<Sprite>(PassiveList[i].PassiveImgPath);
            Icon.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "";
            Icon.GetComponent<Button>().onClick.RemoveAllListeners();
            int temp = i;
            Icon.GetComponent<Button>().onClick.AddListener(() => {
                // 설명 UI 띄우는 기능 추가
                Vector3 screenPos = Camera.main.WorldToScreenPoint(Icon.transform.position);
                NotificationManager.Instance.SetTextBox(PassiveList[temp].PassiveText, screenPos, EnumTypes.TextMotionType.up);
            });
            passiveCount++;
        }


        int activeArtifactCount = passiveCount;
        // Artifact 아이콘 세팅
        for (int i = 0; i < ArtifactList.Count; i++)
        {
            ArtifactDTO artifact = InGameData.Instance.Artifacts.Find(a => a.Id == ArtifactList[i].ArtifactId);
            if (!artifact.IsIcon || ArtifactList[i].IsUse) continue; // 아이콘이 없는 경우 혹은 이미 사용된 경우는 건너뜀
            var Icon = statusIconPos.GetChild(activeArtifactCount);
            Icon.gameObject.SetActive(true);
            Icon.GetComponent<Image>().sprite = Resources.Load<Sprite>(artifact.ImageUrl);
            Icon.transform.GetChild(0).GetComponent<TextMeshProUGUI>().gameObject.SetActive(false);
            // 여기 클릭하면 설명 추가 기능 넣기
            Icon.GetComponent<Button>().onClick.RemoveAllListeners();
            Icon.GetComponent<Button>().onClick.AddListener(() => {
                // 설명 UI 띄우는 기능 추가
                Vector3 screenPos = Camera.main.WorldToScreenPoint(Icon.transform.position);
                NotificationManager.Instance.SetTextBox(artifact.Ability, screenPos, EnumTypes.TextMotionType.up);
            });
            activeArtifactCount++;
        }

        // Equip 아이콘 세팅
        int equipCount = activeArtifactCount;
        for (int i = 0; i < EquipList.Count; i++)
        {
            var Icon = statusIconPos.GetChild(equipCount);
            Icon.gameObject.SetActive(true);
            Icon.GetComponent<Image>().sprite = Resources.Load<Sprite>(EquipList[i].cardDTO.ImageUrl);
            if (EquipList[i].cardDTO.CardActions[0].ExtraData != null && EquipList[i].cardDTO.CardActions[0].ExtraData.ContainsKey("amount_up"))
            {
                Icon.transform.GetChild(0).GetComponent<TextMeshProUGUI>().gameObject.SetActive(true);
                Icon.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = EquipList[i].equipAmount.ToString();
            }
            else
            {
                Icon.transform.GetChild(0).GetComponent<TextMeshProUGUI>().gameObject.SetActive(false);
            }
            // 여기 클릭하면 설명 추가 기능 넣기
            Icon.GetComponent<Button>().onClick.RemoveAllListeners();
            int temp = i;
            Icon.GetComponent<Button>().onClick.AddListener(() => {
                // 설명 UI 띄우는 기능 추가
                Vector3 screenPos = Camera.main.WorldToScreenPoint(Icon.transform.position);
                NotificationManager.Instance.SetTextBox(EquipList[temp].cardDTO.Description, screenPos, EnumTypes.TextMotionType.up);
            });
            equipCount++;
        }

        // status 아이콘 세팅
        int statusIndex = equipCount;
        for (int i = 0; i < StatusList.Count; i++)
        {
            var Icon = statusIconPos.GetChild(statusIndex);
            Icon.gameObject.SetActive(true);
            Icon.GetComponent<Image>().sprite = Resources.Load<Sprite>(StatusList[i].statusDTO.ImgPath);
            Icon.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = StatusList[i].statusValue.ToString();
            Icon.transform.GetChild(0).GetComponent<TextMeshProUGUI>().gameObject.SetActive(true);
            // 여기 클릭하면 설명 추가 기능 넣기
            Icon.GetComponent<Button>().onClick.RemoveAllListeners();
            int temp = i;
            Icon.GetComponent<Button>().onClick.AddListener(() => {
                // 설명 UI 띄우는 기능 추가
                Vector3 screenPos = Camera.main.WorldToScreenPoint(Icon.transform.position);
                NotificationManager.Instance.SetTextBox(StatusList[temp].statusDTO.Description, screenPos, EnumTypes.TextMotionType.up);
            });
            statusIndex++;
        }
    }

    public void SetBuffStartTurn()
    {
        var buffs = StatusList.FindAll(x => x.statusDTO.StatusType == EnumTypes.Status.buff);

        for (int i = 0; i < buffs.Count; i++)
        {
            switch (buffs[i].statusDTO.Id)
            {
                case 1: // 공격력 증가
                    break;
                case 2: // 피해량 감소
                    break;
                case 3: // 각성 - 주는 피해 1.5배 증가
                    buffs[i].statusValue--;
                    break;
                case 4: // 결의 - 적에게 받는 피해 30% 감소
                    buffs[i].statusValue--;
                    break;
                case 5: // 지속 회복
                    GetHealBase(buffs[i].statusValue);
                    buffs[i].statusValue--;
                    break;
                case 6: // 가시 - 피해 받으면 반격
                    buffs[i].statusValue = 0;
                    break;
                case 7: // 의심 - 피해량 1 고정
                    buffs[i].statusValue--;
                    break;
                case 8: // 무적
                    buffs[i].statusValue--;
                    break;
                case 9: // 회피
                    buffs[i].statusValue--;
                    break;
                case 10: // 버티기
                    buffs[i].statusValue--;
                    break;
                case 11: // 연속 행동
                    buffs[i].statusValue = 0;
                    break;
                case 12: // 연속 방어
                    GetShieldBase(buffs[i].statusValue);
                    buffs[i].statusValue = 0;
                    break;
                case 13: // 강화 방어
                    buffs[i].statusValue = 0;
                    break;
                case 14: // 역습 - 피해받으면 카드 뽑기
                    buffs[i].statusValue = 0;
                    break;
                case 15: // 복슬복슬
                    buffs[i].statusValue--;
                    break;
            }
        }
        // buffs 제거
        StatusList.RemoveAll(x => x.statusValue <= 0);
        SetStatusIcon();
    }

    public void SetDebuffEndTurn()
    {
        var debuffs = StatusList.FindAll(x => x.statusDTO.StatusType == EnumTypes.Status.debuff);
        for (int i = 0; i < debuffs.Count; i++)
        {
            switch (debuffs[i].statusDTO.Id)
            {
                case 1: // 공격력 감소
                    break;
                case 2: // 피해량 증가
                    break;
                case 3: // 약화 - 적에게 받는 피해량 1.5배 증가
                    debuffs[i].statusValue--;
                    break;
                case 4: // 위축 - 적에게 주는 피해량 30% 감소
                    debuffs[i].statusValue--;
                    break;
                case 5: // 출혈, 독, 화상 - 매 턴 피해
                case 6:
                case 7:
                    TakeDamageBase(debuffs[i].statusValue, null);
                    debuffs[i].statusValue--;
                    if (debuffs[i].statusDTO.Id == 5) { AudioManager.Instance.BloodSound(); }
                    else { AudioManager.Instance.HitSound(); }

                    break;
                case 8: // 과다출혈
                    TakeDamageBase(debuffs[i].statusValue, null);
                    debuffs[i].statusValue++;
                    if (debuffs[i].statusValue > 10)
                    {
                        debuffs[i].statusValue = 10; // 최대 10
                    }
                    AudioManager.Instance.BloodSound();
                    break;
                case 9: // 혼란
                    debuffs[i].statusValue = 0;
                    break;
                case 10: // 허약 - 주는 피해 1 고정
                    debuffs[i].statusValue = 0;
                    break;
                case 11: // 공포 - 턴 시작 시 행동력 잃기
                    debuffs[i].statusValue = 0;
                    break;
                case 12: // 시야 차단
                    debuffs[i].statusValue--;
                    break;
                case 13: // 밥풀
                    TakeDamageBase(debuffs[i].statusValue * 3, null);
                    //SFX
                    AudioManager.Instance.HitSound();
                    debuffs[i].statusValue = 0;
                    break;
                case 14: // 방어불가
                    debuffs[i].statusValue--;
                    break;
                case 15: // 환술 - 30% 자기 때리지
                    debuffs[i].statusValue--;
                    break;
            }
        }

        // debuffs 제거
        StatusList.RemoveAll(x => x.statusValue <= 0);
        SetStatusIcon();
    }


    // 텍스트 모션을 위한 메서드 - 피해 또는 회복 시 사용
    public void SetMoveTextBase(string text, EnumTypes.TextMotionType motionType, EnumTypes.MoveTextType textType, float time = 1f)
    {
        // 기존 텍스트 오브젝트가 있다면 재사용
        GameObject newTextObj = null;

        bool isExist = false;
        foreach (var moveText in moveTextList)
        {
            if (!moveText.gameObject.activeSelf)
            {
                newTextObj = moveText.gameObject;
                isExist = true;
                break;
            }
        }

        // 새 텍스트 오브젝트 생성
        if (!isExist)
        {
            newTextObj = Instantiate(moveTextPrefab, moveTextPos.position, Quaternion.identity, moveTextPos);
            moveTextList.Add(newTextObj.GetComponent<TextMeshProUGUI>());
        }

        var textMesh = newTextObj.GetComponent<TextMeshProUGUI>();
        textMesh.text = text;
        textMesh.alpha = 1f;
        newTextObj.SetActive(true);
        newTextObj.transform.position = moveTextPos.position; // 초기 위치 설정

        switch (textType)
        {
            case EnumTypes.MoveTextType.damage:
                textMesh.color = Color.red;
                break;
            case EnumTypes.MoveTextType.heal:
                textMesh.color = Color.green;
                break;
            case EnumTypes.MoveTextType.money:
                textMesh.color = Color.green;
                break;
            case EnumTypes.MoveTextType.none:
                textMesh.color = Color.white;
                break;
        }
        switch (motionType)
        {
            case EnumTypes.TextMotionType.up:
                moveTextTween = DOTween.Sequence();
                moveTextTween.SetLink(gameObject);
                moveTextTween.Append(newTextObj.transform.DOLocalJump(new Vector3(transform.localPosition.x + 50, transform.localPosition.y, 0), 50, 1, time));
                moveTextTween.Join(textMesh.DOFade(0.2f, time).SetEase(Ease.InCirc).OnComplete(() => newTextObj.SetActive(false)));
                break;
            case EnumTypes.TextMotionType.down:
                moveTextTween = DOTween.Sequence();
                moveTextTween.SetLink(gameObject);
                moveTextTween.Append(newTextObj.transform.DOLocalMoveY(moveTextPos.localPosition.y - 50, time).SetEase(Ease.InCirc));
                moveTextTween.Join(textMesh.DOFade(0.2f, time).SetEase(Ease.InCirc).OnComplete(() => newTextObj.SetActive(false)));
                break;
        }
    }

    // 랜덤 상태 부여
    public void GetRandomStatus(EnumTypes.Status statusType, bool isEnemy = false)
    {
        if (isDie) return;

        int randomStatusId = 1;
        int amount = 1;

        if (statusType == EnumTypes.Status.buff)
        {
            randomStatusId = UnityEngine.Random.Range(0, 13) + 1;

            if (isEnemy)
            {
                while (randomStatusId == 11)
                {
                    randomStatusId = UnityEngine.Random.Range(1, 14);
                }
            }

            switch (randomStatusId)
            {
                case 1:
                case 2:
                case 3:
                case 4:
                case 7:
                case 8:
                case 9:
                case 10:
                case 11:
                case 13:
                    amount = 1;
                    break;
                case 5:
                case 6:
                    amount = UnityEngine.Random.Range(2, 5); // 2~4 사이의 랜덤 값
                    break;
                case 12:
                    amount = UnityEngine.Random.Range(5, 8); // 5~7 사이의 랜덤 값
                    break;
            }
        }
        else if (statusType == EnumTypes.Status.debuff)
        {
            randomStatusId = UnityEngine.Random.Range(0, 15) + 1;
            amount = 1;
            switch (randomStatusId)
            {
                case 1:
                case 2:
                case 3:
                case 4:
                case 8:
                case 10:
                case 11:
                case 12:
                case 14:
                case 15:
                    amount = 1;
                    break;
                case 5:
                case 6:
                case 7:
                    amount = UnityEngine.Random.Range(2, 5); // 2~4 사이의 랜덤 값
                    break;
                case 9:
                    amount = UnityEngine.Random.Range(3, 6); // 3~5 사이의 랜덤 값
                    break;
                case 13:
                    amount = UnityEngine.Random.Range(2, 4); // 2~3 사이의 랜덤 값
                    break;
            }
        }
        GetStatusBase(randomStatusId, statusType, amount);
    }



    void OnDestroy()
    {
        DOTween.Kill(hpBarTween); // Kills all tweens related to this GameObject
    }
}
