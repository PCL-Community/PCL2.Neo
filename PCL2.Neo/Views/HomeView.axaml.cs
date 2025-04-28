using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using PCL2.Neo.Controls;
using PCL2.Neo.Models.Minecraft.Java;
using System;
using System.Linq;

namespace PCL2.Neo.Views;

public partial class HomeView : UserControl
{
    public HomeView()
    {
        InitializeComponent();
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        this.TestLoading.State = MyLoading.LoadingState.Loading;
    }

    private void Button2_OnClick(object? sender, RoutedEventArgs e)
    {
        this.TestLoading.State = MyLoading.LoadingState.Error;
    }

    private async void Search_Java_Button(object? sender, RoutedEventArgs e)
    {
        try
        {
            var javas = await Java.SearchJava();
            Console.WriteLine($"找到 {javas.Count()} 个Java环境:");

            foreach (var java in javas)
            {
                try
                {
                    Console.WriteLine("----------------------");
                    Console.WriteLine($"路径: {java.DirectoryPath}");
                    var version = java.Version;
                    Console.WriteLine($"版本: Java {version}");
                    Console.WriteLine($"位数: {(java.Is64Bit ? "64位" : "32位")}");
                    Console.WriteLine($"类型: {(java.IsJre ? "JRE" : "JDK")}");
                    Console.WriteLine($"可用: {java.Compability}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"处理Java信息时出错: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"搜索失败: {ex.Message}");
        }
    }
}