using Assets._Project.Develop.Runtime.Configs.Gameplay.Defense;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.MainHero;
using Assets._Project.Develop.Runtime.Gameplay.Features.TeamsFeature;
using Assets._Project.Develop.Runtime.Meta.Features.Wallet;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Defense
{
    // During rest a click on the ground buys a mine with wallet gold.
    public class MinesPlacementService
    {
        private readonly IInputService _inputService;
        private readonly GroundPointService _groundPointService;
        private readonly EntitiesFactory _entitiesFactory;
        private readonly WalletService _walletService;
        private readonly MainHeroHolderService _mainHeroHolderService;
        private readonly DefenseConfig _config;

        public MinesPlacementService(
            IInputService inputService,
            GroundPointService groundPointService,
            EntitiesFactory entitiesFactory,
            WalletService walletService,
            MainHeroHolderService mainHeroHolderService,
            DefenseConfig config)
        {
            _inputService = inputService;
            _groundPointService = groundPointService;
            _entitiesFactory = entitiesFactory;
            _walletService = walletService;
            _mainHeroHolderService = mainHeroHolderService;
            _config = config;
        }

        public void Update()
        {
            if (_inputService.IsPointerPressed == false)
                return;

            if (_groundPointService.TryGetPoint(_inputService.PointerPosition, out Vector3 point) == false)
                return;

            if (CanPlaceAt(point) == false)
                return;

            if (_walletService.Enough(CurrencyTypes.Gold, _config.MinePrice) == false)
                return;

            _walletService.Spend(CurrencyTypes.Gold, _config.MinePrice);
            _entitiesFactory.CreateMine(point, _config, Teams.MainHero);
        }

        private bool CanPlaceAt(Vector3 point)
        {
            Vector3 offset = point - _mainHeroHolderService.MainHero.Transform.position;
            offset.y = 0;

            float distance = offset.magnitude;

            return distance >= _config.MinMineDistanceToBuilding && distance <= _config.MaxMineDistanceToBuilding;
        }
    }
}
