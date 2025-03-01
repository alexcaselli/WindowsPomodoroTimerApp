using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace PomodoroTimerApp.PomodoroTimer.States
{
    internal class PausedState : State
    {
        public PausedState(PomodoroTimer pomodoroTimer) : base(pomodoroTimer)
        {
        }

        public override void Elapsed(object? sender, ElapsedEventArgs e)
        {
            // Do nothing
        }

        public override void Pause()
        {
            // Do nothing
        }

        public override void Resume()
        {
            _timer.Resume();
            _timer.ChangeState(new RunningState(_timer));
        }

        public override void Start()
        {
            // Do nothing
        }

        public override void Stop()
        {
            _timer.Stop();
            _timer.ChangeState(new ReadyState(_timer));
        }
    }
}
