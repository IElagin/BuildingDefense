using Assets._Project.Develop.Runtime.UI.Core;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.UI.Gameplay.Defense
{
    // A ring on the ground under the cursor: the radius of the selected defense, green where it can be built,
    // red where it cannot.
    public class PlacementPreviewView : MonoBehaviour, IView
    {
        private const float NoRadius = -1;
        private const float FullCircleRadians = Mathf.PI * 2;

        [SerializeField] private LineRenderer _ring;
        [SerializeField] private Color _freeColor = new(0.49f, 1f, 0.23f, 0.9f);
        [SerializeField] private Color _blockedColor = new(1f, 0.25f, 0.2f, 0.9f);
        [SerializeField, Min(8)] private int _segments = 72;
        [SerializeField, Min(0)] private float _height = 0.08f;

        private float _radius = NoRadius;

        public void Show(Vector3 center, float radius, bool isFree)
        {
            _ring.enabled = true;
            transform.position = center + Vector3.up * _height;

            if (Mathf.Approximately(radius, _radius) == false)
                BuildRing(radius);

            Color color = isFree ? _freeColor : _blockedColor;
            _ring.startColor = color;
            _ring.endColor = color;
        }

        public void Hide() => _ring.enabled = false;

        private void BuildRing(float radius)
        {
            _radius = radius;
            _ring.useWorldSpace = false;
            _ring.loop = true;
            _ring.positionCount = _segments;

            for (int i = 0; i < _segments; i++)
            {
                float angle = FullCircleRadians * i / _segments;
                _ring.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius));
            }
        }
    }
}
