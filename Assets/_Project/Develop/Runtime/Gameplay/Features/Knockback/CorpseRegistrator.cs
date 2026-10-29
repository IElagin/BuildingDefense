using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Knockback
{
    public class CorpseRegistrator : MonoEntityRegistrator
    {
        [SerializeField] private Transform _model;
        [SerializeField] private Collider _collider;

        public override void Register(Entity entity)
        {
            entity
                .AddCorpseModel(_model)
                .AddCorpseCollider(_collider);
        }
    }
}
