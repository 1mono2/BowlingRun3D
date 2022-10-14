using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using DG.Tweening;

namespace MoNo.Bowling
{
    public class GenerateTrails : MonoBehaviour
    {
        [SerializeField] bool _isChase;
        [SerializeField] ParticleSystem _trailsPref;
        [SerializeField] GameObject _target;

        ParticleSystem trails;
        void Start()
        {
            trails = Instantiate(_trailsPref, _target.transform.position, Quaternion.Euler(0, 180, 0));

            if(_isChase == true) ChaseTarget();
            
        }

        void ChaseTarget()
        {
            this.UpdateAsObservable()
                .Subscribe(_ =>
                {
                    trails.transform.DOMove(_target.transform.position, 0f);
                });
        }


    }
}
