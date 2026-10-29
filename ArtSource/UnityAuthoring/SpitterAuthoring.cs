// Editor authoring for the final assignment spitters (2026-10-29), run in the open Editor through Pipeline run_script:
//   unity command --project-path <BuildingDefense> run_script --file ArtSource/UnityAuthoring/SpitterAuthoring.cs --entry SpitterAuthoring.Run --timeout_ms 180000
// After ArtSource/Blender/spitter_steps (BugSpitter.fbx, AcidSpit.fbx). Idempotent: imports both models, builds the
// spitter controller (the bomber layout plus Spit on IsAttack), the Spitter prefab from a copy of the Bomber prefab
// (new visual with InstantAttackView, shoot point at the spout, the same corpse setup as HordeAuthoring.SetupBomber),
// the AcidSpit projectile prefab, SpitterConfig, and the spitter entries of the waves. The bomber entries stay as
// HordeAuthoring tuned them; old spitter entries are replaced. The copied Bomber prefab brings the NavMeshAgent
// (NavMeshAuthoring).
using System.Linq;
using System.Text;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Levels;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Stages;
using Assets._Project.Develop.Runtime.Gameplay.Common;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using Assets._Project.Develop.Runtime.Gameplay.Features.ApplyDamage;
using Assets._Project.Develop.Runtime.Gameplay.Features.Attack;
using Assets._Project.Develop.Runtime.Gameplay.Features.Attack.Shoot;
using Assets._Project.Develop.Runtime.Gameplay.Features.Knockback;
using Assets._Project.Develop.Runtime.Gameplay.Features.LifeCycle;
using Assets._Project.Develop.Runtime.Gameplay.Features.MovementFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.Sensors;
using Assets._Project.Develop.Runtime.Gameplay.Features.SpawnFeature;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public static class SpitterAuthoring
{
    private const string Models = "Assets/_Project/Art/Models/";
    private const string MaterialsFolder = "Assets/_Project/Art/Materials/";
    private const string SpitterModelPath = Models + "BugSpitter.fbx";
    private const string ControllerPath = "Assets/_Project/Art/Animations/BugSpitter.controller";
    private const string SpawnEffectPath = "Assets/_Project/Art/VFX/BurrowSpawnEffect.prefab";
    private const string HitEffectPath = "Assets/_Project/Art/VFX/BugHitEffect.prefab";
    private const string Entities = "Assets/_Project/Resources/Entities/";
    private const string Configs = "Assets/_Project/Resources/Configs/Gameplay/";
    private const string CorpseMaterial = "Assets/_Project/Art/Materials/CorpsePhysics.asset";

    // a little above and ahead of the spout in BugSpitter.fbx (Blender spitter_steps/02_body.py): the course
    // BodyContactsDetectingSystem overlaps a capsule from the body bounds corners, so a blob flying lower than
    // two radii (2 x 0.13) touches the arena floor and bursts at the mouth
    private static readonly Vector3 ShootPoint = new(0f, 0.4f, 0.5f);
    private static readonly Color Acid = new(0.49f, 1f, 0.23f, 1f);
    private static readonly string[] LoopingClips = { "Walk", "Idle" };

    // spitters per wave of each level, from the second wave of level 1 (plan 2026-10-29)
    private static readonly int[][] LevelSpitters =
    {
        new[] { 0, 3, 5 },
        new[] { 2, 4, 6, 8 },
        new[] { 3, 5, 8, 10, 12 },
    };

    private const float SpitterInterval = 2.5f;
    private const float SpitterStartDelay = 3f;

    private static Scene _stage;

    public static string Run()
    {
        var log = new StringBuilder();
        AssetDatabase.Refresh();
        Material palette = AssetDatabase.LoadAssetAtPath<Material>(MaterialsFolder + "Palette.mat");
        Material glow = AssetDatabase.LoadAssetAtPath<Material>(MaterialsFolder + "PaletteGlow.mat");
        Material particles = AssetDatabase.LoadAssetAtPath<Material>(MaterialsFolder + "ParticleFlat.mat");

        ImportSpitterModel(palette, glow, log);
        ImportStaticModel("AcidSpit", palette, glow);
        AnimatorController controller = SetupController(log);
        SetupSpitterPrefab(controller, log);

        _stage = EditorSceneManager.NewPreviewScene();
        BuildAcidSpit(particles, log);
        EditorSceneManager.ClosePreviewScene(_stage);

        SpitterConfig config = SetupConfig(log);
        SetupWaves(config, log);

        AssetDatabase.SaveAssets();
        return log.ToString();
    }

    private static void ImportSpitterModel(Material palette, Material glow, StringBuilder log)
    {
        AssetDatabase.ImportAsset(SpitterModelPath, ImportAssetOptions.ForceUpdate);
        var importer = (ModelImporter)AssetImporter.GetAtPath(SpitterModelPath);
        importer.globalScale = 1f;
        importer.useFileScale = true;
        importer.bakeAxisConversion = true;
        importer.importCameras = false;
        importer.importLights = false;
        importer.importBlendShapes = false;
        importer.animationType = ModelImporterAnimationType.Generic;
        importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
        importer.importAnimation = true;
        importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
        importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), "Palette"), palette);
        importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), "PaletteGlow"), glow);

        ModelImporterClipAnimation[] clips = importer.defaultClipAnimations;

        foreach (ModelImporterClipAnimation clip in clips)
        {
            clip.name = clip.takeName.Split('|').Last();
            clip.loopTime = LoopingClips.Contains(clip.name);
            clip.loopPose = clip.loopTime;
        }

        importer.clipAnimations = clips;
        importer.SaveAndReimport();
        log.AppendLine("spitter model clips " + string.Join(", ", clips.Select(c => $"{c.name}{(c.loopTime ? " (loop)" : "")} {c.firstFrame}-{c.lastFrame}")));
    }

    private static void ImportStaticModel(string name, Material palette, Material glow)
    {
        string path = Models + name + ".fbx";
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        var importer = (ModelImporter)AssetImporter.GetAtPath(path);
        importer.globalScale = 1f;
        importer.useFileScale = true;
        importer.bakeAxisConversion = true;
        importer.importCameras = false;
        importer.importLights = false;
        importer.importBlendShapes = false;
        importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
        importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), "Palette"), palette);
        importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), "PaletteGlow"), glow);
        importer.animationType = ModelImporterAnimationType.None;
        importer.importAnimation = false;
        importer.SaveAndReimport();
    }

    private static AnimationClip Clip(string name)
        => AssetDatabase.LoadAllAssetsAtPath(SpitterModelPath).OfType<AnimationClip>().First(c => c.name == name);

    private static AnimatorController SetupController(StringBuilder log)
    {
        AssetDatabase.DeleteAsset(ControllerPath);
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        controller.AddParameter("InSpawnProcess", AnimatorControllerParameterType.Bool);
        controller.AddParameter("IsWalking", AnimatorControllerParameterType.Bool);
        controller.AddParameter("IsAttack", AnimatorControllerParameterType.Bool);
        controller.AddParameter("IsDead", AnimatorControllerParameterType.Bool);

        AnimatorStateMachine machine = controller.layers[0].stateMachine;
        AnimatorState spawn = machine.AddState("Spawn");
        AnimatorState idle = machine.AddState("Idle");
        AnimatorState walk = machine.AddState("Walk");
        AnimatorState spit = machine.AddState("Spit");
        AnimatorState death = machine.AddState("Death");
        spawn.motion = Clip("Spawn");
        idle.motion = Clip("Idle");
        walk.motion = Clip("Walk");
        spit.motion = Clip("Spit");
        death.motion = Clip("Death");
        machine.defaultState = spawn;

        Connect(spawn, idle, AnimatorConditionMode.IfNot, "InSpawnProcess");
        Connect(idle, walk, AnimatorConditionMode.If, "IsWalking");
        Connect(walk, idle, AnimatorConditionMode.IfNot, "IsWalking");
        Connect(idle, spit, AnimatorConditionMode.If, "IsAttack");
        Connect(walk, spit, AnimatorConditionMode.If, "IsAttack");
        Connect(spit, idle, AnimatorConditionMode.IfNot, "IsAttack");

        AnimatorStateTransition toDeath = machine.AddAnyStateTransition(death);
        toDeath.hasExitTime = false;
        toDeath.duration = 0.05f;
        toDeath.canTransitionToSelf = false;
        toDeath.AddCondition(AnimatorConditionMode.If, 0, "IsDead");

        EditorUtility.SetDirty(controller);
        log.AppendLine("controller Spawn(default) -> Idle <-> Walk, Idle/Walk <-> Spit on IsAttack, Any -> Death");
        return controller;
    }

    private static void Connect(AnimatorState from, AnimatorState to, AnimatorConditionMode mode, string parameter)
    {
        AnimatorStateTransition transition = from.AddTransition(to);
        transition.hasExitTime = false;
        transition.duration = 0.08f;
        transition.AddCondition(mode, 0, parameter);
    }

    private static void SetupSpitterPrefab(AnimatorController controller, StringBuilder log)
    {
        string path = Entities + "Spitter.prefab";
        AssetDatabase.DeleteAsset(path);
        AssetDatabase.CopyAsset(Entities + "Bomber.prefab", path);
        GameObject root = PrefabUtility.LoadPrefabContents(path);
        root.name = "Spitter";

        Transform oldVisual = root.transform.Find("Visual");
        if (oldVisual != null)
            Object.DestroyImmediate(oldVisual.gameObject);

        var model = AssetDatabase.LoadAssetAtPath<GameObject>(SpitterModelPath);
        var visual = (GameObject)PrefabUtility.InstantiatePrefab(model, root.transform);
        visual.name = "Visual";
        // the model root keeps its imported rotation: it carries the FBX axis conversion of the rig
        visual.transform.localPosition = Vector3.zero;

        var animator = visual.GetComponent<Animator>();
        if (animator == null)
            animator = visual.AddComponent<Animator>();
        animator.runtimeAnimatorController = controller;
        animator.applyRootMotion = false;
        animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;

        var hitPoint = new GameObject("HitPoint").transform;
        hitPoint.SetParent(visual.transform, false);
        hitPoint.localPosition = new Vector3(0f, 0.4f, -0.25f);

        var spawnEffect = AssetDatabase.LoadAssetAtPath<ParticleSystem>(SpawnEffectPath);
        var hitEffect = AssetDatabase.LoadAssetAtPath<ParticleSystem>(HitEffectPath);
        Assign(visual.AddComponent<WalkingView>(), ("_animator", animator));
        Assign(visual.AddComponent<InstantAttackView>(), ("_animator", animator));
        Assign(visual.AddComponent<DeadView>(), ("_animator", animator));
        Assign(visual.AddComponent<SpawnProcessView>(), ("_animator", animator), ("_spawnEffectPrefab", spawnEffect));
        Assign(visual.AddComponent<ApplyDamageView>(), ("_applyDamageEffectPrefab", hitEffect), ("_effectSpawnPoint", hitPoint));

        // the shoot point turns with the entity root (the visual's bones bob during the Spit clip)
        Transform shootPoint = root.transform.Find("ShootPoint");
        if (shootPoint == null)
        {
            shootPoint = new GameObject("ShootPoint").transform;
            shootPoint.SetParent(root.transform, false);
        }

        shootPoint.localPosition = ShootPoint;
        shootPoint.localRotation = Quaternion.identity;
        if (root.TryGetComponent(out ShootPointEntityRegistrator shootRegistrator) == false)
            shootRegistrator = root.AddComponent<ShootPointEntityRegistrator>();
        Assign(shootRegistrator, ("_shootPoint", shootPoint));

        SetupCorpse(root, visual);

        PrefabUtility.SaveAsPrefabAsset(root, path);
        PrefabUtility.UnloadPrefabContents(root);
        log.AppendLine($"Spitter prefab: visual BugSpitter, views Walking/InstantAttack/Dead/SpawnProcess/ApplyDamage, shoot point {ShootPoint}, corpse setup");
    }

    // The bomber corpse setup (HordeAuthoring.SetupBomber) for the new visual.
    private static void SetupCorpse(GameObject root, GameObject model)
    {
        var material = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(CorpseMaterial);

        Bounds bounds = new Bounds(model.transform.position, Vector3.zero);
        foreach (Renderer renderer in model.GetComponentsInChildren<Renderer>())
        {
            bounds.Encapsulate(renderer.bounds);
            bool thin = renderer.name.StartsWith("Leg_") || renderer.name.StartsWith("Mandible_");
            renderer.shadowCastingMode = thin ? ShadowCastingMode.Off : ShadowCastingMode.On;
        }

        var collider = model.AddComponent<BoxCollider>();
        collider.center = model.transform.InverseTransformPoint(bounds.center);
        Vector3 size = model.transform.InverseTransformVector(bounds.size);
        collider.size = new Vector3(Mathf.Abs(size.x), Mathf.Abs(size.y), Mathf.Abs(size.z)) * 0.8f;
        collider.sharedMaterial = material;
        collider.enabled = false;
        model.layer = LayerMask.NameToLayer("Corpses");

        Assign(root.GetComponent<CorpseRegistrator>(), ("_model", model.transform), ("_collider", collider));
        Assign(root.GetComponent<CorpseShrinkView>(), ("_model", model.transform));
    }

    private static void BuildAcidSpit(Material particles, StringBuilder log)
    {
        var root = new GameObject("AcidSpit");
        SceneManager.MoveGameObjectToScene(root, _stage);
        root.AddComponent<MonoEntity>();
        root.AddComponent<TransformEntityRegistrator>();

        var instance = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Models + "AcidSpit.fbx"), _stage);
        PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        Transform blob = instance.transform.Find("AcidSpit") ?? instance.transform;
        blob.SetParent(root.transform, false);
        blob.name = "Visual";
        blob.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
        if (instance != blob.gameObject)
            Object.DestroyImmediate(instance);

        // a trigger body like the course Projectile: it finds contacts by overlap and never pushes anything
        var body = root.AddComponent<CapsuleCollider>();
        body.isTrigger = true;
        body.direction = 2;
        body.radius = 0.13f;
        body.height = 0.36f;

        var rigidbody = root.AddComponent<Rigidbody>();
        rigidbody.useGravity = false;
        rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        root.AddComponent<RigidbodyEntityRegistrator>();
        Assign(root.AddComponent<BodyColliderRegistrator>(), ("_body", body));

        var disable = root.AddComponent<DisableCollidersOnDeathRegistrator>();
        var serialized = new SerializedObject(disable);
        SerializedProperty colliders = serialized.FindProperty("_colliders");
        colliders.arraySize = 1;
        colliders.GetArrayElementAtIndex(0).objectReferenceValue = body;
        serialized.ApplyModifiedPropertiesWithoutUndo();

        TrailRenderer trail = root.AddComponent<TrailRenderer>();
        trail.sharedMaterial = particles;
        trail.time = 0.18f;
        trail.minVertexDistance = 0.05f;
        trail.widthCurve = new AnimationCurve(new Keyframe(0f, 0.18f), new Keyframe(1f, 0f));
        trail.colorGradient = new Gradient
        {
            colorKeys = new[] { new GradientColorKey(Acid, 0f), new GradientColorKey(Acid, 1f) },
            alphaKeys = new[] { new GradientAlphaKey(0.85f, 0f), new GradientAlphaKey(0f, 1f) }
        };
        trail.shadowCastingMode = ShadowCastingMode.Off;
        trail.receiveShadows = false;

        PrefabUtility.SaveAsPrefabAsset(root, Entities + "AcidSpit.prefab");
        Object.DestroyImmediate(root);
        log.AppendLine("AcidSpit: glowing blob, acid trail, trigger capsule body r0.13");
    }

    private static SpitterConfig SetupConfig(StringBuilder log)
    {
        string path = Configs + "Entities/SpitterConfig.asset";
        var config = AssetDatabase.LoadAssetAtPath<SpitterConfig>(path);
        if (config == null)
        {
            config = ScriptableObject.CreateInstance<SpitterConfig>();
            AssetDatabase.CreateAsset(config, path);
        }

        var serialized = new SerializedObject(config);
        serialized.FindProperty("<MoveSpeed>k__BackingField").floatValue = 2.2f;
        serialized.FindProperty("<MaxHealth>k__BackingField").floatValue = 40f;
        serialized.FindProperty("<ReachDistance>k__BackingField").floatValue = 7f;
        // more important than the bombers (50): they step around a standing spitter (NavMeshAuthoring)
        serialized.FindProperty("<AvoidancePriority>k__BackingField").intValue = 30;
        serialized.FindProperty("<SpitExplosion>k__BackingField").objectReferenceValue =
            AssetDatabase.LoadAssetAtPath<ExplosionConfig>(Configs + "Entities/AcidBlastConfig.asset");
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(config);
        log.AppendLine("SpitterConfig: HP 40, speed 2.2, stops 7 m from the dome, avoidance priority 30, acid blast look");
        return config;
    }

    private static void SetupWaves(SpitterConfig spitter, StringBuilder log)
    {
        var levels = AssetDatabase.LoadAssetAtPath<LevelsListConfig>(Configs + "Levels/LevelsListConfig.asset");

        for (int level = 0; level < LevelSpitters.Length; level++)
        {
            LevelConfig levelConfig = levels.Levels[level];
            int burrowCount = levelConfig.BurrowPositions.Count;

            for (int wave = 0; wave < LevelSpitters[level].Length; wave++)
            {
                var waveConfig = (WaveStageConfig)levelConfig.StageConfigs[wave];
                var serialized = new SerializedObject(waveConfig);
                SerializedProperty spawns = serialized.FindProperty("_burrowSpawns");

                for (int i = spawns.arraySize - 1; i >= 0; i--)
                    if (spawns.GetArrayElementAtIndex(i).FindPropertyRelative("<EnemyConfig>k__BackingField").objectReferenceValue is SpitterConfig)
                        spawns.DeleteArrayElementAtIndex(i);

                // spread the spitters over the burrows, one stream per burrow, starting after the first bombers
                int total = LevelSpitters[level][wave];
                int streams = Mathf.Min(total, burrowCount);
                for (int i = 0; i < streams; i++)
                {
                    int index = spawns.arraySize;
                    spawns.arraySize++;
                    SerializedProperty spawn = spawns.GetArrayElementAtIndex(index);
                    spawn.FindPropertyRelative("<BurrowIndex>k__BackingField").intValue = (i * burrowCount / streams + 1) % burrowCount;
                    spawn.FindPropertyRelative("<EnemyConfig>k__BackingField").objectReferenceValue = spitter;
                    spawn.FindPropertyRelative("<Count>k__BackingField").intValue = total / streams + (i < total % streams ? 1 : 0);
                    spawn.FindPropertyRelative("<SpawnInterval>k__BackingField").floatValue = SpitterInterval;
                    spawn.FindPropertyRelative("<StartDelay>k__BackingField").floatValue = SpitterStartDelay + i * 0.7f;
                }

                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(waveConfig);
            }

            log.AppendLine($"level {level + 1}: spitters per wave {string.Join("/", LevelSpitters[level])}");
        }
    }

    private static void Assign(Object target, params (string field, Object value)[] values)
    {
        var serialized = new SerializedObject(target);

        foreach ((string field, Object value) in values)
            serialized.FindProperty(field).objectReferenceValue = value;

        serialized.ApplyModifiedPropertiesWithoutUndo();
    }
}
