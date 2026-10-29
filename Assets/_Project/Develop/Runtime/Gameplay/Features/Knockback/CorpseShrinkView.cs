using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Knockback
{
    // A flying corpse shrinks away during the last moments of its death process, before the entity is released.
    public class CorpseShrinkView : EntityView
    {
        [SerializeField] private Transform _model;
        [SerializeField, Min(0.01f)] private float _shrinkTime = 0.4f;

        private ReactiveVariable<bool> _isCorpseFlying;
        private ReactiveVariable<float> _deathProcessCurrentTime;
        private Vector3 _startScale;

        protected override void OnEntityStartedWork(Entity entity)
        {
            _isCorpseFlying = entity.IsCorpseFlying;
            _deathProcessCurrentTime = entity.DeathProcessCurrentTime;
            _startScale = _model.localScale;
        }

        private void LateUpdate()
        {
            if (_isCorpseFlying == null || _isCorpseFlying.Value == false)
                return;

            float timeLeft = _deathProcessCurrentTime.Value;

            if (timeLeft < _shrinkTime)
                _model.localScale = _startScale * Mathf.Clamp01(timeLeft / _shrinkTime);
        }
    }
}
