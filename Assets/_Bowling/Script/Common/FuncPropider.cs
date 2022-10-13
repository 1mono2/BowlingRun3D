using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FuncPropider : MonoBehaviour
{
    [System.Serializable]
    public class Action : UnityEvent { } ;
    public Action action;

    public void ActionInVoke()
    {
        action?.Invoke();
    }

    public void AddListener(UnityAction call)
    {
        action?.AddListener(call);
    }

}
