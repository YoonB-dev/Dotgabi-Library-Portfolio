using System.Threading.Tasks;
using UnityEngine;

public class JobUtils : Singleton<JobUtils>
{
    public async Task<bool> CheckJobUnlock(int jobId)
    {
        UserData.Instance.OwnedCharacter = await UserOwnedCharacterDAO.Instance.GetUserOwnedCharacterAsync(UserData.Instance.UserAuthId);
        if (jobId == 1)
        {
            return UserData.Instance.OwnedCharacter.OwnedBlacksmith;
        }
        else if (jobId == 2)
        {
            return UserData.Instance.OwnedCharacter.OwnedDosa;
        }
        else if (jobId == 3)
        {
            return UserData.Instance.OwnedCharacter.OwnedPerformer;
        }
        else
        {
            Debug.LogError("Invalid job ID: " + jobId);
            return false;
        }
    }
    public bool CheckJobUnlockSync(int jobId)
    {
        if (jobId == 1)
        {
            return UserData.Instance.OwnedCharacter.OwnedBlacksmith;
        }
        else if (jobId == 2)
        {
            return UserData.Instance.OwnedCharacter.OwnedDosa;
        }
        else if (jobId == 3)
        {
            return UserData.Instance.OwnedCharacter.OwnedPerformer;
        }
        else
        {
            Debug.LogError("Invalid job ID: " + jobId);
            return false;
        }
    }
}
