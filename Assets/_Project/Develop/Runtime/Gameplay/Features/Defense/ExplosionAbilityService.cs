using Assets._Project.Develop.Runtime.Configs.Gameplay.Defense;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.TeamsFeature;
using Assets._Project.Develop.Runtime.Utilities.Timer;
using System;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Defense
{
    // During a wave a click on the ground creates an explosion; a cooldown limits how often.
    public class ExplosionAbilityService : IDisposable
    {
        private readonly IInputService _inputService;
        private readonly GroundPointService _groundPointService;
        private readonly EntitiesFactory _entitiesFactory;
        private readonly DefenseConfig _config;
        private readonly TimerService _cooldown;

        public ExplosionAbilityService(
            IInputService inputService,
            GroundPointService groundPointService,
            EntitiesFactory entitiesFactory,
            DefenseConfig config,
            TimerServiceFactory timerServiceFactory)
        {
            _inputService = inputService;
            _groundPointService = groundPointService;
            _entitiesFactory = entitiesFactory;
            _config = config;
            _cooldown = timerServiceFactory.Create(config.ExplosionCooldown);
        }

        public void Update()
        {
            if (_inputService.IsPointerPressed == false)
                return;

            if (_cooldown.IsOver == false)
                return;

            if (_groundPointService.TryGetPoint(_inputService.PointerPosition, out Vector3 point) == false)
                return;

            _entitiesFactory.CreateExplosion(point, _config.ExplosionRadius, _config.ExplosionDamage, Teams.MainHero);
            _cooldown.Restart();
        }

        public void Dispose()
        {
            _cooldown.Dispose();
        }
    }
}
