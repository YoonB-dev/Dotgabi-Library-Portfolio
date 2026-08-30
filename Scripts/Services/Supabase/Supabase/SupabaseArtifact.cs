using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class SupabaseArtifact : Singleton<SupabaseArtifact>
{

    public async Task<bool> GetArtifact(int artifactId, ScenarioDTO data)
    {
        Debug.Log("GetArtifact 시작: " + artifactId);
        if (data is UserMainScenarioDTO mainScenarioData)
        {
            return await GetArtifactById(artifactId, mainScenarioData);
        }
        else if (data is UserChallengeScenarioDTO challengeScenarioData)
        {
            return await GetArtifactById(artifactId, challengeScenarioData);
        }
        Debug.LogError("GetArtifact: Invalid ScenarioDTO type");
        return false;
    }

    public async Task<bool> GetArtifactById(int artifactId, UserMainScenarioDTO mainScenarioData)
    {
        Debug.Log("GetArtifactById 시작: " + artifactId);
        await SupabaseClientProvider.Instance.InitializeAsync();
        Debug.Log("SupabaseClientProvider 초기화 완료");
        Debug.Log($"AuthId at GetUserMainScenarioOwnedArtifactsAsync: {UserManager.Instance.User?.AuthId}");
        mainScenarioData.OwnedArtifactList = await SupabaseWrap.ExecuteWithRefresh(() => UserMainScenarioDAO.Instance.GetUserMainScenarioOwnedArtifactsAsync(UserManager.Instance.User.AuthId));
        Debug.Log("OwnedArtifactList 업데이트 완료");
        var client = SupabaseClientProvider.Instance.Client;

        // var response = await SupabaseWrap.ExecuteWithRefresh(() =>
        //     client.Rpc("insert_user_main_scenario_artifact", new Dictionary<string, object> {
        //         { "p_artifact_id", artifactId }
        //     })
        // );

        Debug.Log("Getting Artifact by ID: " + artifactId);
        var response = await
            client.Rpc("insert_user_main_scenario_artifact", new Dictionary<string, object>
            {
                { "p_artifact_id", artifactId }
            });

        Debug.Log($"GetArtifactById: {response}");

        UserData.Instance.MainScenarioData = await UserMainScenarioDAO.Instance.GetUserMainScenarioDTO(UserManager.Instance.User.AuthId);
        return bool.TryParse(response.Content, out bool result) && result;
    }

    public async Task<bool> GetArtifactById(int artifactId, UserChallengeScenarioDTO challengeScenarioData)
    {
        await SupabaseClientProvider.Instance.InitializeAsync();
        challengeScenarioData.OwnedArtifactList = await SupabaseWrap.ExecuteWithRefresh(() => UserChallengeScenarioDAO.Instance.GetUserChallengeScenarioOwnedArtifactsAsync(UserManager.Instance.User.AuthId));
        var client = SupabaseClientProvider.Instance.Client;

        var response = await SupabaseWrap.ExecuteWithRefresh(() =>
            client.Rpc("insert_user_challenge_scenario_artifact", new Dictionary<string, object>
            {
                { "p_artifact_id", artifactId }
            })
        );
        Debug.Log($"GetArtifactById: {response}");
        return bool.TryParse(response.Content, out bool result) && result;
    }

    public async Task<bool> GetDotgabiKey(int keyId)
    {
        await SupabaseClientProvider.Instance.InitializeAsync();
        var client = SupabaseClientProvider.Instance.Client;

        var response = await SupabaseWrap.ExecuteWithRefresh(() =>
            client.Rpc("update_main_story_piece_status", new Dictionary<string, object>
            {
                { "p_piece", keyId }
            })
        );

        Debug.Log($"GetDotgabiKey: {response}");

        return bool.TryParse(response.Content, out bool result) && result;
    }
}

