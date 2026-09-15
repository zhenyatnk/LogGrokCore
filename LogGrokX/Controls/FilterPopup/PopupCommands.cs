using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace LogGrokX.Controls.FilterPopup
{
    public static class PopupCommands
    {
        public static ICommand Close  = new DelegateCommand(
            popup => ((Popup)popup).IsOpen = false,
            popup => (popup as Popup)?.IsOpen is true);
    }
}
