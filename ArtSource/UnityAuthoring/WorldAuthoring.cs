// Editor authoring for the L8 world (stage 4a), run in the open Editor through Pipeline run_script:
//   unity command --project-path <BuildingDefense> run_script --file ArtSource/UnityAuthoring/WorldAuthoring.cs --entry WorldAuthoring.Run --timeout_ms 180000
// Idempotent: imports the world models, builds the dome and mine animators, swaps the Building and Mine
// visuals, makes one explosion prefab and config per source (click strike, mine, bomber acid blast),
// and rebuilds the Gameplay scene environment (arena, props, light, background).
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Gameplay.Features.ApplyDamage;
using Assets._Project.Develop.Runtime.Gameplay.Features.Sensors;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public static class WorldAuthoring
{
    private const string Models = "Assets/_Project/Art/Models/";
    private const string Animations = "Assets/_Project/Art/Animations/";
    private const string MaterialsFolder = "Assets/_Project/Art/Materials/";
    private const string Vfx = "Assets/_Project/Art/VFX/";
    private const string Entities = "Assets/_Project/Resources/Entities/";
    private const string EntityConfigs = "Assets/_Project/Resources/Configs/Gameplay/Entities/";
    private const string GameplayScene = "Assets/_Project/Scenes/Gameplay.unity";

    private static readonly string[] StaticModels = { "Arena", "Rock_A", "Rock_B", "Rock_C", "Crystal_A", "Crystal_B" };

    public static string Run()
    {
        Scene active = SceneManager.GetActiveScene();
        if (active.isDirty)
            return $"NOT_RUN: the open scene {active.path} has unsaved changes";

        var log = new StringBuilder();
        Material palette = AssetDatabase.LoadAssetAtPath<Material>(MaterialsFolder + "Palette.mat");
        Material glow = AssetDatabase.LoadAssetAtPath<Material>(MaterialsFolder + "PaletteGlow.mat");
        Material particles = AssetDatabase.LoadAssetAtPath<Material>(MaterialsFolder + "ParticleFlat.mat");

        foreach (string name in StaticModels)
            ImportModel(name, palette, glow, null, log);

        ImportModel("Dome", palette, glow, "Idle", log);
        ImportModel("LandMine", palette, glow, "Armed", log);
        AnimatorController domeController = SingleStateController("Dome", "Idle");
        AnimatorController mineController = SingleStateController("LandMine", "Armed");

        GameObject domeHit = SaveEffect(Vfx + "DomeHitEffect.prefab", "DomeHitEffect", root =>
            Burst(root, "Sparks", particles, 12, (0.2f, 0.35f), (1.5f, 3f), (0.05f, 0.1f), 1.5f, Hex("8AF6FF"), Hex("EDEFF2"), 60f, false));

        SetupBuilding(domeController, domeHit, log);
        SetupMine(mineController, log);
        SetupExplosions(particles, log);
        SetupScene(log);

        AssetDatabase.SaveAssets();
        return log.ToString();
    }

    private static void ImportModel(string name, Material palette, Material glow, string loopClip, StringBuilder log)
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

        // a rig exports one action as its only take; it becomes the named looping clip
        if (loopClip != null)
        {
            importer.animationType = ModelImporterAnimationType.Generic;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.importAnimation = true;
            // keyframe reduction made the dish turn unevenly (±0.6° per step); the clips are tiny
            importer.animationCompression = ModelImporterAnimationCompression.Off;
            ModelImporterClipAnimation[] clips = importer.defaultClipAnimations;

            foreach (ModelImporterClipAnimation clip in clips)
            {
                clip.name = loopClip;
                clip.loopTime = true;
                clip.loopPose = true;
            }

            importer.clipAnimations = clips;
            log.AppendLine($"{name}: rig, clips {string.Join(", ", clips.Select(c => $"{c.name} {c.firstFrame}-{c.lastFrame}"))}");
        }
        else
        {
            importer.animationType = ModelImporterAnimationType.None;
            importer.importAnimation = false;
        }

        importer.SaveAndReimport();
    }

    private static AnimatorController SingleStateController(string model, string clipName)
    {
        string path = Animations + model + ".controller";
        AssetDatabase.DeleteAsset(path);
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(path);
        AnimatorState state = controller.layers[0].stateMachine.AddState(clipName);
        state.motion = AssetDatabase.LoadAllAssetsAtPath(Models + model + ".fbx").OfType<AnimationClip>()
            .First(c => c.name.StartsWith("__preview__") == false);
        controller.layers[0].stateMachine.defaultState = state;
        EditorUtility.SetDirty(controller);
        return controller;
    }

    private static GameObject ReplaceVisual(GameObject root, string model)
    {
        Transform old = root.transform.Find("Visual");
        if (old != null)
            Object.DestroyImmediate(old.gameObject);

        var visual = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Models + model + ".fbx"), root.transform);
        visual.name = "Visual";
        // the model root keeps its imported rotation: it carries the FBX axis conversion of the rig
        visual.transform.localPosition = Vector3.zero;
        return visual;
    }

    private static Animator SetupAnimator(GameObject visual, AnimatorController controller)
    {
        var animator = visual.GetComponent<Animator>();
        if (animator == null)
            animator = visual.AddComponent<Animator>();

        animator.runtimeAnimatorController = controller;
        animator.applyRootMotion = false;
        animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
        return animator;
    }

    private static void SetupBuilding(AnimatorController controller, GameObject hitEffect, StringBuilder log)
    {
        string path = Entities + "Building.prefab";
        GameObject root = PrefabUtility.LoadPrefabContents(path);
        GameObject visual = ReplaceVisual(root, "Dome");
        SetupAnimator(visual, controller);

        var hitPoint = new GameObject("HitPoint").transform;
        hitPoint.SetParent(root.transform, false);
        hitPoint.localPosition = new Vector3(0f, 1.2f, 0f);
        Assign(visual.AddComponent<ApplyDamageView>(), ("_applyDamageEffectPrefab", hitEffect.GetComponent<ParticleSystem>()), ("_effectSpawnPoint", hitPoint));

        var capsule = root.GetComponent<CapsuleCollider>();
        capsule.radius = 1.6f;
        capsule.height = 3.4f;
        capsule.center = new Vector3(0f, 0.9f, 0f);

        PrefabUtility.SaveAsPrefabAsset(root, path);
        PrefabUtility.UnloadPrefabContents(root);
        log.AppendLine("Building: dome visual, Idle animator, hit sparks, capsule r1.6 h3.4");
    }

    private static void SetupMine(AnimatorController controller, StringBuilder log)
    {
        string path = Entities + "Mine.prefab";
        GameObject root = PrefabUtility.LoadPrefabContents(path);
        GameObject visual = ReplaceVisual(root, "LandMine");
        SetupAnimator(visual, controller);

        Transform ring = visual.GetComponentsInChildren<Transform>(true).First(t => t.name == "Mine_TriggerRing");
        ring.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
        Assign(visual.AddComponent<ContactsRadiusView>(), ("_radiusIndicator", ring));

        PrefabUtility.SaveAsPrefabAsset(root, path);
        PrefabUtility.UnloadPrefabContents(root);
        log.AppendLine("Mine: land mine visual, Armed animator, trigger ring scaled to the sensor radius");
    }

    private static void SetupExplosions(Material particles, StringBuilder log)
    {
        // explosion roots are scaled to the blast diameter by ExplosionView, so effects are authored for diameter 1
        Material strikeSphere = BlastMaterial("ExplosionStrike", new Color(0.75f, 0.95f, 1f, 0.5f));
        Material mineSphere = BlastMaterial("ExplosionMine", new Color(1f, 0.55f, 0.1f, 0.55f));
        Material acidSphere = BlastMaterial("ExplosionAcid", new Color(0.5f, 1f, 0.25f, 0.5f));

        ExplosionPrefab("OrbitalStrike", strikeSphere, root =>
        {
            Beam(root, particles);
            Burst(root, "Flash", particles, 16, (0.35f, 0.5f), (1.2f, 2.4f), (0.05f, 0.12f), 0.8f, Hex("FFD34D"), Hex("FF7A1A"), 60f, true);
            DustRing(root, particles);
        });
        ExplosionPrefab("MineBlast", mineSphere, root =>
        {
            Burst(root, "Fire", particles, 18, (0.35f, 0.55f), (1f, 2f), (0.05f, 0.11f), 0.6f, Hex("FF7A1A"), Hex("FFD34D"), 45f, true);
            Burst(root, "Smoke", particles, 8, (0.5f, 0.6f), (0.3f, 0.6f), (0.08f, 0.16f), -0.2f, Hex("4A505A"), Hex("8A919C"), 30f, true);
            DustRing(root, particles);
        });
        ExplosionPrefab("AcidBlast", acidSphere, root =>
        {
            Burst(root, "Acid", particles, 20, (0.35f, 0.5f), (1.2f, 2.6f), (0.05f, 0.1f), 1.2f, Hex("7CFF3A"), Hex("B8FF6A"), 70f, true);
            Burst(root, "Shards", particles, 8, (0.3f, 0.45f), (1f, 2f), (0.04f, 0.08f), 1.5f, Hex("2E2140"), Hex("1E1626"), 70f, true);
        });

        var click = AssetDatabase.LoadAssetAtPath<ExplosionConfig>(EntityConfigs + "ExplosionConfig.asset");
        SetString(click, "<PrefabPath>k__BackingField", "Entities/OrbitalStrike");
        ExplosionConfig mine = ExplosionConfigCopy(click, "MineBlastConfig", "Entities/MineBlast", 0.6f);
        ExplosionConfig acid = ExplosionConfigCopy(click, "AcidBlastConfig", "Entities/AcidBlast", 0.5f);
        SetReference(AssetDatabase.LoadAssetAtPath<MineConfig>(EntityConfigs + "MineConfig.asset"), "<BlastExplosion>k__BackingField", mine);
        SetReference(AssetDatabase.LoadAssetAtPath<BomberConfig>(EntityConfigs + "BomberConfig.asset"), "<BlastExplosion>k__BackingField", acid);
        log.AppendLine("explosions: click -> OrbitalStrike, mine -> MineBlast, bomber -> AcidBlast");
    }

    private static Material BlastMaterial(string name, Color color)
    {
        string path = MaterialsFolder + name + ".mat";
        if (AssetDatabase.LoadAssetAtPath<Material>(path) == null)
            AssetDatabase.CopyAsset(MaterialsFolder + "Explosion.mat", path);

        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        material.SetColor("_BaseColor", color);
        EditorUtility.SetDirty(material);
        return material;
    }

    private static void ExplosionPrefab(string name, Material sphere, System.Action<GameObject> addEffects)
    {
        string path = Entities + name + ".prefab";
        // rebuild in place when the prefab exists, so its GUID stays stable between runs
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null)
            AssetDatabase.CopyAsset(Entities + "Explosion.prefab", path);

        GameObject root = PrefabUtility.LoadPrefabContents(path);
        root.name = name;
        root.transform.Find("Blast").GetComponent<MeshRenderer>().sharedMaterial = sphere;

        foreach (Transform child in root.transform.Cast<Transform>().ToList())
            if (child.name != "Blast")
                Object.DestroyImmediate(child.gameObject);

        addEffects(root);
        PrefabUtility.SaveAsPrefabAsset(root, path);
        PrefabUtility.UnloadPrefabContents(root);
    }

    private static ExplosionConfig ExplosionConfigCopy(ExplosionConfig source, string name, string prefabPath, float effectTime)
    {
        string path = EntityConfigs + name + ".asset";
        var config = AssetDatabase.LoadAssetAtPath<ExplosionConfig>(path);

        if (config == null)
        {
            config = Object.Instantiate(source);
            AssetDatabase.CreateAsset(config, path);
        }

        SetString(config, "<PrefabPath>k__BackingField", prefabPath);
        var serialized = new SerializedObject(config);
        serialized.FindProperty("<EffectTime>k__BackingField").floatValue = effectTime;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        return config;
    }

    private static void Beam(GameObject root, Material material)
    {
        var beam = new GameObject("Beam");
        beam.transform.SetParent(root.transform, false);
        beam.transform.localPosition = new Vector3(0f, 7f, 0f);
        ParticleSystem system = beam.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = system.main;
        main.duration = 0.4f;
        main.loop = false;
        main.startLifetime = 0.32f;
        main.startSpeed = 0f;
        main.startSize3D = true;
        main.startSizeX = 0.11f;
        main.startSizeY = 7f;
        main.startSizeZ = 0.11f;
        main.startColor = new Color(0.9f, 1f, 1f, 1f);
        main.scalingMode = ParticleSystemScalingMode.Hierarchy;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        ParticleSystem.EmissionModule emission = system.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 1) });
        ParticleSystem.ShapeModule shape = system.shape;
        shape.enabled = false;
        ParticleSystem.SizeOverLifetimeModule size = system.sizeOverLifetime;
        size.enabled = true;
        size.separateAxes = true;
        var thin = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(0.4f, 0.8f), new Keyframe(1f, 0f));
        size.x = new ParticleSystem.MinMaxCurve(1f, thin);
        size.y = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Constant(0f, 1f, 1f));
        size.z = new ParticleSystem.MinMaxCurve(1f, thin);
        ParticleSystem.ColorOverLifetimeModule color = system.colorOverLifetime;
        color.enabled = true;
        var gradient = new Gradient();
        gradient.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Hex("8AF6FF"), 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.9f, 0.6f), new GradientAlphaKey(0f, 1f) });
        color.color = gradient;
        ParticleSystemRenderer renderer = beam.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Mesh;
        renderer.mesh = Resources.GetBuiltinResource<Mesh>("Cylinder.fbx");
        renderer.alignment = ParticleSystemRenderSpace.Local;
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
    }

    private static void DustRing(GameObject root, Material material)
    {
        ParticleSystem dust = Burst(root, "Dust", material, 22, (0.5f, 0.7f), (0.8f, 1.6f), (0.04f, 0.09f), 0.4f, Hex("C79A62"), Hex("9C6B3C"), 0f, true);
        ParticleSystem.ShapeModule shape = dust.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.3f;
        shape.arc = 360f;
        shape.rotation = new Vector3(90f, 0f, 0f);
    }

    private static ParticleSystem Burst(GameObject parent, string name, Material material, int count, (float, float) lifetime,
        (float, float) speed, (float, float) size, float gravity, Color colorA, Color colorB, float coneAngle, bool hierarchyScale)
    {
        var child = new GameObject(name);
        child.transform.SetParent(parent.transform, false);
        ParticleSystem system = child.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = system.main;
        main.duration = 0.6f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(lifetime.Item1, lifetime.Item2);
        main.startSpeed = new ParticleSystem.MinMaxCurve(speed.Item1, speed.Item2);
        main.startSize = new ParticleSystem.MinMaxCurve(size.Item1, size.Item2);
        main.startRotation3D = true;
        main.startRotationX = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.startRotationY = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.startRotationZ = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.startColor = new ParticleSystem.MinMaxGradient(colorA, colorB);
        main.gravityModifier = gravity;
        main.scalingMode = hierarchyScale ? ParticleSystemScalingMode.Hierarchy : ParticleSystemScalingMode.Local;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = true;
        if (hierarchyScale == false)
            main.stopAction = ParticleSystemStopAction.Destroy;

        ParticleSystem.EmissionModule emission = system.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });

        ParticleSystem.ShapeModule shape = system.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = coneAngle;
        shape.radius = 0.1f;
        shape.rotation = new Vector3(-90f, 0f, 0f);

        ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = system.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(0.7f, 1f), new Keyframe(1f, 0f)));

        ParticleSystemRenderer renderer = child.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Mesh;
        renderer.mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        return system;
    }

    private static GameObject SaveEffect(string path, string name, System.Action<GameObject> build)
    {
        var root = new GameObject(name);
        build(root);
        // the spawned effect is a child-less root: move the only system up so stopAction destroys the instance
        ParticleSystem only = root.GetComponentInChildren<ParticleSystem>();
        GameObject effect = only.gameObject;
        effect.transform.SetParent(null, false);
        effect.name = name;
        Object.DestroyImmediate(root);
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(effect, path);
        Object.DestroyImmediate(effect);
        return prefab;
    }

    private static void SetupScene(StringBuilder log)
    {
        string previous = SceneManager.GetActiveScene().path;
        Scene scene = EditorSceneManager.OpenScene(GameplayScene, OpenSceneMode.Single);

        foreach (string obsolete in new[] { "Yard", "GameplayGround", "Arena", "Decor" })
        {
            GameObject found = scene.GetRootGameObjects().FirstOrDefault(g => g.name == obsolete);
            if (found != null)
                Object.DestroyImmediate(found);
        }

        GameObject environment = scene.GetRootGameObjects().First(g => g.name == "Environment");
        foreach (Transform child in environment.transform.Cast<Transform>().ToList())
            Object.DestroyImmediate(child.gameObject);

        GameObject arena = Place("Arena", Vector3.zero, 0f, null);
        arena.GetComponentInChildren<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
        arena.transform.SetParent(environment.transform, true);

        var decor = new (string model, float x, float z, float yaw, float scale)[]
        {
            ("Rock_A", -12f, 7f, 30f, 1f), ("Rock_A", 13f, -6f, 200f, 0.9f), ("Rock_B", -8.5f, -8.5f, 80f, 1f),
            ("Rock_B", 9.5f, 8.5f, 140f, 1.1f), ("Rock_C", -15f, -2f, 10f, 1f), ("Rock_C", 4.5f, -10f, 250f, 1f),
            ("Rock_C", 15f, 5f, 120f, 1f), ("Rock_C", -4f, 10f, 300f, 1f), ("Rock_B", 16f, -9.5f, 60f, 0.8f),
            ("Crystal_A", -10.5f, -5f, 0f, 1.8f), ("Crystal_A", 7f, -7.5f, 45f, 1.6f), ("Crystal_B", 11f, 2f, 90f, 1.9f),
            ("Crystal_B", -6f, 6.5f, 200f, 1.9f), ("Crystal_B", 2f, 10.5f, 310f, 1.7f),
        };

        foreach ((string model, float x, float z, float yaw, float scale) in decor)
        {
            GameObject prop = Place(model, new Vector3(x, 0f, z), yaw, environment.transform);
            prop.transform.localScale = Vector3.one * scale;
        }

        foreach (Transform t in environment.GetComponentsInChildren<Transform>(true))
            GameObjectUtility.SetStaticEditorFlags(t.gameObject, StaticEditorFlags.BatchingStatic);

        Camera camera = scene.GetRootGameObjects().First(g => g.name == "Main Camera").GetComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.91f, 0.87f, 0.82f);

        Light sun = scene.GetRootGameObjects().First(g => g.name == "Directional Light").GetComponent<Light>();
        sun.color = new Color(1f, 0.96f, 0.9f);
        sun.intensity = 1.15f;
        sun.shadows = LightShadows.Soft;
        sun.shadowStrength = 0.6f;

        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.8f, 0.8f, 0.82f);
        RenderSettings.ambientEquatorColor = new Color(0.58f, 0.54f, 0.5f);
        RenderSettings.ambientGroundColor = new Color(0.3f, 0.28f, 0.27f);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        log.AppendLine($"scene: arena, {decor.Length} props (batching static), warm sun, trilight ambient, sand background");

        if (string.IsNullOrEmpty(previous) == false && previous != GameplayScene)
            EditorSceneManager.OpenScene(previous, OpenSceneMode.Single);
    }

    private static GameObject Place(string model, Vector3 position, float yaw, Transform parent)
    {
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Models + model + ".fbx"));
        if (parent != null)
            instance.transform.SetParent(parent, false);

        instance.transform.position = position;
        instance.transform.rotation = Quaternion.Euler(0f, yaw, 0f) * instance.transform.rotation;
        return instance;
    }

    private static void Assign(Object target, params (string field, Object value)[] values)
    {
        var serialized = new SerializedObject(target);

        foreach ((string field, Object value) in values)
            serialized.FindProperty(field).objectReferenceValue = value;

        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetString(Object target, string field, string value)
    {
        var serialized = new SerializedObject(target);
        serialized.FindProperty(field).stringValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(target);
    }

    private static void SetReference(Object target, string field, Object value)
    {
        var serialized = new SerializedObject(target);
        serialized.FindProperty(field).objectReferenceValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(target);
    }

    private static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString("#" + hex, out Color color);
        return color;
    }
}
