using UnityEngine;
using UnityEngine.SceneManagement;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using GoogleMobileAds.Placement;
using Facebook.Unity;
using System;
using MyUtility;

public class LoadData : MonoBehaviour
{
    //string fileName = "savefile";
    //string path;
    //SavedValue savedValue;

    const string SAVE_STAGE_INDEX = "StageIndex";
    const string SAVE_PURCHASING_AD_FLAG = "PurchasingAdFlag"; // 1:purshased  0:NOT purchased

    private void Awake()
    {
        //MobileAds.Initialize(initStatus => { });

#if UNITY_IOS
        int status = ATTUtili.GetTrackingAuthorizationStatus();
        Debug.Log("ATT状態 = " + status);
        // ATT状態は4択
        // ATTrackingManagerAuthorizationStatusNotDetermined = 0
        // ATTrackingManagerAuthorizationStatusRestricted    = 1
        // ATTrackingManagerAuthorizationStatusDenied        = 2
        // ATTrackingManagerAuthorizationStatusAuthorized    = 3
        if (status == 0)
        {
            // ATT設定可能 & 未承認なのでATT承認要求アラートを表示
            ATTUtili.RequestTrackingAuthorization(CallbackFunction);
        }
        else
        {
            if (status == 1 || status == 2)
            {
                // ATT設定不可なので、ATT承認が必要になる旨をユーザーに伝える
                
            }
            else if(status == 3)
            {
                FB.Init(this.OnInitComplete, this.OnHideUnity);
            }
            // Google Mobile Ads SDK を初期化
            MobileAds.Initialize(initStatus =>
            {
                // AdMobからのコールバックはメインスレッドで呼び出される保証がないため、次のUpdate()で呼ばれるようにMobileAdsEventExecutorを使用
                MobileAdsEventExecutor.ExecuteInUpdate(() =>
                {
                    // バナーをリクエスト
                    CheckPurchasingAds();
                });
            });
        }

#elif UNITY_ANDROID
        MobileAds.Initialize(initStatus =>
        {
            // AdMobからのコールバックはメインスレッドで呼び出される保証がないため、次のUpdate()で呼ばれるようにMobileAdsEventExecutorを使用
            MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                // バナーをリクエスト
                CheckPurchasingAds();
            });
        });
#endif
    }

    private void OnHideUnity(bool isUnityShown)
    {
        throw new NotImplementedException();
    }

    private void OnInitComplete()
    {
        if (FB.IsInitialized)
        {
            FB.ActivateApp();
        }
        else
        {
            //Handle FB.Init
            FB.Init(() => {
                FB.ActivateApp();
                FB.Mobile.SetAdvertiserTrackingEnabled(true);
            });
        }
    }

    void CallbackFunction(int status)
    {
        Debug.Log("ATT最新状況 --> " + status);
        // ATTの状況を待ってから Google Mobile Ads SDK を初期化
        MobileAds.Initialize(initStatus => {
            // AdMobからのコールバックはメインスレッドで呼び出される保証がないため、次のUpdate()で呼ばれるようにMobileAdsEventExecutorを使用
            MobileAdsEventExecutor.ExecuteInUpdate(() => {
                // バナーをリクエスト
                CheckPurchasingAds();
            });
        });

        // Facebook SDK
        if (status == 1 || status == 2)
        {
            // ATT設定不可なので、ATT承認が必要になる旨をユーザーに伝える
            
            FB.Init(this.OnInitComplete, this.OnHideUnity);
        }
        else if (status == 3)
        {

            FB.Init(this.OnInitComplete, this.OnHideUnity);
        }
    }

    void CheckPurchasingAds()
    {
        if (PlayerPrefs.GetInt(SAVE_PURCHASING_AD_FLAG) == 0)
        {
            // banner is shown.
            BannerAdGameObject bannerAd = MobileAds.Instance.GetAd<BannerAdGameObject>("BannerAd");
            bannerAd.LoadAd();
            //bannerAd.Show();
        }
    }

    private void Start()
    {
        // Load stage index.
        if (!PlayerPrefs.HasKey(SAVE_STAGE_INDEX))
        {
            SceneManager.LoadScene(1);
            return;
        }

        int savedSceneNum = PlayerPrefs.GetInt(SAVE_STAGE_INDEX);
        if (savedSceneNum < SceneManager.sceneCountInBuildSettings - 1)
        {
            SceneManager.LoadScene(savedSceneNum + 1);
        }
        else
        {
            SceneManager.LoadScene(1);
        }

    }

}

