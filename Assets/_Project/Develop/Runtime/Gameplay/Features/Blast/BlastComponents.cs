using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Utilities.Conditions;
using Assets._Project.Develop.Runtime.Utilities.Reactive;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Blast
{
    public class BlastRadius : IEntityComponent
    {
        public ReactiveVariable<float> Value;
    }

    public class BlastDamage : IEntityComponent
    {
        public ReactiveVariable<float> Value;
    }

    public class MustBlast : IEntityComponent
    {
        public ICompositeCondition Value;
    }

    public class IsBlasted : IEntityComponent
    {
        public ReactiveVariable<bool> Value;
    }
}
