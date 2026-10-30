using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets._Project.Develop.Runtime.UI.CommonViews
{
    // Press feedback for buttons (L8 homework, UI interaction): shrinks on press, springs back on release.
    public class ButtonPressFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private const float PressDuration = 0.08f;
        private const float ReleaseDuration = 0.25f;
        private const float DefaultScale = 1;

        [SerializeField] private float _pressedScale = 0.93f;

        private Tween _tween;

        public void OnPointerDown(PointerEventData eventData) => ScaleTo(_pressedScale, PressDuration, Ease.OutQuad);

        public void OnPointerUp(PointerEventData eventData) => ScaleTo(DefaultScale, ReleaseDuration, Ease.OutBack);

        private void OnDisable()
        {
            _tween?.Kill();
            transform.localScale = Vector3.one;
        }

        private void ScaleTo(float scale, float duration, Ease ease)
        {
            _tween?.Kill();
            _tween = transform.DOScale(scale, duration).SetEase(ease).SetUpdate(true);
        }
    }
}
