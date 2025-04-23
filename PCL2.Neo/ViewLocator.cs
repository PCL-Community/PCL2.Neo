using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using PCL2.Neo.ViewModels;

namespace PCL2.Neo;

public class ViewLocator : IDataTemplate
{
    public Control? Build(object? param)
    {
        if (param is null)
            return null;

        var name = param.GetType().FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);
        var type = Type.GetType(name);

        if (type != null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }

        return new TextBlock { Text = "Not Found: " + name };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}

public class LeftViewLocator : IDataTemplate
{
    private static string ReplaceLastOccurrence(string text, string oldValue, string newValue)
    {
        int place = text.LastIndexOf(oldValue, StringComparison.Ordinal);
        return place == -1
            ? text
            : text.Remove(place, oldValue.Length).Insert(place, newValue);
    }

    public Control? Build(object? param)
    {
        if (param is null)
            return null;

        var name = param.GetType().FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);
        name = ReplaceLastOccurrence(name, "View", "LeftView");
        var type = Type.GetType(name);

        if (type != null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }

        // return new TextBlock { Text = "Not Found: " + name };
        return null;
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}