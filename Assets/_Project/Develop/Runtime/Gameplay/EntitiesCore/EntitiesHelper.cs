using Assets._Project.Develop.Runtime.Gameplay.Features.TeamsFeature;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.EntitiesCore
{
    public class EntitiesHelper
    {
        public static bool TryTakeDamageFrom(Entity source, Entity damageable, float damage)
        {
            if (damageable.TryGetTakeDamageRequest(out ReactiveEvent<float> takeDamageRequest) == false)
                return false;

            if (AreSameTeam(source, damageable))
                return false;

            takeDamageRequest.Invoke(damage);
            return true;
        }

        public static bool TryTakeImpulseFrom(Entity source, Entity target, Vector3 impulse)
        {
            if (target.TryGetTakeImpulseEvent(out ReactiveEvent<Vector3> takeImpulseEvent) == false)
                return false;

            if (AreSameTeam(source, target))
                return false;

            takeImpulseEvent.Invoke(impulse);
            return true;
        }

        // The point of the target body nearest to a position: every bug walks and spits at the side of the dome
        // that faces it, not at its centre (inside the carved NavMesh hole all paths met in one point).
        public static Vector3 GetNearestPoint(Entity target, Vector3 position)
        {
            if (target.TryGetBodyCollider(out CapsuleCollider body) && body.enabled)
                return body.ClosestPoint(position);

            return target.Transform.position;
        }

        private static bool AreSameTeam(Entity first, Entity second)
        {
            return first.TryGetTeam(out ReactiveVariable<Teams> firstTeam)
                && second.TryGetTeam(out ReactiveVariable<Teams> secondTeam)
                && firstTeam.Value == secondTeam.Value;
        }
    }
}
