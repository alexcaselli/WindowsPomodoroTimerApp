using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace PomodoroTimerApp.PomodoroTimers.States
{
    internal class RunningState : State
    {
        public RunningState(PomodoroTimer pomodoroTimer) : base(pomodoroTimer)
        {
        }

        public override void Completed()
        {
            _timer.ChangeState(new ReadyState(_timer));
            _timer.HandleTimerCompletion();
            
        }


        public override void StartPauseResume()
        {
            _timer.Pause();
            _timer.ChangeState(new PausedState(_timer));
        }
        public override void Stop()
        {
            _timer.Stop();
            _timer.ChangeState(new ReadyState(_timer));
        }
    }
}
