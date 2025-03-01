using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PomodoroTimerApp.Helpers;
using Windows.UI.Notifications;

namespace PomodoroTimerApp.PomodoroTimer
{
    internal class WorkTimer : PomodoroTimer
    {
        public WorkTimer(double timerDurationMinutes, Window mainWindow, TextBlock timerTextBlock, Button primaryButton, Button stopButton) : base(timerDurationMinutes, mainWindow, timerTextBlock, primaryButton, stopButton)
        {
        }

        public override void Elapsed()
        {
            _remainingTime = _endTime - DateTime.Now;
            if (_remainingTime <= TimeSpan.Zero)
            {
                _timer.Stop();
                _remainingTime = TimeSpan.Zero;
                _dispatcherQueue.TryEnqueue(() =>
                {
                    _timerTextBlock.Text = ZeroTime;
                    _primaryButton.IsEnabled = false;
                    _stopButton.IsEnabled = false;
                    // Gestione della fine del timer e avvio del break
                    HandleWorkTimerCompletion();
                });
                return;
            }

            // Aggiornamento dell’interfaccia
            _dispatcherQueue.TryEnqueue(() =>
                _timerTextBlock.Text = _remainingTime.ToString(TimerFormat));
        }

        public override void Pause()
        {
            _timer.Stop();
            _remainingTime = _endTime - DateTime.Now;
            _primaryButton.Content = ButtonContent_Resume;
        }

        public override void Resume()
        {
            this.Start();
        }

        public void Start()
        {
            base.Start();
            // Anything more
        }

        public override void StartActivityTracker()
        {
            throw new NotImplementedException();
        }

        public override void Stop()
        {
            _timer.Stop();
            this.Init();
        }

        private async void HandleWorkTimerCompletion()
        {

            // Visualizza una notifica toast per avvisare l'utente che il timer è scaduto.
            ShowToastNotification();

            _mainWindow = _windowHelper.LaunchAndBringToForegroundIfNeeded(_mainWindow);
            _windowHelper.EnterFullScreen(_mainWindow);

            StartBreakTimer();
        }
        #region Notifiche Toast

        /// <summary>
        /// Mostra una notifica toast per informare l'utente che il timer è scaduto.
        /// L'argomento "action=toastClick" verrà passato all'attivazione della notifica.
        /// </summary>
        private void ShowToastNotification()
        {
            try
            {

                // Crea il contenuto della notifica toast, includendo un parametro di attivazione.
                var content = new ToastContentBuilder()
                    .AddArgument("action", "openWindow")
                    .SetToastScenario(ToastScenario.Reminder)
                    .AddText("Pomodoro Timer")
                    .AddText("Il timer di lavoro è scaduto! Clicca qui per avviare il break.")
                    .GetToastContent();


                // Crea la notifica e inviala
                var toast = new ToastNotification(content.GetXml());

                ToastNotificationManager.CreateToastNotifier().Show(toast);

            }
            catch (Exception ex)
            {
                Debug.WriteLine("DEBUG ------------- Toast Error: " + ex.Message);
            }
        }

        #endregion
    }

}

