using System.Windows;
using System.Windows.Controls;

namespace LogGrokX.Controls.FilterPopup
{
    public class ResizeablePopupContent : ContentControl
    {        
        static ResizeablePopupContent()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ResizeablePopupContent), 
                new FrameworkPropertyMetadata(typeof(ResizeablePopupContent)));
        }
    }
}
