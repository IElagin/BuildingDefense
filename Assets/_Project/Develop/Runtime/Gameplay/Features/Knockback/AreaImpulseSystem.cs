using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilities;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System.Collections.Generic;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Knockback
{
    // Pushes the explosion's contacts away from its centre once, on the same update as the area damage:
    // up and outwards, stronger near the centre. Whoever dies from the blast flies off with this impulse.
    public class AreaImpulseSystem : IInitializableSystem, IUpdatableSystem
    {
        private const float LiftFactor = 1.1f;
        private const float EdgeImpulseShare = 0.55f;
        private const float MinRadius = 0.01f;
        private const float MinDirectionSqrMagnitude = 0.0001f;

        private readonly HashSet<Entity> _pushedEntities = new();

        private Entity _entity;
        private Buffer<Entity> _contacts;
        private ReactiveVariable<float> _impulse;
        private ReactiveVariable<float> _radius;
        private bool _isApplied;

        public void OnInit(Entity entity)
        {
            _entity = entity;
            _contacts = entity.ContactEntitiesBuffer;
            _impulse = entity.AreaImpulse;
            _radius = entity.ContactsDetectingRadius;
        }

        public void OnUpdate(float deltaTime)
        {
            if (_isApplied)
                return;

            Vector3 centre = _entity.Transform.position;

            for (int i = 0; i < _contacts.Count; i++)
            {
                Entity contact = _contacts.Items[i];

                if (_pushedEntities.Add(contact) && contact.TryGetTransform(out Transform target))
                    EntitiesHelper.TryTakeImpulseFrom(_entity, contact, GetImpulse(centre, target.position));
            }

            _isApplied = true;
        }

        private Vector3 GetImpulse(Vector3 centre, Vector3 target)
        {
            Vector3 away = target - centre;
            away.y = 0;

            float closeness = 1 - Mathf.Clamp01(away.magnitude / Mathf.Max(_radius.Value, MinRadius)) * (1 - EdgeImpulseShare);
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            Vector3 direction = away.sqrMagnitude > MinDirectionSqrMagnitude ? away.normalized : new Vector3(randomDirection.x, 0, randomDirection.y);

            return (direction + Vector3.up * LiftFactor) * (_impulse.Value * closeness);
        }
    }
}
