using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using System;
using VContainer.Unity;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.BuildingFeature
{
    // Keeps the building once it enters the life context, like the course MainHeroHolderService.
    public class BuildingHolderService : IInitializable, IDisposable
    {
        private readonly EntitiesLifeContext _entitiesLifeContext;

        private Entity _building;

        public BuildingHolderService(EntitiesLifeContext entitiesLifeContext)
        {
            _entitiesLifeContext = entitiesLifeContext;
        }

        public Entity Building => _building;

        public void Initialize()
        {
            _entitiesLifeContext.Added += OnEntityAdded;
        }

        public void Dispose()
        {
            _entitiesLifeContext.Added -= OnEntityAdded;
        }

        private void OnEntityAdded(Entity entity)
        {
            if (entity.HasComponent<IsBuilding>() == false)
                return;

            _entitiesLifeContext.Added -= OnEntityAdded;
            _building = entity;
        }
    }
}
