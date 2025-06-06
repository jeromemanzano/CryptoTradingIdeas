using System.Reactive.Linq;
using ReactiveUI;

namespace CryptoTradingIdeas.Core.Services;

public abstract class ExchangeServiceBase
{
    protected static IObservable<long> GetRefreshTimerObservable()
    {
        return Observable.Timer(dueTime: TimeSpan.Zero, period: TimeSpan.FromSeconds(5), RxApp.TaskpoolScheduler);
    }
}