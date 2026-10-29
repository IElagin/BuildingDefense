using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using System;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Heal
{
    // The heal pair of ApplyDamageView: a one-shot effect on every actual heal.
    public class HealView : EntityView
    {
        [SerializeField] private ParticleSystem _healEffectPrefab;

        private IDisposable _healEventDisposable;

        protected override void OnEntityStartedWork(Entity entity)
        {
            _healEventDisposable = entity.HealEvent.Subscribe(OnHealed);
        }

        public override void Cleanup(Entity entity)
        {
            base.Cleanup(entity);

            _healEventDisposable?.Dispose();
        }

        private void OnHealed(float amount)
        {
            Instantiate(_healEffectPrefab, transform.position, Quaternion.identity);
        }
    }
}
