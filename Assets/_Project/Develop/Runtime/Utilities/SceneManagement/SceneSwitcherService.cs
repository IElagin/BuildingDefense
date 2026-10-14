using Assets._Project.Develop.Runtime.Infrastructure;
using VContainer;
using Assets._Project.Develop.Runtime.Utilities.LoadingScreen;
using System;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets._Project.Develop.Runtime.Utilities.SceneManagement
{
    public class SceneSwitcherService : IDisposable
    {
        private readonly SceneLoaderService _sceneLoaderService;
        private readonly ILoadingScreen _loadingScreen;
        private readonly IObjectResolver _projectContainer;

        private IScopedObjectResolver _currentSceneContainer;

        private readonly CancellationToken _projectToken;
        private CancellationTokenSource _sceneLifetime;

        public SceneSwitcherService(
            SceneLoaderService sceneLoaderService,
            ILoadingScreen loadingScreen,
            IObjectResolver projectContainer,
            CancellationToken projectToken)
        {
            _sceneLoaderService = sceneLoaderService;
            _loadingScreen = loadingScreen;
            _projectContainer = projectContainer;
            _projectToken = projectToken;
        }

        public async UniTask ProcessSwitchTo(string sceneName, IInputSceneArgs sceneArgs = null,
            CancellationToken cancellationToken = default)
        {
            _projectToken.ThrowIfCancellationRequested();
            cancellationToken.ThrowIfCancellationRequested();
            using var transitionLifetime = CancellationTokenSource.CreateLinkedTokenSource(_projectToken, cancellationToken);
            CancellationToken token = transitionLifetime.Token;

            _loadingScreen.Show();

            ReleaseScene();

            CancellationTokenSource sceneLifetime = null;

            try
            {
                await _sceneLoaderService.LoadAsync(Scenes.Empty, cancellationToken: token);
                await _sceneLoaderService.LoadAsync(sceneName, cancellationToken: token);

                SceneBootstrap sceneBootstrap = Object.FindAnyObjectByType<SceneBootstrap>();

                if (sceneBootstrap == null)
                    throw new NullReferenceException(nameof(sceneBootstrap) + " not found");

                sceneLifetime = CancellationTokenSource.CreateLinkedTokenSource(_projectToken);
                _sceneLifetime = sceneLifetime;
                _currentSceneContainer = _projectContainer.CreateScope(container =>
                    sceneBootstrap.ProcessRegistrations(container, sceneArgs));

                using (token.Register(sceneLifetime.Cancel))
                    await sceneBootstrap.Initialize(_currentSceneContainer, sceneLifetime.Token);

                token.ThrowIfCancellationRequested();

                _loadingScreen.Hide();

                sceneBootstrap.Run();
            }
            catch (OperationCanceledException)
            {
                if (sceneLifetime != null && ReferenceEquals(_sceneLifetime, sceneLifetime))
                    ReleaseScene();
                throw;
            }
        }

        public void Dispose()
        {
            ReleaseScene();
        }

        private void ReleaseScene()
        {
            _sceneLifetime?.Cancel();
            _currentSceneContainer?.Dispose();
            _currentSceneContainer = null;

            _sceneLifetime?.Dispose();
            _sceneLifetime = null;
        }
    }
}
