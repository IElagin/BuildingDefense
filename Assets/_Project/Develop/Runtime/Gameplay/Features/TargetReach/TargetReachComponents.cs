using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Utilities.Reactive;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.TargetReach
{
    public class ReachDistance : IEntityComponent
    {
        public ReactiveVariable<float> Value;
    }

    public class IsTargetReached : IEntityComponent
    {
        public ReactiveVariable<bool> Value;
    }
}
