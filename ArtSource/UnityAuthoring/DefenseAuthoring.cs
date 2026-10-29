// Editor authoring for the final assignment defenses (2026-10-29), run in the open Editor through Pipeline run_script:
//   unity command --project-path <BuildingDefense> run_script --file ArtSource/UnityAuthoring/DefenseAuthoring.cs --entry DefenseAuthoring.Run --timeout_ms 180000
// After ArtSource/Blender/defense_steps (Turret.fbx, Puddle.fbx, PlasmaBolt.fbx). Idempotent: imports the models, builds
// the Turret prefab (a still base and a turning Head that carries the entity transform, the kinematic rigidbody and the
// shoot point), the Puddle prefab (blob and bubbles scaled to the sensor radius by ContactsRadiusView) and the course
// Projectile prefab (plasma bolt with a trail, trigger capsule body), creates TurretConfig and PuddleConfig and links
// them to DefenseConfig.
using System.Linq;
using System.Text;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Defense;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Gameplay.Common;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using Assets._Project.Develop.Runtime.Gameplay.Features.Attack.Shoot;
using Assets._Project.Develop.Runtime.Gameplay.Features.LifeCycle;
using Assets._Project.Develop.Runtime.Gameplay.Features.Sensors;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public static class DefenseAuthoring
{
    private const string Models = "Assets/_Project/Art/Models/";
    private const string MaterialsFolder = "Assets/_Project/Art/Materials/";
    private const string Entities = "Assets/_Project/Resources/Entities/";
    private const string EntityConfigs = "Assets/_Project/Resources/Configs/Gameplay/Entities/";
    private const string DefenseConfigPath = "Assets/_Project/Resources/Configs/Gameplay/Defense/DefenseConfig.asset";

    // bolt height of the turret barrels (Blender defense_steps/02_models.py) and their muzzle distance
    private static readonly Vector3 ShootPoint = new(0f, 0.45f, 0.72f);
    private static readonly Color Plasma = new(0.54f, 0.96f, 1f, 1f);

    // prefabs are put together in a preview scene, so the open scene stays untouched
    private static Scene _stage;

    public static string Run()
    {
        var log = new StringBuilder();
        Material palette = AssetDatabase.LoadAssetAtPath<Material>(MaterialsFolder + "Palette.mat");
        Material glow = AssetDatabase.LoadAssetAtPath<Material>(MaterialsFolder + "PaletteGlow.mat");
        Material particles = AssetDatabase.LoadAssetAtPath<Material>(MaterialsFolder + "ParticleFlat.mat");

        foreach (string name in new[] { "Turret", "Puddle", "PlasmaBolt" })
            ImportModel(name, palette, glow);

        _stage = EditorSceneManager.NewPreviewScene();
        BuildTurret(log);
        BuildPuddle(particles, log);
        BuildProjectile(particles, log);
        EditorSceneManager.ClosePreviewScene(_stage);
        SetupConfigs(log);

        AssetDatabase.SaveAssets();
        return log.ToString();
    }

    private static void ImportModel(string name, Material palette, Material glow)
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

    private static void BuildTurret(StringBuilder log)
    {
        GameObject root = NewEntityRoot("Turret");

        GameObject model = Unpacked("Turret");
        Take(model, "Turret_Base", root.transform, "Base");

        GameObject head = Child(root.transform, "Head");
        Take(model, "Turret_Head", head.transform, "Visual");
        Object.DestroyImmediate(model);

        Transform shootPoint = Child(head.transform, "ShootPoint").transform;
        shootPoint.localPosition = ShootPoint;

        // the turning head is the entity body: RotateToTargetState and the aim check read its transform
        var rigidbody = head.AddComponent<Rigidbody>();
        rigidbody.isKinematic = true;
        rigidbody.useGravity = false;
        rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        head.AddComponent<TransformEntityRegistrator>();
        head.AddComponent<RigidbodyEntityRegistrator>();
        Assign(head.AddComponent<ShootPointEntityRegistrator>(), ("_shootPoint", shootPoint));

        SavePrefab(root, "Turret");
        log.AppendLine("Turret: still base, turning Head (kinematic rigidbody, transform, shoot point at " + ShootPoint + ")");
    }

    private static void BuildPuddle(Material particles, StringBuilder log)
    {
        GameObject root = NewEntityRoot("Puddle");
        root.AddComponent<TransformEntityRegistrator>();

        Transform blob = Take(Unpacked("Puddle"), "Puddle", root.transform, "Visual");
        blob.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;

        Bubbles(blob.gameObject, particles);
        Assign(root.AddComponent<ContactsRadiusView>(), ("_radiusIndicator", blob));

        SavePrefab(root, "Puddle");
        log.AppendLine("Puddle: blob of radius 1 with bubbles, scaled to the sensor radius");
    }

    private static void BuildProjectile(Material particles, StringBuilder log)
    {
        GameObject root = NewEntityRoot("Projectile");
        root.AddComponent<TransformEntityRegistrator>();

        Transform bolt = Take(Unpacked("PlasmaBolt"), "PlasmaBolt", root.transform, "Visual");
        bolt.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;

        // a trigger body: it finds contacts by overlap and never pushes the bugs
        var body = root.AddComponent<CapsuleCollider>();
        body.isTrigger = true;
        body.direction = 2;
        body.radius = 0.12f;
        body.height = 0.4f;

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
        trail.time = 0.12f;
        trail.minVertexDistance = 0.05f;
        trail.widthCurve = new AnimationCurve(new Keyframe(0f, 0.14f), new Keyframe(1f, 0f));
        trail.colorGradient = new Gradient
        {
            colorKeys = new[] { new GradientColorKey(Plasma, 0f), new GradientColorKey(Plasma, 1f) },
            alphaKeys = new[] { new GradientAlphaKey(0.9f, 0f), new GradientAlphaKey(0f, 1f) }
        };
        trail.shadowCastingMode = ShadowCastingMode.Off;
        trail.receiveShadows = false;

        SavePrefab(root, "Projectile");
        log.AppendLine("Projectile: plasma bolt, cyan trail, trigger capsule body r0.12");
    }

    private static void SetupConfigs(StringBuilder log)
    {
        TurretConfig turret = LoadOrCreate<TurretConfig>("TurretConfig");
        PuddleConfig puddle = LoadOrCreate<PuddleConfig>("PuddleConfig");

        var defense = AssetDatabase.LoadAssetAtPath<DefenseConfig>(DefenseConfigPath);
        var serialized = new SerializedObject(defense);
        serialized.FindProperty("<Turret>k__BackingField").objectReferenceValue = turret;
        serialized.FindProperty("<Puddle>k__BackingField").objectReferenceValue = puddle;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(defense);

        log.AppendLine("configs: TurretConfig and PuddleConfig linked to DefenseConfig");
    }

    private static void Bubbles(GameObject parent, Material material)
    {
        GameObject child = Child(parent.transform, "Bubbles");
        ParticleSystem system = child.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = system.main;
        main.duration = 1f;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1.1f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.15f, 0.4f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.11f);
        main.startColor = new ParticleSystem.MinMaxGradient(Plasma, new Color(0.18f, 0.83f, 0.78f, 1f));
        main.scalingMode = ParticleSystemScalingMode.Shape;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 60;
        main.playOnAwake = true;

        ParticleSystem.EmissionModule emission = system.emission;
        emission.rateOverTime = 12f;

        // the shape follows the blob's scale (the sensor radius), the particles keep their size
        ParticleSystem.ShapeModule shape = system.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.8f;
        shape.rotation = new Vector3(-90f, 0f, 0f);
        shape.position = new Vector3(0f, 0.03f, 0f);

        ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = system.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 0.4f), new Keyframe(0.5f, 1f), new Keyframe(1f, 0f)));

        ParticleSystemRenderer renderer = child.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Mesh;
        renderer.mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
    }

    private static GameObject NewEntityRoot(string name)
    {
        var root = new GameObject(name);
        SceneManager.MoveGameObjectToScene(root, _stage);
        root.AddComponent<MonoEntity>();
        return root;
    }

    private static GameObject Child(Transform parent, string name)
    {
        var child = new GameObject(name);
        child.transform.SetParent(parent, false);
        return child;
    }

    private static GameObject Unpacked(string model)
    {
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Models + model + ".fbx"), _stage);
        PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        return instance;
    }

    // A single-mesh FBX imports as one object, a multi-mesh one as a root with a child per mesh.
    private static Transform Take(GameObject model, string part, Transform parent, string name)
    {
        Transform taken = model.transform.Find(part) ?? model.transform;
        taken.SetParent(parent, false);
        taken.name = name;
        return taken;
    }

    private static void SavePrefab(GameObject root, string name)
    {
        PrefabUtility.SaveAsPrefabAsset(root, Entities + name + ".prefab");
        Object.DestroyImmediate(root);
    }

    private static T LoadOrCreate<T>(string name) where T : ScriptableObject
    {
        string path = EntityConfigs + name + ".asset";
        var asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset != null)
            return asset;

        asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }

    private static void Assign(Object target, params (string field, Object value)[] values)
    {
        var serialized = new SerializedObject(target);

        foreach ((string field, Object value) in values)
            serialized.FindProperty(field).objectReferenceValue = value;

        serialized.ApplyModifiedPropertiesWithoutUndo();
    }
}
