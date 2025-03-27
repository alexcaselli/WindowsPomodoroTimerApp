using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics;

// Namespace per la gestione del fullscreen
using WinRT.Interop;
using static Microsoft.UI.Win32Interop;
using Microsoft.UI.Windowing;

// Per le notifiche toast
using Microsoft.Toolkit.Uwp.Notifications; // Richiede il pacchetto NuGet Microsoft.Toolkit.Uwp.Notifications
using Windows.UI.Notifications;

using PomodoroTimerApp.Helpers;
using Microsoft.Windows.AppNotifications.Builder;
using PomodoroTimerApp.PomodoroTimers;
using PomodoroTimerApp.PomodoroTimers.Events;
using System.ComponentModel.Design;
using PomodoroTimerApp.Managers;
using System.IO;
using Microsoft.UI.Composition.SystemBackdrops;
using Windows.ApplicationModel.VoiceCommands;
using Windows.Storage;


namespace PomodoroTimerApp
{
    /// <summary>
    /// Una finestra che pu� essere usata da sola o navigata all�interno di un Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        #region Campi e costanti

        // Costanti di configurazione
        private const double Fallback_WorkingTimerDurationMinutes = 25;
        private const double Fallback_BreakTimerDurationMinutes = 3;

        private const string WorkingTimerDurationMinutes_CompositeKey = "WorkingTimerDurationMinutes";
        private const string BreakTimerDurationMinutes_CompositeKey = "BreakTimerDurationMinutes";
        private const string Timers_CompositeKey = "TimersDurationMinutes";

        private double WorkingTimerDurationMinutes;
        private double BreakTimerDurationMinutes;
        private SizeInt32 WindowSize = new SizeInt32(720, 560);


        private WindowHelper _windowHelper;
        private ApplicationDataContainer _localConfig;

        private PomodoroTimer _currentTimer;
        private UserActivityPomodoroTimerManager _userActivityPomodoroTimerManager;



        #endregion

        public MainWindow()
        {
            this.InitializeComponent();
            _localConfig = ApplicationData.Current.LocalSettings;
            ReadAndSetLocalConfigs();

            InitializeWorkTimer();
            _windowHelper = new WindowHelper();
            //this.AppWindow.SetIcon("Assets/Square44x44Logo.targetsize-32.png");
            AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets\\Square44x44Logo.targetsize-32.png"));
            AppWindow.Resize(WindowSize);
            //this.AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
        }

        private void ReadAndSetLocalConfigs()
        {
            try
            {
                ApplicationDataCompositeValue composite = (ApplicationDataCompositeValue)_localConfig.Values[Timers_CompositeKey];
                if (composite != null)
                {
                    this.WorkingTimerDurationMinutes = (double)composite[WorkingTimerDurationMinutes_CompositeKey];
                    this.BreakTimerDurationMinutes = (double)composite[BreakTimerDurationMinutes_CompositeKey];
                }
                else
                {
                    this.WorkingTimerDurationMinutes = Fallback_WorkingTimerDurationMinutes;
                    this.BreakTimerDurationMinutes = Fallback_BreakTimerDurationMinutes;
                }
            }
            catch (Exception ex)
            {
                // Handle the exception as needed, for example, log the error or show a message to the user
                debugTextBlock.Text = "Error reading timer configurations: " + ex.Message;
            }
        }

        private void WriteAndSetLocalConfigs(double New_WorkingTimerDurationMinutes, double New_BreakTimerDurationMinutes)
        {
            try
            {
                ApplicationDataCompositeValue composite = new ApplicationDataCompositeValue();

                composite[WorkingTimerDurationMinutes_CompositeKey] = New_WorkingTimerDurationMinutes;
                composite[BreakTimerDurationMinutes_CompositeKey] = New_BreakTimerDurationMinutes;
                _localConfig.Values[Timers_CompositeKey] = composite;
            }
            catch (Exception ex)
            {
                // Handle the exception as needed, for example, log the error or show a message to the user
                debugTextBlock.Text = "Error saving timer configurations: " + ex.Message;
            }
        }

        private void InitializeWorkTimer()
        {
            // Inizializza i timer e lo stato iniziale.
            _currentTimer = new WorkTimer(WorkingTimerDurationMinutes, timerTextBlock, primaryButton, stopButton);
            _userActivityPomodoroTimerManager = new UserActivityWorkTimerManager(_currentTimer, inactivityStopwatchTextBlock);
            _currentTimer.TimerCompleted += OnTimerElapsed;
            
        }
        private void InitializeBreakTimer()
        {
            // Inizializza i timer e lo stato iniziale.
            _currentTimer = new BreakTimer(BreakTimerDurationMinutes, timerTextBlock, primaryButton, stopButton);
            _userActivityPomodoroTimerManager = new UserActivityBreakTimerManager(_currentTimer, inactivityStopwatchTextBlock);
            _currentTimer.TimerCompleted += OnTimerElapsed;

        }

        private void StopAndResetTimer()
        {
            _currentTimer.ClickStop();
            _userActivityPomodoroTimerManager.stopActivityStopwatch();
            inactivityStopwatchTextBlock.Visibility = Visibility.Collapsed;
        }

        private void PrimaryButton_Click(object sender, RoutedEventArgs e)
        {
            _currentTimer.ClickStartPauseResume();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            _currentTimer.ClickStop();
            inactivityStopwatchTextBlock.Visibility = Visibility.Collapsed;
            // Esce dalla modalit� fullscreen se attivo
            _windowHelper.ExitFullScreen(this);
        }
        private async void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            settingsContentDialog.XamlRoot = MainPage.XamlRoot;

