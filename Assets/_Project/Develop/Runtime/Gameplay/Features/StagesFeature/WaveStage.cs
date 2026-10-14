using Assets._Project.Develop.Runtime.Configs.Gameplay.Stages;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.Enemies;
using Assets._Project.Develop.Runtime.Gameplay.Features.BuildingFeature;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature
{
    // A wave: enemies appear one by one on a ring around the building; the wave ends when all of them are dead.
    public class WaveStage : IStage
    {
        private const float FullTurn = Mathf.PI * 2;

        private readonly WaveStageConfig _config;
        private readonly EnemiesFactory _enemiesFactory;
        private readonly EntitiesLifeContext _entitiesLifeContext;
        private readonly BuildingHolderService _buildingHolderService;

        private readonly ReactiveEvent _completed = new();
        private readonly Dictionary<Entity, IDisposable> _aliveEnemiesToRemoveReason = new();

        private bool _inProcess;
        private int _spawnedCount;
        private float _timeToNextSpawn;

        public WaveStage(
            WaveStageConfig config,
            EnemiesFactory enemiesFactory,
            EntitiesLifeContext entitiesLifeContext,
            BuildingHolderService buildingHolderService)
        {
            _config = config;
            _enemiesFactory = enemiesFactory;
            _entitiesLifeContext = entitiesLifeContext;
            _buildingHolderService = buildingHolderService;
        }

        public IReadOnlyEvent Completed => _completed;

        public void Start()
        {
            if (_inProcess)
                throw new InvalidOperationException("Wave already started");

            _spawnedCount = 0;
            _timeToNextSpawn = 0;
            _inProcess = true;
        }

        public void Update(float deltaTime)
        {
            if (_inProcess == false)
                return;

            if (_spawnedCount < _config.EnemiesCount)
            {
                UpdateSpawn(deltaTime);
                return;
            }

            if (_aliveEnemiesToRemoveReason.Count == 0)
                ProcessEnd();
        }

        public void Cleanup()
        {
            foreach (KeyValuePair<Entity, IDisposable> item in _aliveEnemiesToRemoveReason)
            {
                item.Value.Dispose();
                _entitiesLifeContext.Release(item.Key);
            }

            _aliveEnemiesToRemoveReason.Clear();

            _inProcess = false;
        }

        public void Dispose()
        {
            foreach (KeyValuePair<Entity, IDisposable> item in _aliveEnemiesToRemoveReason)
                item.Value.Dispose();

            _aliveEnemiesToRemoveReason.Clear();

            _inProcess = false;
        }

        private void UpdateSpawn(float deltaTime)
        {
            _timeToNextSpawn -= deltaTime;

            if (_timeToNextSpawn > 0)
                return;

            SpawnEnemy();

            _spawnedCount++;
            _timeToNextSpawn = _config.SpawnInterval;
        }

        private void SpawnEnemy()
        {
            Entity spawnedEnemy = _enemiesFactory.Create(GetSpawnPosition(), _config.EnemyConfig);

            IDisposable removeReason = spawnedEnemy.IsDead.Subscribe((oldValue, isDead) =>
            {
                if (isDead)
                {
                    _aliveEnemiesToRemoveReason[spawnedEnemy].Dispose();
                    _aliveEnemiesToRemoveReason.Remove(spawnedEnemy);
                }
            });

            _aliveEnemiesToRemoveReason.Add(spawnedEnemy, removeReason);
        }

        private Vector3 GetSpawnPosition()
        {
            Vector3 center = _buildingHolderService.Building.Transform.position;
            float angle = Random.Range(0, FullTurn);

            return center + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * _config.SpawnRadius;
        }

        private void ProcessEnd()
        {
            _inProcess = false;
            _completed.Invoke();
        }
    }
}
