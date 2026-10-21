using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.SpawnFeature
{
    [RequireComponent(typeof(Animator))]
    public class SpawnProcessView : EntityView
    {
        private readonly int SpawningProcessKey = Animator.StringToHash("InSpawnProcess");

        [SerializeField] private Animator _animator;
        [SerializeField] private ParticleSystem _spawnEffectPrefab;

        private ReactiveVariable<bool> _inSpawnProcess;
        private Transform _entityTransform;

        private IDisposable _inSpawnProcessChangedDisposable;

        private void OnValidate()
        {
            _animator ??= GetComponent<Animator>();
        }

        protected override void OnEntityStartedWork(Entity entity)
        {
            _inSpawnProcess = entity.InSpawnProcess;
            _entityTransform = entity.Transform;

            _inSpawnProcessChangedDisposable = _inSpawnProcess.Subscribe(OnSpawnProcessChanged);
            UpdateSpawnProcessKey(_inSpawnProcess.Value);
        }

        public override void Cleanup(Entity entity)
        {
            base.Cleanup(entity);

            _inSpawnProcessChangedDisposable.Dispose();
        }

        private void OnSpawnProcessChanged(bool oldValue, bool newValue) => UpdateSpawnProcessKey(newValue);

        private void UpdateSpawnProcessKey(bool value)
        {
            _animator.SetBool(SpawningProcessKey, value);

            if (value)
                Instantiate(_spawnEffectPrefab, _entityTransform.position, _spawnEffectPrefab.transform.rotation, null);
        }
    }
}
