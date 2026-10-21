using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.BuildingFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.Defense;
using Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature;
using Assets._Project.Develop.Runtime.Utilities.Audio;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System;
using System.Collections.Generic;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.AudioFeature
{
    // Gameplay sounds from entity and service events in one place (Studio audio addition, as the FinalProject service):
    // bugs (spawn, death, acid blast at the dome), the dome (hit, destruction), mines (placed, blast, refused click),
    // the orbital strike and the wave start. Views and prefabs stay free of audio.
    public class GameplaySoundsService : IDisposable
    {
        private readonly IAudioService _audio;
        private readonly EntitiesLifeContext _entitiesLifeContext;
        private readonly Dictionary<Entity, List<IDisposable>> _subscriptions = new();
        private readonly List<IDisposable> _serviceSubscriptions = new();

        public GameplaySoundsService(
            IAudioService audio,
            EntitiesLifeContext entitiesLifeContext,
            StageProviderService stageProviderService,
            ClickExplosionService clickExplosionService,
            MinePlacementService minePlacementService)
        {
            _audio = audio;
            _entitiesLifeContext = entitiesLifeContext;

            _entitiesLifeContext.Added += OnEntityAdded;
            _entitiesLifeContext.Released += OnEntityReleased;

            foreach (Entity entity in _entitiesLifeContext.Entities)
                OnEntityAdded(entity);

            _serviceSubscriptions.Add(stageProviderService.CurrentStageNumber.Subscribe(OnStageChanged));
            _serviceSubscriptions.Add(clickExplosionService.Exploded.Subscribe(_ => _audio.Play(SoundIds.OrbitalStrike)));
            _serviceSubscriptions.Add(minePlacementService.Denied.Subscribe(() => _audio.Play(SoundIds.MineDenied)));
        }

        public void Dispose()
        {
            _entitiesLifeContext.Added -= OnEntityAdded;
            _entitiesLifeContext.Released -= OnEntityReleased;

            foreach (IDisposable subscription in _serviceSubscriptions)
                subscription.Dispose();

            _serviceSubscriptions.Clear();

            foreach (List<IDisposable> subscriptions in _subscriptions.Values)
                foreach (IDisposable subscription in subscriptions)
                    subscription.Dispose();

            _subscriptions.Clear();
        }

        private void OnEntityAdded(Entity entity)
        {
            List<IDisposable> subscriptions = new();

            if (entity.HasComponent<IsBuilding>())
            {
                subscriptions.Add(entity.TakeDamageEvent.Subscribe(_ => _audio.Play(SoundIds.DomeHit)));
                subscriptions.Add(entity.IsDead.Subscribe((_, isDead) => PlayIf(isDead, SoundIds.DomeDestroyed)));
            }
            else if (entity.TryGetIsTouchAnotherTeam(out _))
            {
                // only mines react to another team's touch: placed now, blast when it dies
                _audio.Play(SoundIds.MinePlace);
                subscriptions.Add(entity.IsDead.Subscribe((_, isDead) => PlayIf(isDead, SoundIds.MineBlast)));
            }
            else if (entity.TryGetInSpawnProcess(out ReactiveVariable<bool> inSpawnProcess))
            {
                if (inSpawnProcess.Value)
                    _audio.Play(SoundIds.BugSpawn);

                ReactiveVariable<bool> isBlasted = entity.IsBlasted;
                subscriptions.Add(entity.IsDead.Subscribe((_, isDead) =>
                    PlayIf(isDead, isBlasted.Value ? SoundIds.AcidBlast : SoundIds.BugDeath)));
            }

            if (subscriptions.Count > 0)
                _subscriptions.Add(entity, subscriptions);
        }

        private void OnEntityReleased(Entity entity)
        {
            if (_subscriptions.Remove(entity, out List<IDisposable> subscriptions) == false)
                return;

            foreach (IDisposable subscription in subscriptions)
                subscription.Dispose();
        }

        private void OnStageChanged(int oldNumber, int newNumber)
        {
            if (newNumber > oldNumber)
                _audio.Play(SoundIds.WaveStart);
        }

        private void PlayIf(bool condition, SoundIds id)
        {
            if (condition)
                _audio.Play(id);
        }
    }
}
