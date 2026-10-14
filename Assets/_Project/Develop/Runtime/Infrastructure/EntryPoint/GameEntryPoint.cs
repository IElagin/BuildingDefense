using VContainer;
using Assets._Project.Develop.Runtime.Utilities.ConfigsManagement;
using Assets._Project.Develop.Runtime.Utilities.DataManagement.DataProviders;
using Assets._Project.Develop.Runtime.Utilities.LoadingScreen;
using Assets._Project.Develop.Runtime.Utilities.SceneManagement;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Infrastructure.EntryPoint
{
    public class GameEntryPoint : MonoBehaviour
    {
        private IObjectResolver _projectContainer;
        private CancellationTokenSource _projectLifetime;
        private SceneSwitcherService _sceneSwitcher;

        private void Awake()
        {
            Debug.Log("Старт проекта, сетап настроек");

            SetupAppSettings();

            Debug.Log("Процесс регистрации сервисов всего проекта");

            DontDestroyOnLoad(gameObject);
            _projectLifetime = new CancellationTokenSource();

            ContainerBuilder projectContainer = new ContainerBuilder();

            ProjectContextRegistrations.Process(projectContainer, _projectLifetime.Token);

            _projectContainer = projectContainer.Build();

            Initialize(_projectContainer, _projectLifetime.Token).Forget(AsyncErrors.Report);
        }

        private void SetupAppSettings()
        {
            const int targetFrameRate = 60;
            const int minimumVSyncCount = 1;
            const int maximumVSyncCount = 4;

            Application.targetFrameRate = targetFrameRate;
            Application.runInBackground = false;

#if UNITY_EDITOR
            // Game View VSync must also be enabled in the Editor window.
            int refreshRate = Mathf.RoundToInt((float)Screen.currentResolution.refreshRateRatio.value);
            QualitySettings.vSyncCount = Mathf.Clamp(
                Mathf.CeilToInt(refreshRate / (float)targetFrameRate), minimumVSyncCount, maximumVSyncCount);
#else
            QualitySettings.vSyncCount = 0;
#endif
        }

        private async UniTask Initialize(IObjectResolver container, CancellationToken cancellationToken)
        {
            ILoadingScreen loadingScreen = container.Resolve<ILoadingScreen>();
            SceneSwitcherService sceneSwitcherService = container.Resolve<SceneSwitcherService>();
            PlayerDataProvider playerDataProvider = container.Resolve<PlayerDataProvider>();
            _sceneSwitcher = sceneSwitcherService;

            loadingScreen.Show();

            Debug.Log("Начинается инициализация сервисов");

            await container.Resolve<ConfigsProviderService>().LoadAsync(cancellationToken);

            bool playerDataSaveExists = false;

            playerDataSaveExists = await playerDataProvider.ExistsAsync(cancellationToken);

            if (playerDataSaveExists)
                await playerDataProvider.LoadAsync(cancellationToken);
            else
                playerDataProvider.Reset();

            await UniTask.WaitForSeconds(1f, cancellationToken: cancellationToken);

            Debug.Log("Завершается инициализация сервисов");

            loadingScreen.Hide();

            await sceneSwitcherService.ProcessSwitchTo(Scenes.MainMenu);
        }

        private void OnApplicationQuit()
        {
            ReleaseServices();
        }

        private void OnDestroy()
        {
            ReleaseServices();
        }

        private void ReleaseServices()
        {
            _projectLifetime?.Cancel();

            _sceneSwitcher?.Dispose();
            _sceneSwitcher = null;

            _projectContainer?.Dispose();
            _projectContainer = null;

            _projectLifetime?.Dispose();
            _projectLifetime = null;
        }
    }
}
