using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Sensors
{
    // Entity view in the course L8 style: shows the sphere sensor radius on the ground
    // (the mine's trigger ring, modelled at radius 1) by scaling the indicator to it.
    public class ContactsRadiusView : EntityView
    {
        [SerializeField] private Transform _radiusIndicator;

        private Vector3 _unitScale;

        private void Awake()
        {
            _unitScale = _radiusIndicator.localScale;
        }

        protected override void OnEntityStartedWork(Entity entity)
        {
            _radiusIndicator.localScale = _unitScale * entity.ContactsDetectingRadius.Value;
        }
    }
}
