using Assets._Project.Develop.Runtime.Configs.Gameplay.Stages;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.Burrows;
using Assets._Project.Develop.Runtime.Gameplay.Features.Enemies;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature
{
    // A wave: every burrow of the wave releases its enemies one by one with its own interval after its start delay;
    // the wave ends when all burrows are empty and every spawned enemy is dead.
    public class WaveStage : IStage
    {
        private const float SpawnScatter = 1.2f;

        private readonly WaveStageConfig _config;
        private readonly EnemiesFactory _enemiesFactory;
        private readonly EntitiesLifeContext _entitiesLifeContext;
        private readonly BurrowsService _burrowsService;

        private readonly ReactiveEvent _completed = new();
        private readonly Dictionary<Entity, IDisposable> _aliveEnemiesToRemoveReason = new();
        private readonly List<BurrowSpawn> _burrowSpawns = new();

        private bool _inProcess;

        public WaveStage(
            WaveStageConfig config,
            EnemiesFactory enemiesFactory,
            EntitiesLifeContext entitiesLifeContext,
            BurrowsService burrowsService)
        {
            _config = config;
            _enemiesFactory = enemiesFactory;
            _entitiesLifeContext = entitiesLifeContext;
            _burrowsService = burrowsService;
        }

        public IReadOnlyEvent Completed => _completed;

        public void Start()
        {
            if (_inProcess)
                throw new InvalidOperationException("Wave already started");

            _burrowSpawns.Clear();

            foreach (BurrowSpawnConfig spawnConfig in _config.BurrowSpawns)
                _burrowSpawns.Add(new BurrowSpawn(spawnConfig, _burrowsService.GetPosition(spawnConfig.BurrowIndex)));

            _inProcess = true;
        }

        public void Update(float deltaTime)
        {
            if (_inProcess == false)
                return;

            bool allSpawned = true;

            foreach (BurrowSpawn burrowSpawn in _burrowSpawns)
            {
                if (burrowSpawn.IsDone)
                    continue;

                UpdateSpawn(burrowSpawn, deltaTime);
                allSpawned = false;
            }

            if (allSpawned && _aliveEnemiesToRemoveReason.Count == 0)
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

        private void UpdateSpawn(BurrowSpawn burrowSpawn, float deltaTime)
        {
            burrowSpawn.TimeToNextSpawn -= deltaTime;

            if (burrowSpawn.TimeToNextSpawn > 0)
                return;

            SpawnEnemy(burrowSpawn);
            burrowSpawn.SpawnedCount++;
            burrowSpawn.TimeToNextSpawn = burrowSpawn.Config.SpawnInterval;
        }

        private void SpawnEnemy(BurrowSpawn burrowSpawn)
        {
            Vector2 scatter = Random.insideUnitCircle * SpawnScatter;
            Vector3 position = burrowSpawn.Position + new Vector3(scatter.x, 0, scatter.y);

            Entity spawnedEnemy = _enemiesFactory.Create(position, burrowSpawn.Config.EnemyConfig);

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

        private void ProcessEnd()
        {
            _inProcess = false;
            _completed.Invoke();
        }

        private class BurrowSpawn
        {
            public BurrowSpawn(BurrowSpawnConfig config, Vector3 position)
            {
                Config = config;
                Position = position;
                TimeToNextSpawn = config.StartDelay;
            }

            public BurrowSpawnConfig Config { get; }
            public Vector3 Position { get; }
            public int SpawnedCount { get; set; }
            public float TimeToNextSpawn { get; set; }
            public bool IsDone => SpawnedCount >= Config.Count;
        }
    }
}
