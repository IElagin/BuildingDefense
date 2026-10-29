using Assets._Project.Develop.Runtime.Configs.Gameplay.Defense;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.BuildingFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.TeamsFeature;
using Assets._Project.Develop.Runtime.Meta.Features.Wallet;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Defense
{
    // During rest the player picks a defense (keys 1-3 or a HUD card) and a click on the ground buys it with wallet
    // gold. A place is free inside the build ring around the building and away from other defenses (found by their
    // role markers). A click it cannot serve is reported as denied. The pointer point and whether it is free are
    // kept for the placement preview.
    public class DefensePlacementService
    {
        private readonly IInputService _inputService;
        private readonly GroundPointService _groundPointService;
        private readonly EntitiesFactory _entitiesFactory;
        private readonly EntitiesLifeContext _entitiesLifeContext;
        private readonly WalletService _walletService;
        private readonly BuildingHolderService _buildingHolderService;
        private readonly DefenseConfig _config;

        private readonly ReactiveVariable<DefenseTypes> _selected = new(DefenseTypes.Mine);
        private readonly ReactiveEvent _denied = new();

        public DefensePlacementService(
            IInputService inputService,
            GroundPointService groundPointService,
            EntitiesFactory entitiesFactory,
            EntitiesLifeContext entitiesLifeContext,
            WalletService walletService,
            BuildingHolderService buildingHolderService,
            DefenseConfig config)
        {
            _inputService = inputService;
            _groundPointService = groundPointService;
            _entitiesFactory = entitiesFactory;
            _entitiesLifeContext = entitiesLifeContext;
            _walletService = walletService;
            _buildingHolderService = buildingHolderService;
            _config = config;
        }

        public IReadOnlyVariable<DefenseTypes> Selected => _selected;

        public IReadOnlyEvent Denied => _denied;

        public bool HasPointerPoint { get; private set; }

        public Vector3 PointerPoint { get; private set; }

        public bool CanPlaceAtPointer { get; private set; }

        // Turrets and puddles get their recipes with the next part of the assignment.
        public bool IsAvailable(DefenseTypes type) => type == DefenseTypes.Mine;

        public int GetPrice(DefenseTypes type)
        {
            switch (type)
            {
                case DefenseTypes.Mine:
                    return _config.MinePrice;

                case DefenseTypes.Turret:
                    return _config.TurretPrice;

                case DefenseTypes.Puddle:
                    return _config.PuddlePrice;

                default:
                    throw new ArgumentException($"Not support {type} defense");
            }
        }

        public float GetRadius(DefenseTypes type)
        {
            switch (type)
            {
                case DefenseTypes.Mine:
                    return _config.Mine.BlastRadius;

                default:
                    throw new ArgumentException($"Not support {type} defense");
            }
        }

        public void Select(DefenseTypes type)
        {
            if (IsAvailable(type))
                _selected.Value = type;
        }

        public void Update()
        {
            if (_inputService.TryGetPressedSlot(out int slot))
                Select((DefenseTypes)(slot - 1));

            HasPointerPoint = false;

            if (_inputService.IsPointerOverUI
                || _groundPointService.TryGetPoint(_inputService.PointerPosition, out Vector3 point) == false)
                return;

            HasPointerPoint = true;

            PointerPoint = point;
            DefenseTypes type = _selected.Value;
            CanPlaceAtPointer = IsFreePlace(point) && _walletService.Enough(CurrencyTypes.Gold, GetPrice(type));

            if (_inputService.IsPointerClicked == false)
                return;

            if (CanPlaceAtPointer == false)
            {
                _denied.Invoke();
                return;
            }

            _walletService.Spend(CurrencyTypes.Gold, GetPrice(type));
            Create(type, point);
        }

        private void Create(DefenseTypes type, Vector3 point)
        {
            switch (type)
            {
                case DefenseTypes.Mine:
                    _entitiesFactory.CreateMine(point, _config.Mine, Teams.Player);
                    break;

                default:
                    throw new ArgumentException($"Not support {type} defense");
            }
        }

        private bool IsFreePlace(Vector3 point)
        {
            float distance = FlatDistance(point, _buildingHolderService.Building.Transform.position);

            if (distance < _config.MinDistanceToBuilding || distance > _config.MaxDistanceToBuilding)
                return false;

            foreach (Entity entity in _entitiesLifeContext.Entities)
                if (IsDefense(entity) && FlatDistance(point, entity.Transform.position) < _config.MinDistanceBetweenDefenses)
                    return false;

            return true;
        }

        private static bool IsDefense(Entity entity) => entity.HasComponent<Mine>();

        private static float FlatDistance(Vector3 a, Vector3 b)
        {
            Vector3 offset = a - b;
            offset.y = 0;

            return offset.magnitude;
        }
    }
}
