using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets._Project.Develop.Runtime.Utilities.SceneManagement
{
    public class SceneLoaderService
    {
        public async UniTask LoadAsync(string sceneName, LoadSceneMode loadSceneMode = LoadSceneMode.Single,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            AsyncOperation wait = SceneManager.LoadSceneAsync(sceneName, loadSceneMode);

            await UniTask.WaitWhile(() => wait.isDone == false);
            cancellationToken.ThrowIfCancellationRequested();
        }

        public async UniTask UnloadAsync(string sceneName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            AsyncOperation wait = SceneManager.UnloadSceneAsync(sceneName);

            await UniTask.WaitWhile(() => wait.isDone == false);
            cancellationToken.ThrowIfCancellationRequested();
        }
    }
}
