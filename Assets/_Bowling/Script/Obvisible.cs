using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class Obvisible : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        this.OnBecameVisibleAsObservable()
            .Subscribe(_ => Debug.Log("visible: on obsever"));

        this.OnBecameInvisibleAsObservable()
            .Subscribe(_ => Debug.Log("Invisible: on obsever"));
            
    }

    private void OnBecameInvisible()
    {
        Debug.Log("invisible: on method");
    }

    private void OnBecameVisible()
    {
        Debug.Log("Visible: on method");
    }

}
