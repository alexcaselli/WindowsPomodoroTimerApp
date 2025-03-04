using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PomodoroTimerApp.PomodoroTimer;



namespace PomodoroTimerApp.PomodoroTimers
{
    internal class ActivityTimer : PomodoroTimer
    {
        public ActivityTimer(double timerDurationMinutes, Window mainWindow, TextBlock timerTextBlock, Button primaryButton, Button stopButton) : base(timerDurationMinutes, mainWindow, timerTextBlock, primaryButton, stopButton)
        {
        }

        public override void Elapsed()
        {
            throw new NotImplementedException();
        }

        public override void Pause()
        {
            throw new NotImplementedException();
        }


        public override void Resume()
        {
            throw new NotImplementedException();
        }


        public override void StartActivityTracker()
        {
            throw new NotImplementedException();
        }

        public override void Stop()
        {
            throw new NotImplementedException();
        }

    }
}
