using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Defense
{
    // Converts a screen point into a point on the ground plane (y = 0) seen by the main camera.
    public class GroundPointService
    {
        private readonly Plane _ground = new Plane(Vector3.up, Vector3.zero);

        public bool TryGetPoint(Vector2 screenPosition, out Vector3 point)
        {
            Camera camera = Camera.main;
            Ray ray = camera.ScreenPointToRay(screenPosition);

            if (_ground.Raycast(ray, out float distance))
            {
                point = ray.GetPoint(distance);
                return true;
            }

            point = Vector3.zero;
            return false;
        }
    }
}
