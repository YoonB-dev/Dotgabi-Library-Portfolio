using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    // 메인시나리오에서 전투 후 다음 노드 인덱스 저장용.
    public int? nextNodeIndex { get; set; } = null;
    // 배경 사운드
    public float musicVolume { get; set; } = 0;
    // 효과음 사운드
    public float soundVolume { get; set; } = 0;
    // 현재 언어
    public EnumTypes.LanguageType languageType { get; set; } = EnumTypes.LanguageType.ko;

    // 메인 시나리오 데이터
    // 다음 전투 적 타입
    public EnumTypes.EnemyType nextEnemyType { get; set; } = EnumTypes.EnemyType.normal;
    public int? nextEnemyId { get; set; } = null;

    // 오디오 데이터 저장 및 불러오기
    public void SaveAudioData()
    {
        PlayerPrefs.SetFloat("musicVolume", musicVolume);
        PlayerPrefs.SetFloat("soundVolume", soundVolume);
        PlayerPrefs.Save();
    }

    public void LoadAudioData()
    {
        musicVolume = PlayerPrefs.GetFloat("musicVolume", 0);
        soundVolume = PlayerPrefs.GetFloat("soundVolume", 0);
    }

    // 언어 데이터 저장 및 불러오기
    public void SaveLanguageDataById(int languageId)
    {
        PlayerPrefs.SetInt("languageType", languageId);
        PlayerPrefs.Save();
    }
    public void LoadLanguageData()
    {
        languageType = (EnumTypes.LanguageType)PlayerPrefs.GetInt("languageType", 0);
    }
}
