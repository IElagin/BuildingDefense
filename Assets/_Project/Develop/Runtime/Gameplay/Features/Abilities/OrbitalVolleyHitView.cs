using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using System;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Abilities
{
    // The beam of the orbital volley on a bug, like ApplyDamageView for the hit sparks.
    public class OrbitalVolleyHitView : EntityView
    {
        [SerializeField] private ParticleSystem _hitEffectPrefab;

        private IDisposable _hitEventDisposable;

        protected override void OnEntityStartedWork(Entity entity)
        {
            _hitEventDisposable = entity.OrbitalVolleyHitEvent.Subscribe(OnHit);
        }

        public override void Cleanup(Entity entity)
        {
            base.Cleanup(entity);

            _hitEventDisposable?.Dispose();
        }

        private void OnHit()
        {
            Instantiate(_hitEffectPrefab, transform.position, Quaternion.identity);
        }
    }
}
