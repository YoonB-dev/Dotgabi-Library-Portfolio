using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.SmartFormat;
using UnityEngine.UI;
using Random = UnityEngine.Random;


public class LogManager : SceneSingleton<LogManager>
{
    public GameObject content, txtPrefab;
    public ScrollRect scrollRect;
    public bool isDetail = false;
    Coroutine SetPivotCoroutine;
    public bool canLogMove; // Log 이동 가능 여부
    public Canvas footerCanvas;
    public readonly string MAIN_LOG_TABLE = "MainSceneTable";
    public readonly string BATTLE_LOG_TABLE = "BattleLogTable";
    public readonly string LOCAL_TABLE = "LocalTable";

    public string GetMainLogText(string tableKey)
    {
        return new LocalizedString(MAIN_LOG_TABLE, tableKey).GetLocalizedString();
    }
    public string GetLocalizedText(string tableKey)
    {
        return new LocalizedString(BATTLE_LOG_TABLE, tableKey).GetLocalizedString();
    }
    public string GetLocalText(string tableKey)
    {
        return new LocalizedString(LOCAL_TABLE, tableKey).GetLocalizedString();
    }
    public string GetDBLogText(EnumTypes.LogActionType logActionType)
    {
        var log = InGameData.Instance.Logs.FirstOrDefault(l => l.LogAction == logActionType);
        if (log != null)
        {
            return log.LogText;
        }
        return string.Empty; // 해당하는 로그가 없을 경우 빈 문자열 반환
    }

    public void AddLogBattle(string text)
    {
        var logObj = Instantiate(txtPrefab, content.transform);

        logObj.GetComponent<TextMeshProUGUI>().text = text;
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0;
    }

    public void AddLogMain(string text)
    {
        var logObj = Instantiate(txtPrefab, content.transform);

        var ranText = text.Split("&&");
        if (ranText.Length > 1)
        {
            text = ranText[Random.Range(0, ranText.Length)];
        }

        logObj.GetComponent<TextMeshProUGUI>().text = text;
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0;
    }

