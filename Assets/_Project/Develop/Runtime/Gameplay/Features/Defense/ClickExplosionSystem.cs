using Assets._Project.Develop.Runtime.Configs.Gameplay.Defense;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Gameplay.Features.TeamsFeature;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Defense
{
    // Serves the click explosion requests of the wave state: a request makes an explosion unless the cooldown is running.
    public class ClickExplosionSystem : IInitializableSystem, IUpdatableSystem, IDisposableSystem
    {
        private readonly EntitiesFactory _entitiesFactory;
        private readonly DefenseConfig _config;

        private ReactiveEvent<Vector3> _request;
        private ReactiveEvent<Vector3> _event;
        private IDisposable _requestDisposable;
        private float _cooldownLeft;

        public ClickExplosionSystem(EntitiesFactory entitiesFactory, DefenseConfig config)
        {
            _entitiesFactory = entitiesFactory;
            _config = config;
        }

        public void OnInit(Entity entity)
        {
            _request = entity.ClickExplosionRequest;
            _event = entity.ClickExplosionEvent;

            _requestDisposable = _request.Subscribe(OnRequest);
        }

        public void OnUpdate(float deltaTime)
        {
            if (_cooldownLeft > 0)
                _cooldownLeft -= deltaTime;
        }

        public void OnDispose()
        {
            _requestDisposable.Dispose();
        }

        private void OnRequest(Vector3 point)
        {
            if (_cooldownLeft > 0)
                return;

            _entitiesFactory.CreateExplosion(
                point,
                _config.ClickExplosion,
                _config.ClickExplosionRadius,
                _config.ClickExplosionDamage,
                Teams.Player);

            _cooldownLeft = _config.ClickExplosionCooldown;
            _event.Invoke(point);
        }
    }
}
