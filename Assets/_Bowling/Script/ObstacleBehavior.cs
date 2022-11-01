using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace MoNo.Bowling
{
    public class ObstacleBehavior : MonoBehaviour
    {

        // Start is called before the first frame update
        void Start()
        {
            PinBehavior pin;
            this.OnTriggerEnterAsObservable()
                //.Where(collider => collider.TryGetComponent<PinBehavior>(out pin))
                .Subscribe(collider =>
                {
                    if(collider.TryGetComponent<PinBehavior>(out pin))
                    {
                        pin.DeleteThis();
                    }
                });

        }
    }
}
