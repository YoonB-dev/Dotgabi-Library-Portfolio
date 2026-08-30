using System.Threading.Tasks;
using UnityEngine;

public class LoginStrategy : Singleton<LoginStrategy>, ILogin
{
    public async Task<bool> LoginAsync()
    {
        bool success =  await AuthService.Instance.TryCreateUser();
        Debug.Log($"게스트 계정 생성 성공 여부: {success}");

        return success;
    }

}

