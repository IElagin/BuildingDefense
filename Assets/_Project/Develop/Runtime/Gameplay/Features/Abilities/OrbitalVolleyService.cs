using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.Enemies;
using Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature;
using Assets._Project.Develop.Runtime.Utilities;
using System;
using System.Collections.Generic;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Abilities
{
    // Mediator of the bought orbital volley: the first bugs of every wave get damage of a share of their max HP
    // once they are out of the burrow (during the spawn process a bug takes no damage).
    public class OrbitalVolleyService : IDisposable
    {
        private readonly EntitiesLifeContext _entitiesLifeContext;
        private readonly StageProviderService _stageProviderService;
        private readonly float _damagePercent;
        private readonly int _targetsCount;

        private readonly Dictionary<Entity, IDisposable> _spawningTargets = new();

        private IDisposable _stageNumberDisposable;
        private int _targetsLeft;

        public OrbitalVolleyService(
            EntitiesLifeContext entitiesLifeContext,
            StageProviderService stageProviderService,
            float damagePercent,
            int targetsCount)
        {
            _entitiesLifeContext = entitiesLifeContext;
            _stageProviderService = stageProviderService;
            _damagePercent = damagePercent;
            _targetsCount = targetsCount;
        }

        public void Initialize()
        {
            _stageNumberDisposable = _stageProviderService.CurrentStageNumber.Subscribe(OnStageNumberChanged);
            _entitiesLifeContext.Added += OnEntityAdded;
            _entitiesLifeContext.Released += OnEntityReleased;
        }

        public void Dispose()
        {
            _stageNumberDisposable?.Dispose();
            _entitiesLifeContext.Added -= OnEntityAdded;
            _entitiesLifeContext.Released -= OnEntityReleased;

            foreach (IDisposable spawnDisposable in _spawningTargets.Values)
                spawnDisposable.Dispose();

            _spawningTargets.Clear();
        }

        private void OnStageNumberChanged(int oldNumber, int newNumber) => _targetsLeft = _targetsCount;

        private void OnEntityAdded(Entity entity)
        {
            if (_damagePercent <= 0 || _targetsLeft <= 0 || entity.HasComponent<Bug>() == false)
                return;

            _targetsLeft--;

            _spawningTargets.Add(entity, entity.InSpawnProcess.Subscribe((oldValue, inSpawnProcess) =>
            {
                if (inSpawnProcess)
                    return;

                ForgetTarget(entity);
                entity.OrbitalVolleyHitEvent.Invoke();
                entity.TakeDamageRequest.Invoke(entity.MaxHealth.Value * Percent.ToShare(_damagePercent));
            }));
        }

        private void OnEntityReleased(Entity entity) => ForgetTarget(entity);

        private void ForgetTarget(Entity entity)
        {
            if (_spawningTargets.Remove(entity, out IDisposable spawnDisposable))
                spawnDisposable.Dispose();
        }
    }
}
