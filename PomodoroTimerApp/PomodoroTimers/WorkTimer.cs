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
using PomodoroTimerApp.PomodoroTimers.Events;
using Windows.UI.Notifications;

namespace PomodoroTimerApp.PomodoroTimers
{
    internal class WorkTimer : PomodoroTimer
    {
        public WorkTimer(double timerDurationMinutes, TextBlock timerTextBlock, Button primaryButton, Button stopButton) : base(timerDurationMinutes, timerTextBlock, primaryButton, stopButton)
        {
        }

        //public void Start()
        //{
        //    base.Start();
        //    // Anything more
        //}

        public override void StartActivityTracker()
        {
            throw new NotImplementedException();
        }


        public override void HandleTimerCompletion()
        {
            // Invoke the TimerCompleted event from the base class
            var args = new TimerCompletedEventArgs { TimerType = "Work" };
            OnTimerCompleted(args);
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

