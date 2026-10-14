using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.AI;
using Assets._Project.Develop.Runtime.Gameplay.Features.BuildingFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.TeamsFeature;
using VContainer;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Enemies
{
    public class EnemiesFactory
    {
        private readonly IObjectResolver _container;

        private readonly EntitiesFactory _entitiesFactory;
        private readonly BrainsFactory _brainsFactory;
        private readonly EntitiesLifeContext _entitiesLifeContext;
        private readonly BuildingHolderService _buildingHolderService;

        public EnemiesFactory(IObjectResolver container)
        {
            _container = container;
            _entitiesFactory = _container.Resolve<EntitiesFactory>();
            _brainsFactory = _container.Resolve<BrainsFactory>();
            _entitiesLifeContext = _container.Resolve<EntitiesLifeContext>();
            _buildingHolderService = _container.Resolve<BuildingHolderService>();
        }

        public Entity Create(Vector3 position, EntityConfig config)
        {
            Entity entity;

            switch (config)
            {
                case BomberConfig bomberConfig:
                    entity = _entitiesFactory.CreateBomber(position, bomberConfig);
                    entity.AddCurrentTarget(new ReactiveVariable<Entity>(_buildingHolderService.Building));
                    _brainsFactory.CreateBomberBrain(entity);
                    break;

                default:
                    throw new ArgumentException($"Not support {config.GetType()} type config");
            }

            entity.AddTeam(new ReactiveVariable<Teams>(Teams.Enemies));

            _entitiesLifeContext.Add(entity);

            return entity;
        }
    }
}
