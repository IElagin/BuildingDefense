using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Utilities.Reactive;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Explosions
{
    public class AreaDamage : IEntityComponent
    {
        public ReactiveVariable<float> Value;
    }

    public class IsAreaDamageApplied : IEntityComponent
    {
        public ReactiveVariable<bool> Value;
    }
}
