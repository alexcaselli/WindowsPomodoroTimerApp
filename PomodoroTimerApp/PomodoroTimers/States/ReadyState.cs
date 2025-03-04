using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace PomodoroTimerApp.PomodoroTimers.States
{
    internal class ReadyState : State
    {
        public ReadyState(PomodoroTimer pomodoroTimer) : base(pomodoroTimer)
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
            // Do nothing
        }
        public override void Start()
        {
            _timer.Start();
            _timer.ChangeState(new RunningState(_timer));
        }
        public override void Stop()
        {
            // Do nothing
        }
    }
}