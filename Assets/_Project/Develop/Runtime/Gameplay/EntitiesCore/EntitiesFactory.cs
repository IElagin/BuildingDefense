using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using Assets._Project.Develop.Runtime.Gameplay.Common;
using Assets._Project.Develop.Runtime.Gameplay.Features.ApplyDamage;
using Assets._Project.Develop.Runtime.Gameplay.Features.Attack;
using Assets._Project.Develop.Runtime.Gameplay.Features.Attack.Shoot;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Defense;
using Assets._Project.Develop.Runtime.Gameplay.Features.AudioFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.Blast;
using Assets._Project.Develop.Runtime.Gameplay.Features.ContactTakeDamage;
using Assets._Project.Develop.Runtime.Gameplay.Features.Defense;
using Assets._Project.Develop.Runtime.Gameplay.Features.Explosions;
using Assets._Project.Develop.Runtime.Gameplay.Features.Knockback;
using Assets._Project.Develop.Runtime.Gameplay.Features.LifeCycle;
using Assets._Project.Develop.Runtime.Gameplay.Features.MovementFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.Rest;
using Assets._Project.Develop.Runtime.Gameplay.Features.Sensors;
using Assets._Project.Develop.Runtime.Gameplay.Features.SpawnFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.TargetReach;
using Assets._Project.Develop.Runtime.Gameplay.Features.TeamsFeature;
using Assets._Project.Develop.Runtime.Utilities;
using Assets._Project.Develop.Runtime.Utilities.Audio;
using Assets._Project.Develop.Runtime.Utilities.Conditions;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.EntitiesCore
{
    public class EntitiesFactory
    {
        private const int ContactsBufferSize = 256;
        // the arena has no walls: a bolt that missed dies this far from where it was shot
        private const float ProjectileMaxDistance = 30;

        private readonly EntitiesLifeContext _entitiesLifeContext;
        private readonly CollidersRegistryService _collidersRegistryService;
        private readonly MonoEntitiesFactory _monoEntitiesFactory;
        private readonly IAudioService _audio;
        private readonly RestPhaseService _restPhaseService;

        public EntitiesFactory(
            EntitiesLifeContext entitiesLifeContext,
            MonoEntitiesFactory monoEntitiesFactory,
            CollidersRegistryService collidersRegistryService,
            IAudioService audio,
            RestPhaseService restPhaseService)
        {
            _entitiesLifeContext = entitiesLifeContext;
            _monoEntitiesFactory = monoEntitiesFactory;
            _collidersRegistryService = collidersRegistryService;
            _audio = audio;
            _restPhaseService = restPhaseService;
        }

        public Entity CreateBuilding(Vector3 position, BuildingConfig config, float maxHealth)
        {
            Entity entity = CreateEmpty();

            _monoEntitiesFactory.Create(entity, position, config.PrefabPath);

            entity
                .AddMaxHealth(new ReactiveVariable<float>(maxHealth))
                .AddCurrentHealth(new ReactiveVariable<float>(maxHealth))
                .AddIsDead()
                .AddTakeDamageRequest()
                .AddTakeDamageEvent();

            ICompositeCondition mustDie = new CompositeCondition()
                .Add(new FuncCondition(() => entity.CurrentHealth.Value <= 0));

            ICompositeCondition canApplyDamage = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsDead.Value == false));

            entity
                .AddMustDie(mustDie)
                .AddCanApplyDamage(canApplyDamage);

            entity
                .AddSystem(new ApplyDamageSystem())
                .AddSystem(new DeathSystem())
                .AddSystem(new BuildingSoundsSystem(_audio));

            return entity;
        }

        public Entity CreateBomber(Vector3 position, BomberConfig config)
        {
            Entity entity = CreateEmpty();

            _monoEntitiesFactory.Create(entity, position, config.PrefabPath);

            entity
                .AddMoveDestination(new ReactiveVariable<Vector3>(position))
                .AddMoveSpeed(new ReactiveVariable<float>(config.MoveSpeed))
                .AddAvoidancePriority(new ReactiveVariable<int>(config.AvoidancePriority))
                .AddIsMoving()
                .AddRotationDirection()
                .AddRotationSpeed(new ReactiveVariable<float>(config.RotationSpeed))
                .AddMaxHealth(new ReactiveVariable<float>(config.MaxHealth))
                .AddCurrentHealth(new ReactiveVariable<float>(config.MaxHealth))
                .AddIsDead()
                .AddInDeathProcess()
                .AddDeathProcessInitialTime(new ReactiveVariable<float>(config.DeathProcessTime))
                .AddDeathProcessCurrentTime()
                .AddTakeDamageRequest()
                .AddTakeDamageEvent()
                .AddReachDistance(new ReactiveVariable<float>(config.ReachDistance))
                .AddIsTargetReached()
                .AddBlastRadius(new ReactiveVariable<float>(config.BlastRadius))
                .AddBlastDamage(new ReactiveVariable<float>(config.BlastDamage))
                .AddIsBlasted()
                .AddSpawnInitialTime(new ReactiveVariable<float>(config.SpawnProcessTime))
                .AddSpawnCurrentTime()
                .AddInSpawnProcess()
                .AddTakeImpulseEvent()
                .AddIsCorpseFlying();

            ICompositeCondition canMove = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsDead.Value == false))
                .Add(new FuncCondition(() => entity.InSpawnProcess.Value == false));

            ICompositeCondition canRotate = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsDead.Value == false))
                .Add(new FuncCondition(() => entity.InSpawnProcess.Value == false));

            ICompositeCondition mustBlast = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsTargetReached.Value))
                .Add(new FuncCondition(() => entity.IsDead.Value == false));

            ICompositeCondition mustDie = new CompositeCondition(LogicOperations.Or)
                .Add(new FuncCondition(() => entity.CurrentHealth.Value <= 0))
                .Add(new FuncCondition(() => entity.IsBlasted.Value));

            // A bomber that blew itself up disappears at once; a killed one plays its death first.
            ICompositeCondition mustSelfRelease = new CompositeCondition(LogicOperations.Or)
                .Add(new FuncCondition(() => entity.IsBlasted.Value))
                .Add(new FuncCondition(() => entity.IsDead.Value && entity.InDeathProcess.Value == false));

            ICompositeCondition canApplyDamage = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsDead.Value == false))
                .Add(new FuncCondition(() => entity.InSpawnProcess.Value == false));

            entity
                .AddCanMove(canMove)
                .AddCanRotate(canRotate)
                .AddMustBlast(mustBlast)
                .AddMustDie(mustDie)
                .AddMustSelfRelease(mustSelfRelease)
                .AddCanApplyDamage(canApplyDamage);

            entity
                .AddSystem(new SpawnProcessTimerSystem())
                .AddSystem(new NavMeshMovementSystem())
                .AddSystem(new TransformRotationSystem())
                .AddSystem(new TargetReachDetectorSystem())
                .AddSystem(new BlastSystem(this, config.BlastExplosion))
                .AddSystem(new ApplyDamageSystem())
                .AddSystem(new DeathSystem())
                .AddSystem(new DeathFlightSystem(config))
                .AddSystem(new DisableCollidersOnDeathSystem())
                .AddSystem(new DisableNavMeshAgentOnDeathSystem())
                .AddSystem(new DeathProcessTimerSystem())
                .AddSystem(new BugSoundsSystem(_audio))
                .AddSystem(new SelfReleaseSystem(_entitiesLifeContext));

            return entity;
        }

        // A bug that stops at its reach distance and spits at the target; the course attack, cancelled by death.
        public Entity CreateSpitter(Vector3 position, SpitterConfig config)
        {
            Entity entity = CreateEmpty();

            _monoEntitiesFactory.Create(entity, position, config.PrefabPath);

            entity
                .AddMoveDestination(new ReactiveVariable<Vector3>(position))
                .AddMoveSpeed(new ReactiveVariable<float>(config.MoveSpeed))
                .AddAvoidancePriority(new ReactiveVariable<int>(config.AvoidancePriority))
                .AddIsMoving()
                .AddRotationDirection()
                .AddRotationSpeed(new ReactiveVariable<float>(config.RotationSpeed))
                .AddMaxHealth(new ReactiveVariable<float>(config.MaxHealth))
                .AddCurrentHealth(new ReactiveVariable<float>(config.MaxHealth))
                .AddIsDead()
                .AddInDeathProcess()
                .AddDeathProcessInitialTime(new ReactiveVariable<float>(config.DeathProcessTime))
                .AddDeathProcessCurrentTime()
                .AddTakeDamageRequest()
                .AddTakeDamageEvent()
                .AddReachDistance(new ReactiveVariable<float>(config.ReachDistance))
                .AddIsTargetReached()
                .AddSpawnInitialTime(new ReactiveVariable<float>(config.SpawnProcessTime))
                .AddSpawnCurrentTime()
                .AddInSpawnProcess()
                .AddTakeImpulseEvent()
                .AddIsCorpseFlying()
                .AddStartAttackRequest()
                .AddStartAttackEvent()
                .AddEndAttackEvent()
                .AddAttackProcessModifiedTime(new ReactiveVariable<float>(config.AttackProcessTime))
                .AddAttackProcessCurrentTime()
                .AddInAttackProcess()
                .AddAttackDelayModifiedTime(new ReactiveVariable<float>(config.AttackDelayTime))
                .AddAttackDelayEndEvent()
                .AddAttackCanceledEvent()
                .AddAttackCooldownModifiedTime(new ReactiveVariable<float>(config.AttackCooldown))
                .AddAttackCooldownCurrentTime()
                .AddInAttackCooldown();

            ICompositeCondition canMove = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsDead.Value == false))
                .Add(new FuncCondition(() => entity.InSpawnProcess.Value == false));

            ICompositeCondition canRotate = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsDead.Value == false))
                .Add(new FuncCondition(() => entity.InSpawnProcess.Value == false))
                .Add(new FuncCondition(() => entity.InAttackProcess.Value == false));

            ICompositeCondition canStartAttack = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsDead.Value == false))
                .Add(new FuncCondition(() => entity.InSpawnProcess.Value == false))
                .Add(new FuncCondition(() => entity.InAttackProcess.Value == false))
                .Add(new FuncCondition(() => entity.InAttackCooldown.Value == false));

            ICompositeCondition mustCancelAttack = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsDead.Value));

            ICompositeCondition mustDie = new CompositeCondition()
                .Add(new FuncCondition(() => entity.CurrentHealth.Value <= 0));

            ICompositeCondition mustSelfRelease = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsDead.Value))
                .Add(new FuncCondition(() => entity.InDeathProcess.Value == false));

            ICompositeCondition canApplyDamage = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsDead.Value == false))
                .Add(new FuncCondition(() => entity.InSpawnProcess.Value == false));

            entity
                .AddCanMove(canMove)
                .AddCanRotate(canRotate)
                .AddCanStartAttack(canStartAttack)
                .AddMustCancelAttack(mustCancelAttack)
                .AddMustDie(mustDie)
                .AddMustSelfRelease(mustSelfRelease)
                .AddCanApplyDamage(canApplyDamage);

            entity
                .AddSystem(new SpawnProcessTimerSystem())
                .AddSystem(new NavMeshMovementSystem())
                .AddSystem(new TransformRotationSystem())
                .AddSystem(new TargetReachDetectorSystem())
                .AddSystem(new AttackCancelSystem())
                .AddSystem(new StartAttackSystem())
                .AddSystem(new AttackProcessTimerSystem())
                .AddSystem(new AttackDelayEndTriggerSystem())
                .AddSystem(new AcidSpitShootSystem(this, config))
                .AddSystem(new EndAttackSystem())
                .AddSystem(new AttackCooldownTimerSystem())
                .AddSystem(new ApplyDamageSystem())
                .AddSystem(new DeathSystem())
                .AddSystem(new DeathFlightSystem(config))
                .AddSystem(new DisableCollidersOnDeathSystem())
                .AddSystem(new DisableNavMeshAgentOnDeathSystem())
                .AddSystem(new DeathProcessTimerSystem())
                .AddSystem(new SpitterSoundsSystem(_audio))
                .AddSystem(new SelfReleaseSystem(_entitiesLifeContext));

            return entity;
        }

        // The course projectile that bursts into a small acid blast on its first touch instead of hitting by contact.
        public Entity CreateAcidSpit(Vector3 position, Vector3 direction, Entity owner, SpitterConfig config)
        {
            Entity entity = CreateEmpty();

            _monoEntitiesFactory.Create(entity, position, config.SpitPrefabPath);

            entity
                .AddIsProjectile()
                .AddOwner(new ReactiveVariable<Entity>(owner))
                .AddMoveDirection(new ReactiveVariable<Vector3>(direction))
                .AddMoveSpeed(new ReactiveVariable<float>(config.SpitSpeed))
                .AddIsMoving()
                .AddRotationDirection(new ReactiveVariable<Vector3>(direction))
                .AddRotationSpeed(new ReactiveVariable<float>(9999))
                .AddIsDead()
                .AddContactsDetectingMask(Layers.CharactersMask | Layers.EnvironmentMask)
                .AddContactCollidersBuffer(new Buffer<Collider>(64))
                .AddContactEntitiesBuffer(new Buffer<Entity>(64))
                .AddDeathMask(Layers.EnvironmentMask)
                .AddIsTouchDeathMask()
                .AddIsTouchAnotherTeam()
                .AddTeam(new ReactiveVariable<Teams>(owner.Team.Value))
                .AddBlastRadius(new ReactiveVariable<float>(config.SpitBlastRadius))
                .AddBlastDamage(new ReactiveVariable<float>(config.SpitBlastDamage))
                .AddIsBlasted();

            ICompositeCondition canMove = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsDead.Value == false));

            ICompositeCondition canRotate = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsDead.Value == false));

            ICompositeCondition mustBlast = new CompositeCondition(LogicOperations.Or)
                .Add(new FuncCondition(() => entity.IsTouchDeathMask.Value))
                .Add(new FuncCondition(() => entity.IsTouchAnotherTeam.Value));

            ICompositeCondition mustDie = new CompositeCondition(LogicOperations.Or)
                .Add(new FuncCondition(() => entity.IsBlasted.Value))
                .Add(new FuncCondition(() => (entity.Transform.position - position).sqrMagnitude > ProjectileMaxDistance * ProjectileMaxDistance));

            ICompositeCondition mustSelfRelease = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsDead.Value));

            entity
                .AddCanMove(canMove)
                .AddCanRotate(canRotate)
                .AddMustBlast(mustBlast)
                .AddMustDie(mustDie)
                .AddMustSelfRelease(mustSelfRelease);

            entity
                .AddSystem(new RigidbodyMovementSystem())
                .AddSystem(new RigidbodyRotationSystem())
                .AddSystem(new BodyContactsDetectingSystem())
                .AddSystem(new BodyContactsEntitiesFilterSystem(_collidersRegistryService))
                .AddSystem(new DeathMaskTouchDetectorSystem())
                .AddSystem(new AnotherTeamTouchDetectorSystem())
                .AddSystem(new BlastSystem(this, config.SpitExplosion))
                .AddSystem(new DeathSystem())
                .AddSystem(new DisableCollidersOnDeathSystem())
                .AddSystem(new AcidSpitSoundsSystem(_audio))
                .AddSystem(new SelfReleaseSystem(_entitiesLifeContext));

            _entitiesLifeContext.Add(entity);

            return entity;
        }

        public Entity CreateExplosion(Vector3 position, ExplosionConfig config, float radius, float damage, Teams team)
        {
            Entity entity = CreateEmpty();

            _monoEntitiesFactory.Create(entity, position, config.PrefabPath);

            entity
                .AddTeam(new ReactiveVariable<Teams>(team))
                .AddContactsDetectingMask(Layers.CharactersMask)
                .AddContactsDetectingRadius(new ReactiveVariable<float>(radius))
                .AddContactCollidersBuffer(new Buffer<Collider>(ContactsBufferSize))
                .AddContactEntitiesBuffer(new Buffer<Entity>(ContactsBufferSize))
                .AddAreaDamage(new ReactiveVariable<float>(damage))
                .AddIsAreaDamageApplied()
                .AddAreaImpulse(new ReactiveVariable<float>(config.ImpulseForce))
                .AddIsDead()
                .AddInDeathProcess()
                .AddDeathProcessInitialTime(new ReactiveVariable<float>(config.EffectTime))
                .AddDeathProcessCurrentTime();

            ICompositeCondition mustDie = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsAreaDamageApplied.Value));

            ICompositeCondition mustSelfRelease = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsDead.Value))
                .Add(new FuncCondition(() => entity.InDeathProcess.Value == false));

            entity
                .AddMustDie(mustDie)
                .AddMustSelfRelease(mustSelfRelease);

            entity
                .AddSystem(new SphereContactsDetectingSystem())
                .AddSystem(new BodyContactsEntitiesFilterSystem(_collidersRegistryService))
                .AddSystem(new AreaImpulseSystem())
                .AddSystem(new AreaDamageSystem())
                .AddSystem(new DeathSystem())
                .AddSystem(new DeathProcessTimerSystem())
                .AddSystem(new SelfReleaseSystem(_entitiesLifeContext));

            _entitiesLifeContext.Add(entity);

            return entity;
        }

        public Entity CreateMine(Vector3 position, MineConfig config, Teams team)
        {
            Entity entity = CreateEmpty();

            _monoEntitiesFactory.Create(entity, position, config.PrefabPath);

            entity
                .AddMine()
                .AddTeam(new ReactiveVariable<Teams>(team))
                .AddContactsDetectingMask(Layers.CharactersMask)
                .AddContactsDetectingRadius(new ReactiveVariable<float>(config.TriggerRadius))
                .AddContactCollidersBuffer(new Buffer<Collider>(ContactsBufferSize))
                .AddContactEntitiesBuffer(new Buffer<Entity>(ContactsBufferSize))
                .AddIsTouchAnotherTeam()
                .AddBlastRadius(new ReactiveVariable<float>(config.BlastRadius))
                .AddBlastDamage(new ReactiveVariable<float>(config.BlastDamage))
                .AddIsBlasted()
                .AddIsDead();

            ICompositeCondition mustBlast = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsTouchAnotherTeam.Value));

            ICompositeCondition mustDie = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsBlasted.Value));

            ICompositeCondition mustSelfRelease = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsDead.Value));

            entity
                .AddMustBlast(mustBlast)
                .AddMustDie(mustDie)
                .AddMustSelfRelease(mustSelfRelease);

            entity
                .AddSystem(new SphereContactsDetectingSystem())
                .AddSystem(new BodyContactsEntitiesFilterSystem(_collidersRegistryService))
                .AddSystem(new AnotherTeamTouchDetectorSystem())
                .AddSystem(new BlastSystem(this, config.BlastExplosion))
                .AddSystem(new DeathSystem())
                .AddSystem(new MineSoundsSystem(_audio))
                .AddSystem(new SelfReleaseSystem(_entitiesLifeContext));

            _entitiesLifeContext.Add(entity);

            return entity;
        }

        // The turret holds its aim while a shot is in process; the role factory adds the target, team and brain.
        public Entity CreateTurret(Vector3 position, TurretConfig config)
        {
            Entity entity = CreateEmpty();

            _monoEntitiesFactory.Create(entity, position, config.PrefabPath);

            entity
                .AddRotationDirection()
                .AddRotationSpeed(new ReactiveVariable<float>(config.RotationSpeed))
                .AddAttackProcessModifiedTime(new ReactiveVariable<float>(config.AttackProcessTime))
                .AddAttackProcessCurrentTime()
                .AddInAttackProcess()
                .AddStartAttackRequest()
                .AddStartAttackEvent()
                .AddEndAttackEvent()
                .AddAttackDelayModifiedTime(new ReactiveVariable<float>(config.AttackDelayTime))
                .AddAttackDelayEndEvent()
                .AddInstantAttackDamage(new ReactiveVariable<float>(config.Damage))
                .AddAttackCooldownModifiedTime(new ReactiveVariable<float>(config.AttackCooldown))
                .AddAttackCooldownCurrentTime()
                .AddInAttackCooldown();

            ICompositeCondition canRotate = new CompositeCondition()
                .Add(new FuncCondition(() => entity.InAttackProcess.Value == false));

            ICompositeCondition canStartAttack = new CompositeCondition()
                .Add(new FuncCondition(() => entity.InAttackProcess.Value == false))
                .Add(new FuncCondition(() => entity.InAttackCooldown.Value == false));

            entity
                .AddCanRotate(canRotate)
                .AddCanStartAttack(canStartAttack);

            entity
                .AddSystem(new RigidbodyRotationSystem())
                .AddSystem(new StartAttackSystem())
                .AddSystem(new AttackProcessTimerSystem())
                .AddSystem(new AttackDelayEndTriggerSystem())
                .AddSystem(new InstantShootSystem(this))
                .AddSystem(new EndAttackSystem())
                .AddSystem(new AttackCooldownTimerSystem())
                .AddSystem(new TurretSoundsSystem(_audio));

            return entity;
        }

        public Entity CreateProjectile(Vector3 position, Vector3 direction, float damage, Entity owner)
        {
            Entity entity = CreateEmpty();

            _monoEntitiesFactory.Create(entity, position, "Entities/Projectile");

            entity
                .AddIsProjectile()
                .AddOwner(new ReactiveVariable<Entity>(owner))
                .AddMoveDirection(new ReactiveVariable<Vector3>(direction))
                .AddMoveSpeed(new ReactiveVariable<float>(25))
                .AddIsMoving()
                .AddRotationDirection(new ReactiveVariable<Vector3>(direction))
                .AddRotationSpeed(new ReactiveVariable<float>(9999))
                .AddIsDead()
                .AddContactsDetectingMask(Layers.CharactersMask | Layers.EnvironmentMask)
                .AddContactCollidersBuffer(new Buffer<Collider>(64))
                .AddContactEntitiesBuffer(new Buffer<Entity>(64))
                .AddBodyContactDamage(new ReactiveVariable<float>(damage))
                .AddDeathMask(Layers.EnvironmentMask)
                .AddIsTouchDeathMask()
                .AddIsTouchAnotherTeam()
                .AddTeam(new ReactiveVariable<Teams>(owner.Team.Value));

            ICompositeCondition canMove = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsDead.Value == false));

            ICompositeCondition canRotate = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsDead.Value == false));

            ICompositeCondition mustDie = new CompositeCondition(LogicOperations.Or)
                .Add(new FuncCondition(() => entity.IsTouchDeathMask.Value))
                .Add(new FuncCondition(() => entity.IsTouchAnotherTeam.Value))
                .Add(new FuncCondition(() => (entity.Transform.position - position).sqrMagnitude > ProjectileMaxDistance * ProjectileMaxDistance));

            ICompositeCondition mustSelfRelease = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsDead.Value));

            entity
                .AddCanMove(canMove)
                .AddCanRotate(canRotate)
                .AddMustDie(mustDie)
                .AddMustSelfRelease(mustSelfRelease);

            entity
                .AddSystem(new RigidbodyMovementSystem())
                .AddSystem(new RigidbodyRotationSystem())
                .AddSystem(new BodyContactsDetectingSystem())
                .AddSystem(new BodyContactsEntitiesFilterSystem(_collidersRegistryService))
                .AddSystem(new DealDamageOnContactSystem())
                .AddSystem(new DeathMaskTouchDetectorSystem())
                .AddSystem(new AnotherTeamTouchDetectorSystem())
                .AddSystem(new DeathSystem())
                .AddSystem(new DisableCollidersOnDeathSystem())
                .AddSystem(new SelfReleaseSystem(_entitiesLifeContext));

            _entitiesLifeContext.Add(entity);

            return entity;
        }

        // A puddle hurts the other team standing in it and dries up when the wave after its rest is over.
        public Entity CreatePuddle(Vector3 position, PuddleConfig config, Teams team)
        {
            Entity entity = CreateEmpty();

            _monoEntitiesFactory.Create(entity, position, config.PrefabPath);

            entity
                .AddPuddle()
                .AddTeam(new ReactiveVariable<Teams>(team))
                .AddContactsDetectingMask(Layers.CharactersMask)
                .AddContactsDetectingRadius(new ReactiveVariable<float>(config.Radius))
                .AddContactCollidersBuffer(new Buffer<Collider>(ContactsBufferSize))
                .AddContactEntitiesBuffer(new Buffer<Entity>(ContactsBufferSize))
                .AddAreaDamage(new ReactiveVariable<float>(config.Damage))
                .AddAreaDamagePeriod(new ReactiveVariable<float>(config.DamagePeriod))
                .AddAreaDamageCurrentTime()
                .AddIsWaveOver()
                .AddIsDead();

            ICompositeCondition mustDie = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsWaveOver.Value));

            ICompositeCondition mustSelfRelease = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsDead.Value));

            entity
                .AddMustDie(mustDie)
                .AddMustSelfRelease(mustSelfRelease);

            entity
                .AddSystem(new SphereContactsDetectingSystem())
                .AddSystem(new BodyContactsEntitiesFilterSystem(_collidersRegistryService))
                .AddSystem(new PeriodicAreaDamageSystem())
                .AddSystem(new WaveOverDetectorSystem(_restPhaseService))
                .AddSystem(new DeathSystem())
                .AddSystem(new PuddleSoundsSystem(_audio))
                .AddSystem(new SelfReleaseSystem(_entitiesLifeContext));

            _entitiesLifeContext.Add(entity);

            return entity;
        }

        // The click explosion caster has no view: the wave state puts a request, the system makes the explosion.
        public Entity CreateClickExplosionCaster(DefenseConfig config)
        {
            Entity entity = CreateEmpty();

            entity
                .AddClickExplosionRequest()
                .AddClickExplosionEvent();

            entity
                .AddSystem(new ClickExplosionSystem(this, config))
                .AddSystem(new ClickExplosionSoundSystem(_audio));

            _entitiesLifeContext.Add(entity);

            return entity;
        }

        private Entity CreateEmpty() => new Entity();
    }
}
