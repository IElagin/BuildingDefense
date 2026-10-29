using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilities.Conditions;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;
using UnityEngine.AI;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.MovementFeature
{
    // The course AgentMover (NavMeshTest) as a system: the agent walks the path to MoveDestination and steps
    // around the other agents by priority; the bug turns along the agent velocity in TransformRotationSystem.
    public class NavMeshMovementSystem : IInitializableSystem, IUpdatableSystem
    {
        private const float AgentAcceleration = 999;
        private const float DestinationChangeDistance = 0.1f;
        private const float MinMovingSpeed = 0.1f;

        private NavMeshAgent _agent;
        private ReactiveVariable<Vector3> _destination;
        private ReactiveVariable<float> _moveSpeed;
        private ReactiveVariable<bool> _isMoving;
        private ReactiveVariable<Vector3> _rotationDirection;

        private ICompositeCondition _canMove;

        public void OnInit(Entity entity)
        {
            _agent = entity.NavMeshAgent;
            _destination = entity.MoveDestination;
            _moveSpeed = entity.MoveSpeed;
            _isMoving = entity.IsMoving;
            _rotationDirection = entity.RotationDirection;

            _canMove = entity.CanMove;

            _agent.updateRotation = false;
            _agent.acceleration = AgentAcceleration;
            _agent.avoidancePriority = entity.AvoidancePriority.Value;
        }

        public void OnUpdate(float deltaTime)
        {
            if (_agent.enabled == false || _agent.isOnNavMesh == false)
                return;

            if (_canMove.Evaluate() == false)
            {
                _agent.isStopped = true;
                _agent.velocity = Vector3.zero;
                _isMoving.Value = false;
                return;
            }

            _agent.isStopped = false;
            _agent.speed = _moveSpeed.Value;

            if (Vector3.Distance(_agent.destination, _destination.Value) > DestinationChangeDistance)
                _agent.SetDestination(_destination.Value);

            Vector3 velocity = _agent.velocity;
            velocity.y = 0;

            _isMoving.Value = velocity.magnitude > MinMovingSpeed;

            if (_isMoving.Value)
                _rotationDirection.Value = velocity;
        }
    }
}
