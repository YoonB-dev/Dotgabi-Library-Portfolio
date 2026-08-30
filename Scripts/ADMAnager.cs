using GoogleMobileAds.Api;
using UnityEngine;
using System;
using TMPro;
using System.IO;
using UnityEngine.Localization;

public class ADMAnager : MonoBehaviour
{
    // These ad units are configured to always serve test ads.
#if UNITY_ANDROID
    private string _adUnitId_AdPoint = "YOUR_ANDROID_AD_UNIT_ID_ADPOINT";
    private string _adUnitId_Revive = "YOUR_ANDROID_AD_UNIT_ID_REVIVE";
    private string _adUnitId_Treasure = "YOUR_ANDROID_AD_UNIT_ID_TREASURE";
    private string _adUnitId_Battle = "YOUR_ANDROID_AD_UNIT_ID_BATTLE";

#elif UNITY_IPHONE
    private string _adUnitId_AdPoint = "YOUR_IOS_AD_UNIT_ID_ADPOINT";
    private string _adUnitId_Revive = "YOUR_IOS_AD_UNIT_ID_REVIVE";
    private string _adUnitId_Treasure = "YOUR_IOS_AD_UNIT_ID_TREASURE";
    private string _adUnitId_Battle = "YOUR_IOS_AD_UNIT_ID_BATTLE";
#elif UNITY_EDITOR
    private string _adUnitId_AdPoint = "YOUR_EDITOR_AD_UNIT_ID";
    private string _adUnitId_Revive = "YOUR_EDITOR_AD_UNIT_ID";
    private string _adUnitId_Treasure = "YOUR_EDITOR_AD_UNIT_ID";
    private string _adUnitId_Battle = "YOUR_EDITOR_AD_UNIT_ID";
#else
  private string _adText = "unused";
#endif

    private RewardedAd _rewardedAd_AdPoint;
    private RewardedAd _rewardedAd_Revive;
    private RewardedAd _rewardedAd_Treasure;
    private InterstitialAd _interstitialAd;

    public Player player;
    [SerializeField] private MoveSystem moveSystem;
    [Header("Canvas")]
    [SerializeField]
    private GameObject AdCanvas;

    public void Start()
    {
        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize((InitializationStatus initStatus) =>
        {
            Debug.Log("Google Mobile Ads SDK initialized. Status: " + initStatus);
            LoadRewardedAd();
            LoadRewardedAd_Revive();
            LoadRewardedAd_Treasure();
            LoadInterstitialAd();
        });
    }
    public void LoadRewardedAd()
    {

        // Clean up the old ad before loading a new one.
        if (_rewardedAd_AdPoint != null)
        {
            _rewardedAd_AdPoint.Destroy();
            _rewardedAd_AdPoint = null;
        }

        Debug.Log("Loading the rewarded ad.");

        // create our request used to load the ad.
        var adRequest = new AdRequest();

        // 광고 재화 획득 광고
        RewardedAd.Load(_adUnitId_AdPoint, adRequest,
            (RewardedAd ad, LoadAdError error) =>
            {
                // if error is not null, the load request failed.
                if (error != null || ad == null)
                {
                    Debug.LogError("Rewarded ad failed to load an ad " +
                                   "with error : " + error);
                    return;
                }

                Debug.Log("Rewarded ad loaded with response : "
                          + ad.GetResponseInfo());

                _rewardedAd_AdPoint = ad;
                RegisterEventHandlers(_rewardedAd_AdPoint);
            });
    }

    public void LoadRewardedAd_Revive()
    {

        // Clean up the old ad before loading a new one.
        if (_rewardedAd_Revive != null)
        {
            _rewardedAd_Revive.Destroy();
            _rewardedAd_Revive = null;
        }

        Debug.Log("Loading the rewarded ad.");

        // create our request used to load the ad.
        var adRequest = new AdRequest();

        // Revive 광고
        RewardedAd.Load(_adUnitId_Revive, adRequest,
            (RewardedAd ad, LoadAdError error) =>
            {
                // if error is not null, the load request failed.
                if (error != null || ad == null)
                {
                    Debug.LogError("Rewarded ad failed to load an ad " +
                                   "with error : " + error);
                    return;
                }

                Debug.Log("Rewarded ad loaded with response : "
                          + ad.GetResponseInfo());

                _rewardedAd_Revive = ad;
                RegisterEventHandlers(_rewardedAd_Revive);
            });
    }

