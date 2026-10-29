using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Entities
{
    // What every bug of the swarm has: it climbs out of a burrow, walks to the dome, dies and its corpse can fly.
    public abstract class BugConfig : EntityConfig
    {
        [field: SerializeField, Min(0)] public float MoveSpeed { get; private set; } = 2.5f;
        [field: SerializeField, Min(0)] public float RotationSpeed { get; private set; } = 720;
        // NavMeshAgent.avoidancePriority: lower is more important, a lower bug is walked around by the others
        [field: SerializeField, Range(0, 99)] public int AvoidancePriority { get; private set; } = 50;
        [field: SerializeField, Min(0)] public float MaxHealth { get; private set; } = 50;
        [field: SerializeField, Min(0)] public float DeathProcessTime { get; private set; } = 2.2f;
        [field: SerializeField, Min(0)] public float SpawnProcessTime { get; private set; } = 0.6f;
        [field: SerializeField, Min(0)] public float ReachDistance { get; private set; } = 2.2f;
        [field: SerializeField, Min(0.01f)] public float CorpseMass { get; private set; } = 1;
        [field: SerializeField, Min(0)] public float CorpseLinearDamping { get; private set; } = 0.05f;
        [field: SerializeField, Min(0)] public float CorpseAngularDamping { get; private set; } = 0.4f;
        [field: SerializeField, Min(0)] public float CorpseSpin { get; private set; } = 12;
    }
}
