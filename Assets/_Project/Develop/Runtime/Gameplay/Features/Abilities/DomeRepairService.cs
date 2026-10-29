using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.BuildingFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature;
using System;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Abilities
{
    // Mediator of the bought dome repair: at the start of every wave it asks the dome to heal a share of its max HP.
    public class DomeRepairService : IDisposable
    {
        private readonly StageProviderService _stageProviderService;
        private readonly BuildingHolderService _buildingHolderService;
        private readonly float _healPercent;

        private IDisposable _stageNumberDisposable;

        public DomeRepairService(
            StageProviderService stageProviderService,
            BuildingHolderService buildingHolderService,
            float healPercent)
        {
            _stageProviderService = stageProviderService;
            _buildingHolderService = buildingHolderService;
            _healPercent = healPercent;
        }

        public void Initialize()
        {
            _stageNumberDisposable = _stageProviderService.CurrentStageNumber.Subscribe(OnStageNumberChanged);
        }

        public void Dispose()
        {
            _stageNumberDisposable?.Dispose();
        }

        private void OnStageNumberChanged(int oldNumber, int newNumber)
        {
            Entity building = _buildingHolderService.Building;

            if (_healPercent <= 0 || building == null)
                return;

            building.HealRequest.Invoke(building.MaxHealth.Value * _healPercent / 100);
        }
    }
}
