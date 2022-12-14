using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
using Rand = UnityEngine.Random;
using UnityEngine.Events;
using UniRx;
using UniRx.Triggers;
using DG.Tweening;
using Cysharp.Threading.Tasks;

namespace MoNo.Bowling
{
    public class PinsManager : MonoBehaviour
    {

        // property
        [HideInInspector] public ReactiveCollection<PinBehavior> pins => _pins;
        [HideInInspector] public ReactiveCollection<PinBehavior> knockedPins => _knockedPins;
        [HideInInspector] public ReactiveProperty<bool> isAllPinsKnocked => _isAllPinsKnocked;
        [HideInInspector] public int finalPinsNum => _finalPinsNum;
        ObjectPool<PinBehavior> _pinPool;


        // field
        [SerializeField] PinBehavior _pinPrefab;
        ReactiveCollection<PinBehavior> _pins = new ReactiveCollection<PinBehavior>();
        ReactiveCollection<PinBehavior> _knockedPins = new ReactiveCollection<PinBehavior>();
        ReactiveProperty<bool> _isAllPinsKnocked = new ReactiveProperty<bool>(false);
        int _finalPinsNum;


        // const value
        float PINS_MOVE_SPEED_AFTER_GOAL = 1f;
        float AMONG_PINS_DISTANCE = 0.5f;

        int LIMIT_PINS_NUM = 650;

        private void Start()
        {
            _pinPool = new ObjectPool<PinBehavior>(
                createFunc: () => Instantiate(_pinPrefab),
                actionOnGet: target => {
                    target.gameObject.SetActive(true);
                    // Reset Pin's paramater
                    target.ResetParam();
                    // When unenable pin, Release() invoke.
                    target.PinDeletedAsync.
                        Subscribe(_target =>  _pinPool.Release(_target)).AddTo(this).AddTo(target);
                    _pins.Add(target);
                    // set name of pin
                    if(target.name == _pinPrefab.name + "(Clone)")
                    {
                        target.name = _pinPool.CountAll.ToString();
                    }
                },
                actionOnRelease: target => {
                    target.gameObject.SetActive(false);
                    // remove pin instance from pinsList
                    var pinIndex = _pins.ToList().FindIndex(pin => pin.Equals(target));
                    _pins.Pop(pinIndex);
                },
                actionOnDestroy: target => Destroy(target),
                collectionCheck: true,
                defaultCapacity: 10,
                maxSize: 1000);
        }


        public void GeneratePin(Vector3 where, Transform parent)
        {
            Vector3 pos = where + new Vector3(Rand.insideUnitCircle.x, 0, Rand.insideUnitCircle.y);
            PinBehavior pin = _pinPool.Get();
            //pin = Instantiate(_pinPrefab, pos, Quaternion.identity, parent);
            pin.transform.position = pos;
            pin.transform.SetParent(parent);

            // a system when pin's deleted.
            //pin.PinDeletedAsync
            //    .Subscribe(pinBehavior =>
            //    {
            //        var pinIndex = _pins.ToList().FindIndex(pin => pin.Equals(pinBehavior));
            //        _pins.Pop(pinIndex);
            //        //Debug.Log($"Pin's name:" + pinBehavior.name + ", index:" + pinIndex);
            //    }).AddTo(pin).AddTo(this);
            //_pins.Add(pin);
        }

        public void DeletePinFromFirst()
        {
            if (!_pins.Any())
            {
                return;
            }
            var firstPin = _pins.Peek();
            //Destroy(firstPin.gameObject);
            _pinPool.Release(firstPin);

        }

        void AllChangeStateGoingToInOrder()
        {
            // adjust a state of pins.
            foreach (PinBehavior pin in _pins)
            {
                pin.ChangeStateGoingToInOrder();
            }
        }

        void AllChangeStateInOrderToBowling()
        {
            // adjust a state of pins.
            foreach (PinBehavior pin in _pins)
            {
                pin.ChangeStateInOrderToBowling();
            }
        }

        void EmitPins()
        {
            if(_pins.Count > LIMIT_PINS_NUM)
            {

                for (int i = _pins.Count -1; i >= LIMIT_PINS_NUM; i--)
                {

                    PinBehavior removePin =  _pins[i];
                    //Destroy(removePin.gameObject);
                    //_pins.RemoveAt(i);
                    _pinPool.Release(removePin);
                }
            }
        }

