using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace MoNo.Bowling
{
    public class PinBehavior : MonoBehaviour
    {
        // property
        /// Publish Observable when this pin is deleted.
        [HideInInspector] public ReactiveProperty<PinBehaviorState> pinBehaviorState => _pinBehaviorState;
        [HideInInspector] public IObservable<PinBehavior> PinDeletedAsync => _pinDeletedSubject;
        [HideInInspector] public ReactiveProperty<bool> knocked => _knocked;


        // state
        public enum PinBehaviorState
        {
            Going,
            InOrder,
            Bowling,
            nothing
        }


        // field
        ReactiveProperty<PinBehaviorState> _pinBehaviorState = new ReactiveProperty<PinBehaviorState>(PinBehaviorState.Going);
        readonly AsyncSubject<PinBehavior> _pinDeletedSubject = new AsyncSubject<PinBehavior>();
        ReactiveProperty<bool> _knocked = new ReactiveProperty<bool>(false);
        Rigidbody _rb;
        [SerializeField] Vector3 _centerOfMass = Vector3.zero;
        [SerializeField] float _power = 1f;
        float _angle;
        IDisposable _disposable;
        [SerializeField] ParticleSystem _destroyedEffect;

        // const
        const int KNOCKED_ANGLE = 20;


        void Start()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.centerOfMass = _centerOfMass;

            _pinBehaviorState
                .Where(state => PinBehaviorState.Going == state)
                .FirstOrDefault()
                .Subscribe(_ =>
                {
                    OnGoing();
                }).AddTo(this);

            _pinBehaviorState
               .Where(state => PinBehaviorState.InOrder == state)
               .FirstOrDefault()
               .Subscribe(_ =>
               {
                   OnInOrder();
               }).AddTo(this);

            _pinBehaviorState
               .Where(state => PinBehaviorState.Bowling == state)
               .FirstOrDefault()
               .Subscribe(_ =>
               {
                   OnBowling();
               }).AddTo(this);


        }

        void OnGoing()
        {
            _disposable?.Dispose();
            
            _disposable = this.FixedUpdateAsObservable()
                .Subscribe(collider =>
                {
                    // behavior of moveing to center
                    var diff = Vector3.zero - this.transform.localPosition;
                    _rb.AddForce(diff * 0.001f * _power, ForceMode.Impulse);

                }).AddTo(this);

        }

        void OnInOrder()
        {
            _disposable?.Dispose();
            Destroy(this.gameObject.GetComponent<Rigidbody>());
            Destroy(this.gameObject.GetComponent<CapsuleCollider>());
        }

        void OnBowling()
        {
            _disposable?.Dispose();
            _rb = this.gameObject.AddComponent<Rigidbody>();
            var capsuleCol = this.gameObject.AddComponent<CapsuleCollider>();

            _rb.mass = 0.3f;
            _rb.drag = 1;
            _rb.angularDrag = 5f;
            _rb.constraints = RigidbodyConstraints.None;

            capsuleCol.radius = 0.3f;

            _disposable = this.FixedUpdateAsObservable()
               .ThrottleFirstFrame(20) // To save resource
               .Where(_ => false == _knocked.Value)
               .Subscribe(_ =>
               {
                   // a behavior of judging pin being knocked down
                   _angle = Vector3.Angle(Vector3.up, this.transform.up);
                   if (_angle > KNOCKED_ANGLE)
                   {
                       _knocked.Value = true;
                   }
               }).AddTo(this);
        }


        /// <summary>
        /// Change pins behavior state
        /// </summary>
        public bool ChangeStateGoingToInOrder()
        {
            if(PinBehaviorState.Going == _pinBehaviorState.Value)
            {
                _pinBehaviorState.Value = PinBehaviorState.InOrder;
                return true;
            }
            else
            {
                return false;
            }
            
        }

        public bool ChangeStateInOrderToBowling()
        {
            if(PinBehaviorState.InOrder == _pinBehaviorState.Value)
            {
                _pinBehaviorState.Value = PinBehaviorState.Bowling;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void DeleteThis()
        {
            // subsvribe a notification of pin being deleted
            _pinDeletedSubject.OnNext(this);
            _pinDeletedSubject.OnCompleted();
            _pinDeletedSubject.Dispose();
            ParticleSystem destroyedEffect = Instantiate(_destroyedEffect, this.transform.position, Quaternion.identity);
            destroyedEffect.Play();

        }

        private void OnDestroy()
        {
            _disposable?.Dispose();
        }

    }
}
