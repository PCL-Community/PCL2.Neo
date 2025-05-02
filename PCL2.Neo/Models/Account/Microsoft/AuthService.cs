using PCL2.Neo.Models.Account.OAuthService;
using System.Threading.Tasks;

namespace PCL2.Neo.Models.Account.Microsoft;

public partial class MicrosoftAuth
{
    private static async Task<AccountInfo> AuthCodeLogin() => await DeviceCode.Login();

    private static async Task<AccountInfo> DeviceCodeLogin() => await OAuth.LogIn();
}