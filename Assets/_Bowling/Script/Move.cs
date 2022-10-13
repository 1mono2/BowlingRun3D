using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UniRx;
using UniRx.Triggers;


namespace MoNo.Bowling {
    public class Move : MonoBehaviour
    {
        [SerializeField] float movePower = 1.0f;
        [SerializeField] float rotatePower = 1.0f;
        void Start()
        {

            this.OnBecameVisibleAsObservable()
                .Take(1)
                .Subscribe(_ => MoveStart())
                .AddTo(this);

        }

        void MoveStart()
        {
            this.FixedUpdateAsObservable()
                .Where(_ => GameManager.I.gameProgressState.Value == GameProgressState.Going)
                .Subscribe(_ =>
                {
                    this.transform.position += Vector3.back * Time.fixedDeltaTime * movePower;
                    this.transform.rotation *= Quaternion.Euler(-1 * Time.fixedDeltaTime * rotatePower, 0, 0);
                });
        }
    }
}
