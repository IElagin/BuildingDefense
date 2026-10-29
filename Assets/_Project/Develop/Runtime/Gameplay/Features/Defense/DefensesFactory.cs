using Assets._Project.Develop.Runtime.Configs.Gameplay.Defense;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.AI;
using Assets._Project.Develop.Runtime.Gameplay.Features.AI.States;
using Assets._Project.Develop.Runtime.Gameplay.Features.TeamsFeature;
using Assets._Project.Develop.Runtime.Utilities.ConfigsManagement;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System;
using UnityEngine;
using VContainer;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Defense
{
    // Role factory of the player's defenses, the counterpart of the enemies factory: mines and puddles are whole
    // in their recipes; a turret gets its role marker, team, target and brain here, then joins the life context.
    public class DefensesFactory
    {
        private readonly IObjectResolver _container;

        private readonly EntitiesFactory _entitiesFactory;
        private readonly BrainsFactory _brainsFactory;
        private readonly EntitiesLifeContext _entitiesLifeContext;
        private readonly DefenseConfig _config;

        public DefensesFactory(IObjectResolver container)
        {
            _container = container;
            _entitiesFactory = _container.Resolve<EntitiesFactory>();
            _brainsFactory = _container.Resolve<BrainsFactory>();
            _entitiesLifeContext = _container.Resolve<EntitiesLifeContext>();
            _config = _container.Resolve<ConfigsProviderService>().GetConfig<DefenseConfig>();
        }

        public Entity Create(DefenseTypes type, Vector3 position)
        {
            switch (type)
            {
                case DefenseTypes.Mine:
                    return _entitiesFactory.CreateMine(position, _config.Mine, Teams.Player);

                case DefenseTypes.Turret:
                    return CreateTurret(position);

                case DefenseTypes.Puddle:
                    return _entitiesFactory.CreatePuddle(position, _config.Puddle, Teams.Player);

                default:
                    throw new ArgumentException($"Not support {type} defense");
            }
        }

        private Entity CreateTurret(Vector3 position)
        {
            Entity entity = _entitiesFactory.CreateTurret(position, _config.Turret);

            entity
                .AddTurret()
                .AddTeam(new ReactiveVariable<Teams>(Teams.Player))
                .AddCurrentTarget();

            ITargetSelector targetSelector = new TargetInRadiusSelector(
                new NearestDamageableTargetSelector(entity),
                entity,
                _config.Turret.AttackRadius);

            _brainsFactory.CreateTurretBrain(entity, targetSelector);

            _entitiesLifeContext.Add(entity);

            return entity;
        }
    }
}
