using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class GierMove : MonoBehaviour
{

    [SerializeField] GameObject gier;
    [SerializeField] GameObject basis;

    [SerializeField] int initDirection = 1; //  left < 0 <  right 
    [SerializeField] float speed = 1.0f;

    private void Start()
    {
        Vector3 dir;
        if (initDirection >= 0)
        {
            dir = Vector3.right;
        }
        else
        {
            dir = Vector3.left;
        }

        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                var gierPosX = gier.transform.localPosition.x;
                if (gierPosX > 0.7f)
                {
                    dir = Vector3.left;
                }
                else if (gierPosX < -1.8f)
                {
                    dir = Vector3.right;
                }

                gier.transform.localPosition += dir * Time.deltaTime * speed;
            });
    }

}
