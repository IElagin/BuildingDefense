using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilities;
using Assets._Project.Develop.Runtime.Utilities.Reactive;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Explosions
{
    // The explosion is instant: contacts found on the first update take damage once, later arrivals do not.
    public class AreaDamageSystem : IInitializableSystem, IUpdatableSystem
    {
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
                EntitiesHelper.TryTakeDamageFrom(_entity, _contacts.Items[i], _damage.Value);

            _isApplied.Value = true;
        }
    }
}
