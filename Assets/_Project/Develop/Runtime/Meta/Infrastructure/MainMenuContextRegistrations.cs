using VContainer;
using Assets._Project.Develop.Runtime.UI;
using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.UI.MainMenu;
using Assets._Project.Develop.Runtime.Utilities.AssetsManagement;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Meta.Infrastructure
{
    public class MainMenuContextRegistrations
    {
        public static void Process(IContainerBuilder container)
        {
            container.Register(CreateMainMenuUIRoot, Lifetime.Scoped);
            container.Register(CreateMainMenuPresentersFactory, Lifetime.Scoped);
            container.Register(CreateMainMenuScreenPresenter, Lifetime.Scoped);
            container.RegisterBuildCallback(c =>
            {
                c.Resolve<MainMenuUIRoot>();
                c.Resolve<MainMenuScreenPresenter>().Initialize();
            });
        }

        private static MainMenuUIRoot CreateMainMenuUIRoot(IObjectResolver c)
        {
            ResourcesAssetsLoader resourcesAssetsLoader = c.Resolve<ResourcesAssetsLoader>();

            MainMenuUIRoot mainMenuUIRootPrefab = resourcesAssetsLoader
                .Load<MainMenuUIRoot>("UI/MainMenu/MainMenuUIRoot");

            return Object.Instantiate(mainMenuUIRootPrefab);
        }

        private static MainMenuPresentersFactory CreateMainMenuPresentersFactory(IObjectResolver c)
        {
            return new MainMenuPresentersFactory(c);
        }

        private static MainMenuScreenPresenter CreateMainMenuScreenPresenter(IObjectResolver c)
        {
            MainMenuUIRoot uiRoot = c.Resolve<MainMenuUIRoot>();

            MainMenuScreenView view = c
                .Resolve<ViewsFactory>()
                .Create<MainMenuScreenView>(ViewIDs.MainMenuScreen, uiRoot.HUDLayer);

            MainMenuScreenPresenter presenter = c
                .Resolve<MainMenuPresentersFactory>()
                .CreateMainMenuScreen(view);

            return presenter;
        }
    }
}
