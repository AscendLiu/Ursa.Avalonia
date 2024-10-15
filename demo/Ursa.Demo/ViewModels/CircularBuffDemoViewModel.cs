
using System.Collections.ObjectModel;
using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using Toll.Core.Shared.Collections;

namespace Ursa.Demo.ViewModels;

public partial class CircularBuffDemoViewModel:ViewModelBase
{
    public AvaloniaList<string>? BuffAvaloniaList { get; set; } = new();
    public ObservableCircularBuff<string>? BuffCircular { get; set; } = new(10);
    public ObservableCollection<string>? BuffCollect { get; set; } = new();

    [ObservableProperty] 
    private string? _inText;

    public void AppendBuff()
    {
        if (!string.IsNullOrEmpty(InText))
        {
            BuffAvaloniaList?.Add(InText);
            BuffCircular?.Append(InText);
            BuffCollect?.Add(InText);
        }   
    }

    public void ClearBuff()
    { 
        BuffAvaloniaList?.Clear();
        BuffCircular?.Clear();
        BuffCollect?.Clear();
    }
}