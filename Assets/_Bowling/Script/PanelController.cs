using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelController : MonoBehaviour
{
    [SerializeField] GameObject panel;

    public void Appear()
    {
        panel.SetActive(true);
    }

    public void DisAppear()
    {
        panel.SetActive(false);
    }

}
