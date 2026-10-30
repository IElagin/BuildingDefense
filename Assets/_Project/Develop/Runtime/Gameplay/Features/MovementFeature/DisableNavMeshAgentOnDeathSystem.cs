using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System;
using UnityEngine.AI;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.MovementFeature
{
    // A dead bug stops being an agent, so the living ones no longer step around its corpse.
    public class DisableNavMeshAgentOnDeathSystem : IInitializableSystem, IDisposableSystem
    {
        private NavMeshAgent _agent;
        private ReactiveVariable<bool> _isDead;

        private IDisposable _isDeadChangedDisposable;

        public void OnInit(Entity entity)
        {
            _agent = entity.NavMeshAgent;
            _isDead = entity.IsDead;

            _isDeadChangedDisposable = _isDead.Subscribe(OnIsDeadChanged);
        }

        public void OnDispose()
        {
            _isDeadChangedDisposable.Dispose();
        }

        private void OnIsDeadChanged(bool oldValue, bool isDead)
        {
            if (isDead)
                _agent.enabled = false;
        }
    }
}
