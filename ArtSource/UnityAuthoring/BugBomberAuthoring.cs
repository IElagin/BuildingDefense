// Editor authoring for the L8 bug bomber, run in the open Editor through Pipeline run_script:
//   unity command --project-path <BuildingDefense> run_script --file ArtSource/UnityAuthoring/BugBomberAuthoring.cs --entry BugBomberAuthoring.Run --timeout_ms 120000
// Idempotent: palette texture and materials, BugBomber.fbx import (Generic rig, named looping clips,
// palette material remap), the animator controller, spawn and hit effects, and the Bomber prefab visual.
using System.Linq;
using System.Text;
using Assets._Project.Develop.Runtime.Gameplay.Features.ApplyDamage;
using Assets._Project.Develop.Runtime.Gameplay.Features.LifeCycle;
using Assets._Project.Develop.Runtime.Gameplay.Features.MovementFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.SpawnFeature;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class BugBomberAuthoring
{
    private const string PalettePath = "Assets/_Project/Art/Materials/Palette.png";
    private const string PaletteMaterialPath = "Assets/_Project/Art/Materials/Palette.mat";
    private const string PaletteGlowMaterialPath = "Assets/_Project/Art/Materials/PaletteGlow.mat";
    private const string ParticleMaterialPath = "Assets/_Project/Art/Materials/ParticleFlat.mat";
    private const string ModelPath = "Assets/_Project/Art/Models/BugBomber.fbx";
    private const string ControllerPath = "Assets/_Project/Art/Animations/BugBomber.controller";
    private const string SpawnEffectPath = "Assets/_Project/Art/VFX/BurrowSpawnEffect.prefab";
    private const string HitEffectPath = "Assets/_Project/Art/VFX/BugHitEffect.prefab";
    private const string BomberPrefabPath = "Assets/_Project/Resources/Entities/Bomber.prefab";

    private static readonly string[] LoopingClips = { "Walk", "Idle" };

    public static string Run()
    {
        var log = new StringBuilder();

        Texture2D palette = SetupPalette(log);
        Material paletteMaterial = SetupLitMaterial(PaletteMaterialPath, palette, false);
        Material glowMaterial = SetupLitMaterial(PaletteGlowMaterialPath, palette, true);
        SetupModel(paletteMaterial, glowMaterial, log);
        AnimatorController controller = SetupController(log);
        Material particleMaterial = SetupParticleMaterial();
        GameObject spawnEffect = SetupSpawnEffect(particleMaterial);
        GameObject hitEffect = SetupHitEffect(particleMaterial);
        SetupBomberPrefab(controller, spawnEffect, hitEffect, log);

        AssetDatabase.SaveAssets();
        return log.ToString();
    }

    private static Texture2D SetupPalette(StringBuilder log)
    {
        AssetDatabase.ImportAsset(PalettePath, ImportAssetOptions.ForceUpdate);
        var importer = (TextureImporter)AssetImporter.GetAtPath(PalettePath);
        importer.textureType = TextureImporterType.Default;
        importer.sRGBTexture = true;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Point;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(PalettePath);
        log.AppendLine($"palette {texture.width}x{texture.height} point, no mips, uncompressed");
        return texture;
    }

    private static Material SetupLitMaterial(string path, Texture2D palette, bool glow)
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);

        if (material == null)
        {
            material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(material, path);
        }

        material.SetTexture("_BaseMap", palette);
        material.SetColor("_BaseColor", Color.white);
        material.SetFloat("_Smoothness", 0.1f);
        material.SetFloat("_Metallic", 0f);

        if (glow)
        {
            material.EnableKeyword("_EMISSION");
            material.SetTexture("_EmissionMap", palette);
            material.SetColor("_EmissionColor", Color.white * 1.6f);
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
        }

        EditorUtility.SetDirty(material);
        return material;
    }

    private static void SetupModel(Material palette, Material glow, StringBuilder log)
    {
        var importer = (ModelImporter)AssetImporter.GetAtPath(ModelPath);
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
        log.AppendLine("model clips " + string.Join(", ", clips.Select(c => $"{c.name}{(c.loopTime ? " (loop)" : "")} {c.firstFrame}-{c.lastFrame}")));
    }

    private static AnimationClip Clip(string name)
        => AssetDatabase.LoadAllAssetsAtPath(ModelPath).OfType<AnimationClip>().First(c => c.name == name);

    private static AnimatorController SetupController(StringBuilder log)
    {
        AssetDatabase.DeleteAsset(ControllerPath);
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        controller.AddParameter("InSpawnProcess", AnimatorControllerParameterType.Bool);
        controller.AddParameter("IsWalking", AnimatorControllerParameterType.Bool);
        controller.AddParameter("IsDead", AnimatorControllerParameterType.Bool);

        AnimatorStateMachine machine = controller.layers[0].stateMachine;
        AnimatorState spawn = machine.AddState("Spawn");
        AnimatorState idle = machine.AddState("Idle");
        AnimatorState walk = machine.AddState("Walk");
        AnimatorState death = machine.AddState("Death");
        spawn.motion = Clip("Spawn");
        idle.motion = Clip("Idle");
        walk.motion = Clip("Walk");
        death.motion = Clip("Death");
        machine.defaultState = spawn;

        Connect(spawn, idle, AnimatorConditionMode.IfNot, "InSpawnProcess");
        Connect(idle, walk, AnimatorConditionMode.If, "IsWalking");
        Connect(walk, idle, AnimatorConditionMode.IfNot, "IsWalking");

        AnimatorStateTransition toDeath = machine.AddAnyStateTransition(death);
        toDeath.hasExitTime = false;
        toDeath.duration = 0.05f;
        toDeath.canTransitionToSelf = false;
        toDeath.AddCondition(AnimatorConditionMode.If, 0, "IsDead");

        EditorUtility.SetDirty(controller);
        log.AppendLine("controller Spawn(default) -> Idle <-> Walk, Any -> Death");
        return controller;
    }

    private static void Connect(AnimatorState from, AnimatorState to, AnimatorConditionMode mode, string parameter)
    {
        AnimatorStateTransition transition = from.AddTransition(to);
        transition.hasExitTime = false;
        transition.duration = 0.1f;
        transition.AddCondition(mode, 0, parameter);
    }

    private static Material SetupParticleMaterial()
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(ParticleMaterialPath);

        if (material == null)
        {
            material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            AssetDatabase.CreateAsset(material, ParticleMaterialPath);
        }

        // flat vertex-coloured chunks; alpha blended so they can fade
        material.SetFloat("_Surface", 1f);
        material.SetFloat("_Blend", 0f);
        material.SetColor("_BaseColor", Color.white);
        material.SetOverrideTag("RenderType", "Transparent");
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        EditorUtility.SetDirty(material);
        return material;
    }

    private static GameObject SetupSpawnEffect(Material material)
    {
        var root = new GameObject("BurrowSpawnEffect");

        // dirt chunks thrown out of the burrow
        ParticleSystem dirt = root.AddComponent<ParticleSystem>();
        ConfigureBurst(dirt, material, count: 16, lifetime: (0.5f, 0.9f), speed: (2.2f, 3.8f), size: (0.06f, 0.14f), gravity: 1.6f,
            colorA: Hex("9C6B3C"), colorB: Hex("5A2414"), coneAngle: 35f, cube: true);

        // the burrow: a dark flat disc that closes after the bug climbs out
        var holeObject = new GameObject("Hole");
        holeObject.transform.SetParent(root.transform, false);
        holeObject.transform.localPosition = new Vector3(0f, 0.012f, 0f);
        ParticleSystem hole = holeObject.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = hole.main;
        main.duration = 1.6f;
        main.loop = false;
        main.startLifetime = 1.6f;
        main.startSpeed = 0f;
        main.startSize3D = true;
        main.startSizeX = 1.0f;
        main.startSizeY = 0.02f;
        main.startSizeZ = 1.0f;
        main.startColor = Hex("2B1A12");
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = true;
        ParticleSystem.EmissionModule emission = hole.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 1) });
        ParticleSystem.ShapeModule shape = hole.shape;
        shape.enabled = false;
        ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = hole.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 0.3f), new Keyframe(0.12f, 1f), new Keyframe(0.7f, 1f), new Keyframe(1f, 0f)));
        ParticleSystemRenderer holeRenderer = hole.GetComponent<ParticleSystemRenderer>();
        holeRenderer.renderMode = ParticleSystemRenderMode.Mesh;
        holeRenderer.mesh = Resources.GetBuiltinResource<Mesh>("Cylinder.fbx");
        holeRenderer.alignment = ParticleSystemRenderSpace.World;
        holeRenderer.sharedMaterial = material;
        holeRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        ParticleSystem.MainModule rootMain = dirt.main;
        rootMain.stopAction = ParticleSystemStopAction.Destroy;
        rootMain.duration = 1.6f;

        return SavePrefab(root, SpawnEffectPath);
    }

    private static GameObject SetupHitEffect(Material material)
    {
        var root = new GameObject("BugHitEffect");
        ParticleSystem acid = root.AddComponent<ParticleSystem>();
        ConfigureBurst(acid, material, count: 10, lifetime: (0.25f, 0.45f), speed: (1.8f, 3.2f), size: (0.05f, 0.1f), gravity: 2.2f,
            colorA: Hex("7CFF3A"), colorB: Hex("B8FF6A"), coneAngle: 70f, cube: true);
        ParticleSystem.MainModule main = acid.main;
        main.stopAction = ParticleSystemStopAction.Destroy;
        return SavePrefab(root, HitEffectPath);
    }

    private static void ConfigureBurst(ParticleSystem system, Material material, int count, (float, float) lifetime, (float, float) speed,
        (float, float) size, float gravity, Color colorA, Color colorB, float coneAngle, bool cube)
    {
        ParticleSystem.MainModule main = system.main;
        main.duration = 1f;
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
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = true;

        ParticleSystem.EmissionModule emission = system.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });

        ParticleSystem.ShapeModule shape = system.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = coneAngle;
        shape.radius = 0.15f;
        shape.rotation = new Vector3(-90f, 0f, 0f);

        ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = system.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(0.7f, 1f), new Keyframe(1f, 0f)));

        ParticleSystemRenderer renderer = system.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = cube ? ParticleSystemRenderMode.Mesh : ParticleSystemRenderMode.Billboard;
        renderer.mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    private static GameObject SavePrefab(GameObject root, string path)
    {
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static void SetupBomberPrefab(AnimatorController controller, GameObject spawnEffect, GameObject hitEffect, StringBuilder log)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(BomberPrefabPath);

        Transform oldVisual = root.transform.Find("Visual");
        if (oldVisual != null)
            Object.DestroyImmediate(oldVisual.gameObject);

        var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
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

        Assign(visual.AddComponent<WalkingView>(), ("_animator", animator));
        Assign(visual.AddComponent<DeadView>(), ("_animator", animator));
        Assign(visual.AddComponent<SpawnProcessView>(), ("_animator", animator), ("_spawnEffectPrefab", spawnEffect.GetComponent<ParticleSystem>()));
        Assign(visual.AddComponent<ApplyDamageView>(), ("_applyDamageEffectPrefab", hitEffect.GetComponent<ParticleSystem>()), ("_effectSpawnPoint", hitPoint));

        var capsule = root.GetComponent<CapsuleCollider>();
        capsule.direction = 2;
        capsule.radius = 0.38f;
        capsule.height = 1.0f;
        capsule.center = new Vector3(0f, 0.35f, -0.1f);

        PrefabUtility.SaveAsPrefabAsset(root, BomberPrefabPath);
        PrefabUtility.UnloadPrefabContents(root);

        int renderers = model.GetComponentsInChildren<MeshRenderer>(true).Length;
        int skinned = model.GetComponentsInChildren<SkinnedMeshRenderer>(true).Length;
        log.AppendLine($"bomber prefab: visual BugBomber ({renderers} mesh renderers, {skinned} skinned), views Walking/Dead/SpawnProcess/ApplyDamage, capsule r0.38 h1.0");
    }

    private static void Assign(Object target, params (string field, Object value)[] values)
    {
        var serialized = new SerializedObject(target);

        foreach ((string field, Object value) in values)
            serialized.FindProperty(field).objectReferenceValue = value;

        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString("#" + hex, out Color color);
        return color;
    }
}
