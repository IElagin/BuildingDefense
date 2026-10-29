using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilities.Conditions;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.MovementFeature
{
    // The course TransformDirectionalRotator (NavMeshTest) for NavMesh agents: the agent writes the transform every
    // frame, so a kinematic body's MoveRotation never reached it and the bugs walked sideways.
    public class TransformRotationSystem : IInitializableSystem, IUpdatableSystem
    {
        private Transform _transform;

        private ReactiveVariable<float> _rotationSpeed;
        private ReactiveVariable<Vector3> _direction;

        private ICompositeCondition _canRotate;

        public void OnInit(Entity entity)
        {
            _transform = entity.Transform;
            _rotationSpeed = entity.RotationSpeed;
            _direction = entity.RotationDirection;

            _canRotate = entity.CanRotate;

            if (_direction.Value != Vector3.zero)
                _transform.rotation = Quaternion.LookRotation(_direction.Value.normalized);
        }

        public void OnUpdate(float deltaTime)
        {
            if (_canRotate.Evaluate() == false)
                return;

            if (_direction.Value == Vector3.zero)
                return;

            Quaternion lookRotation = Quaternion.LookRotation(_direction.Value.normalized);

            float step = _rotationSpeed.Value * deltaTime;

            _transform.rotation = Quaternion.RotateTowards(_transform.rotation, lookRotation, step);
        }
    }
}