    public void LoadRewardedAd_Treasure()
    {

        // Clean up the old ad before loading a new one.
        if (_rewardedAd_Treasure != null)
        {
            _rewardedAd_Treasure.Destroy();
            _rewardedAd_Treasure = null;
        }

        Debug.Log("Loading the rewarded ad.");

        // create our request used to load the ad.
        var adRequest = new AdRequest();

        // Treasure 획득 광고
        RewardedAd.Load(_adUnitId_Treasure, adRequest,
            (RewardedAd ad, LoadAdError error) =>
            {
                // if error is not null, the load request failed.
                if (error != null || ad == null)
                {
                    Debug.LogError("Rewarded ad failed to load an ad " +
                                   "with error : " + error);
                    return;
                }

                Debug.Log("Rewarded ad loaded with response : "
                          + ad.GetResponseInfo());

                _rewardedAd_Treasure = ad;
                RegisterEventHandlers(_rewardedAd_Treasure);
            });
    }

    public void LoadInterstitialAd()
    {
        // Clean up the old ad before loading a new one.
        if (_interstitialAd != null)
        {
            _interstitialAd.Destroy();
            _interstitialAd = null;
        }

        Debug.Log("Loading the interstitial ad.");

        // Create our request used to load the ad.
        var adRequest = new AdRequest();

        // 전면 광고 로드
        InterstitialAd.Load(_adUnitId_Battle, adRequest,
            (InterstitialAd ad, LoadAdError error) =>
            {
                // if error is not null, the load request failed.
                if (error != null || ad == null)
                {
                    Debug.LogError("Interstitial ad failed to load an ad " +
                                   "with error : " + error);
                    return;
                }

                Debug.Log("Interstitial ad loaded with response : "
                          + ad.GetResponseInfo());

                _interstitialAd = ad;
                RegisterEventHandlers(_interstitialAd);
            });
    }
    //재화 광고
    public void ShowRewardedAd()
    {
        if (_rewardedAd_AdPoint != null && _rewardedAd_AdPoint.CanShowAd() && CanShowAdDay("AdPoint"))
        {
            Debug.Log("Showing rewarded ad.");
            _rewardedAd_AdPoint.Show((Reward reward) =>
            {
                // TODO: Reward the user.
                //GetComponent<MainManager>().GetPoint("Ad", GameManager.gameManager.totalGameData.NextAdPoint);
                //GameManager.gameManager.totalGameData.NextAdPoint = UnityEngine.Random.Range(1, 4);
                CloseAdCanvas();
                SupabaseAchieve.Instance.AchieveCurrData(EnumTypes.AchieveType.show_ad_count, 1);
                LoadRewardedAd();

                SaveAdWatchData("AdPoint");
            });
        }
        else
        {
            Debug.Log("Rewarded ad is not ready yet.");
            string text = new LocalizedString("LocalTable", "All-Ad").GetLocalizedString();
            NotificationManager.Instance.SetShownNotification(text);
        }
    }
    //부활 광고
    public void ShowRewardedAd_Player()
    {
        const string rewardMsg =
            "Rewarded ad rewarded the user. Type: {0}, amount: {1}.";

        if (_rewardedAd_Revive != null && _rewardedAd_Revive.CanShowAd() && CanShowAdDay("Revive"))
        {
            Debug.Log("Showing rewarded ad.");
            _rewardedAd_Revive.Show((Reward reward) =>
            {
                // TODO: Reward the user.
                VictoryManager.Instance.GameOverReviveButton();
                Debug.Log(String.Format(rewardMsg, reward.Type, reward.Amount));
                SupabaseAchieve.Instance.AchieveCurrData(EnumTypes.AchieveType.show_ad_count, 1);
                LoadRewardedAd_Revive();

                SaveAdWatchData("Revive");
            });
        }
        else
        {
            string textAd = new LocalizedString("LocalTable", "All-Ad").GetLocalizedString();
            NotificationManager.Instance.SetCheckNotification(textAd);
            Debug.Log("Rewarded ad is not ready yet.");
        }
    }
    //보물 광고
    public void ShowRewardedAd_Treasure()
    {
        if (_rewardedAd_Treasure != null && _rewardedAd_Treasure.CanShowAd() && CanShowAdDay("Treasure"))
        {
            Debug.Log("Showing rewarded ad.");
            _rewardedAd_Treasure.Show((Reward reward) =>
            {
                // TODO: Reward the user.
                // if(MoveSystem.moveSystem!= null && MoveSystem.moveSystem.treasureManager != null)
                {
                //     MoveSystem.moveSystem.treasureManager.SetReItemAD();
                //     Debug.Log(String.Format(rewardMsg, reward.Type, reward.Amount));
                //     AchieveManager.instance.AchieveCheck("Ad", 1);
                //     LoadRewardedAd_Treasure();

                //     SaveAdWatchData("Treasure");
                }

            });
        }
        else
        {
            Debug.Log("Rewarded ad is not ready yet.");
            string textAd = new LocalizedString("LocalTable", "All-Ad").GetLocalizedString();
            //moveSystem.StartCoroutine(moveSystem.showNotificatrion(textAd));
        }
    }
    //전투 후 광고
    public void ShowInterstitialAd()
    {
        if (_interstitialAd != null && _interstitialAd.CanShowAd() && CanShowAdDay("Battle"))
        {
            Debug.Log("Showing interstitial ad.");
            _interstitialAd.Show();

            SaveAdWatchData("Battle");
        }
        else
        {
            Debug.Log("Interstitial ad is not ready yet.");
            BattleManager.Instance.GoToGameScene();
        }
    }


