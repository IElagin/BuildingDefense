using Assets._Project.Develop.Runtime.Configs.Gameplay.Defense;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.BuildingFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.TeamsFeature;
using Assets._Project.Develop.Runtime.Meta.Features.Wallet;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Defense
{
    // During rest a click on the ground buys a mine with wallet gold.
    public class MinePlacementService
    {
        private readonly IInputService _inputService;
        private readonly GroundPointService _groundPointService;
        private readonly EntitiesFactory _entitiesFactory;
        private readonly WalletService _walletService;
        private readonly BuildingHolderService _buildingHolderService;
        private readonly DefenseConfig _config;

        public MinePlacementService(
            IInputService inputService,
            GroundPointService groundPointService,
            EntitiesFactory entitiesFactory,
            WalletService walletService,
            BuildingHolderService buildingHolderService,
            DefenseConfig config)
        {
            _inputService = inputService;
            _groundPointService = groundPointService;
            _entitiesFactory = entitiesFactory;
            _walletService = walletService;
            _buildingHolderService = buildingHolderService;
            _config = config;
        }

        public void Update()
        {
            if (_inputService.IsPointerClicked == false)
                return;

            if (_groundPointService.TryGetPoint(_inputService.PointerPosition, out Vector3 point) == false)
                return;

            if (CanPlaceAt(point) == false)
                return;

            if (_walletService.Enough(CurrencyTypes.Gold, _config.MinePrice) == false)
                return;

            _walletService.Spend(CurrencyTypes.Gold, _config.MinePrice);
            _entitiesFactory.CreateMine(point, _config.Mine, Teams.Player);
        }

        private bool CanPlaceAt(Vector3 point)
        {
            Vector3 offset = point - _buildingHolderService.Building.Transform.position;
            offset.y = 0;

            float distance = offset.magnitude;

            return distance >= _config.MinMineDistanceToBuilding && distance <= _config.MaxMineDistanceToBuilding;
        }
    }
}
