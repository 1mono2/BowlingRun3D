using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using Rando = UnityEngine.Random;
using TMPro;
using UniRx;
using UniRx.Triggers;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using MyUtility;

namespace MoNo.Bowling
{
    public class PlayerBehavior : MonoBehaviour
    {

        [Header("Realistic object")]
        [SerializeField] MainCamBehavior mainCamBehavior;
        [SerializeField] GameObject camStopperPref;
        [SerializeField] BowlingBallBehavior ball;
        [SerializeField] GameObject[] _stages;
        [SerializeField] ParticleSystem _confettiPref;

        [Header("System object")]
        [SerializeField] CapsuleCollider triggerCollider;
        [SerializeField] PinsManager pinsManager;
        [SerializeField] IDisposable timer;
        [SerializeField] List<Transform> camStopperPosList;

        [Header("UI object")]
        [SerializeField] RectTransform pinsCount;
        [SerializeField] TextMeshProUGUI pinsCountText;
        [SerializeField] TextMeshProUGUI addPinPref;
        [SerializeField] TextMeshProUGUI multiplyRateText;
        [SerializeField] StartCanvasBehavior startCanvas;
        [SerializeField] Canvas goingCanvas;
        [SerializeField] BowlingCanvasBehavior bowlingCanvas;
        [SerializeField] ResultCanvasBehavior resultCanvas;
        [SerializeField] GameOverCanvasBehavior gameOverCanvas;

        [Header("Move property")]
        [SerializeField] float damping = 10f;
        [SerializeField] float horizontalMoveSpeed = 1.0f;
        [SerializeField] float moveSpeed = 1.0f;
        [SerializeField] Vector2 endPoint = new Vector2(-5, 5);

        Vector3 remainingDelta = Vector3.zero;

        [SerializeField] int initPinsQuantity = 10;

        // hide state
        int multiplyRate = 0; // 0 ~10
        List<float> multiplyRateCriterion = new List<float>() {1, 1.2f, 1.4f, 1.6f, 1.8f, 2, 2.2f, 2.4f, 2.6f, 2.8f, 3}; // 11 rank

        const string SAVE_STAGE_INDEX = "StageIndex";


