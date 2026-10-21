using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Explosions
{
    // Course entity view (L8): takes the blast radius and the explosion lifetime from the entity,
    // grows the blast sphere to that radius and fades it out while the explosion lives.
    public class ExplosionView : EntityView
    {
        private const float DiameterPerRadius = 2;

        [SerializeField] private Transform _sphere;
        [SerializeField] private Renderer _sphereRenderer;
        [SerializeField, Range(0, 1)] private float _startScale = 0.3f;

        private float _duration;
        private Vector3 _targetScale;
        private Material _material;
        private Color _color;
        private float _time;

        protected override void OnEntityStartedWork(Entity entity)
        {
            float radius = entity.ContactsDetectingRadius.Value;

            transform.localScale = Vector3.one * (radius * DiameterPerRadius);
            _duration = entity.DeathProcessInitialTime.Value;
        }

        private void Awake()
        {
            _targetScale = _sphere.localScale;
            _material = _sphereRenderer.material;
            _color = _material.color;
            _sphere.localScale = _targetScale * _startScale;
        }

        private void Update()
        {
            _time += Time.deltaTime;

            float progress = Mathf.Clamp01(_time / _duration);
            float growth = 1 - (1 - progress) * (1 - progress);

            _sphere.localScale = _targetScale * Mathf.Lerp(_startScale, 1, growth);
            _material.color = new Color(_color.r, _color.g, _color.b, _color.a * (1 - progress));
        }

        private void OnDestroy()
        {
            Destroy(_material);
        }
    }
}
