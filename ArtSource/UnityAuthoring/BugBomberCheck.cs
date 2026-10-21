// Read-only check of the Bomber prefab after BugBomberAuthoring (opens it in an isolated prefab stage):
//   unity command --project-path <BuildingDefense> run_script --file ArtSource/UnityAuthoring/BugBomberCheck.cs --entry BugBomberCheck.Run
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class BugBomberCheck
{
    public static string Run()
    {
        var log = new StringBuilder();
        GameObject root = PrefabUtility.LoadPrefabContents("Assets/_Project/Resources/Entities/Bomber.prefab");

        Transform visual = root.transform.Find("Visual");
        Transform head = visual.GetComponentsInChildren<Transform>(true).First(t => t.name == "Head" && t.GetComponent<MeshRenderer>() == null);
        Transform abdomen = visual.GetComponentsInChildren<Transform>(true).First(t => t.name == "Abdomen" && t.GetComponent<MeshRenderer>() == null);
        Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(true);
        Bounds bounds = renderers[0].bounds;

        foreach (Renderer renderer in renderers)
            bounds.Encapsulate(renderer.bounds);

        log.AppendLine($"head bone {head.position}, abdomen bone {abdomen.position} -> forward is {(head.position.z > abdomen.position.z ? "+Z (ok)" : "NOT +Z")}");
        log.AppendLine($"bounds min {bounds.min} max {bounds.max} size {bounds.size}");
        log.AppendLine("materials " + string.Join(", ", renderers.SelectMany(r => r.sharedMaterials).Select(m => m == null ? "null" : m.name).Distinct()));
        Animator animator = visual.GetComponent<Animator>();
        log.AppendLine($"animator controller {animator.runtimeAnimatorController?.name}, avatar {animator.avatar?.name}, culling {animator.cullingMode}, root motion {animator.applyRootMotion}");
        log.AppendLine("views " + string.Join(", ", visual.GetComponents<MonoBehaviour>().Select(c => c.GetType().Name)));

        foreach (Transform t in visual.GetComponentsInChildren<Transform>(true).Take(6))
            log.AppendLine($"node {t.name} parent {t.parent.name} localPos {t.localPosition} localEuler {t.localEulerAngles} scale {t.localScale}");

        PrefabUtility.UnloadPrefabContents(root);
        return log.ToString();
    }
}
