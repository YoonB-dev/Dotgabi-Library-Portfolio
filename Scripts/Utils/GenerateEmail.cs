using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class GenerateEmail : Singleton<GenerateEmail>
{
    private const string Base36Chars = "0123456789abcdefghijklmnopqrstuvwxyz";
    public string GenerateVirtualPassWord()
    {
        // 디바이스 고유 ID 가져오기
        string deviceId = SystemInfo.deviceUniqueIdentifier;

        // 예외 처리: 디바이스 ID가 null이거나 비어있으면 임의 값 사용
        if (string.IsNullOrEmpty(deviceId))
        {
            deviceId = System.Guid.NewGuid().ToString();
        }

        // SHA256 해시 계산
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(deviceId));

            // 16진수 문자열로 변환
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hashBytes)
                sb.Append(b.ToString("x2"));

            // 이메일로 사용할 길이(10~16자리)만 추출
            string hashString = sb.ToString().Substring(0, 16);

            // 가상 이메일 생성
            return hashString;
        }
    }

    public string GenerateVirtualEmail()
    {
        long ticks = System.DateTime.UtcNow.Ticks;
        int ran = Random.Range(0, 10000);
        long combined = ticks + ran;

        return ToBase36(combined) + "@guest.dotgabi.com";
    }
    private string ToBase36(long value)
    {
        StringBuilder sb = new StringBuilder();
        while (value > 0)
        {
            sb.Insert(0, Base36Chars[(int)(value % 36)]);
            value /= 36;
        }
        return sb.ToString();
    }



}
