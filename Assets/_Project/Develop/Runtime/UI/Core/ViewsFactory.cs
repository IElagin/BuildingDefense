using Assets._Project.Develop.Runtime.Utilities.AssetsManagement;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets._Project.Develop.Runtime.UI.Core
{
    public class ViewsFactory
    {
        private readonly ResourcesAssetsLoader _resourcesAssetsLoader;

        private readonly Dictionary<string, string> _viewIDToResourcesPath = new Dictionary<string, string>()
        {
            { ViewIDs.CurrencyView, "UI/Wallet/CurrencyView" },
            { ViewIDs.MainMenuScreen, "UI/MainMenu/MainMenuScreen" },
            { ViewIDs.GameplayScreen, "UI/Gameplay/GameplayScreen" },
            { ViewIDs.WinPopup, "UI/Gameplay/ResultsPopup/WinPopup" },
            { ViewIDs.DefeatPopup, "UI/Gameplay/ResultsPopup/DefeatPopup" },
            { ViewIDs.SimpleHealthBar, "UI/Gameplay/HealthBars/SimpleHealthBar" },
            { ViewIDs.BuildingHealthBar, "UI/Gameplay/HealthBars/BuildingHealthBar" }
        };

        public ViewsFactory(ResourcesAssetsLoader resourcesAssetsLoader)
        {
            _resourcesAssetsLoader = resourcesAssetsLoader;
        }

        public TView Create<TView>(string viewID, Transform parent = null) where TView : MonoBehaviour, IView
        {
            if (_viewIDToResourcesPath.TryGetValue(viewID, out string resourcePath) == false)
                throw new ArgumentException($"You didn't set resource path for {typeof(TView)}, searched id: {viewID}");

            GameObject prefab = _resourcesAssetsLoader.Load<GameObject>(resourcePath);
            GameObject instance = Object.Instantiate(prefab, parent);
            TView view = instance.GetComponent<TView>();

            if (view == null)
                throw new InvalidOperationException($"Not found {typeof(TView)} component on view instance");

            return view;
        }

        public void Release<TView>(TView view) where TView : MonoBehaviour, IView
        {
            Object.Destroy(view.gameObject);
        }
    }
}
