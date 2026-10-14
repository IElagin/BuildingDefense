using Assets._Project.Develop.Runtime.Infrastructure;
using VContainer;
using Cysharp.Threading.Tasks;
using Assets._Project.Develop.Runtime.Utilities.SceneManagement;
using System.Threading;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Meta.Infrastructure
{
    public class MainMenuBootstrap : SceneBootstrap
    {
        public override void ProcessRegistrations(IContainerBuilder container, IInputSceneArgs sceneArgs = null)
        {
            MainMenuContextRegistrations.Process(container);
        }

        public override UniTask Initialize(IObjectResolver container, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Debug.Log("Инициализация сцены меню");

            return UniTask.CompletedTask;
        }

        public override void Run()
        {
            Debug.Log("Старт сцены меню");
        }
    }
}