        private void Start()
        {
            DOTween.SetTweensCapacity(1500, 50);

            ball.leanFingerSwipeListener.AddListener(() =>
            {
                bowlingCanvas.swipeDisplay.SetActive(false);
            });

            GameManager.I.gameProgressState
                .Where(state => state == GameProgressState.Going)
                .Subscribe(state =>
                {
                    goingCanvas.gameObject.SetActive(true);

                });

            this.FixedUpdateAsObservable()
                .Where(_ => GameManager.I.gameProgressState.Value == GameProgressState.Going)
                .Subscribe(_ =>
                {
                    MoveForward();
                    FixedUpdatePosition();
                    pinsCount.position = RectTransformUtility.WorldToScreenPoint(mainCamBehavior.mainCam, pinsManager.transform.position)
                                                        + new Vector2(0, 100);
                }).AddTo(this);


            pinsManager.pins.ObserveCountChanged()
                .Subscribe(go =>
                {
                    if (!pinsManager.pins.Any())
                    {
                        OnGameOver();
                    }
                }).AddTo(this);

            pinsManager.pins.ObserveCountChanged()
                .Subscribe(i =>
                {
                    pinsCountText.text = i.ToString();
                }).AddTo(this);


            triggerCollider.OnTriggerEnterAsObservable()
                .Subscribe(triggerObj =>
                {
                    OrganizeQuantity quantity;
                    if (triggerObj.TryGetComponent<OrganizeQuantity>(out quantity))
                    {
                        int num = quantity.Calculation(pinsManager.pins.Count);
                        var addPinText = Instantiate(addPinPref, goingCanvas.gameObject.transform);
                        if (num > 0)
                        {
                            for (int i = 0; i < num; i++)
                            {
                                pinsManager.GeneratePin(pinsManager.transform.position, pinsManager.transform);
                            }
                            addPinText.text = $"+" + num.ToString();
                        }
                        else
                        {
                            for (int i = 0; i < -num; i++)
                            {
                                pinsManager.DeletePinFromFirst();
                            }
                            addPinText.text = num.ToString();
                        }

                        // panel disappear
                        quantity.GetComponent<PanelController>().DisAppear();
                    }
                }).AddTo(triggerCollider).AddTo(this);

            // after a goal
            triggerCollider.OnTriggerEnterAsObservable()
                .Subscribe(triggerObj =>
                {
                    if (triggerObj.CompareTag("BowlingFlag"))
                    {
                        GameManager.I.gameProgressState.Value = GameProgressState.Bowling;
                        triggerCollider.gameObject.SetActive(false);

                        pinsManager.ReverseAlignPin();
                        pinsManager.StartObserveKnockedPin();

                        ball.gameObject.SetActive(true);
                        goingCanvas.gameObject.SetActive(false);
                        bowlingCanvas.gameObject.SetActive(true);

                        int pinNum = pinsManager.finalPinsNum;

                        if (0 < pinNum && pinNum <= 54)
                        {
                            multiplyRate = 0;
                        }
                        else if (54 < pinNum && pinNum <= 118)
                        {
                            multiplyRate = 1;
                        }
                        else if (118 < pinNum && pinNum <= 183)
                        {
                            multiplyRate = 2;
                        }
                        else if (183 < pinNum && pinNum <= 248)
                        {
                            multiplyRate = 3;
                        }
                        else if (248 < pinNum && pinNum <= 313)
                        {
                            multiplyRate = 4;
                        }
                        else if (313 < pinNum && pinNum <= 378)
                        {
                            multiplyRate = 5;
                        }
                        else if (378 < pinNum && pinNum <= 443)
                        {
                            multiplyRate = 6;
                        }
                        else if (443 < pinNum && pinNum <= 508)
                        {
                            multiplyRate = 7;
                        }
                        else if (508 < pinNum && pinNum <= 573)
                        {
                            multiplyRate = 8;
                        }
                        else if (573 < pinNum && pinNum <= 620)
                        {
                            multiplyRate = 9;
                        }
                        else // 621 ~ 650
                        {
                            multiplyRate = 10;
                        }
                        //0~53, 54~118 119~183 184~248 ,249~313 , 314~378, 379~443 , 444~508, 509~573 , 574~620, 621~650

                        multiplyRateText.text = $"x{multiplyRateCriterion[multiplyRate]}";

                        Vector3 camStopperPos = camStopperPosList[multiplyRate].position;
                        Instantiate(camStopperPref, camStopperPos, Quaternion.identity);
                        Instantiate(_confettiPref, camStopperPos + new Vector3(-4, 0, 5), Quaternion.Euler(-45, 90, 0));
                        Instantiate(_confettiPref, camStopperPos + new Vector3(4, 0, 5), Quaternion.Euler(-45, -90, 0));

                        _stages[multiplyRate].GetComponent<Renderer>().material.DOColor(Color.white, 1).SetLoops(-1, LoopType.Yoyo);
                    }

                }).AddTo(triggerCollider).AddTo(this);

            ball.isCollision
                .Where(boolVal => boolVal == true)
                .Subscribe(_ =>
                {
                    mainCamBehavior.ShakeCamera();
                });



            // When a ball's through, timer starts.
            ball.isThrow
                .Where(isThrow => isThrow == true)
                .Subscribe(isThrow =>
                {
                    ResetTimer();
                });


            pinsManager.knockedPins
                .ObserveCountChanged(true)
                .Where(_ => GameManager.I.gameProgressState.Value == GameProgressState.Bowling)
                .Subscribe(num =>
                {
                    ResetTimer();
                });

            GameManager.I.gameProgressState
                .Where(state => state == GameProgressState.Result)
                .Subscribe(state =>
                {
                    bowlingCanvas.gameObject.SetActive(false);
                    OnResult();
                });



            // tenp examine
            var observable = Observable.IntervalFrame(1)
            .Take(initPinsQuantity)
            .Select(x => (int)x)
            .Subscribe(_ =>
            {
                pinsManager.GeneratePin(pinsManager.transform.position, pinsManager.transform);
            }).AddTo(this);





        }

