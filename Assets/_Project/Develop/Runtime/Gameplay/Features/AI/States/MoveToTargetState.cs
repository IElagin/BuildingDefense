using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using Assets._Project.Develop.Runtime.Utilities.StateMachineCore;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.AI.States
{
    // Asks NavMeshMovementSystem to walk to the nearest point of the target; leaving the state, or losing the target, stops the bug where it stands.
    public class MoveToTargetState : State, IUpdatableState
    {
        private readonly ReactiveVariable<Vector3> _moveDestination;
        private readonly ReactiveVariable<Entity> _currentTarget;
        private readonly Transform _transform;

        public MoveToTargetState(Entity entity)
        {
            _moveDestination = entity.MoveDestination;
            _currentTarget = entity.CurrentTarget;
            _transform = entity.Transform;
        }

        public override void Exit()
        {
            base.Exit();

            _moveDestination.Value = _transform.position;
        }

        public void Update(float deltaTime)
        {
            Entity target = _currentTarget.Value;

            if (target == null)
            {
                _moveDestination.Value = _transform.position;
                return;
            }

            _moveDestination.Value = EntitiesHelper.GetNearestPoint(target, _transform.position);
        }
    }
}
