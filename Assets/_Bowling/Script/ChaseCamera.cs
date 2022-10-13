using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class ChaseCamera : MonoBehaviour
{
    [SerializeField] GameObject target;

    private void Start()
    {

        var diff = this.gameObject.transform.position - target.transform.position;

        this.FixedUpdateAsObservable()
            .Subscribe(_ =>
            {
                this.gameObject.transform.position = target.transform.position + diff;
            });
    }
}
