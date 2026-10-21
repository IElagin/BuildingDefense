using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.ApplyDamage
{
    public class ApplyDamageView : EntityView
    {
        [SerializeField] private ParticleSystem _applyDamageEffectPrefab;
        [SerializeField] private Transform _effectSpawnPoint;

        private ReactiveEvent<float> _damageEvent;

        private IDisposable _damageEventDisposable;

        protected override void OnEntityStartedWork(Entity entity)
        {
            _damageEvent = entity.TakeDamageEvent;
            _damageEventDisposable = _damageEvent.Subscribe(OnDamaged);
        }

        public override void Cleanup(Entity entity)
        {
            base.Cleanup(entity);

            _damageEventDisposable.Dispose();
        }

        private void OnDamaged(float damage)
        {
            Instantiate(_applyDamageEffectPrefab, _effectSpawnPoint.position, Quaternion.identity);
        }
    }
}
