using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PomodoroTimerApp.PomodoroTimer
{
    internal class RunningState : State
    {
        public RunningState(PomodoroTimer pomodoroTimer) : base(pomodoroTimer)
        {
        }
        public override void Pause()
        {
            _timer.Pause();
            _timer.ChangeState(new PausedState(_timer));
        }
        public override void Reset()
        {
            // Do nothing
        }
        public override void Resume()
        {
            // Do nothing
        }
        public override void Start()
        {
            // Do nothing
        }
        public override void Stop()
        {
            _timer.Stop();
            _timer.ChangeState(new StoppedState(_timer));
        }
    }
    {
    }
}
