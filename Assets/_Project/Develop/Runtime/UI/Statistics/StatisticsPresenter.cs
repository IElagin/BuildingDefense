using Assets._Project.Develop.Runtime.Meta.Features.Statistics;
using Assets._Project.Develop.Runtime.UI.Core;
using System;

namespace Assets._Project.Develop.Runtime.UI.Statistics
{
    public class StatisticsPresenter : IPresenter
    {
        private readonly GameStatisticsService _statisticsService;
        private readonly StatisticsView _view;

        private IDisposable _winsDisposable;
        private IDisposable _lossesDisposable;

        public StatisticsPresenter(GameStatisticsService statisticsService, StatisticsView view)
        {
            _statisticsService = statisticsService;
            _view = view;
        }

        public void Initialize()
        {
            UpdateWins(_statisticsService.Wins.Value);
            UpdateLosses(_statisticsService.Losses.Value);

            _winsDisposable = _statisticsService.Wins.Subscribe(OnWinsChanged);
            _lossesDisposable = _statisticsService.Losses.Subscribe(OnLossesChanged);
        }

        public void Dispose()
        {
            _winsDisposable.Dispose();
            _lossesDisposable.Dispose();
        }

        private void OnWinsChanged(int oldValue, int newValue) => UpdateWins(newValue);

        private void OnLossesChanged(int oldValue, int newValue) => UpdateLosses(newValue);

        private void UpdateWins(int wins) => _view.SetWins($"Победы: {wins}");

        private void UpdateLosses(int losses) => _view.SetLosses($"Поражения: {losses}");
    }
}
