using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PomodoroTimerApp.Managers
{
    public class UserActivityMonitor
    {
        private List<IActivityObserver> _observers = new List<IActivityObserver>();
        private static readonly TimeSpan WorkInactivityThreshold = TimeSpan.FromSeconds(15);
        private System.Timers.Timer _activityCheckTimer;

        public UserActivityMonitor()
        {

            // Controllo periodico dell'attività
            ScheduleActivityCheck();
        }


        private void ScheduleActivityCheck()
        {
            _activityCheckTimer = new System.Timers.Timer(1000); 
            _activityCheckTimer.Elapsed += (sender, e) => CheckActivity();
            _activityCheckTimer.AutoReset = true;
            _activityCheckTimer.Start();
        }

        public void AddObserver(IActivityObserver observer)
        {
            _observers.Add(observer);
        }

        public void RemoveObserver(IActivityObserver observer)
        {
            _observers.Remove(observer);
        }



        private TimeSpan GetInactivityDuration()
        {
            var lastInputInfo = new LASTINPUTINFO
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
        //public void UpdateActivity()
        //{
        //    TimeSpan _inactivityDuration = DateTime.Now - _lastActivityTime;
        //    bool wasInactive = _inactivityDuration > WorkInactivityThreshold;
        //    _lastActivityTime = DateTime.Now;

        //    if (wasInactive)
        //    {
        //        NotifyUserActive();
        //    }
        //}

        private void CheckActivity()
        {

            TimeSpan _inactivityDuration = GetInactivityDuration();

            if (_inactivityDuration > WorkInactivityThreshold)
            {
                NotifyUserInactive();
            }
            else
            {
                NotifyUserActive();
            }
        }

        private void NotifyUserActive()
        {
            foreach (var observer in _observers)
            {
                observer.OnUserActive();
            }
        }

        private void NotifyUserInactive()
        {
            foreach (var observer in _observers)
            {
                observer.OnUserInactive();
            }
        }

        #region Inattività tramite Win32 API

        [StructLayout(LayoutKind.Sequential)]
        private struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        [DllImport("user32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);
        #endregion
    }
}
