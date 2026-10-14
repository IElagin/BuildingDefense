using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Gameplay.Features.TeamsFeature;
using Assets._Project.Develop.Runtime.Utilities.Conditions;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Blast
{
    public class BlastSystem : IInitializableSystem, IUpdatableSystem
    {
        private readonly EntitiesFactory _entitiesFactory;
        private readonly ExplosionConfig _explosionConfig;

        private ICompositeCondition _mustBlast;
        private ReactiveVariable<bool> _isBlasted;
        private ReactiveVariable<float> _radius;
        private ReactiveVariable<float> _damage;
        private ReactiveVariable<Teams> _team;
        private Transform _transform;

        public BlastSystem(EntitiesFactory entitiesFactory, ExplosionConfig explosionConfig)
        {
            _entitiesFactory = entitiesFactory;
            _explosionConfig = explosionConfig;
        }

        public void OnInit(Entity entity)
        {
            _mustBlast = entity.MustBlast;
            _isBlasted = entity.IsBlasted;
            _radius = entity.BlastRadius;
            _damage = entity.BlastDamage;
            _team = entity.Team;
            _transform = entity.Transform;
        }

        public void OnUpdate(float deltaTime)
        {
            if (_isBlasted.Value)
                return;

            if (_mustBlast.Evaluate() == false)
                return;

            _isBlasted.Value = true;
            _entitiesFactory.CreateExplosion(_transform.position, _explosionConfig, _radius.Value, _damage.Value, _team.Value);
        }
    }
}
