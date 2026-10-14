using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.TeamsFeature;
using VContainer;
using Assets._Project.Develop.Runtime.Utilities.ConfigsManagement;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.MainHero
{
    // In this game the building plays the course "main hero" role: the player's side whose death means defeat.
    public class MainHeroFactory
    {
        private readonly IObjectResolver _container;

        private readonly EntitiesFactory _entitiesFactory;
        private readonly ConfigsProviderService _configsProviderService;
        private readonly EntitiesLifeContext _entitiesLifeContext;

        public MainHeroFactory(IObjectResolver container)
        {
            _container = container;
            _entitiesFactory = _container.Resolve<EntitiesFactory>();
            _configsProviderService = _container.Resolve<ConfigsProviderService>();
            _entitiesLifeContext = _container.Resolve<EntitiesLifeContext>();
        }

        public Entity Create(Vector3 position, float maxHealth)
        {
            BuildingConfig config = _configsProviderService.GetConfig<BuildingConfig>();

            Entity entity = _entitiesFactory.CreateBuilding(position, config, maxHealth);

            entity
                .AddIsMainHero()
                .AddTeam(new ReactiveVariable<Teams>(Teams.MainHero));

            _entitiesLifeContext.Add(entity);

            return entity;
        }
    }
}
