using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Knockback
{
    // An enemy killed by a blast flies off by physics with that blast's impulse: its model gets its own rigidbody and
    // switches on the corpse collider (layer Corpses: ground, rocks and other corpses only), tumbles, bounces, may fall
    // off the plateau and shrinks away before the entity is released. Gameplay does not see corpses. The rigidbody is
    // added only at death: a kinematic body on the living model kept its spawn rotation while the entity turned.
    public class DeathFlightView : EntityView
    {
        private const float ImpulseWindow = 0.25f;

        [SerializeField] private Transform _model;
        [SerializeField] private Collider _corpseCollider;
        [SerializeField, Min(0.01f)] private float _mass = 1f;
        [SerializeField, Min(0)] private float _linearDamping = 0.05f;
        [SerializeField, Min(0)] private float _angularDamping = 0.4f;
        [SerializeField, Min(0)] private float _spin = 12f;
        [SerializeField, Min(0.01f)] private float _shrinkTime = 0.4f;

        private IDisposable _impulseDisposable;
        private IDisposable _isDeadDisposable;

        private Vector3 _impulse;
        private float _impulseTime = float.NegativeInfinity;
        private float _lifetime;
        private float _flightTime;
        private bool _isFlying;
        private Vector3 _startScale;

        protected override void OnEntityStartedWork(Entity entity)
        {
            _lifetime = entity.DeathProcessInitialTime.Value;
            _impulseDisposable = entity.TakeImpulseEvent.Subscribe(OnImpulse);
            _isDeadDisposable = entity.IsDead.Subscribe(OnIsDeadChanged);
        }

        public override void Cleanup(Entity entity)
        {
            base.Cleanup(entity);
            _impulseDisposable.Dispose();
            _isDeadDisposable.Dispose();
        }

        private void Update()
        {
            if (_isFlying == false)
                return;

            _flightTime += Time.deltaTime;
            float timeLeft = _lifetime - _flightTime;

            if (timeLeft < _shrinkTime)
                _model.localScale = _startScale * Mathf.Clamp01(timeLeft / _shrinkTime);
        }

        private void OnImpulse(Vector3 impulse)
        {
            _impulse = impulse;
            _impulseTime = Time.time;
        }

        private void OnIsDeadChanged(bool oldValue, bool isDead)
        {
            if (isDead == false || Time.time - _impulseTime > ImpulseWindow)
                return;

            _isFlying = true;
            _startScale = _model.localScale;

            Rigidbody body = _model.gameObject.AddComponent<Rigidbody>();
            body.mass = _mass;
            body.linearDamping = _linearDamping;
            body.angularDamping = _angularDamping;
            body.interpolation = RigidbodyInterpolation.Interpolate;

            _corpseCollider.enabled = true;
            body.AddForce(_impulse * Random.Range(0.85f, 1.15f), ForceMode.VelocityChange);
            body.AddTorque(Random.onUnitSphere * _spin, ForceMode.VelocityChange);
        }
    }
}
