
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using Toll.Core.Common;
using Toll.Core.Shared.Collections;

namespace Ursa.Demo.ViewModels;

public partial class CircularBuffDemoViewModel:ViewModelBase
{
    public CircularBuffDemoViewModel()
    {
        if (Bytes != null) Bytes.MaxCapacity = 10;
    }

    public AvaloniaList<string>? BuffAvaloniaList { get; set; } = new();
    public ObservableCircularQueue<TransferData>? Bytes { get; set; } = new(5);
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
            // BuffCircular?.Add(InText);
            BuffCollect?.Add(InText);
            InsertCnt++;
        }  
        Bytes.Add(new(){TrDirection = GenerateRandomDirection(),Data = genBytes()});
        
    }

    public void ClearBuff()
    { 
        BuffAvaloniaList?.Clear();
        Bytes?.Clear();
        BuffCollect?.Clear();
        InsertCnt = 0;
    }
    
    Random random = new Random();

    DataDirection GenerateRandomDirection()
    {
        int arrayLength = random.Next(0, 1);
        return (DataDirection)arrayLength;
    }
    byte[] genBytes()
    {
        int arrayLength = random.Next(1, 11);
        byte[] randomBytes = new byte[arrayLength];
        random.NextBytes(randomBytes);
        return randomBytes;
    }
}