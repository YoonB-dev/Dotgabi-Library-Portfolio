using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization.SmartFormat;

public class ScenarioArtifactUtils : Singleton<ScenarioArtifactUtils>
{
    public async Task<bool> GetArtifact(ArtifactDTO artifact, ScenarioDTO targetData, bool isLog = true)
    {
        //유물 획득 로고
        if (isLog)
        {
            var logData = new UserScenarioLogDTO {
                ArtifactId = artifact.Id,
            };
            LogManager.Instance?.SetLogMainScene(EnumTypes.LogActionType.player_get_something, logData, targetData);
        }
        bool result = await SupabaseArtifact.Instance.GetArtifact(artifact.Id, targetData);

        if (result)
        {
            // SFX
            AudioManager.Instance.GetItemSound();
            bool isUse = false;
            // 획득 시 효과 적용
            var artifactResult = ArtifactFunction.Instance.ArtifactOnObtainEffect(artifact);
            ObtainArtifactAction(artifactResult, targetData);
            var artifactData = new UserScenarioOwnedArtifactDTO {
                ArtifactId = artifact.Id,
                IsUse = isUse
            };
            targetData.OwnedArtifactList.Add(artifactData);
            string artifactText = LogManager.Instance.GetDBLogText(EnumTypes.LogActionType.player_get_something).FormatSmart($"<color=green>{artifact.Name}</color>");
            NotificationManager.Instance.SetShownNotification(artifactText);
            Debug.Log(artifactText);
        }
        else
        {
            string artifactErrorText = LocalString.Instance.GetLocalizedString("GetArtifactFail");
            NotificationManager.Instance.SetShownNotification(artifactErrorText);
            Debug.LogError(artifactErrorText);
        }
        ArtifactShowManager.Instance?.SetArtifactIcon();
        return result;
    }

    public async void GetRandomArtifact(ScenarioDTO targetData, EnumTypes.RarityType rare)
    {
        UserData.Instance.MainScenarioData.OwnedArtifactList = await UserMainScenarioDAO.Instance.GetUserMainScenarioOwnedArtifactsAsync(UserManager.Instance.User.AuthId);
        var artifactList = new List<ArtifactDTO>();
        switch (rare)
        {
            case EnumTypes.RarityType.common:
                for (int i = 0; i < InGameData.Instance.Artifacts.Count; i++)
                {
                    if (UserData.Instance.MainScenarioData.OwnedArtifactList.Find(a => a.ArtifactId == InGameData.Instance.Artifacts[i].Id) == null &&
                        (InGameData.Instance.Artifacts[i].Place == "public" || InGameData.Instance.Artifacts[i].Place == "shop"))
                        {
                        artifactList.Add(InGameData.Instance.Artifacts[i]);
                    }
                }
                break;
            case EnumTypes.RarityType.rare:
                for (int i = 0; i < InGameData.Instance.Artifacts.Count; i++)
                {
                    if (InGameData.Instance.Artifacts[i].Rarity != EnumTypes.RarityType.common && (InGameData.Instance.Artifacts[i].Place == "public" || InGameData.Instance.Artifacts[i].Place == "shop") &&
                        UserData.Instance.MainScenarioData.OwnedArtifactList.Find(a => a.ArtifactId == InGameData.Instance.Artifacts[i].Id) == null)
                        {
                        artifactList.Add(InGameData.Instance.Artifacts[i]);
                    }
                }
                break;
            case EnumTypes.RarityType.epic:
                for (int i = 0; i < InGameData.Instance.Artifacts.Count; i++)
                {
                    if (InGameData.Instance.Artifacts[i].Rarity != EnumTypes.RarityType.common && InGameData.Instance.Artifacts[i].Rarity != EnumTypes.RarityType.rare &&
                        UserData.Instance.MainScenarioData.OwnedArtifactList.Find(a => a.ArtifactId == InGameData.Instance.Artifacts[i].Id) == null &&
                        (InGameData.Instance.Artifacts[i].Place == "public" || InGameData.Instance.Artifacts[i].Place == "shop"))
                        {
                        artifactList.Add(InGameData.Instance.Artifacts[i]);
                    }
                }
                break;
        }

        if (rare != EnumTypes.RarityType.common && artifactList.Count == 0)
        {
            for (int i = 0; i < InGameData.Instance.Artifacts.Count; i++)
            {
                if (UserData.Instance.MainScenarioData.OwnedArtifactList.Find(a => a.ArtifactId == InGameData.Instance.Artifacts[i].Id) == null &&
                    (InGameData.Instance.Artifacts[i].Place == "public" || InGameData.Instance.Artifacts[i].Place == "shop"))
                    {
                    artifactList.Add(InGameData.Instance.Artifacts[i]);
                }
            }
        }

        if (artifactList.Count == 0)
        {
            string noRemain = LocalString.Instance.GetLocalizedString("artifact_no_remain");
            NotificationManager.Instance.SetShownNotification(noRemain);
            Debug.LogError(noRemain);
            return;
        }
        var rand = new System.Random(targetData.GenerateSeed + targetData.SelectList.Count * 3);
        await GetArtifact(artifactList[rand.Next(0, artifactList.Count)], targetData);
    }

    private void ObtainArtifactAction(DamageReceiveResult result, ScenarioDTO targetScenario)
    {
        if (result == null) return;
        var IsRandom = result.IsRandom;
        int upgradeCardAmount = result.UpgradeCardAmount;
        int deleteCardAmount = result.DeleteCardAmount;
        // 획득 시 카드 강화
        if (upgradeCardAmount > 0)
        {
            if (IsRandom)
            {
                CardUpgradeUtils.Instance.UpgradeRandomUpgradeableCard(upgradeCardAmount, targetScenario);
            }
            else
            {

            }
        }

        // 획득 시 카드 삭제
        if (deleteCardAmount > 0)
        {
            if (IsRandom)
            {

            }
            else
            {
                PopupManager.Instance.ShowCardDeletePopup(false);
            }
        }
    }

    public async Task<bool> GetDotgabiKey(int keyId, ScenarioDTO targetData)
    {
        bool success = await SupabaseArtifact.Instance.GetDotgabiKey(keyId);
        if (!success) { return false; }

        // 획득 로그
        Dictionary<string, object> extraData = new Dictionary<string, object> {
            { "dotgabi_key", keyId }
        };

        var logData = new UserScenarioLogDTO {
            ExtraData = extraData
        };

        LogManager.Instance?.SetLogMainScene(EnumTypes.LogActionType.player_get_something, logData, targetData);
        AudioManager.Instance.GetItemSound();

        var key = InGameData.Instance.DotgabiKeys.Find(k => k.KeyId == keyId);

        string artifactText = LogManager.Instance.GetDBLogText(EnumTypes.LogActionType.player_get_something).FormatSmart($"<color=green>{key.KeyName}</color>");
        NotificationManager.Instance.SetShownNotification(artifactText);

        // 도깨비 키 획득 처리 -> 하드 코딩 사유: 유저 데이터 전체를 불러오기에는 쿼리 요청이 너무 쌓임. +) 어차피 확인은 DB에서 하기 때문에 조작 시 문제 없음.
        var data = (UserMainScenarioDTO)targetData;
        if (keyId == 1)
        {
            data.FirstPiece = true;
        }
        else if (keyId == 2)
        {
            data.SecondPiece = true;
        }
        else if (keyId == 3)
        {
            data.ThirdPiece = true;
        }

        MoveSystem.Instance?.SetDotgabiKey();

        return true;
    }
}
