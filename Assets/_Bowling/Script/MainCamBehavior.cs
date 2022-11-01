using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UniRx;
using UniRx.Triggers;
using DG.Tweening;

namespace MoNo.Bowling
{
    public class MainCamBehavior : MonoBehaviour
    {

        [SerializeField] Camera _mainCam;
        [SerializeField] BowlingBallBehavior _bowlingBallBehavior;
        [SerializeField] PinsManager _pinsManager;


        IDisposable _disposableChase;
        Tweener _shakeRotationTweener;


        [SerializeField] float _duration = 1.0f;
        [SerializeField] float _strength = 1.0f;
        [SerializeField] int _vibrato = 3;


        // property
        public Camera mainCam => _mainCam;



        void Start()
        {
            GameManager.I.gameProgressState
                .Where(state => state == GameProgressState.Bowling)
                .Subscribe(state =>
                {

                    // move to side position
                    Vector3 diff = Vector3.zero;
                    mainCam.transform.DOLocalMove(new Vector3(5f, -6f, 0), 1f)
                                        .SetRelative(true);
                    mainCam.transform.DOLocalRotate(new Vector3(-10f,  -30f, 0), 1f)
                                        .SetRelative(true)
                                        .OnComplete(() =>
                                        {
                                            diff = mainCam.transform.position - _bowlingBallBehavior.transform.position;
                                            _disposableChase = this.FixedUpdateAsObservable()
                                                                .Subscribe(_ =>
                                                                {
                                                                    mainCam.transform.DOMove(_bowlingBallBehavior.transform.position + diff, 0f);
                                                                });
                                        });


                    
                });

            this.OnTriggerEnterAsObservable()
                .Where(collider => collider.CompareTag("CamStopper"))
                .Subscribe(_ =>
                {
                    _disposableChase?.Dispose();
                });


        }

        public void ShakeCamera()
        {

            _shakeRotationTweener?.Kill();

            _shakeRotationTweener = mainCam.DOShakeRotation(_duration, _strength, _vibrato)
                                            .SetRelative(true);
        }


        private void OnDestroy()
        {
            _disposableChase?.Dispose();
            _shakeRotationTweener?.Kill();
        }

    }
}
