
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using Toll.Core.Shared.Collections;

namespace Ursa.Demo.ViewModels;

public partial class CircularBuffDemoViewModel:ViewModelBase
{
    public AvaloniaList<string>? BuffAvaloniaList { get; set; } = new();
    public ObservableCircularQueue<string>? BuffCircular { get; set; } = new(5);
    public ObservableCollection<string>? BuffCollect { get; set; } = new();

    [ObservableProperty] 
    private string? _inText;
    
    [ObservableProperty]
    private int insertCnt = 0;

    public void AppendBuff()
    {
        if (!string.IsNullOrEmpty(InText))
        {
            BuffAvaloniaList?.Add(InText);
            BuffCircular?.Add(InText);
            BuffCollect?.Add(InText);
            InsertCnt++;
        }   
    }

    public void ClearBuff()
    { 
        BuffAvaloniaList?.Clear();
        BuffCircular?.Clear();
        BuffCollect?.Clear();
        InsertCnt = 0;
    }
}