            ContentDialogResult result = await settingsContentDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                StopAndResetTimer();
                this.WorkingTimerDurationMinutes = NumberBoxWorkTimerDuration.Value;
                this.BreakTimerDurationMinutes = NumberBoxBreakTimerDuration.Value;

                WriteAndSetLocalConfigs(NumberBoxWorkTimerDuration.Value, NumberBoxBreakTimerDuration.Value);

                if (_currentTimer is WorkTimer)
                {
                    InitializeWorkTimer();
                }
                else if (_currentTimer is BreakTimer)
                {
                    InitializeBreakTimer();
                }
            }
            else
            {
                // User pressed Cancel, ESC, or the back arrow.
                // Terms of use were not accepted.
            }
        }
        private void SettingsContentDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            // Ensure that the check box is unchecked each time the dialog opens.
            //ConfirmAgeCheckBox.IsChecked = false;
            NumberBoxWorkTimerDuration.Value = WorkingTimerDurationMinutes;
            NumberBoxBreakTimerDuration.Value = BreakTimerDurationMinutes;
        }


        private void SelectorBarTimer_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
        {
            SelectorBarItem selectedItem = sender.SelectedItem;
            int currentSelectedIndex = sender.Items.IndexOf(selectedItem);

            switch (currentSelectedIndex)
            {
                case 0:
                    if (_currentTimer is WorkTimer)
                    {
                        // Do Nothing
                    }
                    else if (_currentTimer is BreakTimer)
                    {
                        StopAndResetTimer();
                        InitializeWorkTimer();
                    }
                    break;
                case 1:
                    if (_currentTimer is WorkTimer)
                    {
                        StopAndResetTimer();
                        InitializeBreakTimer();
                    }
                    else if (_currentTimer is BreakTimer)
                    {
                        // Do Nothing
                    }
                    break;
                default:
                    // Do Nothing
                    break;
            }


        }

        private void OnTimerElapsed(object? sender, TimerCompletedEventArgs e)
        {
            if (e.TimerType == "Work")
            {
                // Visualizza una notifica toast per avvisare l'utente che il timer � scaduto.
                ShowToastNotification();

                Window _mainWindow = _windowHelper.LaunchAndBringToForegroundIfNeeded(this);
                _windowHelper.EnterFullScreen(_mainWindow);

                SelectorBarItemBreakTimer.IsSelected = true;

                // Avvia il timer di pausa appropriato
                StartPomodoroTimer("Break");
            }
            else if (e.TimerType == "Break")
            {
                // Dopo una pausa, avvia un nuovo timer di lavoro
                _windowHelper.ExitFullScreen(this);

                SelectorBarItemWorkTimer.IsSelected = true;

                StartPomodoroTimer("Work");
            }
        }
        private void StartPomodoroTimer(String TimerType)
        {
            // Disconnetti l'evento dal timer precedente se esiste
            if (_currentTimer != null)
            {
                _currentTimer.TimerCompleted -= OnTimerElapsed;
            }

            // Crea un nuovo timer di lavoro
            switch (TimerType)
            {
                case "Work":
                    _currentTimer = new WorkTimer(WorkingTimerDurationMinutes, timerTextBlock, primaryButton, stopButton);
                    _userActivityPomodoroTimerManager = new UserActivityWorkTimerManager(_currentTimer, inactivityStopwatchTextBlock);
                    break;
                case "Break":
                    _currentTimer = new BreakTimer(BreakTimerDurationMinutes, timerTextBlock, primaryButton, stopButton);
                    _userActivityPomodoroTimerManager = new UserActivityBreakTimerManager(_currentTimer, inactivityStopwatchTextBlock);
                    break;
                default:
                    throw new ArgumentException("Invalid timer type");
            }

            // Sottoscrivi all'evento TimerCompleted
            _currentTimer.TimerCompleted += OnTimerElapsed;

            //// Configura i pulsanti per controllare il timer
            //ConfigureButtonsForTimer();

            // Avvia il timer
            _currentTimer.ClickStartPauseResume();
        }


        #region Notifiche Toast 
        private void ShowToastNotification()
        {

            //debugTextBlock.Visibility = Visibility.Visible;
            //debugTextBlock.Text = "Check app permission for notifications";

            try
            {
                //debugTextBlock.Text = debugTextBlock.Text + ", Notification Status: " + ToastNotificationManager.CreateToastNotifier().Setting;
                //// Ensure the app has permission to send toast notifications
                //ToastNotificationHistoryCompat historyCompat = ToastNotificationManagerCompat.History;
                //debugTextBlock.Text = debugTextBlock.Text + ", Got history";
                //if ( ToastNotificationManagerCompat.History.GetHistory().Count > 0)
                //{
                //    debugTextBlock.Text = debugTextBlock.Text + ", There is history";
                //    ToastNotificationManagerCompat.History.Clear();
                //    debugTextBlock.Text = debugTextBlock.Text + ", History Cleared";
                //}
                

                // Crea il contenuto della notifica toast, includendo un parametro di attivazione.
                var content = new ToastContentBuilder()
                    .AddArgument("action", "openWindow")
                    .SetToastScenario(ToastScenario.Reminder)
                    .AddText("Pomodoro Timer")
                    .AddText("Il timer di lavoro � scaduto! Clicca qui per avviare il break.")
                    .GetToastContent();

                //debugTextBlock.Text = debugTextBlock.Text + ", Toast Content Built";

                // Crea la notifica e inviala
                var toast = new ToastNotification(content.GetXml());

                ToastNotificationManager.CreateToastNotifier().Show(toast);

                //debugTextBlock.Text = debugTextBlock.Text + ", Toast Sent";
            }
            catch (Exception ex)
            {
                debugTextBlock.Text = debugTextBlock.Text + ", Toast Error: " + ex.Message;
            }
        }

        #endregion
    }
}
