using System;
using System.Runtime.InteropServices;
using System.Timers;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace PomodoroTimerApp.Managers
{
    /// <summary>
    /// Controlla periodicamente l'inattività (utilizzando le API Win32) e solleva eventi
    /// per indicare il superamento o il ripristino delle soglie di inattività.
    /// </summary>
    public class InactivityManager
    {
        private readonly Timer _checkTimer;
        private readonly DispatcherTimer _stopwatchTimer;
        private TimeSpan _inactivityDuration = TimeSpan.Zero;

        public TimeSpan WorkingInactivityThreshold { get; }
        public TimeSpan BreakInactivityThreshold { get; }

        // Eventi per il timer di lavoro
        public event EventHandler WorkingInactivityDetected;
        public event EventHandler WorkingActivityDetected;

        // Eventi per il break
        public event EventHandler BreakActivityDetected;
        public event EventHandler BreakInactivityCleared;

        public InactivityManager(TimeSpan workingThreshold, TimeSpan breakThreshold)
        {
            WorkingInactivityThreshold = workingThreshold;
            BreakInactivityThreshold = breakThreshold;

            _checkTimer = new Timer(1000);
            _checkTimer.Elapsed += CheckTimerElapsed;
            _checkTimer.Start();

            _stopwatchTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _stopwatchTimer.Tick += (s, e) => _inactivityDuration = _inactivityDuration.Add(TimeSpan.FromSeconds(1));
        }

        private void CheckTimerElapsed(object sender, ElapsedEventArgs e)
        {
            TimeSpan idleTime = GetIdleTime();

            // Logica per il timer di lavoro:
            if (idleTime >= WorkingInactivityThreshold)
            {
                WorkingInactivityDetected?.Invoke(this, EventArgs.Empty);
                if (!_stopwatchTimer.IsEnabled)
                    _stopwatchTimer.Start();
            }
            else
            {
                if (_stopwatchTimer != null && _stopwatchTimer.IsEnabled)
                {
                    _stopwatchTimer.Stop();
                    _inactivityDuration = TimeSpan.Zero;
                    WorkingActivityDetected?.Invoke(this, EventArgs.Empty);
                }
            }

            // Logica per il break:
            if (idleTime < BreakInactivityThreshold)
            {
                BreakActivityDetected?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                BreakInactivityCleared?.Invoke(this, EventArgs.Empty);
            }
        }

        #region Win32 API per il rilevamento dell'inattività

        [StructLayout(LayoutKind.Sequential)]
        private struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        [DllImport("user32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        private TimeSpan GetIdleTime()
        {
            LASTINPUTINFO lastInputInfo = new LASTINPUTINFO
            {
                cbSize = (uint)Marshal.SizeOf(typeof(LASTINPUTINFO))
            };

            if (!GetLastInputInfo(ref lastInputInfo))
            {
                return TimeSpan.Zero;
            }
            uint idleTimeMillis = ((uint)Environment.TickCount - lastInputInfo.dwTime);
            return TimeSpan.FromMilliseconds(idleTimeMillis);
        }

        #endregion
    }
}