        void ResetTimer()
        {
            timer?.Dispose();
            int countTime = 1;
            timer = Observable.Interval(TimeSpan.FromMilliseconds(1000))
                                                .Select(x => (int)(countTime - x))
                                                .TakeWhile(x => x > 0)
                                                .Subscribe(_ => { }, () => GameManager.I.gameProgressState.Value = GameProgressState.Result);
        }

        private void FixedUpdatePosition()
        {
            var fact = DampenFactor(damping, Time.fixedDeltaTime);
            var finalTransform = pinsManager.transform;
            var newDelta = Vector3.Lerp(remainingDelta, Vector3.zero, fact);

            var deltaDelta = remainingDelta - newDelta;
            if (pinsManager.transform.position.x < endPoint.x && Mathf.Sign(deltaDelta.x) < 0) // left side
            {
                deltaDelta = new Vector3(0, deltaDelta.y, deltaDelta.z);
            }
            else if (pinsManager.transform.position.x > endPoint.y && Mathf.Sign(deltaDelta.x) > 0)
            {
                deltaDelta = new Vector3(0, deltaDelta.y, deltaDelta.z);
            }

            finalTransform.position += deltaDelta;

            remainingDelta = newDelta;
        }

        private void MoveForward()
        {
            this.transform.position += new Vector3(0, 0, moveSpeed * Time.fixedDeltaTime);
        }


        public void MoveHorizontal(Vector2 magnitude)
        {
            var horizontalDelta = Vector3.right * magnitude.x * horizontalMoveSpeed;
            remainingDelta += horizontalDelta;
        }

        public static float DampenFactor(float speed, float elapsed)
        {
            if (speed < 0.0f)
            {
                return 1.0f;
            }

#if UNITY_EDITOR
			if (Application.isPlaying == false)
			{
				return 1.0f;
			}
#endif

            return 1.0f - Mathf.Pow((float)System.Math.E, -speed * elapsed);
        }

        void OnGameOver()
        {
            GameManager.I.gameProgressState.Value = GameProgressState.GameOver;
            gameOverCanvas.gameObject.SetActive(true);
            gameOverCanvas.retryButtonAction += () => SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        void OnResult()
        {
            PlayerPrefs.SetInt(SAVE_STAGE_INDEX, SceneManager.GetActiveScene().buildIndex);
            

            var resultScoreCount = pinsManager.knockedPins.Count;
            resultCanvas.gameObject.SetActive(true);
            if (LoadData.I.isShowAd == true)
            {
                resultCanvas.nextLevelButtonAction += ShowAds;
            }
            else
            {
                resultCanvas.nextLevelButtonAction += NextStage;
            }
        }

        void ShowAds()
        {
            InterstitialAds.I.OnAdClosed.AddListener(NextStage);
            InterstitialAds.I.ShowIfLoaded();
        }

        public void NextStage()
        {

            var currentStageIndex = SceneManager.GetActiveScene().buildIndex;
            int nextStageIndex;

            if (currentStageIndex < SceneManager.sceneCountInBuildSettings - 1)
            {
                nextStageIndex = currentStageIndex + 1;
            }
            else
            {
                nextStageIndex = 1;  // Return the First Stage without Preload
            }

            SceneManager.LoadScene(nextStageIndex);

        }

        private void OnDestroy()
        {
            this.transform.DOKill();
        }
    }
}
