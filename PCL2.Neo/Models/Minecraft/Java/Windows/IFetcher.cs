using System.Threading.Tasks;

namespace PCL2.Neo.Models.Minecraft.Java.WindowsJava;

/// <summary>
/// 统一搜寻器接口
/// </summary>
public interface IFetcher<T> {

    /// <summary>
    /// 同步搜寻 <typeparamref name="T"/> 类型方法
    /// </summary>
    T Fetch();

    /// <summary>
    /// 异步搜寻 <typeparamref name="T"/> 类型方法
    /// </summary>
    ValueTask<T> FetchAsync();
}