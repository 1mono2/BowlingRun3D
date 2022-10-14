using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace MoNo.Bowling
{
    public class MoveHorizonral : MonoBehaviour
    {
        [SerializeField] Renderer _targetRender;
        [SerializeField] float movePower = 1.0f;
        [SerializeField] float meter = 4f;

        void Start()
        {

            _targetRender.OnBecameVisibleAsObservable()
                .Take(1)
                .Subscribe(_ => {
                    MoveStart(); })
                .AddTo(this);

        }

        void MoveStart()
        {
            Vector3 initPos = this.transform.position;
            Vector3 dir = Vector3.right;
            this.FixedUpdateAsObservable()
                .Where(_ => GameManager.I.gameProgressState.Value == GameProgressState.Going)
                .Subscribe(_ =>
                {
                    Vector3 diff =  this.transform.position - initPos;
                    if(diff.x >= meter)
                    {
                        dir = Vector3.left;
                    }
                    else if (-meter > diff.x)
                    {
                        dir = Vector3.right;
                    }

                    this.transform.position += dir * Time.fixedDeltaTime * movePower;
                }).AddTo(this);
        }
    }
}