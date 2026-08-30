using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class UserChallengeScenarioDAO : Singleton<UserChallengeScenarioDAO>
{
    // 이어하기
    public async Task<UserChallengeScenarioDTO> GetUserChallengeScenarioDTO(string auth_id)
    {
        var client = SupabaseClientProvider.Instance.Client;

        var response = await SupabaseWrap.ExecuteWithRefresh(() => client
            .From<UserChallengeScenarioEntity>()
            .Where(x => x.userAuthId == auth_id)
            .Single());

        if (response == null)
        {
            // var text = LogManager.Instance?.GetLocalText("no_data_exist");
            // NotificationManager.Instance?.SetShownNotification(text);
            return null;
        }

        // 병렬로 데이터 가져오기
        var artifactTask = GetUserChallengeScenarioOwnedArtifactsAsync(auth_id);
        var cardTask = GetUserChallengeScenarioOwnedCardsAsync(auth_id);
        var logTask = LogDAO.Instance.GetUserAllMainScenarioLogsAsync(auth_id);

        await Task.WhenAll(artifactTask, cardTask, logTask);
        Debug.Log(response.currStage);

        return new UserChallengeScenarioDTO {
            MapSeed = response.mapSeed,
            GenerateSeed = response.generateSeed,
            StageList = response.stageList,
            CurrStageLevel = response.currStageLevel,
            EventClear = response.eventClear,
            NextEvent = response.nextEvent,
            FightTime = response.fightTime,
            GameCoins = response.gameCoins,
            TotalGameCoins = response.totalGameCoins,
            JobId = response.jobId,
            CurrHp = response.currHp,
            MaxHp = response.maxHp,
            SelectList = response.selectList,
            OwnedArtifactList = artifactTask.Result,
            OwnedCardList = cardTask.Result,
            LogList = logTask.Result,
            IsNextEnemyStory = response.isNextEnemyStory,
            IsEliteClear = response.isEliteClear,
        };
    }
    // 새로 생성
    public async Task<UserChallengeScenarioDTO> CreateNewChallengeScenario(string auth_id, int jobId)
    {
        var client = SupabaseClientProvider.Instance.Client;

        var result = await SupabaseWrap.ExecuteWithRefresh(() =>
            client.Rpc("insert_user_challenge_scenario", new Dictionary<string, object>
            {
                { "p_job_id", jobId },
            })
        );

        if (result == null)
        {
            Debug.LogError($"Failed to create new user main scenario: {result}");
            return null;
        }

        // 새로 생성된 데이터를 가져옵니다.
        return await GetUserChallengeScenarioDTO(auth_id);
    }

    public async Task<List<UserScenarioOwnedArtifactDTO>> GetUserChallengeScenarioOwnedArtifactsAsync(string auth_id)
    {
        var client = SupabaseClientProvider.Instance.Client;

        var response = await client
            .From<UserChallengeScenarioArtifactEntity>()
            .Select(x => new object[] { x.ownedId, x.userAuthId, x.artifactId, x.isUse })
            .Where(x => x.userAuthId == auth_id)
            .Order("get_at", Supabase.Postgrest.Constants.Ordering.Ascending)
            .Get();

        var artifacts = new List<UserScenarioOwnedArtifactDTO>();
        foreach (var entity in response.Models)
        {
            artifacts.Add(new UserScenarioOwnedArtifactDTO
            {
                ArtifactId = entity.artifactId,
                IsUse = entity.isUse
            });
        }
        return artifacts;
    }

    public async Task<List<UserScenarioOwnedCardDTO>> GetUserChallengeScenarioOwnedCardsAsync(string auth_id)
    {
        var client = SupabaseClientProvider.Instance.Client;

        var response = await client
            .From<UserChallengeScenarioCardEntity>()
            .Select(x => new object[] { x.ownedId, x.userAuthId, x.cardId, x.upgradeTime })
            .Where(x => x.userAuthId == auth_id)
            .Get();

        var cards = new List<UserScenarioOwnedCardDTO>();
        foreach (var entity in response.Models)
        {
            cards.Add(new UserScenarioOwnedCardDTO
            {
                OwnedId = entity.ownedId,
                CardId = entity.cardId,
                UpgradeTime = entity.upgradeTime
            });
        }
        return cards;
    }
}
