using System;

namespace PCL2.Neo;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class SubViewModelOfAttribute(Type parentViewModel) : Attribute
{
    public Type ParentViewModel { get; } = parentViewModel;
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class DefaultSubViewModelAttribute(Type subViewModel) : Attribute
{
    public Type SubViewModel { get; } = subViewModel;
}