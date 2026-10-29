using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.AI.States
{
    // Narrows another selector to the targets inside a radius around the source (a turret's attack radius).
    public class TargetInRadiusSelector : ITargetSelector
    {
        private readonly ITargetSelector _selector;
        private readonly Transform _sourceTransform;
        private readonly float _radius;

        public TargetInRadiusSelector(ITargetSelector selector, Entity entity, float radius)
        {
            _selector = selector;
            _sourceTransform = entity.Transform;
            _radius = radius;
        }

        public Entity SelectTargetFrom(IEnumerable<Entity> targets)
        {
            return _selector.SelectTargetFrom(targets.Where(IsInRadius));
        }

        private bool IsInRadius(Entity target)
        {
            if (target.TryGetTransform(out Transform targetTransform) == false)
                return false;

            Vector3 offset = targetTransform.position - _sourceTransform.position;
            offset.y = 0;

            return offset.sqrMagnitude <= _radius * _radius;
        }
    }
}
