using VContainer;
using Assets._Project.Develop.Runtime.Utilities.SceneManagement;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Infrastructure
{
    public abstract class SceneBootstrap : MonoBehaviour
    {
        public abstract void ProcessRegistrations(IContainerBuilder container, IInputSceneArgs sceneArgs = null);

        public abstract UniTask Initialize(IObjectResolver container, CancellationToken cancellationToken);

        public abstract void Run();
    }
}
