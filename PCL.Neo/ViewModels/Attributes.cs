using System;

namespace PCL.Neo.ViewModels
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class SubViewModelOfAttribute : Attribute
    {
        public Type MainViewModelType { get; }

        public SubViewModelOfAttribute(Type mainViewModelType)
        {
            MainViewModelType = mainViewModelType;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class DefaultSubViewModelAttribute : Attribute
    {
        public Type SubViewModel { get; }

        public DefaultSubViewModelAttribute(Type subViewModel)
        {
            SubViewModel = subViewModel;
        }
    }
} 