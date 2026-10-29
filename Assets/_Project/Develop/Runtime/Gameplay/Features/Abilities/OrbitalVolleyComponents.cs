using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Utilities.Reactive;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Abilities
{
    // The bug was hit by the orbital volley; the damage itself goes through TakeDamageRequest, the event is for views.
    public class OrbitalVolleyHitEvent : IEntityComponent
    {
        public ReactiveEvent Value;
    }
}
