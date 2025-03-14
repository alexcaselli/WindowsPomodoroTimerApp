using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PomodoroTimerApp.PomodoroTimers;

namespace PomodoroTimerApp.Managers
{
    internal class UserActivityPomodoroTimerManager : IActivityObserver
    {
        private PomodoroTimer _currentTimer;
        private bool _pausedDueToInactivity = false;

        public UserActivityPomodoroTimerManager(PomodoroTimer currentTimer)
        {
            _currentTimer = currentTimer;

            // Registrazione come observer
            var monitor = new UserActivityMonitor();
            monitor.AddObserver(this);
        }

        public void OnUserActive()
        {
            // Se il timer era stato messo in pausa per inattività, riavvialo
            if (_pausedDueToInactivity)
            {
                _currentTimer.ClickActivityResume();
                _pausedDueToInactivity = false;
            }
        }

        public void OnUserInactive()
        {
            // Metti in pausa il timer e segna che è stato fatto per inattività
            bool paused = _currentTimer.ClickActivityPause();
            _pausedDueToInactivity = paused;
        }
    }
}
