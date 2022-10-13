using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using UniRx;
using UniRx.Triggers;
using Lean.Touch;
using DG.Tweening;


namespace MoNo.Bowling
{
    public class BowlingBallBehavior : MonoBehaviour
    {
        // property
        public ReactiveProperty<bool> isThrow => _isThrow;
        public ReactiveProperty<bool> isCollision => _isCollision;
        public UnityEvent leanFingerSwipeListener = new UnityEvent();

        // field
        [SerializeField] float _throwPower = 1.0f;
        [SerializeField] float _horizontalMoveSpeed = 1.0f;

        [SerializeField] Rigidbody _rb;
        [SerializeField] LeanFingerSwipe _leanFingerSwipe;
        ReactiveProperty<bool> _isThrow = new ReactiveProperty<bool>(false);
        ReactiveProperty<bool> _isCollision = new ReactiveProperty<bool>(false);

        [SerializeField] float _force = 20;
        [SerializeField] float _radius = 5;
        [SerializeField] float _upwards = 0;
        Vector3 _position;

        [SerializeField] float _acceleration = 1;

        [SerializeField] ParticleSystem _trailsPref; 
        [SerializeField] ParticleSystem _hitEffectPref; 

        IDisposable accelerateDisposable;

        void Start()
        {

            this.OnCollisionEnterAsObservable()
                .Where(collision => collision.collider.CompareTag("Pin"))
                .Subscribe(collision =>
                {

                    _isCollision.Value = true;

                    _position = this.gameObject.transform.position;
                    Collider[] hitColliders = Physics.OverlapSphere(_position, _radius);
                    for (int i = 0; i < hitColliders.Length; i++)
                    {
                        var hitrb = hitColliders[i].GetComponent<Rigidbody>();
                        if (hitrb)
                        {
                            hitrb.AddExplosionForce(_force, _position, _radius, _upwards, ForceMode.Impulse);
                        }

                    }

                });

            this.OnCollisionEnterAsObservable()
                .Where(collision => collision.collider.CompareTag("Pin"))
                .ThrottleFirst(TimeSpan.FromMilliseconds(150))
                .Subscribe(collision =>
                {

                   
                    Instantiate(_hitEffectPref, this.transform.position + Vector3.forward * 3, Quaternion.identity);

                });

            _leanFingerSwipe.OnFinger.AddListener(leanFinger =>
            {
                Throw();
                leanFingerSwipeListener.Invoke();
            });

            _isThrow.Where(boolVal => boolVal == true)
                .Subscribe(_ =>
                {
                    ParticleSystem trails = Instantiate(_trailsPref, this.transform.position, Quaternion.identity);
                    trails.Play();

                    this.UpdateAsObservable()
                    .Subscribe(_ =>
                    {
                        trails.transform.DOMove(this.transform.position + new Vector3(0, 0, 2), 0);
                    }).AddTo(this);
                });

            _isCollision.Where(boolVal => boolVal == true)
                .Subscribe();

        }

        public void Throw()
        {
            _rb.AddForce(Vector3.forward * _throwPower, ForceMode.Impulse);
            _isThrow.Value = true;
            AccelerationStart();
        }


        public void MoveHorizontal(Vector2 magnitude)
        {
            var tempHorizontal = Vector3.right * magnitude.x * _horizontalMoveSpeed;
            transform.position += tempHorizontal;
        }


        void AccelerationStart()
        {
            accelerateDisposable?.Dispose();
            
            accelerateDisposable = this.FixedUpdateAsObservable()
                                        .Subscribe(_ => _rb.AddForce(Vector3.forward * _acceleration * Time.fixedDeltaTime , ForceMode.VelocityChange));
            

        }

        void AccelerateStop()
        {
            accelerateDisposable?.Dispose();
        }



    }
}
