using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Toll.Core.Common;

namespace Ursa.Controls.Register;

public class ByteTextBlock : Control
{
    private string? _text = string.Empty;
    
    static ByteTextBlock()
    {
        
    }
    
    public ByteTextBlock()
    {
        
    }
    
    public static readonly StyledProperty<TransferData?> DataProperty =
        AvaloniaProperty.Register<ByteTextBlock, TransferData?>(nameof(Data), defaultBindingMode: BindingMode.OneTime);
    
    public TransferData? Data
    {
        get => GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }
    
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == DataProperty)
        {
            RefreshByteText();
            Console.WriteLine(_text);
        }
    }

    private void RefreshByteText()
    {
        _text = DateTime.Now.ToString();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        return new Size(200, 1000);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        return base.ArrangeOverride(finalSize);
    }

    public override void Render(DrawingContext context)
    {
        context.DrawRectangle(Brush.Parse("#457815"),new Pen(Brushes.Gray, 1),new Rect(0,0,100,100));
        context.DrawLine(new Pen(Brushes.Brown,2), new Point(0,0), new Point(100,100));
    }
    
}