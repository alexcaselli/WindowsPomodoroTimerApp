using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PomodoroTimerApp.PomodoroTimer.States;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using PomodoroTimerApp.Helpers;

namespace PomodoroTimerApp.PomodoroTimer
{
    internal abstract class PomodoroTimer
    {
        protected State _state;
        protected double _timerDurationMinutes;
        protected Timer _timer;

        // Tempo
        protected DateTime _endTime;
        protected TimeSpan _remainingTime;

        // UI
        protected TextBlock _timerTextBlock;
        protected const string TimerFormat = @"mm\:ss";
        protected const string ZeroTime = "00:00";
        protected Button _primaryButton;
        protected Button _stopButton;
        protected const string ButtonContent_Resume = "Resume Timer";
        protected const string ButtonContent_Start = "Start Timer";
        protected DispatcherQueue _dispatcherQueue;

        //Windowing
        protected Window _mainWindow;
        protected WindowHelper _windowHelper;

        public PomodoroTimer(double timerDurationMinutes, Window mainWindow, TextBlock timerTextBlock, Button primaryButton, Button stopButton)
        {
            _timerDurationMinutes = timerDurationMinutes;
            _timerTextBlock = timerTextBlock;
            _primaryButton = primaryButton;
            _stopButton = stopButton;
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            _mainWindow = mainWindow;
            _windowHelper = new WindowHelper();

            // Timer (ogni 1 secondo)
            _timer = new Timer(1000);
            _timer.Elapsed += OnTimerElapsed;
            this.Init();
            this.ChangeState(new ReadyState(this));
        }
        public void ChangeState(State state)
        {
            _state = state;
        }

        public void Init() {

            _remainingTime = TimeSpan.FromMinutes(_timerDurationMinutes);
            _timerTextBlock.Text = _remainingTime.ToString(TimerFormat);
            _primaryButton.Content = ButtonContent_Start;
            _stopButton.IsEnabled = false;

        }

        public void ClickStart()
        {
            _timer.Start();
        }

        public void ClickStop(){
            _state.Stop();
        }
        public void ClickPause()
        {
            _state.Pause();
        }
        public void ClickResume()
        {
            _state.Resume();
        }
        public abstract void StartActivityTracker();
        protected void OnTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            _state.Elapsed(sender, e);
        }

        public abstract void Pause();
        public abstract void Resume();
        public abstract void Stop();
        public void Start()
        {
            _endTime = DateTime.Now.Add(_remainingTime);
            _timer.Start();
            _primaryButton.Content = "Pause Timer";
            _stopButton.IsEnabled = true;
            StartActivityTracker();
        }
        public abstract void Elapsed();



    }
}
