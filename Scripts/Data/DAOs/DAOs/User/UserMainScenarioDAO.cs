using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class UserMainScenarioDAO : Singleton<UserMainScenarioDAO>
{
    // 메인 시나리오 정보 가져오기
    public async Task<UserMainScenarioDTO> GetUserMainScenarioDTO(string auth_id)
    {
        var client = SupabaseClientProvider.Instance.Client;

        var response = await SupabaseWrap.ExecuteWithRefresh(() => client
            .From<UserMainScenarioEntity>()
            .Where(x => x.userAuthId == auth_id)
            .Single());

        if (response == null)
        {
            return null;
        }


        // 병렬로 데이터 가져오기
        var artifactTask = GetUserMainScenarioOwnedArtifactsAsync(auth_id);
        var cardTask = GetUserMainScenarioOwnedCardsAsync(auth_id);
        var logTask = LogDAO.Instance.GetUserAllMainScenarioLogsAsync(auth_id);
        var storyTask = GetUserMainscenarioStoryClearDataAsync(auth_id);
        var storyCardTask = UserMainScenarioStoryCardDAO.Instance.GetUserMainscenarioStoryCardAsync(auth_id);

        await Task.WhenAll(artifactTask, cardTask, logTask, storyTask, storyCardTask);

        return new UserMainScenarioDTO {
            MapSeed = response.mapSeed,
            GenerateSeed = response.generateSeed,
            StageList = response.stageList,
            CurrStageLevel = response.currStageLevel,
            EventClear = response.eventClear,
            NextEvent = response.nextEvent,
            FightTime = response.fightTime,
            GameCoins = response.gameCoins,
            TotalGameCoins = response.totalGameCoins,
            FirstPiece = response.firstPiece,
            SecondPiece = response.secondPiece,
            ThirdPiece = response.thirdPiece,
            Difficulty = Enum.TryParse(typeof(EnumTypes.Difficulty), response.difficulty ?? "balance", out var difficulty_2) ? (EnumTypes.Difficulty)difficulty_2 : EnumTypes.Difficulty.balance,
            JobId = response.jobId,
            CurrHp = response.currHp,
            MaxHp = response.maxHp,
            SelectList = response.selectList,
            OwnedArtifactList = artifactTask.Result,
            OwnedCardList = cardTask.Result,
            LogList = logTask.Result,
            IsNextEnemyStory = response.isNextEnemyStory,
            IsEliteClear = response.isEliteClear,
            StoryClearData = storyTask.Result,
            OwnedStoryCardList = storyCardTask.Result
        };
    }
    // 새로 생성
    public async Task<UserMainScenarioDTO> CreateNewMainScenario(string auth_id, int jobId, int difficulty)
    {
        var client = SupabaseClientProvider.Instance.Client;

        var result = await client.Rpc("insert_user_main_scenario", new Dictionary<string, object>
            {
                { "p_user_auth_id", auth_id },
                { "p_job_id", jobId },
                { "p_difficulty", ((EnumTypes.Difficulty)difficulty).ToString() }
            });

        if (result == null)
        {
            Debug.LogError($"Failed to create new user main scenario: {result}");
            return null;
        }

        // 새로 생성된 데이터를 가져옵니다.
        return await GetUserMainScenarioDTO(auth_id);
    }

    public async Task<List<UserScenarioOwnedArtifactDTO>> GetUserMainScenarioOwnedArtifactsAsync(string auth_id)
    {
        var client = SupabaseClientProvider.Instance.Client;

        var response = await client
            .From<UserMainScenarioArtifactEntity>()
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

    public async Task<List<UserScenarioOwnedCardDTO>> GetUserMainScenarioOwnedCardsAsync(string auth_id)
    {
        var client = SupabaseClientProvider.Instance.Client;

        var response = await client
            .From<UserMainScenarioCardEntity>()
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

    public async Task<UserMainscenarioStoryClearDTO> GetUserMainscenarioStoryClearDataAsync(string auth_id)
    {
        var client = SupabaseClientProvider.Instance.Client;

        var response = await client
            .From<UserMainScenarioStoryClearEntity>()
            .Where(x => x.UserAuthId == auth_id)
            .Get();

        if (response == null)
        {
            Debug.LogError($"Failed to get user main scenario story clear data: {response}");
        }

        var storyList = response.Model;

        return new UserMainscenarioStoryClearDTO {
            CrimeSceneClear = storyList.CrimeSceneClear,
            OnuHouseClear = storyList.OnuHouseClear,
            TigerArrest = storyList.TigerArrest,
            OnuTrust = storyList.OnuTrust
        };
    }
}
