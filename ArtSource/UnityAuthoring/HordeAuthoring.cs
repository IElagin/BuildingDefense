// Editor authoring for the hordes (L8 look pass, 2026-10-21), run in the open Editor through Pipeline run_script:
//   unity command --project-path <BuildingDefense> run_script --file ArtSource/UnityAuthoring/HordeAuthoring.cs --entry HordeAuthoring.Run --timeout_ms 180000
// Idempotent: the Corpses physics layer (corpses hit only the plateau, props and each other; living entities never
// touch the Environment colliders), the bomber's death flight (corpse collider and model for DeathFlightSystem, CorpseShrinkView;
// thin legs and mandibles cast no shadows), blast impulses per explosion config, the level burrows and waves
// (up to ~150 bugs at once) and the balance for such waves.
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Defense;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Levels;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Stages;
using Assets._Project.Develop.Runtime.Gameplay.Features.Knockback;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public static class HordeAuthoring
{
    private const string Configs = "Assets/_Project/Resources/Configs/Gameplay/";
    private const string BomberPrefab = "Assets/_Project/Resources/Entities/Bomber.prefab";
    private const string CorpseMaterial = "Assets/_Project/Art/Materials/CorpsePhysics.asset";
    private const string CorpsesLayer = "Corpses";
    private const float BurrowRadius = 11.5f;

    // burrow angles (degrees around the dome) of each level; props are placed clear of all six
    private static readonly float[][] LevelBurrows =
    {
        new[] { 15f, 135f, 195f, 315f },
        new[] { 15f, 75f, 135f, 255f, 315f },
        new[] { 15f, 75f, 135f, 195f, 255f, 315f },
    };

    // per level, per wave: (burrows used, bugs per burrow, interval)
    private static readonly (int burrows, int count, float interval)[][] LevelWaves =
    {
        new[] { (2, 10, 0.6f), (3, 12, 0.5f), (4, 15, 0.4f) },
        new[] { (3, 10, 0.5f), (4, 12, 0.45f), (4, 18, 0.4f), (5, 20, 0.35f) },
        new[] { (4, 10, 0.5f), (4, 16, 0.4f), (6, 16, 0.35f), (6, 20, 0.3f), (6, 25, 0.28f) },
    };

    private static readonly float[] LevelBuildingHealth = { 150f, 180f, 220f };

    public static string Run()
    {
        var log = new StringBuilder();
        int corpses = EnsureLayer(CorpsesLayer, log);
        SetupCollisions(corpses, log);
        SetupBomber(corpses, log);
        SetupBalance(log);
        SetupLevels(log);
        AssetDatabase.SaveAssets();
        return log.ToString();
    }

    // Only the bomber prefab (entry HordeAuthoring.RunBomber): keeps the tuned balance and levels untouched.
    public static string RunBomber()
    {
        var log = new StringBuilder();
        SetupBomber(EnsureLayer(CorpsesLayer, log), log);
        AssetDatabase.SaveAssets();
        return log.ToString();
    }

    private static int EnsureLayer(string name, StringBuilder log)
    {
        int existing = LayerMask.NameToLayer(name);
        if (existing >= 0)
            return existing;

        var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");
        for (int i = 8; i < layers.arraySize; i++)
        {
            SerializedProperty layer = layers.GetArrayElementAtIndex(i);
            if (string.IsNullOrEmpty(layer.stringValue))
            {
                layer.stringValue = name;
                tagManager.ApplyModifiedPropertiesWithoutUndo();
                log.AppendLine($"layer {name} = {i}");
                return i;
            }
        }

        throw new System.InvalidOperationException("no free user layer");
    }

    // Corpses collide with Environment and each other only; Environment colliders exist for corpses alone.
    private static void SetupCollisions(int corpses, StringBuilder log)
    {
        int environment = LayerMask.NameToLayer("Environment");
        for (int layer = 0; layer < 32; layer++)
        {
            Physics.IgnoreLayerCollision(corpses, layer, layer != environment && layer != corpses);
            Physics.IgnoreLayerCollision(environment, layer, layer != corpses);
        }

        log.AppendLine("collisions: Corpses <-> Environment, Corpses; Environment ignores the rest");
    }

    private static void SetupBomber(int corpses, StringBuilder log)
    {
        var material = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(CorpseMaterial);
        if (material == null)
        {
            material = new PhysicsMaterial("Corpse");
            AssetDatabase.CreateAsset(material, CorpseMaterial);
        }

        material.bounciness = 0.35f;
        material.dynamicFriction = 0.5f;
        material.staticFriction = 0.6f;
        material.bounceCombine = PhysicsMaterialCombine.Maximum;
        EditorUtility.SetDirty(material);

        GameObject root = PrefabUtility.LoadPrefabContents(BomberPrefab);
        GameObject model = root.GetComponentInChildren<Animator>().gameObject;

        Bounds bounds = new Bounds(model.transform.position, Vector3.zero);
        foreach (Renderer renderer in model.GetComponentsInChildren<Renderer>())
        {
            bounds.Encapsulate(renderer.bounds);
            bool thin = renderer.name.StartsWith("Leg_") || renderer.name.StartsWith("Mandible_");
            renderer.shadowCastingMode = thin ? ShadowCastingMode.Off : ShadowCastingMode.On;
        }

        // the corpse rigidbody is added by DeathFlightSystem at death: a kinematic body on the living model held its
        // spawn rotation while the entity turned, so bugs walked sideways or backwards
        if (model.TryGetComponent(out Rigidbody body))
            Object.DestroyImmediate(body);

        if (model.TryGetComponent(out BoxCollider collider) == false)
            collider = model.AddComponent<BoxCollider>();
        collider.center = model.transform.InverseTransformPoint(bounds.center);
        Vector3 size = model.transform.InverseTransformVector(bounds.size);
        collider.size = new Vector3(Mathf.Abs(size.x), Mathf.Abs(size.y), Mathf.Abs(size.z)) * 0.8f;
        collider.sharedMaterial = material;
        collider.enabled = false;
        model.layer = corpses;

        // DeathFlightSystem flies the corpse (curator L8: physics in a system), CorpseShrinkView only displays
        GameObjectUtility.RemoveMonoBehavioursWithMissingScript(root);

        if (root.TryGetComponent(out CorpseRegistrator registrator) == false)
            registrator = root.AddComponent<CorpseRegistrator>();
        var serialized = new SerializedObject(registrator);
        serialized.FindProperty("_model").objectReferenceValue = model.transform;
        serialized.FindProperty("_collider").objectReferenceValue = collider;
        serialized.ApplyModifiedPropertiesWithoutUndo();

        if (root.TryGetComponent(out CorpseShrinkView shrinkView) == false)
            shrinkView = root.AddComponent<CorpseShrinkView>();
        serialized = new SerializedObject(shrinkView);
        serialized.FindProperty("_model").objectReferenceValue = model.transform;
        serialized.ApplyModifiedPropertiesWithoutUndo();

        log.AppendLine($"bomber: corpse collider {collider.size} on {model.name}, CorpseRegistrator, CorpseShrinkView, thin parts without shadows");
        PrefabUtility.SaveAsPrefabAsset(root, BomberPrefab);
        PrefabUtility.UnloadPrefabContents(root);
    }

    private static void SetupBalance(StringBuilder log)
    {
        Set(Load<ExplosionConfig>("Entities/ExplosionConfig"), ("<ImpulseForce>k__BackingField", 9f));
        Set(Load<ExplosionConfig>("Entities/MineBlastConfig"), ("<ImpulseForce>k__BackingField", 10f));
        Set(Load<ExplosionConfig>("Entities/AcidBlastConfig"), ("<ImpulseForce>k__BackingField", 0f));
        Set(Load<BomberConfig>("Entities/BomberConfig"), ("<DeathProcessTime>k__BackingField", 2.2f), ("<BlastDamage>k__BackingField", 5f));
        Set(Load<DefenseConfig>("Defense/DefenseConfig"),
            ("<ClickExplosionRadius>k__BackingField", 3.2f), ("<ClickExplosionCooldown>k__BackingField", 0.45f));
        log.AppendLine("balance: impulses strike 9 / mine 10 / acid 0, corpse time 2.2 s, bug blast 5, click radius 3.2 every 0.45 s");
    }

    private static void SetupLevels(StringBuilder log)
    {
        var bomber = Load<BomberConfig>("Entities/BomberConfig");
        var levels = Load<LevelsListConfig>("Levels/LevelsListConfig");

        for (int level = 0; level < LevelBurrows.Length; level++)
        {
            LevelConfig levelConfig = levels.Levels[level];
            var serialized = new SerializedObject(levelConfig);
            SerializedProperty positions = serialized.FindProperty("_burrowPositions");
            positions.arraySize = LevelBurrows[level].Length;
            for (int i = 0; i < LevelBurrows[level].Length; i++)
            {
                float angle = LevelBurrows[level][i] * Mathf.Deg2Rad;
                positions.GetArrayElementAtIndex(i).vector3Value =
                    new Vector3(Mathf.Cos(angle) * BurrowRadius, 0f, Mathf.Sin(angle) * BurrowRadius);
            }

            serialized.FindProperty("<BuildingMaxHealth>k__BackingField").floatValue = LevelBuildingHealth[level];
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(levelConfig);

            var total = new List<int>();
            for (int wave = 0; wave < LevelWaves[level].Length; wave++)
            {
                var waveConfig = (WaveStageConfig)levelConfig.StageConfigs[wave];
                (int burrows, int count, float interval) = LevelWaves[level][wave];
                int burrowCount = LevelBurrows[level].Length;
                var waveSerialized = new SerializedObject(waveConfig);
                SerializedProperty spawns = waveSerialized.FindProperty("_burrowSpawns");
                spawns.arraySize = burrows;

                for (int i = 0; i < burrows; i++)
                {
                    SerializedProperty spawn = spawns.GetArrayElementAtIndex(i);
                    // spread the used burrows around the ring and stagger their starts a little
                    spawn.FindPropertyRelative("<BurrowIndex>k__BackingField").intValue = i * burrowCount / burrows;
                    spawn.FindPropertyRelative("<EnemyConfig>k__BackingField").objectReferenceValue = bomber;
                    spawn.FindPropertyRelative("<Count>k__BackingField").intValue = count;
                    spawn.FindPropertyRelative("<SpawnInterval>k__BackingField").floatValue = interval;
                    spawn.FindPropertyRelative("<StartDelay>k__BackingField").floatValue = i * 0.5f;
                }

                waveSerialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(waveConfig);
                total.Add(burrows * count);
            }

            log.AppendLine($"level {level + 1}: {LevelBurrows[level].Length} burrows, building {LevelBuildingHealth[level]} HP, waves {string.Join("/", total)}");
        }
    }

    private static T Load<T>(string path) where T : Object =>
        AssetDatabase.LoadAssetAtPath<T>(Configs + path + ".asset");

    private static void Set(Object target, params (string field, float value)[] values)
    {
        var serialized = new SerializedObject(target);
        foreach ((string field, float value) in values)
            serialized.FindProperty(field).floatValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(target);
    }
}
