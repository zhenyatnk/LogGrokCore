using System;

namespace LogGrokX.Controls
{
    public class SetFocusRequest
    {
        public void Invoke()
        {
            SetFocus?.Invoke();
        }

        public event Action? SetFocus;
    }
}