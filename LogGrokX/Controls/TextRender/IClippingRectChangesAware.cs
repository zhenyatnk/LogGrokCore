using System.Windows;

namespace LogGrokX.Controls.TextRender;

public interface IClippingRectChangesAware
{
    void OnChildRectChanged(Rect rect);
}