    private void RegisterEventHandlers(RewardedAd ad)
    {
        // Raised when the ad is estimated to have earned money.
        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("Rewarded ad paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
        };
        // Raised when an impression is recorded for an ad.
        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Rewarded ad recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        ad.OnAdClicked += () =>
        {
            Debug.Log("Rewarded ad was clicked.");
        };
        // Raised when an ad opened full screen content.
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Rewarded ad full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Rewarded ad full screen content closed.");
            LoadRewardedAd();
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Rewarded ad failed to open full screen content " +
                           "with error : " + error);
        };
    }

    private void RegisterEventHandlers(InterstitialAd ad)
    {
        // Raised when the ad is estimated to have earned money.
        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("Interstitial ad paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
        };
        // Raised when an impression is recorded for an ad.
        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Interstitial ad recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        ad.OnAdClicked += () =>
        {
            Debug.Log("Interstitial ad was clicked.");
        };
        // Raised when an ad opened full screen content.
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Interstitial ad full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Interstitial ad full screen content closed.");
            BattleManager.Instance.GoToGameScene();

            LoadInterstitialAd();
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Interstitial ad failed to open full screen content " +
                           "with error : " + error);
        };
    }




    public void ShowAdCanvas()
    {
        //SFX
        AudioManager.Instance.ButtonClickSound1();

        AdCanvas.SetActive(true);
        var targetBox = AdCanvas.transform.GetChild(1).gameObject;
        ButtonAnim.Instance.ButtonScaleIn(targetBox, 0f, 1f);

        //targetBox.transform.GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>().text = "+" + GameManager.gameManager.totalGameData.NextAdPoint;
    }

    public void CloseAdCanvas()
    {
        //SFX
        AudioManager.Instance.ButtonClickSound2();
        AdCanvas.SetActive(false);
    }


    private bool CanShowAdDay(string type)
    {

        // JSON 파일 읽기
        // string json = File.ReadAllText(filePath);
        // GameData.AdData data = JsonUtility.FromJson<GameData.AdData>(json);

        // string today = DateTime.Now.ToString("yyyy-MM-dd");

        // // 날짜가 다르면 광고 카운트 초기화
        // if (data.lastDate != today)
        // {
        //     data.lastDate = today;
        //     data.battleCount = 0;
        //     data.adPointCount = 0;
        //     data.reviveCount = 0;
        //     data.treasureCount = 0;
        //     data.exCount = 0;
        //     File.WriteAllText(filePath, JsonUtility.ToJson(data));
        // }

        // 광고 횟수가 제한을 초과했는지 확인
        // switch(type)
        //{
        //     case "AdPoint":
        //         return data.adPointCount < GameData.gameData.maxAdPerDay;
        //     case "Revive":
        //         return data.reviveCount < GameData.gameData.maxAdPerDay;
        //     case "Treasure":
        //         return data.treasureCount < GameData.gameData.maxAdPerDay;
        //     case "Battle":
        //         return data.battleCount < GameData.gameData.maxAdPerDay;
        //     case "Ex":
        //         return data.exCount < GameData.gameData.maxAdPerDay;
        // }

        return false;
    }

    private void SaveAdWatchData(string type)
    {
        // GameData.AdData data;

        // if (File.Exists(filePath))
        // {
        //     // 기존 파일이 있으면 데이터 불러오기
        //     string json = File.ReadAllText(filePath);
        //     data = JsonUtility.FromJson<GameData.AdData>(json);
        // }
        // else
        // {
        //     // 파일이 없으면 새로 생성
        //     data = new GameData.AdData();
        // }

        // data.lastDate = DateTime.Now.ToString("yyyy-MM-dd");
        // switch(type)
        //{
        //     case "AdPoint":
        //         data.adPointCount++;
        //         break;
        //     case "Revive":
        //         data.reviveCount++;
        //         break;
        //     case "Treasure":
        //         data.treasureCount++;
        //         break;
        //     case "Battle":
        //         data.battleCount++;
        //         break;
        //     case "Ex":
        //         data.exCount++;
        //         break;
        // }
        // // JSON 파일로 저장
        // File.WriteAllText(filePath, JsonUtility.ToJson(data));
    }
}

