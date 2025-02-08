using System;
using System.Timers;

namespace PomodoroTimerApp.Managers
{
    /// <summary>
    /// Gestisce il timer di lavoro e il timer di break.
    /// Solleva eventi per notificare aggiornamenti e completamenti.
    /// </summary>
    public class TimerManager
    {
        private Timer _workingTimer;
        private Timer _breakTimer;

        // Configurazione e stato del timer di lavoro
        public TimeSpan WorkingDuration { get; }
        public TimeSpan RemainingWorkingTime { get; private set; }
        private DateTime _workingEndTime;
        public bool IsWorkingRunning { get; private set; }

        // Configurazione e stato del timer di break
        public TimeSpan BreakDuration { get; }
        public TimeSpan RemainingBreakTime { get; private set; }
        private DateTime _breakEndTime;
        public bool IsBreakRunning { get; private set; }

        // Eventi per il timer di lavoro
        public event EventHandler<TimeSpan> WorkingTick;
        public event EventHandler WorkingCompleted;

        // Eventi per il timer di break
        public event EventHandler<TimeSpan> BreakTick;
        public event EventHandler BreakCompleted;

        public TimerManager(TimeSpan workingDuration, TimeSpan breakDuration)
        {
            WorkingDuration = workingDuration;
            BreakDuration = breakDuration;
            RemainingWorkingTime = workingDuration;

            _workingTimer = new Timer(1000);
            _workingTimer.Elapsed += WorkingTimerElapsed;
        }

        #region Timer di Lavoro

        public void StartWorkingTimer()
        {
            _workingEndTime = DateTime.Now.Add(RemainingWorkingTime);
            IsWorkingRunning = true;
            _workingTimer.Start();
        }

        public void PauseWorkingTimer()
        {
            if (IsWorkingRunning)
            {
                _workingTimer.Stop();
                RemainingWorkingTime = _workingEndTime - DateTime.Now;
                IsWorkingRunning = false;
            }
        }

        public void ResumeWorkingTimer()
        {
            if (!IsWorkingRunning)
            {
                _workingEndTime = DateTime.Now.Add(RemainingWorkingTime);
                IsWorkingRunning = true;
                _workingTimer.Start();
            }
        }

        public void StopWorkingTimer()
        {
            _workingTimer.Stop();
            IsWorkingRunning = false;
            RemainingWorkingTime = WorkingDuration;
        }

        private void WorkingTimerElapsed(object sender, ElapsedEventArgs e)
        {
            RemainingWorkingTime = _workingEndTime - DateTime.Now;
            if (RemainingWorkingTime <= TimeSpan.Zero)
            {
                _workingTimer.Stop();
                RemainingWorkingTime = TimeSpan.Zero;
                WorkingCompleted?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                WorkingTick?.Invoke(this, RemainingWorkingTime);
            }
        }

        #endregion

        #region Timer di Break

        public void StartBreakTimer()
        {
            _breakTimer = new Timer(1000);
            _breakTimer.Elapsed += BreakTimerElapsed;
            RemainingBreakTime = BreakDuration;
            _breakEndTime = DateTime.Now.Add(RemainingBreakTime);
            IsBreakRunning = true;
            _breakTimer.Start();
        }

        public void PauseBreakTimer()
        {
            if (IsBreakRunning)
            {
                _breakTimer.Stop();
                RemainingBreakTime = _breakEndTime - DateTime.Now;
                IsBreakRunning = false;
            }
        }

        public void ResumeBreakTimer()
        {
            if (!IsBreakRunning)
            {
                _breakEndTime = DateTime.Now.Add(RemainingBreakTime);
                IsBreakRunning = true;
                _breakTimer.Start();
            }
        }

        private void BreakTimerElapsed(object sender, ElapsedEventArgs e)
        {
            RemainingBreakTime = _breakEndTime - DateTime.Now;
            if (RemainingBreakTime <= TimeSpan.Zero)
            {
                _breakTimer.Stop();
                RemainingBreakTime = TimeSpan.Zero;
                BreakCompleted?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                BreakTick?.Invoke(this, RemainingBreakTime);
            }
        }

        #endregion
    }
}
