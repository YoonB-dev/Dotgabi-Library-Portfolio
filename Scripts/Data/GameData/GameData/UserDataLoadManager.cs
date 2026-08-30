using System.Threading.Tasks;
using UnityEngine;

public class UserDataLoadManager : Singleton<UserDataLoadManager>
{
    public async Task DataSettings()
    {
        // 유저 업적 상태 로드
        var userClearAchieveTask = AchieveDAO.Instance.GetUserClearAchievesAsync(UserData.Instance.UserAuthId);
        var userCurrAchieveTask = AchieveDAO.Instance.GetUserAchieveCurrDataAsync(UserData.Instance.UserAuthId);
        var userAchievePriceTask = AchieveDAO.Instance.GetUserAchievePriceGetAsync(UserData.Instance.UserAuthId);

        // 보유 목록
        var userOwnedCardDataTask = UserOwnedCollectionDAO.Instance.GetUserOwnedCardDataAsync(UserData.Instance.UserAuthId);
        var userOwnedArtifactDataTask = UserOwnedCollectionDAO.Instance.GetUserOwnedArtifactDataAsync(UserData.Instance.UserAuthId);
        var userOwnedCardFramesTask = UserOwnedCardFrameDAO.Instance.GetUserOwnedCardFrameAsync(UserData.Instance.UserAuthId);
        var userOwnedCharacterTask = UserOwnedCharacterDAO.Instance.GetUserOwnedCharacterAsync(UserData.Instance.UserAuthId);

        // 클리어 목록
        var UserMainClearRecordTask = UserMainClearRecordDAO.Instance.GetUserMainClearRecord(UserData.Instance.UserAuthId);

        // 시나리오 관련 데이터 불러오기
        // 메인 시나리오 데이터
        var userMainScenarioDataTask = UserMainScenarioDAO.Instance.GetUserMainScenarioDTO(UserData.Instance.UserAuthId);
        // 챌린지 시나리오 데이터
        var UserChallengeScenarioTask = UserChallengeScenarioDAO.Instance.GetUserChallengeScenarioDTO(UserData.Instance.UserAuthId);

        Debug.Log("비동기 데이터 로딩 시작");
        // 비동기 데이터 로딩
        await Task.WhenAll(
            userOwnedCardFramesTask,
            userOwnedCharacterTask,
            userClearAchieveTask,
            userOwnedCardDataTask,
            userOwnedArtifactDataTask,
            userCurrAchieveTask,
            userAchievePriceTask
        );

        await Task.WhenAll(
            // 시나리오 데이터
            userMainScenarioDataTask,
            UserChallengeScenarioTask
        );

        UserData.Instance.OwnedCardFrameList = userOwnedCardFramesTask.Result;
        UserData.Instance.OwnedCharacter = userOwnedCharacterTask.Result;
        UserData.Instance.UserClearAchieveList = userClearAchieveTask.Result;
        UserData.Instance.UserAchievePriceGetData = userAchievePriceTask.Result;
        // 유저 소유 카드, 아티팩트 데이터 초기화
        UserData.Instance.UserOwnedCardList = userOwnedCardDataTask.Result;
        UserData.Instance.UserOwnedArtifactList = userOwnedArtifactDataTask.Result;
        UserData.Instance.UserAchieveCurrData = userCurrAchieveTask.Result;
        // 클리어 데이터
        UserData.Instance.UserMainClearRecordList = UserMainClearRecordTask.Result;

        // 시나리오 데이터 초기화
        if (userMainScenarioDataTask != null && userMainScenarioDataTask.Result != null)
        {
            UserData.Instance.MainScenarioData = userMainScenarioDataTask.Result;
        }

        if (UserChallengeScenarioTask != null && UserChallengeScenarioTask.Result != null)
        {
            UserData.Instance.ChallengeScenarioData = UserChallengeScenarioTask.Result;
        }

        Debug.Log("유저 프레임 데이터 로드: " + UserData.Instance.OwnedCardFrameList.Count);

        Debug.Log("비동기 데이터 로딩 완료");
    }
}
