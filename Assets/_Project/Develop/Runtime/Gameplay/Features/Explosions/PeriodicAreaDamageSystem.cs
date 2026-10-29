using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilities;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System.Collections.Generic;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Explosions
{
    // A lasting area (a puddle): once per period every entity standing in it takes the damage once.
    // An entity with several colliders appears in the contacts several times, so each one is hit only once per period.
    public class PeriodicAreaDamageSystem : IInitializableSystem, IUpdatableSystem
    {
        private readonly HashSet<Entity> _damagedEntities = new();

        private Entity _entity;
        private Buffer<Entity> _contacts;
        private ReactiveVariable<float> _damage;
        private ReactiveVariable<float> _period;
        private ReactiveVariable<float> _currentTime;

        public void OnInit(Entity entity)
        {
            _entity = entity;
            _contacts = entity.ContactEntitiesBuffer;
            _damage = entity.AreaDamage;
            _period = entity.AreaDamagePeriod;
            _currentTime = entity.AreaDamageCurrentTime;
        }

        public void OnUpdate(float deltaTime)
        {
            _currentTime.Value += deltaTime;

            if (_currentTime.Value < _period.Value)
                return;

            _currentTime.Value -= _period.Value;
            _damagedEntities.Clear();

            for (int i = 0; i < _contacts.Count; i++)
            {
                Entity contact = _contacts.Items[i];

                if (_damagedEntities.Add(contact))
                    EntitiesHelper.TryTakeDamageFrom(_entity, contact, _damage.Value);
            }
        }
    }
}
