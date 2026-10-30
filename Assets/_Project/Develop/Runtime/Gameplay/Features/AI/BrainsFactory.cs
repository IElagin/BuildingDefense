using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.AI.States;
using Assets._Project.Develop.Runtime.Utilities.Conditions;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;
using VContainer;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.AI
{
    public class BrainsFactory
    {
        private const float AttackAimToleranceDegrees = 3f;

        private readonly IObjectResolver _container;
        private readonly AIBrainsContext _brainsContext;
        private readonly EntitiesLifeContext _entitiesLifeContext;

        public BrainsFactory(IObjectResolver container)
        {
            _container = container;
            _brainsContext = _container.Resolve<AIBrainsContext>();
            _entitiesLifeContext = _container.Resolve<EntitiesLifeContext>();
        }

        public StateMachineBrain CreateBomberBrain(Entity entity)
        {
            AIStateMachine stateMachine = CreateMoveToTargetStateMachine(entity);
            StateMachineBrain brain = new StateMachineBrain(stateMachine);

            _brainsContext.SetFor(entity, brain);

            return brain;
        }

        // Walk to the target, then stand at the reach distance and attack it like the turret does.
        public StateMachineBrain CreateSpitterBrain(Entity entity)
        {
            AIStateMachine stateMachine = CreateMoveThenAttackStateMachine(entity);
            StateMachineBrain brain = new StateMachineBrain(stateMachine);

            _brainsContext.SetFor(entity, brain);

            return brain;
        }

        // The course main hero's combat without movement: find the target, turn to it, shoot.
        public StateMachineBrain CreateTurretBrain(Entity entity, ITargetSelector targetSelector)
        {
            AIStateMachine combatState = CreateAutoAttackStateMachine(entity);

            FindTargetState findTargetState = new FindTargetState(targetSelector, _entitiesLifeContext, entity);
            AIParallelState parallelState = new AIParallelState(findTargetState, combatState);

            AIStateMachine rootStateMachine = new AIStateMachine();
            rootStateMachine.AddState(parallelState);

            StateMachineBrain brain = new StateMachineBrain(rootStateMachine);

            _brainsContext.SetFor(entity, brain);

            return brain;
        }

        private AIStateMachine CreateMoveToTargetStateMachine(Entity entity)
        {
            MoveToTargetState moveToTargetState = new MoveToTargetState(entity);

            AIStateMachine stateMachine = new AIStateMachine();
            stateMachine.AddState(moveToTargetState);

            return stateMachine;
        }

        private AIStateMachine CreateMoveThenAttackStateMachine(Entity entity)
        {
            MoveToTargetState moveToTargetState = new MoveToTargetState(entity);

            AIStateMachine attackState = CreateAutoAttackStateMachine(entity);

            ReactiveVariable<bool> isTargetReached = entity.IsTargetReached;

            ICondition fromMoveToAttackCondition = new FuncCondition(() => isTargetReached.Value);
            ICondition fromAttackToMoveCondition = new FuncCondition(() => isTargetReached.Value == false);

            AIStateMachine stateMachine = new AIStateMachine();

            stateMachine.AddState(moveToTargetState);
            stateMachine.AddState(attackState);

            stateMachine.AddTransition(moveToTargetState, attackState, fromMoveToAttackCondition);
            stateMachine.AddTransition(attackState, moveToTargetState, fromAttackToMoveCondition);

            return stateMachine;
        }

        private AIStateMachine CreateAutoAttackStateMachine(Entity entity)
        {
            RotateToTargetState rotateToTargetState = new RotateToTargetState(entity);

            AttackTriggerState attackTriggerState = new AttackTriggerState(entity);

            ICondition canAttack = entity.CanStartAttack;
            Transform transform = entity.Transform;
            ReactiveVariable<Entity> currentTarget = entity.CurrentTarget;

            ICompositeCondition fromRotateToAttackCondition = new CompositeCondition()
                .Add(canAttack)
                .Add(new FuncCondition(() =>
                {
                    Entity target = currentTarget.Value;

                    if (target == null)
                        return false;

                    float angleToTarget = Quaternion.Angle(transform.rotation, Quaternion.LookRotation(target.Transform.position - transform.position));
                    return angleToTarget < AttackAimToleranceDegrees;
                }));

            ReactiveVariable<bool> inAttackProcess = entity.InAttackProcess;

            ICondition fromAttackToRotateStateCondition = new FuncCondition(() => inAttackProcess.Value == false);

            AIStateMachine stateMachine = new AIStateMachine();

            stateMachine.AddState(rotateToTargetState);
            stateMachine.AddState(attackTriggerState);

            stateMachine.AddTransition(rotateToTargetState, attackTriggerState, fromRotateToAttackCondition);
            stateMachine.AddTransition(attackTriggerState, rotateToTargetState, fromAttackToRotateStateCondition);

            return stateMachine;
        }
    }
}
