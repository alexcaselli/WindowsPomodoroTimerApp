using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PomodoroTimerApp.PomodoroTimer
{
    abstract class State
    {
        protected PomodoroTimer _timer;

        public State (PomodoroTimer pomodoroTimer)
        {
            _timer = pomodoroTimer;
        }
        public abstract void Start();
        public abstract void Stop();
        public abstract void Pause();
        public abstract void Resume();
        public abstract void Reset();
    }
}
