using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using Assets._Project.Develop.Runtime.Gameplay.Features.AI;
using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using VContainer;
using Assets._Project.Develop.Runtime.Utilities.AssetsManagement;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Infrastructure
{
    public class GameplayContextRegistrations
    {
        public static void Process(IContainerBuilder container, GameplayInputArgs args)
        {
            EntitiesLifeContext entitiesLifeContext = null;
            MonoEntitiesFactory monoEntitiesFactory = null;

            container.Register(c => entitiesLifeContext ??= CreateEntitiesLifeContext(c), Lifetime.Transient);

            container.Register(CreateCollidersRegistryService, Lifetime.Scoped);

            container.Register(CreateAIBrainsContext, Lifetime.Scoped);

            container.Register<IInputService>(CreateDesktopInput, Lifetime.Scoped);

            container.Register(c => monoEntitiesFactory ??= CreateMonoEntitiesFactory(c), Lifetime.Transient);

            container.RegisterBuildCallback(c => c.Resolve<MonoEntitiesFactory>().Initialize());

            // VContainer disposes tracked services in reverse creation order; the course releases entities before views.
            container.RegisterDisposeCallback(c =>
            {
                entitiesLifeContext.Dispose();
                monoEntitiesFactory.Dispose();
            });
        }

        private static DesktopInput CreateDesktopInput(IObjectResolver c)
        {
            return new DesktopInput();
        }

        private static AIBrainsContext CreateAIBrainsContext(IObjectResolver c)
        {
            return new AIBrainsContext();
        }

        private static CollidersRegistryService CreateCollidersRegistryService(IObjectResolver c)
        {
            return new CollidersRegistryService();
        }

        private static MonoEntitiesFactory CreateMonoEntitiesFactory(IObjectResolver c)
        {
            return new MonoEntitiesFactory(
                c.Resolve<ResourcesAssetsLoader>(),
                c.Resolve<EntitiesLifeContext>(),
                c.Resolve<CollidersRegistryService>());
        }

        private static EntitiesLifeContext CreateEntitiesLifeContext(IObjectResolver c)
        {
            return new EntitiesLifeContext();
        }
    }
}
