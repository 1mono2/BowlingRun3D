using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


namespace MoNo.Bowling
{
    public class ResultCanvasBehavior : MonoBehaviour
    {
        [SerializeField] Canvas resultCanvas;
        [SerializeField] Button nextLevelButton;

        public UnityAction nextLevelButtonAction;

        void Start()
        {
            nextLevelButton.onClick.AddListener(nextLevelButtonAction);
        }
    }
}