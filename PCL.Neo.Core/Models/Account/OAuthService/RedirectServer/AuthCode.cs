namespace PCL.Neo.Core.Models.Account.OAuthService.RedirectServer;

public class AuthCode : IObserver<RedirectAuthCode>
{
    public ManualResetEvent IsGetAuthCode = new(false);

    public RedirectAuthCode? AuthCodeValue { get; private set; }

    /// <inheritdoc />
    public void OnCompleted() { }

    /// <inheritdoc />
    public void OnError(Exception error)
    {
        // todo: log this error
        throw error;
    }

    /// <inheritdoc />
    public void OnNext(RedirectAuthCode value)
    {
        AuthCodeValue = value;
        IsGetAuthCode.Set();
    }

    public RedirectAuthCode GetAuthCode()
    {
        IsGetAuthCode.WaitOne();

        return AuthCodeValue;
    }
}