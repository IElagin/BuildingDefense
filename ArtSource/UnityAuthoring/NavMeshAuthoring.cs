// Editor authoring for the NavMesh movement of the bugs (2026-10-29), run in the open Editor through Pipeline run_script:
//   unity command --project-path <BuildingDefense> run_script --file ArtSource/UnityAuthoring/NavMeshAuthoring.cs --entry NavMeshAuthoring.Run --timeout_ms 180000
// After WorldAuthoring, BugBomberAuthoring, SpitterAuthoring and DefenseAuthoring (they rebuild the prefabs and the
// arena). Idempotent: the course NavMeshTest agent (CharactersFactory, AgentMover) on the Bomber and Spitter prefabs with
// a kinematic body, carving obstacles on the dome and the turret, and the arena NavMesh baked from the Environment
// colliders of the Gameplay scene (plateau, rocks, crystals) into Assets/_Project/Scenes/Gameplay/NavMesh.asset.
using System.Linq;
using System.Text;
using Assets._Project.Develop.Runtime.Gameplay.Features.MovementFeature;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public static class NavMeshAuthoring
{
    private const string Entities = "Assets/_Project/Resources/Entities/";
    private const string GameplayScene = "Assets/_Project/Scenes/Gameplay.unity";
    private const string NavMeshFolder = "Assets/_Project/Scenes/Gameplay";
    private const string NavMeshPath = NavMeshFolder + "/NavMesh.asset";

    // the bug body capsule (BugBomberAuthoring: radius 0.38, length 1.0 lying along the bug)
    private const float BugRadius = 0.4f;
    private const float BugHeight = 1f;

    public static string Run()
    {
        var log = new StringBuilder();

        SetupBug("Bomber", log);
        SetupBug("Spitter", log);
        SetupBuilding(log);
        SetupTurret(log);
        BakeArena(log);

        AssetDatabase.SaveAssets();
        return log.ToString();
    }

    private static void SetupBug(string name, StringBuilder log)
    {
        string path = Entities + name + ".prefab";
        GameObject root = PrefabUtility.LoadPrefabContents(path);

        if (root.TryGetComponent(out NavMeshAgent agent) == false)
            agent = root.AddComponent<NavMeshAgent>();
        agent.agentTypeID = 0;
        agent.radius = BugRadius;
        agent.height = BugHeight;
        agent.baseOffset = 0f;
        agent.stoppingDistance = 0f;
        agent.autoBraking = true;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;

        if (root.TryGetComponent(out NavMeshAgentEntityRegistrator _) == false)
            root.AddComponent<NavMeshAgentEntityRegistrator>();

        // the agent moves the transform and TransformRotationSystem turns it; the kinematic body only carries the collider
        var body = root.GetComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;
        body.mass = 1f;
        body.interpolation = RigidbodyInterpolation.None;

        PrefabUtility.SaveAsPrefabAsset(root, path);
        PrefabUtility.UnloadPrefabContents(root);
        log.AppendLine($"{name}: NavMeshAgent r{BugRadius} h{BugHeight}, high quality avoidance, kinematic body");
    }

    private static void SetupBuilding(StringBuilder log)
    {
        string path = Entities + "Building.prefab";
        GameObject root = PrefabUtility.LoadPrefabContents(path);
        var capsule = root.GetComponent<CapsuleCollider>();
        float radius = capsule.radius;
        float height = capsule.height;

        NavMeshObstacle obstacle = Obstacle(root);
        obstacle.radius = radius;
        obstacle.height = height;
        obstacle.center = new Vector3(0f, height / 2f, 0f);

        PrefabUtility.SaveAsPrefabAsset(root, path);
        PrefabUtility.UnloadPrefabContents(root);
        log.AppendLine($"Building: carving obstacle r{radius}");
    }

    private static void SetupTurret(StringBuilder log)
    {
        string path = Entities + "Turret.prefab";
        GameObject root = PrefabUtility.LoadPrefabContents(path);

        // the still base: the barrels of the turning head would widen the hole whenever they point sideways
        Renderer[] baseRenderers = root.transform.Find("Base").GetComponentsInChildren<Renderer>();
        Bounds bounds = baseRenderers[0].bounds;
        foreach (Renderer renderer in baseRenderers)
            bounds.Encapsulate(renderer.bounds);

        NavMeshObstacle obstacle = Obstacle(root);
        obstacle.radius = Mathf.Max(bounds.extents.x, bounds.extents.z);
        // the base is a flat disc: a bug-tall obstacle surely crosses the NavMesh it carves
        obstacle.height = Mathf.Max(bounds.size.y, BugHeight);
        obstacle.center = new Vector3(0f, obstacle.height / 2f, 0f);

        log.AppendLine($"Turret: carving obstacle r{obstacle.radius:0.00} h{obstacle.height:0.00}");
        PrefabUtility.SaveAsPrefabAsset(root, path);
        PrefabUtility.UnloadPrefabContents(root);
    }

    private static NavMeshObstacle Obstacle(GameObject root)
    {
        if (root.TryGetComponent(out NavMeshObstacle obstacle) == false)
            obstacle = root.AddComponent<NavMeshObstacle>();
        obstacle.shape = NavMeshObstacleShape.Capsule;
        obstacle.carving = true;
        obstacle.carveOnlyStationary = true;
        return obstacle;
    }

    private static void BakeArena(StringBuilder log)
    {
        string previous = SceneManager.GetActiveScene().path;
        Scene scene = EditorSceneManager.OpenScene(GameplayScene, OpenSceneMode.Single);
        GameObject environment = scene.GetRootGameObjects().First(g => g.name == "Environment");

        if (environment.TryGetComponent(out NavMeshSurface surface) == false)
            surface = environment.AddComponent<NavMeshSurface>();
        surface.agentTypeID = 0;
        surface.collectObjects = CollectObjects.Children;
        surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
        surface.layerMask = LayerMask.GetMask("Environment");
        // the default voxel (a third of the agent radius) lifts the NavMesh about 0.1 m above the plateau and the
        // bugs float; a finer one keeps their feet on the ground
        surface.overrideVoxelSize = true;
        surface.voxelSize = 0.04f;

        surface.navMeshData = null;
        surface.BuildNavMesh();
        NavMeshData data = surface.navMeshData;

        if (AssetDatabase.IsValidFolder(NavMeshFolder) == false)
            AssetDatabase.CreateFolder("Assets/_Project/Scenes", "Gameplay");
        AssetDatabase.DeleteAsset(NavMeshPath);
        AssetDatabase.CreateAsset(data, NavMeshPath);
        surface.navMeshData = AssetDatabase.LoadAssetAtPath<NavMeshData>(NavMeshPath);

        NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        log.AppendLine($"arena NavMesh: {triangulation.indices.Length / 3} triangles from the Environment colliders, {NavMeshPath}");

        if (string.IsNullOrEmpty(previous) == false && previous != GameplayScene)
            EditorSceneManager.OpenScene(previous, OpenSceneMode.Single);
    }
}
