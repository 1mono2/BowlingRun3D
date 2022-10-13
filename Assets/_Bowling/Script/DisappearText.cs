using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

[RequireComponent(typeof(Animator))]
public class DisappearText : MonoBehaviour
{

    void Start()
    {
        Animator animator = this.GetComponent<Animator>();
        ObservableStateMachineTrigger trigger = animator.GetBehaviour<ObservableStateMachineTrigger>();

        IDisposable exitState = trigger.
            OnStateExitAsObservable()
            .Subscribe(onStateInfo =>
            {
                Destroy(this.gameObject);

            }).AddTo(this);
        
    }


}
