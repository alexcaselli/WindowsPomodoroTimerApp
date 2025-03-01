using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.UI.Xaml.Controls;
using PomodoroTimerApp.PomodoroTimer;



namespace PomodoroTimerApp.PomodoroTimer
{
    internal class ActivityTimer : PomodoroTimer
    {
        public ActivityTimer(double timerDurationMinutes, TextBlock timerTextBlock, Button primaryButton, Button stopButton) : base(timerDurationMinutes, timerTextBlock, primaryButton, stopButton) { }

        public override void ClickPause()
        {
            throw new NotImplementedException();
        }

        public override void ClickReset()
        {
            throw new NotImplementedException();
        }

        public override void ClickResume()
        {
            throw new NotImplementedException();
        }

        public override void ClickStop()
        {
            throw new NotImplementedException();
        }

        public override void Elapsed()
        {
            throw new NotImplementedException();
        }

        public override void Pause()
        {
            throw new NotImplementedException();
        }

        public override void Reset()
        {
            throw new NotImplementedException();
        }

        public override void Resume()
        {
            throw new NotImplementedException();
        }

        public override void Start()
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

        protected override void OnTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
