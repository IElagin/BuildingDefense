using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Knockback
{
    // A bug killed by a blast flies off by physics with that blast's impulse: the corpse model gets its own rigidbody
    // and switches on the corpse collider (layer Corpses: ground, rocks and other corpses only). "Killed by a blast"
    // is the fact that the bug took an impulse and died in the same tick of its systems: an explosion pushes before
    // it damages, the death comes on the bug's next update. The rigidbody is added only at death: a kinematic body
    // on the living model kept its spawn rotation while the entity turned. Goes after DeathSystem in the recipe.
    public class DeathFlightSystem : IInitializableSystem, IUpdatableSystem, IDisposableSystem
    {
        private readonly BugConfig _config;

        private Transform _model;
        private Collider _collider;
        private ReactiveVariable<bool> _isCorpseFlying;

        private IDisposable _impulseDisposable;
        private IDisposable _isDeadDisposable;

        private Vector3 _impulse;
        private bool _hasImpulse;

        public DeathFlightSystem(BugConfig config)
        {
            _config = config;
        }

        public void OnInit(Entity entity)
        {
            _model = entity.CorpseModel;
            _collider = entity.CorpseCollider;
            _isCorpseFlying = entity.IsCorpseFlying;

            _impulseDisposable = entity.TakeImpulseEvent.Subscribe(OnImpulse);
            _isDeadDisposable = entity.IsDead.Subscribe(OnIsDeadChanged);
        }

        public void OnUpdate(float deltaTime)
        {
            _hasImpulse = false;
        }

        public void OnDispose()
        {
            _impulseDisposable.Dispose();
            _isDeadDisposable.Dispose();
        }

        private void OnImpulse(Vector3 impulse)
        {
            _impulse = impulse;
            _hasImpulse = true;
        }

        private void OnIsDeadChanged(bool oldValue, bool isDead)
        {
            if (isDead == false || _hasImpulse == false)
                return;

            Rigidbody body = _model.gameObject.AddComponent<Rigidbody>();
            body.mass = _config.CorpseMass;
            body.linearDamping = _config.CorpseLinearDamping;
            body.angularDamping = _config.CorpseAngularDamping;
            body.interpolation = RigidbodyInterpolation.Interpolate;

            _collider.enabled = true;
            body.AddForce(_impulse * Random.Range(0.85f, 1.15f), ForceMode.VelocityChange);
            body.AddTorque(Random.onUnitSphere * _config.CorpseSpin, ForceMode.VelocityChange);

            _isCorpseFlying.Value = true;
        }
    }
}
