using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Rando = UnityEngine.Random;
using TMPro;
using UniRx;
using UniRx.Triggers;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using Lean.Touch;
using MyUtility;


namespace MoNo.Bowling {

    public enum GameProgressState
    {
        Going,
        InOrder,
        Bowling,
        Result,
        GameOver,
        nothing,
    }
    [System.Serializable]
    public class GameProgressStateReactiveProperty : ReactiveProperty<GameProgressState>
    {
        public GameProgressStateReactiveProperty() { }
        public GameProgressStateReactiveProperty(GameProgressState initialValue) : base(initialValue) { }

    }

    public class GameManager : SingletonMonoBehaviour<GameManager>
    {
        protected override bool DontDestroy => false;

        // property
        // System
        GameProgressStateReactiveProperty _gameProgressState = new GameProgressStateReactiveProperty(GameProgressState.nothing);
        [SerializeField] PlayerBehavior _player;

        // UI
        [SerializeField] Canvas _startCanvas;
        [SerializeField] FuncPropider _tapToStart;

        // field
        public GameProgressStateReactiveProperty gameProgressState => _gameProgressState;


        void Start()
        {
   
        }


    }
}