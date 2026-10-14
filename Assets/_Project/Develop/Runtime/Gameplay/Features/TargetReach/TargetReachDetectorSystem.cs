using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.TargetReach
{
    public class TargetReachDetectorSystem : IInitializableSystem, IUpdatableSystem
    {
        private ReactiveVariable<Entity> _currentTarget;
        private ReactiveVariable<float> _reachDistance;
        private ReactiveVariable<bool> _isTargetReached;
        private Transform _transform;

        public void OnInit(Entity entity)
        {
            _currentTarget = entity.CurrentTarget;
            _reachDistance = entity.ReachDistance;
            _isTargetReached = entity.IsTargetReached;
            _transform = entity.Transform;
        }

        public void OnUpdate(float deltaTime)
        {
            Entity target = _currentTarget.Value;

            if (target == null)
            {
                _isTargetReached.Value = false;
                return;
            }

            Vector3 offset = target.Transform.position - _transform.position;
            offset.y = 0;

            _isTargetReached.Value = offset.magnitude <= _reachDistance.Value;
        }
    }
}
