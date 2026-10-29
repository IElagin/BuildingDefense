using Assets._Project.Develop.Runtime.Gameplay.Features.Defense;
using Assets._Project.Develop.Runtime.Meta.Features.Wallet;
using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.Utilities.Audio;
using System;
using System.Collections.Generic;

namespace Assets._Project.Develop.Runtime.UI.Gameplay.Defense
{
    // Fills the defense cards from the placement service and the wallet; a card click selects its defense.
    public class DefensePanelPresenter : IPresenter
    {
        private readonly DefensePanelView _view;
        private readonly DefensePlacementService _defensePlacementService;
        private readonly WalletService _walletService;
        private readonly IAudioService _audio;

        private readonly Dictionary<DefenseTypes, DefenseCardView> _cards = new();
        private readonly Dictionary<DefenseCardView, Action> _clickHandlers = new();
        private readonly List<IDisposable> _subscriptions = new();

        public DefensePanelPresenter(
            DefensePanelView view,
            DefensePlacementService defensePlacementService,
            WalletService walletService,
            IAudioService audio)
        {
            _view = view;
            _defensePlacementService = defensePlacementService;
            _walletService = walletService;
            _audio = audio;
        }

        public void Initialize()
        {
            _cards.Add(DefenseTypes.Mine, _view.MineCard);
            _cards.Add(DefenseTypes.Turret, _view.TurretCard);
            _cards.Add(DefenseTypes.Puddle, _view.PuddleCard);

            foreach (KeyValuePair<DefenseTypes, DefenseCardView> card in _cards)
            {
                DefenseTypes type = card.Key;
                Action handler = () => OnCardClicked(type);

                card.Value.SetPrice(_defensePlacementService.GetPrice(type).ToString());
                card.Value.Clicked += handler;
                _clickHandlers.Add(card.Value, handler);
            }

            UpdateCards();

            _subscriptions.Add(_defensePlacementService.Selected.Subscribe((oldValue, newValue) => UpdateCards()));
            _subscriptions.Add(_walletService.GetCurrency(CurrencyTypes.Gold).Subscribe((oldValue, newValue) => UpdateCards()));
        }

        public void Dispose()
        {
            foreach (KeyValuePair<DefenseCardView, Action> handler in _clickHandlers)
                handler.Key.Clicked -= handler.Value;

            _clickHandlers.Clear();

            foreach (IDisposable subscription in _subscriptions)
                subscription.Dispose();

            _subscriptions.Clear();
            _cards.Clear();
        }

        private void OnCardClicked(DefenseTypes type)
        {
            _audio.Play(SoundIds.UIClick);
            _defensePlacementService.Select(type);
        }

        private void UpdateCards()
        {
            foreach (KeyValuePair<DefenseTypes, DefenseCardView> card in _cards)
            {
                int price = _defensePlacementService.GetPrice(card.Key);

                card.Value.SetSelected(_defensePlacementService.Selected.Value == card.Key);
                card.Value.SetDimmed(_walletService.Enough(CurrencyTypes.Gold, price) == false);
            }
        }
    }
}
