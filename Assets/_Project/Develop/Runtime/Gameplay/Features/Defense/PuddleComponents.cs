using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Utilities.Reactive;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Defense
{
    // Marks a puddle placed by the player, checked with HasComponent.
    public class Puddle : IEntityComponent
    {
    }

    // Set when the wave after the puddle's rest is over.
    public class IsWaveOver : IEntityComponent
    {
        public ReactiveVariable<bool> Value;
    }
}
