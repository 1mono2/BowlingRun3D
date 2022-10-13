using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using UniRx.Triggers;

namespace MoNo.Bowling
{

    public class GameOverCanvasBehavior : MonoBehaviour
    {

        [SerializeField] Canvas gameOverCanvas;
        [SerializeField] Button retryButton;

        public UnityAction retryButtonAction;

        void Start()
        {
            retryButton.onClick.AddListener(retryButtonAction);
        }
    }
}
