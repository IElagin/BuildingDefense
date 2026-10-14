using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using Assets._Project.Develop.Runtime.Utilities.StateMachineCore;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.AI.States
{
    public class MoveToTargetState : State, IUpdatableState
    {
        private readonly ReactiveVariable<Vector3> _moveDirection;
        private readonly ReactiveVariable<Vector3> _rotationDirection;
        private readonly ReactiveVariable<Entity> _currentTarget;
        private readonly Transform _transform;

        public MoveToTargetState(Entity entity)
        {
            _moveDirection = entity.MoveDirection;
            _rotationDirection = entity.RotationDirection;
            _currentTarget = entity.CurrentTarget;
            _transform = entity.Transform;
        }

        public override void Exit()
        {
            base.Exit();

            _moveDirection.Value = Vector3.zero;
        }

        public void Update(float deltaTime)
        {
            Entity target = _currentTarget.Value;

            if (target == null)
            {
                _moveDirection.Value = Vector3.zero;
                return;
            }

            Vector3 direction = target.Transform.position - _transform.position;
            direction.y = 0;
            direction.Normalize();

            _moveDirection.Value = direction;
            _rotationDirection.Value = direction;
        }
    }
}
