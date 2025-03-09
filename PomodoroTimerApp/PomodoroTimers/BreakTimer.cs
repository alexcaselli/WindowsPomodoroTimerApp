using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace PomodoroTimerApp.PomodoroTimers
{
    internal class BreakTimer : PomodoroTimer
    {
        public BreakTimer(double timerDurationMinutes, TextBlock timerTextBlock, Button primaryButton, Button stopButton) : base(timerDurationMinutes, timerTextBlock, primaryButton, stopButton)
        {
        }


        public override void HandleTimerCompletion()
        {
            throw new NotImplementedException();
        }

        public override void StartActivityTracker()
        {
            throw new NotImplementedException();
        }

    }
}
