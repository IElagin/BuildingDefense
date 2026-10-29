using Assets._Project.Develop.Runtime.Meta.Features.Wallet;

namespace Assets._Project.Develop.Runtime.Gameplay.States
{
    // Defenses live one run, so whatever the run ends with, the gold spent on them comes back:
    // a win only adds its reward, a loss changes nothing. Created with the scene, before the first purchase.
    public class RunSpendingRefund
    {
        private readonly WalletService _walletService;
        private readonly int _runStartGold;

        public RunSpendingRefund(WalletService walletService)
        {
            _walletService = walletService;
            _runStartGold = walletService.GetCurrency(CurrencyTypes.Gold).Value;
        }

        public void Refund()
        {
            int spent = _runStartGold - _walletService.GetCurrency(CurrencyTypes.Gold).Value;

            if (spent > 0)
                _walletService.Add(CurrencyTypes.Gold, spent);
        }
    }
}
