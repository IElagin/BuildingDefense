using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilities.Conditions;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Heal
{
    // The course ApplyDamageSystem the other way: a request adds health up to the maximum.
    public class HealSystem : IInitializableSystem, IDisposableSystem
    {
        private ReactiveEvent<float> _healRequest;
        private ReactiveEvent<float> _healEvent;

        private ReactiveVariable<float> _health;
        private ReactiveVariable<float> _maxHealth;

        private ICompositeCondition _canHeal;

        private IDisposable _requestDisposable;

        public void OnInit(Entity entity)
        {
            _healRequest = entity.HealRequest;
            _healEvent = entity.HealEvent;

            _health = entity.CurrentHealth;
            _maxHealth = entity.MaxHealth;

            _canHeal = entity.CanHeal;

            _requestDisposable = _healRequest.Subscribe(OnHealRequest);
        }

        public void OnDispose()
        {
            _requestDisposable.Dispose();
        }

        private void OnHealRequest(float amount)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount));

            if (_canHeal.Evaluate() == false)
                return;

            float healed = MathF.Min(_health.Value + amount, _maxHealth.Value) - _health.Value;

            if (healed <= 0)
                return;

            _health.Value += healed;
            _healEvent.Invoke(healed);
        }
    }
}
