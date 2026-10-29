using Assets._Project.Develop.Runtime.Gameplay.Features.Defense;
using Assets._Project.Develop.Runtime.Gameplay.Features.Rest;
using Assets._Project.Develop.Runtime.UI.Core;

namespace Assets._Project.Develop.Runtime.UI.Gameplay.Defense
{
    // Shows the placement ring during rest while the cursor is over the ground.
    public class PlacementPreviewPresenter : IPresenter
    {
        private readonly PlacementPreviewView _view;
        private readonly ViewsFactory _viewsFactory;
        private readonly DefensePlacementService _defensePlacementService;
        private readonly RestPhaseService _restPhaseService;

        private bool _isDisposed;

        public PlacementPreviewPresenter(
            PlacementPreviewView view,
            ViewsFactory viewsFactory,
            DefensePlacementService defensePlacementService,
            RestPhaseService restPhaseService)
        {
            _view = view;
            _viewsFactory = viewsFactory;
            _defensePlacementService = defensePlacementService;
            _restPhaseService = restPhaseService;
        }

        public void Initialize()
        {
            _view.Hide();
        }

        public void Dispose()
        {
            _isDisposed = true;
            _viewsFactory.Release(_view);
        }

        // the scene container is disposed a frame before the bootstrap stops calling LateUpdate
        public void LateUpdate()
        {
            if (_isDisposed)
                return;

            if (_restPhaseService.IsActive.Value == false || _defensePlacementService.HasPointerPoint == false)
            {
                _view.Hide();
                return;
            }

            DefenseTypes type = _defensePlacementService.Selected.Value;

            _view.Show(
                _defensePlacementService.PointerPoint,
                _defensePlacementService.GetRadius(type),
                _defensePlacementService.CanPlaceAtPointer);
        }
    }
}