        public async void ReverseAlignPin()
        {
            EmitPins();
            AllChangeStateGoingToInOrder();

            _finalPinsNum = _pins.Count;

            List<PinBehavior> tempPins = new List<PinBehavior>(_pins);
            List<List<PinBehavior>> line = new List<List<PinBehavior>>();

            // create pins lists
            // untill 4 rows
            for (int i = 0; i <= 4; i++)
            {
                var innerLine = new List<PinBehavior>();
                for (int j = 0; j <= i; j++)
                {
                    if (!tempPins.Any()) { break; }
                    var currentPins = tempPins.Pop();
                    innerLine.Add(currentPins);
                }

                if (!innerLine.Any()) { break; }
                line.Add(innerLine);
            }

            // later 5 rows
            for (int i = 5; i < 1000; i++)
            {
                var innerLine = new List<PinBehavior>();
                if (i % 2 == 0) // even
                {
                    for (int j = 0; j <= 6; j++)
                    {
                        if (!tempPins.Any()) { break; }
                        var currentPins = tempPins.Pop();
                        innerLine.Add(currentPins);
                    }

                    if (!innerLine.Any()) { break; }
                    line.Add(innerLine);
                }
                else // odd
                {
                    for (int j = 0; j <= 5; j++)
                    {
                        if (!tempPins.Any()) { break; }
                        var currentPins = tempPins.Pop();
                        innerLine.Add(currentPins);
                    }

                    if (!innerLine.Any()) { break; }
                    line.Add(innerLine);
                }

            }

            // make pins tidy and in order
            this.transform.DOLocalMove(new Vector3(0, 0, 1), PINS_MOVE_SPEED_AFTER_GOAL);
            for (int i = 0; i < line.Count; i++)
            {
                //Debug.Log(line[i].Count);
                var currentLine = line[i];
                if (i % 2 == 0) //even line
                {
                    for (int j = 0; j < currentLine.Count; j++)
                    {
                        float posInLine = 0;
                        if (j == 0)
                        {
                            posInLine = 0;
                        }
                        else if (j % 2 == 0)
                        {

                            posInLine += (int)j / 2 * AMONG_PINS_DISTANCE * 2;
                        }
                        else
                        {
                            posInLine += (((int)j / 2) + 1) * AMONG_PINS_DISTANCE * 2;
                            posInLine *= -1;
                        }
                        currentLine[j].transform
                                .DOLocalMove(new Vector3(posInLine, 0, i * AMONG_PINS_DISTANCE), PINS_MOVE_SPEED_AFTER_GOAL);
                    }
                }
                else // odd line
                {
                    for (int j = 0; j < currentLine.Count; j++)
                    {
                        float posInLine = AMONG_PINS_DISTANCE;
                        if (j % 2 == 0)
                        {

                            posInLine += ((int)j / 2) * AMONG_PINS_DISTANCE * 2;
                        }
                        else
                        {
                            posInLine += ((int)j / 2) * AMONG_PINS_DISTANCE * 2;
                            posInLine *= -1;
                        }

                        currentLine[j].transform
                                .DOLocalMove(new Vector3(posInLine, 0, i * AMONG_PINS_DISTANCE), PINS_MOVE_SPEED_AFTER_GOAL);

                    }
                }
            }


            await UniTask.Delay(System.TimeSpan.FromSeconds(PINS_MOVE_SPEED_AFTER_GOAL));
            AllChangeStateInOrderToBowling();
        }


        public void StartObserveKnockedPin()
        {

            // subscribe a function of adding to knocked pins list when pins're knocked down.
            foreach (PinBehavior pin in _pins)
            {
                pin.knocked
                    .Where(boolState => true == boolState)
                    .Subscribe(boolState => { _knockedPins.Add(pin); }).AddTo(this).AddTo(pin);
            }

            _knockedPins.ObserveCountChanged()
                .Subscribe(count =>
                {
                    if (count == _pins.Count)
                    {
                        _isAllPinsKnocked.Value = true;
                        Debug.Log("Exactly!!");
                    }
                }).AddTo(this);


        }

        private void OnDestroy()
        {
            _pinPool.Clear();
        }

    }
}