using System;
using System.Collections.Generic;

namespace PCL2.Neo.Models.Account.OAuthService.RedirectServer;

internal sealed class Unsubscriber<TRedirectAuthCode> : IDisposable
{
    private readonly ISet<IObserver<TRedirectAuthCode>> _observers;
    private readonly IObserver<TRedirectAuthCode> _observer;

    internal Unsubscriber(ISet<IObserver<TRedirectAuthCode>> observers, IObserver<TRedirectAuthCode> observer)
        => (_observers, _observer) = (observers, observer);

    /// <inheritdoc />
    public void Dispose()
    {
        _observers.Remove(_observer);
    }
}