    public void ShowDetail()
    {
        if (!isDetail)
        {
            scrollRect.GetComponent<RectTransform>().DOSizeDelta(new Vector2(0, 600), 0.5f).SetEase(Ease.OutBack);
            scrollRect.transform.GetChild(0).GetComponent<RectTransform>().DOSizeDelta(new Vector2(0, 600), 0.5f).SetEase(Ease.OutBack);
            scrollRect.transform.parent.GetComponent<Button>().enabled = true;
            scrollRect.transform.parent.GetComponent<Image>().enabled = true;
            //컨텐츠 엥커 피벗 조절
            if (scrollRect.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>().rect.height < 600)
            {
                scrollRect.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>().anchorMin = new Vector2(0f, 1);
                scrollRect.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
                scrollRect.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1);
            }
            else
            {
                scrollRect.verticalNormalizedPosition = 0;
            }
            if (SetPivotCoroutine != null)
            {
                StopCoroutine(SetPivotCoroutine);
            }
            if (canLogMove && footerCanvas != null)
            {
                footerCanvas.sortingOrder = 6;
            }
        }
        else
        {
            scrollRect.GetComponent<RectTransform>().DOSizeDelta(new Vector2(0, 50), 0.5f).SetEase(Ease.OutBack);
            scrollRect.transform.GetChild(0).GetComponent<RectTransform>().DOSizeDelta(new Vector2(0, 50), 0.5f).SetEase(Ease.OutBack);
            scrollRect.transform.parent.GetComponent<Button>().enabled = false;
            scrollRect.transform.parent.GetComponent<Image>().enabled = false;
            //컨텐츠 엥커 피벗 조절
            if (SetPivotCoroutine != null)
            {
                StopCoroutine(SetPivotCoroutine);
            }
            SetPivotCoroutine = StartCoroutine(SetPivot());
        }
        isDetail = !isDetail;
    }
    IEnumerator SetPivot()
    {
        yield return new WaitForSecondsRealtime(0.3f);
        scrollRect.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
        scrollRect.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>().anchorMax = new Vector2(1, 0);
        scrollRect.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0);
        //이동 씬일때 캔버스 정렬 순서 조절
        if (canLogMove && footerCanvas != null)
        {
            footerCanvas.sortingOrder = 0;
        }
    }


    /// <summary>
    /// 로그 데이터 초기화
    /// 이동 씬에서만 사용 -> 메인 시나리오
    /// </summary>
    ///

    // DB에서 로그 데이터를 가져와서 초기화하는 메소드
    public void InitLogData(List<UserScenarioLogDTO> logList, Transform logGroupPosition = null)
    {
        // 로그 위치 지정
        if (logGroupPosition == null)
        {
            Debug.Log("Log group position not provided, using content transform.");
            logGroupPosition = content.transform;
        }

        // 기존 오브젝트 비활성화
        foreach (Transform child in logGroupPosition.transform)
        {
            child.gameObject.SetActive(false);
        }


        for (int i = 0; i < logList.Count; i++)
        {
            // 오브젝트 폴링
            GameObject logObj;
            if (i < logGroupPosition.transform.childCount)
            {
                logGroupPosition.transform.GetChild(i).gameObject.SetActive(true);
                logObj = logGroupPosition.transform.GetChild(i).gameObject;
            }
            else
            {
                logObj = Instantiate(txtPrefab, logGroupPosition, false);
            }
            var logData = logList[i];
            SetLogObjData(logObj, logData);
        }
    }

    public void SetLogObjData(GameObject logObj, UserScenarioLogDTO logData)
    {
        var logText = InGameData.Instance.Logs.Find(l => l.LogId == logData.LogId).LogText;
        // logText 가공
        var logAction = InGameData.Instance.Logs.Find(l => l.LogId == logData.LogId).LogAction;
        switch (logAction)
        {
            case EnumTypes.LogActionType.shop_buy:
                logText = PurchaseTextLog(logData, logText);
                break;
            case EnumTypes.LogActionType.player_get_something:
                logText = GetSomethingLog(logData, logText);
                break;
            case EnumTypes.LogActionType.player_lose_something:
                logText = LoseSomethingLog(logData, logText);
                break;
            case EnumTypes.LogActionType.player_use_something:
                logText = UseSomethingLog(logData, logText);
                break;
            case EnumTypes.LogActionType.card_upgrade:
                logText = UpgradeCardLog(logData, logText);
                break;
            case EnumTypes.LogActionType.card_delete:
                logText = DeleteCardLog(logData, logText);
                break;
            case EnumTypes.LogActionType.player_heal:
            case EnumTypes.LogActionType.player_get_max_hp:
            case EnumTypes.LogActionType.small_event_heal:
            case EnumTypes.LogActionType.small_event_coin:
                logText = HealPlayerLog(logData, logText);
                break;
            case EnumTypes.LogActionType.player_lose_max_hp:
            case EnumTypes.LogActionType.player_get_damage:
            case EnumTypes.LogActionType.small_event_damage:
                logText = DamagePlayerLog(logData, logText);
                break;
        }

        var newLogText = logText.Split("&&");
        if (newLogText.Length > 1)
        {
            logText = newLogText[Random.Range(0, newLogText.Length)];
        }
        logObj.GetComponent<TextMeshProUGUI>().text = logText;
    }

    private string PurchaseTextLog(UserScenarioLogDTO logDTO, string text)
    {
        if (logDTO.CardId != null)
        {
            var cardName = InGameData.Instance.Cards.Find(c => c.Id == logDTO.CardId)?.Name;
            if (cardName != null)
            {
                return text.FormatSmart(cardName);
            }
        }
        else if (logDTO.ArtifactId != null)
        {
            var artifactName = InGameData.Instance.Artifacts.Find(a => a.Id == logDTO.ArtifactId)?.Name;
            if (artifactName != null)
            {
                return text.FormatSmart(artifactName);
            }
        }
        return text;
    }

    private string GetSomethingLog(UserScenarioLogDTO logDTO, string text)
    {
        if (logDTO.CardId != null)
        {
            var cardDTO = InGameData.Instance.Cards.Find(c => c.Id == logDTO.CardId);

            if (cardDTO == null)
            {
                return text.FormatSmart("Unknown Card");
            }

            if (cardDTO.CardType == EnumTypes.CardType.curse)
            {
                return text.FormatSmart($"<color=red>{cardDTO.Name}</color>");
            }
            else
            {
                return text.FormatSmart($"<color=green>{cardDTO.Name}</color>");
            }
        }
        else if (logDTO.ArtifactId != null)
        {
            var artifactName = InGameData.Instance.Artifacts.Find(a => a.Id == logDTO.ArtifactId)?.Name;
            if (artifactName != null)
            {
                return text.FormatSmart($"<color=green>{artifactName}</color>");
            }
            else
            {
                return text.FormatSmart("Unknown Artifact");
            }
        }
        else if (logDTO.ExtraData != null && logDTO.ExtraData.ContainsKey("coin"))
        {
            // 코인 획득 로그
            int coinAmount = logDTO.value ?? 0;
            string money = GetLocalText("coin");
            return text.FormatSmart($"<color=green>{coinAmount} {money}</color>");
        }
        else if (logDTO.ExtraData != null && logDTO.ExtraData.ContainsKey("dotgabi_key"))
        {
            int keyId;
            bool success = int.TryParse(logDTO.ExtraData["dotgabi_key"].ToString(), out keyId);
            if (!success)
            {
                // 파싱 실패 시 처리 로직 (예: 기본값 할당, 에러로그 등)
                keyId = 0; // 기본값 예시
            }
            var keyData = InGameData.Instance.DotgabiKeys.Find(k => k.KeyId == keyId);
            if (keyData == null)
            {
                return text.FormatSmart("Unknown Dotgabi Key");
            }
            return text.FormatSmart($"<color=green>{keyData.KeyName}</color>");
        }

        return text;
    }

    private string LoseSomethingLog(UserScenarioLogDTO logDTO, string text)
    {
        if (logDTO.CardId != null)
        {
            var cardDTO = InGameData.Instance.Cards.Find(c => c.Id == logDTO.CardId);

            if (cardDTO == null)
            {
                return text.FormatSmart("Unknown Card");
            }

            if (cardDTO.CardType == EnumTypes.CardType.curse)
            {
                return text.FormatSmart($"<color=green>{cardDTO.Name}</color>");
            }
            else
            {
                return text.FormatSmart($"<color=red>{cardDTO.Name}</color>");
            }
        }
        else if (logDTO.ArtifactId != null)
        {
            var artifactName = InGameData.Instance.Artifacts.Find(a => a.Id == logDTO.ArtifactId)?.Name;
            if (artifactName != null)
            {
                return text.FormatSmart($"<color=red>{artifactName}</color>");
            }
            else
            {
                return text.FormatSmart("Unknown Artifact");
            }
        }
        else if (logDTO.ExtraData != null && logDTO.ExtraData.ContainsKey("coin"))
        {
            // 코인 사용 로그
            int coinAmount = -logDTO.value ?? 0;
            string money = GetLocalText("coin");
            return text.FormatSmart($"<color=red>{coinAmount} {money}</color>");
        }
        return text;
    }

    private string UseSomethingLog(UserScenarioLogDTO logDTO, string text)
    {
        if (logDTO.CardId != null)
        {
            var cardName = InGameData.Instance.Cards.Find(c => c.Id == logDTO.CardId)?.Name;
            if (cardName != null)
            {
                return text.FormatSmart($"<color=green>{cardName}</color>");
            }
        }
        else if (logDTO.ArtifactId != null)
        {
            var artifactName = InGameData.Instance.Artifacts.Find(a => a.Id == logDTO.ArtifactId)?.Name;
            if (artifactName != null)
            {
                return text.FormatSmart($"<color=green>{artifactName}</color>");
            }
        }
        else if (logDTO.ExtraData != null && logDTO.ExtraData.ContainsKey("coin"))
        {
            // 코인 사용 로그
            int coinAmount = logDTO.value ?? 0;
            string money = GetLocalText("coin");
            return text.FormatSmart($"<color=green>{coinAmount} {money}</color>");
        }
        return text;
    }

    private string UpgradeCardLog(UserScenarioLogDTO logDTO, string text)
    {
        if (logDTO.CardId != null)
        {
            var cardName = InGameData.Instance.Cards.Find(c => c.Id == logDTO.CardId)?.Name;

            for (int i = 0; i < logDTO.value; i++)
            {
                cardName += "+";
            }
            if (cardName != null)
            {
                return text.FormatSmart($"{cardName}");
            }
        }
        return text;
    }

    private string DeleteCardLog(UserScenarioLogDTO logDTO, string text)
    {
        if (logDTO.CardId != null)
        {
            var cardName = InGameData.Instance.Cards.Find(c => c.Id == logDTO.CardId)?.Name;

            for (int i = 0; i < logDTO.value; i++)
            {
                cardName += "+";
            }

            if (cardName != null)
            {
                return text.FormatSmart($"{cardName}");
            }
        }
        else if (logDTO.ArtifactId != null)
        {
            var artifactName = InGameData.Instance.Artifacts.Find(a => a.Id == logDTO.ArtifactId)?.Name;
            if (artifactName != null)
            {
                return text.FormatSmart($"{artifactName}");
            }
        }
        return text;
    }

    private string HealPlayerLog(UserScenarioLogDTO logDTO, string text)
    {
        if (logDTO.value != null)
        {
            return text.FormatSmart($"<color=green>{logDTO.value}</color>");
        }
        return text;
    }

    private string DamagePlayerLog(UserScenarioLogDTO logDTO, string text)
    {
        if (logDTO.value != null)
        {
            return text.FormatSmart($"<color=red>{logDTO.value}</color>");
        }
        return text;
    }

    public void SetLogMainScene(EnumTypes.LogActionType logActionType, UserScenarioLogDTO logData, ScenarioDTO scenarioData)
    {
        // 로그 Action을 이용해 ID값을 가져오기 -> ID는 나중에 바뀔 수 있기 때문에
        int logId = InGameData.Instance.Logs.Find(log => log.LogAction == logActionType)?.LogId ?? 0;
        logData.LogId = logId;
        // 로그 오브젝트 생성 및 데이터 설정
        var logObj = Instantiate(txtPrefab, content.transform);
        SetLogObjData(logObj, logData);

        if (logActionType == EnumTypes.LogActionType.move_forward)
        {
            if (scenarioData.LogList.Count > 1 && scenarioData.LogList.Last().LogId == logId)
            {
                // 임시 방편 -> 마지막과 로그가 둘다 forward라면 중복 방지
                return; // 중복 방지
            }
        }

        SupabaseLog.Instance.LogUserAction(UserData.Instance.UserAuthId, logData, scenarioType: scenarioData);
    }
}
