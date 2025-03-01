using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace PomodoroTimerApp.PomodoroTimer
{
    internal abstract class PomodoroTimer
    {
        private State _state;
        private double _timerDurationMinutes;
        private Timer _timer;

        // Tempo
        private DateTime _endTime;
        private TimeSpan _remainingTime;

        // UI
        private TextBlock _timerTextBlock;
        private const string _timerFormat = @"mm\:ss";
        private Button _primaryButton;
        private Button _stopButton; 

        public PomodoroTimer(double timerDurationMinutes, TextBlock timerTextBlock, Button primaryButton, Button stopButton)
        {
            _timerDurationMinutes = timerDurationMinutes;
            _state = new ReadyState();
            _timerTextBlock = timerTextBlock;
            _primaryButton = primaryButton;
            _stopButton = stopButton;
        }

        public void Init() {
            _remainingTime = TimeSpan.FromMinutes(_timerDurationMinutes);
            _timerTextBlock.Text = _remainingTime.ToString(_timerFormat);

            // Timer principale (ogni 1 secondo)
            _timer = new Timer(1000);
            _timer.Elapsed += OnTimerElapsed;
        }
        public void ClickStart()
        {
            _endTime = DateTime.Now.Add(_remainingTime);
            _timer.Start();
            _primaryButton.Content = "Pause Timer";
            _stopButton.IsEnabled = true;
            StartActivityTracker();
        }
        public void ChangeState(State state)
        {
            _state = state;
        }

        protected abstract void OnTimerElapsed(object? sender, ElapsedEventArgs e);

        public abstract void ClickStop();
        public abstract void ClickPause();
        public abstract void ClickResume();
        public abstract void ClickReset();
        public abstract void StartActivityTracker();

        public abstract void Pause();
        public abstract void Reset();
        public abstract void Resume();
        public abstract void Stop();
        public abstract void Start();



    }
}
