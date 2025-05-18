using PCL.Neo.Core.Utils;

namespace PCL.Neo.Core.Service.Accounts.YggdrasilAuth;

public interface IYggdrasilAuthServiceClassic
{
    /// <summary>
    /// API的基础URL
    /// </summary>
    Uri BaseUrl { get; }

    /// <summary>
    /// 客户端Token
    /// </summary>
    string ClientToken { get; set; }

    /// <summary>
    /// 当前的AccessToken
    /// </summary>
    string CurrentAccessToken { get; set; }

    /// <summary>
    /// 当前选择的角色
    /// </summary>
    YggdrasilClassicData.SelectedProfileData CurrentProfileData { get; set; }

    /// <summary>
    /// 根据服务器地址获取ALI地址
    /// </summary>
    /// <param name="url">请求地址</param>
    /// <returns>ALI地址</returns>
    Task<Result<Uri, Exception>> GetAliAsync(Uri url);

    /// <summary>
    /// 获取服务器的元数据
    /// </summary>
    /// <param name="authServerUrl">API地址</param>
    /// <returns>服务器元数据</returns>
    Task<Result<YggdrasilClassicData.Response.MetaInfo, Exception>> GetAuthServerMetaInfoAsync();

    /// <summary>
    /// 登录账户
    /// </summary>
    /// <param name="email">邮箱</param>
    /// <param name="pwd">密码</param>
    /// <returns>登录结果</returns>
    Task<Result<YggdrasilClassicData.Response.Login, Exception>> LoginAsync(string email, string pwd);

    /// <summary>
    /// 刷新令牌，并绑定角色
    /// </summary>
    /// <param name="selectedProfile">要绑定的角色</param>
    /// <returns>刷新结果</returns>
    Task<Result<YggdrasilClassicData.Response.Refresh, Exception>> RefreshAsync(
        YggdrasilClassicData.SelectedProfileData? selectedProfile = null);

    /// <summary>
    /// 验证令牌
    /// </summary>
    Task<Result<bool, Exception>> Validata();

    /// <summary>
    /// 吊销令牌
    /// </summary>
    Task Invalidata();

    /// <summary>
    /// 登出
    /// </summary>
    Task SignoutAsync(string userName, string pwd);

    Task<Result<YggdrasilClassicData.PlayerTexture, Exception>> GetPlayerTextureAsync();
}