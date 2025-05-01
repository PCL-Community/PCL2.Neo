using PCL2.Neo.Models.Account.OAuthService.RedirectServer;

namespace PCL2.Neo.Tests.Models.Account.OAuthService.RedirectServer
{
    public class RedirectServerTests
    {
        [Test]
        public void ServerTest()
        {
            const ushort port = 5080;
            var server = new Neo.Models.Account.OAuthService.RedirectServer.RedirectServer(port);

            var result = new AuthCode();

            server.Subscribe(result);

            Console.Write(result.GetAuthCode());
        }
    }
}
