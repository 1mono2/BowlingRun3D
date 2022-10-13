using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Lean.Touch;

namespace MoNo.Bowling
{
    public class StartCanvasBehavior : MonoBehaviour
    {
        [SerializeField] Canvas _startCanvas;
        [SerializeField] LeanFingerDownCanvas _tapToStart;
        [SerializeField] TextMeshProUGUI _levelText;

        void Start()
        {
            _tapToStart.OnFinger.AddListener(leanFinger =>
            {
                _startCanvas.gameObject.SetActive(false);
                GameManager.I.gameProgressState.Value = GameProgressState.Going;
            });

            _levelText.text =  $"Level { SceneManager.GetActiveScene().buildIndex}";
        }

    }
}
