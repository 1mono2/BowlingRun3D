using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UniRx;
using UniRx.Triggers;

namespace MoNo.Bowling
{
    public class SmokeSpawner : MonoBehaviour
    {
        [SerializeField] GameObject _target;
        [SerializeField] VisualEffect _smoke;
        void Start()
        {
            bool isFire = false;
            this.UpdateAsObservable()
                .Subscribe(_ =>
                {
                    if(_target.transform.rotation.eulerAngles.x > 80 && isFire == false)
                    {
                        _smoke.Play();
                        isFire = true;
                    }
                    else if(_target.transform.rotation.eulerAngles.x <= 80)
                    {
                        isFire = false;
                    }
                });
            
        }
    }
}

