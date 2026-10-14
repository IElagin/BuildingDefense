using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Utilities.Reactive;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Sensors
{
    public class ContactsDetectingRadius : IEntityComponent
    {
        public ReactiveVariable<float> Value;
    }
}
