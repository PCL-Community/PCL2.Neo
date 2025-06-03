using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Net;

namespace PCL.Neo.Core.Service.Accounts.OAuthService.RedirectServer;

public class RequestHelper(HttpListenerRequest request)
{
    public delegate void ExecutingDespatch(FileStream fileStream);

    private HttpListenerRequest Request { get; } = request;
    public Stream RequestStream { get; set; } = request.InputStream;

    public void DispatchResources(ExecutingDespatch action, out RedirectAuthCode authCode)
    {
        var code = Request.QueryString["code"];
        if (code == null)
            throw new ArgumentNullException(nameof(code));
            
        authCode = new RedirectAuthCode(code);

        var file = new FileStream("OAuthRedirectHttpPage.html", FileMode.Open, FileAccess.Read);

        action?.Invoke(file);
    }

    public static string GetAuthCode(string url)
    {
        var code = GetQueryParameter(url, "code");
        
        if (code == null)
            throw new ArgumentNullException(nameof(code));
        
        return code;
    }

    private static string? GetQueryParameter(string url, string paramName)
    {
        var uri = new Uri(url);
        var queryParams = HttpUtility.ParseQueryString(uri.Query);
        return queryParams[paramName];
    }
}