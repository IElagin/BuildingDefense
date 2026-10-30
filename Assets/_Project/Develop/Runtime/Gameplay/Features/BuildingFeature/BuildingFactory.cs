using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Levels;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.TeamsFeature;
using Assets._Project.Develop.Runtime.Meta.Features.AbilitiesUpgrade;
using VContainer;
using Assets._Project.Develop.Runtime.Utilities.ConfigsManagement;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.BuildingFeature
{
    // Role factory of the building, the game's counterpart of the course main hero (L7/14):
    // it finishes the recipe with the role marker and the team, then adds the entity to the life context.
    public class BuildingFactory
    {
        private readonly IObjectResolver _container;

        private readonly EntitiesFactory _entitiesFactory;
        private readonly ConfigsProviderService _configsProviderService;
        private readonly EntitiesLifeContext _entitiesLifeContext;
        private readonly LevelConfig _levelConfig;
        private readonly AbilitiesUpgradeService _abilitiesUpgradeService;

        public BuildingFactory(IObjectResolver container)
        {
            _container = container;
            _entitiesFactory = _container.Resolve<EntitiesFactory>();
            _configsProviderService = _container.Resolve<ConfigsProviderService>();
            _entitiesLifeContext = _container.Resolve<EntitiesLifeContext>();
            _levelConfig = _container.Resolve<LevelConfig>();
            _abilitiesUpgradeService = _container.Resolve<AbilitiesUpgradeService>();
        }

        public Entity Create(Vector3 position)
        {
            BuildingConfig config = _configsProviderService.GetConfig<BuildingConfig>();

            Entity entity = _entitiesFactory.CreateBuilding(position, config, _levelConfig.BuildingMaxHealth);

            entity
                .AddIsBuilding()
                .AddTeam(new ReactiveVariable<Teams>(Teams.Player));

            _entitiesFactory.AddHealOnStageStart(entity, _abilitiesUpgradeService.GetCurrentValueFor(AbilityTypes.DomeRepair));

            _entitiesLifeContext.Add(entity);

            return entity;
        }
    }
}
