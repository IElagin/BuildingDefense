using System;
using System.Collections.Generic;

namespace Assets._Project.Develop.Runtime.Gameplay.EntitiesCore
{
    public class EntitiesLifeContext : IDisposable
    {
        public event Action<Entity> Added;
        public event Action<Entity> Released;

        private readonly List<Entity> _entities = new();
        private readonly List<Entity> _releaseRequests = new();

        public IReadOnlyList<Entity> Entities => _entities;

        public void Add(Entity entity)
        {
            _entities.Add(entity);

            entity.Initialize();

            Added?.Invoke(entity);
        }

        public void Update(float deltaTime)
        {
            for (int i = 0; i < _entities.Count; i++)
                _entities[i].OnUpdate(deltaTime);

            foreach (Entity entity in _releaseRequests)
            {
                _entities.Remove(entity);
                entity.Dispose();
                Released?.Invoke(entity);
            }

            _releaseRequests.Clear();
        }

        // An entity can get two release reasons in one frame (the stage cleanup on defeat and its own self-release
        // after a blast); it is released once.
        public void Release(Entity entity)
        {
            if (_releaseRequests.Contains(entity) == false)
                _releaseRequests.Add(entity);
        }

        public void Dispose()
        {
            foreach (Entity entity in _entities)
                entity.Dispose();

            _entities.Clear();
            _releaseRequests.Clear();
        }
    }
}
