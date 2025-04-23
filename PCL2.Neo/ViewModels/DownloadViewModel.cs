using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PCL2.Neo.ViewModels;

public partial class DownloadViewModel : ViewModelBase
{
    public override bool IsPaneVisible => true;

    [ObservableProperty]
    private string _message = "I am from DownloadViewModel";

    [RelayCommand]
    private void Btn_Test1()
    {
        Message = "I am from DownloadViewModel Test1";
    }

    [RelayCommand]
    private void Btn_Test2()
    {
        Message = "I am from DownloadViewModel Test2";
    }
}