namespace PCL.Neo.Core.Service.Accounts.OAuthService;

public partial class MinecraftInfo
{
    public class NotHaveGameException(string msg) : Exception(msg);
}