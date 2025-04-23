using CommunityToolkit.Mvvm.ComponentModel;

namespace PCL2.Neo.ViewModels;

public class ViewModelBase : ObservableObject
{
    public virtual bool IsPaneVisible => false;
}