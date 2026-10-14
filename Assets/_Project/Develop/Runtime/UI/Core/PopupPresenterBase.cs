using System;
using System.Threading;
using Assets._Project.Develop.Runtime.Utilities.SceneManagement;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace Assets._Project.Develop.Runtime.UI.Core
{
    public abstract class PopupPresenterBase : IPresenter
    {
        public event Action<PopupPresenterBase> CloseRequest;

        private CancellationTokenSource _process;

        protected abstract PopupViewBase PopupView { get; }

        public virtual void Initialize()
        {
        }

        public virtual void Dispose()
        {
            KillProcess();

            PopupView.CloseRequest -= OnCloseRequest;
        }

        public void Show()
        {
            KillProcess();

            _process = new CancellationTokenSource();
            ProcessShow(_process.Token).Forget(AsyncErrors.Report);
        }

        public void Hide(Action callback = null)
        {
            KillProcess();

            _process = new CancellationTokenSource();
            ProcessHide(callback, _process.Token).Forget(AsyncErrors.Report);
        }

        protected virtual void OnPostShow() { }

        protected virtual void OnPreShow()
        {
            PopupView.CloseRequest += OnCloseRequest;
        }

        protected virtual void OnPostHide() { }

        protected virtual void OnPreHide()
        {
            PopupView.CloseRequest -= OnCloseRequest;
        }

        protected void OnCloseRequest() => CloseRequest?.Invoke(this);

        private async UniTask ProcessShow(CancellationToken cancellationToken)
        {
            OnPreShow();

            Tween animation = PopupView.Show();
            await UniTask.WaitWhile(() => animation.active && !animation.IsComplete(),
                PlayerLoopTiming.LastUpdate, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            OnPostShow();
        }

        private async UniTask ProcessHide(Action callback, CancellationToken cancellationToken)
        {
            OnPreHide();

            Tween animation = PopupView.Hide();
            await UniTask.WaitWhile(() => animation.active && !animation.IsComplete(),
                PlayerLoopTiming.LastUpdate, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            OnPostHide();

            callback?.Invoke();
        }

        private void KillProcess()
        {
            CancellationTokenSource lifetime = _process;
            _process = null;
            lifetime?.Cancel();
            lifetime?.Dispose();
        }
    }
}
