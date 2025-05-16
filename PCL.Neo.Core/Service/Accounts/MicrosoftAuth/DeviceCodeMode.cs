namespace PCL.Neo.Core.Service.Accounts.MicrosoftAuth;

public static class DeviceCodeMode
{
    public record DeviceCodeInfo(string DeviceCode, string UserCode, string VerificationUri, int Interval);

    public record DeviceCodeAccessToken(string AccessToken, string RefreshToken, DateTimeOffset ExpiresIn);

    public record McAccountInfo(
        List<Storage.Skin> Skins,
        List<Storage.Cape> Capes,
        string UserName,
        string Uuid);
}