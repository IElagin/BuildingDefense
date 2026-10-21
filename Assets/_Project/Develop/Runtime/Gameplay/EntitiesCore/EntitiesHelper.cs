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

        private static bool AreSameTeam(Entity first, Entity second)
        {
            return first.TryGetTeam(out ReactiveVariable<Teams> firstTeam)
                && second.TryGetTeam(out ReactiveVariable<Teams> secondTeam)
                && firstTeam.Value == secondTeam.Value;
        }
    }
}
