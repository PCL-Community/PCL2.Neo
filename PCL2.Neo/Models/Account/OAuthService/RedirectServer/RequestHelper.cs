using System;
using System.IO;
using System.Net;

namespace PCL2.Neo.Models.Account.OAuthService.RedirectServer;

public class RequestHelper(HttpListenerRequest request)
{
    private HttpListenerRequest Request { get; } = request;
    public Stream RequestStream { get; set; } = request.InputStream;

    public delegate void ExecutingDespatch(FileStream fileStream);

    public void DispatchResources(ExecutingDespatch action, out RedirectAuthCode authCode)
    {
        var code = Request.QueryString["code"];
        ArgumentNullException.ThrowIfNull(code);
        authCode = new RedirectAuthCode(code);

        var file = new FileStream("OAuthRedirectHttpPage.html", FileMode.Open, FileAccess.Read);

        action?.Invoke(file);
    }
}