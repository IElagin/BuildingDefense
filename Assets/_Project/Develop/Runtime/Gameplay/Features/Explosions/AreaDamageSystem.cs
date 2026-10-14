using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilities;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System.Collections.Generic;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Explosions
{
    // The explosion is instant: contacts found on the first update take damage once, later arrivals do not.
    // An entity with several colliders appears in the contacts several times, so each one is hit only once.
    public class AreaDamageSystem : IInitializableSystem, IUpdatableSystem
    {
        private readonly HashSet<Entity> _damagedEntities = new();

        private Entity _entity;
        private Buffer<Entity> _contacts;
        private ReactiveVariable<float> _damage;
        private ReactiveVariable<bool> _isApplied;

        public void OnInit(Entity entity)
        {
            _entity = entity;
            _contacts = entity.ContactEntitiesBuffer;
            _damage = entity.AreaDamage;
            _isApplied = entity.IsAreaDamageApplied;
        }

        public void OnUpdate(float deltaTime)
        {
            if (_isApplied.Value)
                return;

            for (int i = 0; i < _contacts.Count; i++)
            {
                Entity contact = _contacts.Items[i];

                if (_damagedEntities.Add(contact))
                    EntitiesHelper.TryTakeDamageFrom(_entity, contact, _damage.Value);
            }

            _isApplied.Value = true;
        }
    }
}
