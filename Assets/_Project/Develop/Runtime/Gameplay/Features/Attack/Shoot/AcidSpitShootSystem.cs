using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Attack.Shoot
{
    // The course InstantShootSystem for the spitter: the shot is an acid spit that bursts on touch.
    public class AcidSpitShootSystem : IInitializableSystem, IDisposableSystem
    {
        private readonly EntitiesFactory _entitiesFactory;
        private readonly SpitterConfig _config;

        private Entity _entity;

        private ReactiveEvent _attackDelayEndEvent;

        private Transform _shootPoint;

        private IDisposable _attackDelayEndDisposable;

        public AcidSpitShootSystem(EntitiesFactory entitiesFactory, SpitterConfig config)
        {
            _entitiesFactory = entitiesFactory;
            _config = config;
        }

        public void OnInit(Entity entity)
        {
            _entity = entity;

            _attackDelayEndEvent = entity.AttackDelayEndEvent;

            _shootPoint = entity.ShootPoint;

            _attackDelayEndDisposable = _attackDelayEndEvent.Subscribe(OnAttackDelayEnd);
        }

        private void OnAttackDelayEnd()
        {
            _entitiesFactory.CreateAcidSpit(_shootPoint.position, _shootPoint.forward, _entity, _config);
        }

        public void OnDispose()
        {
            _attackDelayEndDisposable.Dispose();
        }
    }
}
