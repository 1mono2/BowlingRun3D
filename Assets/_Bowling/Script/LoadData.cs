using UnityEngine;
using UnityEngine.SceneManagement;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using GoogleMobileAds.Placement;
using Facebook.Unity;
using System;
//using MyUtility;
using MoNo.Utility;


namespace MoNo.Bowling
{
    public class LoadData : SingletonMonoBehaviour<LoadData> 
    {

        [SerializeField] bool _isShowAd = true;
        public bool isShowAd => _isShowAd;
        const string SAVE_STAGE_INDEX = "StageIndex";

        protected override bool DontDestroy => true;

        protected override void Awake()
        {
            base.Awake();

# if UNITY_IOS
            AppTrackingTranceparencyCheck att = new AppTrackingTranceparencyCheck();
            StartCoroutine(att.Check());
#endif

            // Facebook
            //FB.Init(this.OnInitComplete);
            // Google Admob
            MobileAds.Initialize(initStatus =>
            {
                // AdMobからのコールバックはメインスレッドで呼び出される保証がないため、次のUpdate()で呼ばれるようにMobileAdsEventExecutorを使用
                MobileAdsEventExecutor.ExecuteInUpdate(() =>
                {
                    // バナーをリクエスト
                    RequestAds();
                });
            });

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
                FB.Init(() =>
                {
                    FB.ActivateApp();
                    FB.Mobile.SetAdvertiserTrackingEnabled(true);
                });
            }
        }

        void RequestAds()
        {
            if (_isShowAd == false) return;
            // banner is shown.
            BannerAdGameObject bannerAd = MobileAds.Instance.GetAd<BannerAdGameObject>("BannerAd");
            bannerAd.LoadAd();
            
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
}
