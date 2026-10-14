using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.AI;
using Assets._Project.Develop.Runtime.Gameplay.Features.BuildingFeature;
using Assets._Project.Develop.Runtime.Gameplay.States;
using Assets._Project.Develop.Runtime.Infrastructure;
using Assets._Project.Develop.Runtime.UI.Gameplay;
using VContainer;
using Cysharp.Threading.Tasks;
using Assets._Project.Develop.Runtime.Utilities.SceneManagement;
using System;
using System.Threading;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Infrastructure
{
    public class GameplayBootstrap : SceneBootstrap
    {
        private IObjectResolver _container;
        private GameplayInputArgs _inputArgs;

        private EntitiesLifeContext _entitiesLifeContext;
        private AIBrainsContext _brainsContext;

        private GameplayStatesContext _gameplayStatesContext;

        public override void ProcessRegistrations(IContainerBuilder container, IInputSceneArgs sceneArgs = null)
        {
            if (sceneArgs is not GameplayInputArgs gameplayInputArgs)
                throw new ArgumentException($"{nameof(sceneArgs)} is not match with {typeof(GameplayInputArgs)} type");

            _inputArgs = gameplayInputArgs;

            GameplayContextRegistrations.Process(container, _inputArgs);
        }

        public override UniTask Initialize(IObjectResolver container, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _container = container;

            Debug.Log($"Вы попали на уровень {_inputArgs.LevelNumber}");

            Debug.Log("Инициализация геймплейной сцены");

            _entitiesLifeContext = _container.Resolve<EntitiesLifeContext>();
            _brainsContext = _container.Resolve<AIBrainsContext>();

            _gameplayStatesContext = _container.Resolve<GameplayStatesContext>();

            _container.Resolve<BuildingFactory>().Create(Vector3.zero);

            _container.Resolve<GameplayScreenPresenter>().Initialize();

            return UniTask.CompletedTask;
        }

        public override void Run()
        {
            Debug.Log("Старт геймплейной сцены");

            _gameplayStatesContext.Run();
        }

        private void Update()
        {
            _brainsContext?.Update(Time.deltaTime);
            _entitiesLifeContext?.Update(Time.deltaTime);
            _gameplayStatesContext?.Update(Time.deltaTime);
        }
    }
}
