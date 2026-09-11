using System;

namespace LogGrokCore
{
    public class TimelinePlacementService
    {
        private readonly ApplicationSettings _applicationSettings;

        public TimelinePlacementService(ApplicationSettings applicationSettings)
        {
            _applicationSettings = applicationSettings;
            IsAtTop = applicationSettings.ViewSettings.TimelineAtTop;
        }

        public bool IsAtTop { get; private set; }

        public event Action? Changed;

        public void SetAtTop(bool isAtTop)
        {
            if (IsAtTop == isAtTop)
                return;

            IsAtTop = isAtTop;
            _applicationSettings.SetTimelineAtTop(isAtTop);
            Changed?.Invoke();
        }
    }
}
