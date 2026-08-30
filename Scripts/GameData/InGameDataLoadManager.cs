using System.Threading.Tasks;
using UnityEngine;

public class InGameDataLoadManager : Singleton<InGameDataLoadManager>
{
    public async Task DataSettings()
    {
        // InGameData 로드
        var cardTask = CardDAO.Instance.GetAllCardsAsync(EnumTypes.LanguageType.ko);
        var jobTask = JobDAO.Instance.GetAllJobAsync(EnumTypes.LanguageType.ko);
        var artifactTask = ArtifactDAO.Instance.GetAllArtifactsAsync(EnumTypes.LanguageType.ko);
        var enemyTask = EnemyDAO.Instance.GetAllEnemysAsync(EnumTypes.LanguageType.ko);
        var enemyTextTask = EnemyDAO.Instance.GetEnemyTextsAsync(EnumTypes.LanguageType.ko);
        var storyTask = StoryDAO.Instance.GetAllStoriesAsync(EnumTypes.LanguageType.ko);
        var eventTask = EventDAO.Instance.GetAllEventsAsync(EnumTypes.LanguageType.ko);
        var shopItemTask = ShopItemDAO.Instance.GetAllShopItemsAsync(EnumTypes.LanguageType.ko);
        var frameShopItemTask = ShopItemDAO.Instance.GetShopItemsByTypeAsync(EnumTypes.ShopItemType.frame, EnumTypes.LanguageType.ko);
        var decoShopItemTask = ShopItemDAO.Instance.GetShopItemsByTypeAsync(EnumTypes.ShopItemType.deco, EnumTypes.LanguageType.ko);
        var characterShopItemTask = ShopItemDAO.Instance.GetShopItemsByTypeAsync(EnumTypes.ShopItemType.character, EnumTypes.LanguageType.ko);
        var statusTask = StatusDAO.Instance.GetAllStatusesAsync(EnumTypes.LanguageType.ko);
        var summonTask = SummonDAO.Instance.GetAllSummonsAsync(EnumTypes.LanguageType.ko);
        var maskTask = MaskDAO.Instance.GetAllMasksAsync(EnumTypes.LanguageType.ko);
        var logTask = LogDAO.Instance.GetAllLogDataAsync(EnumTypes.LanguageType.ko);
        var AchieveTask = AchieveDAO.Instance.GetAchievesAsync(EnumTypes.LanguageType.ko);

        var mainStoryTextTask = MainStoryTextDAO.Instance.GetAllMainStoryAsync(EnumTypes.LanguageType.ko);
        var eventSmallTask = EventSmallDAO.Instance.GetAllEventSmallsAsync(EnumTypes.LanguageType.ko);
        var mainStoryItemTask = StoryDAO.Instance.GetAllMainStoryItemsAsync(EnumTypes.LanguageType.ko);

        var dotgabiKeyTask = DotgabiKeyDAO.Instance.GetDotgabiKeyByIdAsync(EnumTypes.LanguageType.ko);

        Debug.Log("비동기 데이터 로딩 시작");
        // 비동기 데이터 로딩
        await Task.WhenAll(
            cardTask,
            jobTask,
            artifactTask,
            enemyTask,
            enemyTextTask,
            storyTask,
            eventTask,
            shopItemTask,
            frameShopItemTask,
            decoShopItemTask,
            characterShopItemTask,
            statusTask,
            summonTask,
            maskTask,
            logTask,
            AchieveTask,
            mainStoryTextTask,
            eventSmallTask,
            mainStoryItemTask,
            dotgabiKeyTask
        );
        Debug.Log("비동기 데이터 로딩 완료");
        // 게임 데이터 초기화
        InGameData.Instance.Cards = cardTask.Result;
        InGameData.Instance.Jobs = jobTask.Result;
        InGameData.Instance.Artifacts = artifactTask.Result;
        InGameData.Instance.Enemys = enemyTask.Result;
        InGameData.Instance.EnemyTexts = enemyTextTask.Result; // 적 대화 정보 초기화
        InGameData.Instance.Stories = storyTask.Result;
        InGameData.Instance.Events = eventTask.Result;
        InGameData.Instance.ShopItems = shopItemTask.Result;
        InGameData.Instance.FrameShopItems = frameShopItemTask.Result;
        InGameData.Instance.DecoShopItems = decoShopItemTask.Result;
        InGameData.Instance.CharacterShopItems = characterShopItemTask.Result;

        InGameData.Instance.Buffs = statusTask.Result.FindAll(s => s.StatusType == EnumTypes.Status.buff);
        InGameData.Instance.Debuffs = statusTask.Result.FindAll(s => s.StatusType == EnumTypes.Status.debuff);

        InGameData.Instance.Summons = summonTask.Result;
        InGameData.Instance.Masks = maskTask.Result;

        // 로그 데이터 초기화
        InGameData.Instance.Logs = logTask.Result;

        InGameData.Instance.AchieveDTOLists = AchieveTask.Result;

        InGameData.Instance.MainStoryTexts = mainStoryTextTask.Result;
        InGameData.Instance.EventSmalls = eventSmallTask.Result;
        InGameData.Instance.MainStoryItems = mainStoryItemTask.Result;

        // 도깨비 키 데이터 초기화
        InGameData.Instance.DotgabiKeys = dotgabiKeyTask.Result;

        Debug.Log("게임 데이터 초기화 완료");
        Debug.Log("도깨비 키 데이터 로드 완료: " + InGameData.Instance.DotgabiKeys.Count + "개 키 로드됨");
    }
}
