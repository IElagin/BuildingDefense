using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature;
using Assets._Project.Develop.Runtime.Utilities;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Heal
{
    // At the start of every wave the entity asks itself to heal a share of its max HP. Works on anything with HealSystem.
    public class HealOnStageStartSystem : IInitializableSystem, IDisposableSystem
    {
        private readonly StageProviderService _stageProviderService;

        private ReactiveEvent<float> _healRequest;
        private ReactiveVariable<float> _maxHealth;
        private ReactiveVariable<float> _healPercent;

        private IDisposable _stageNumberDisposable;

        public HealOnStageStartSystem(StageProviderService stageProviderService)
        {
            _stageProviderService = stageProviderService;
        }

        public void OnInit(Entity entity)
        {
            _healRequest = entity.HealRequest;
            _maxHealth = entity.MaxHealth;
            _healPercent = entity.HealOnStageStartPercent;

            _stageNumberDisposable = _stageProviderService.CurrentStageNumber.Subscribe(OnStageNumberChanged);
        }

        public void OnDispose()
        {
            _stageNumberDisposable.Dispose();
        }

        private void OnStageNumberChanged(int oldNumber, int newNumber)
        {
            _healRequest.Invoke(_maxHealth.Value * Percent.ToShare(_healPercent.Value));
        }
    }
}
