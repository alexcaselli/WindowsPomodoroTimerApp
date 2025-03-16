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


namespace PomodoroTimerApp
{
    /// <summary>
    /// Una finestra che può essere usata da sola o navigata all’interno di un Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        #region Campi e costanti

        // Timers
        private Timer _workingTimer;
        private Timer _inactivityTimer;
        private Timer _breakTimer;
        private DispatcherTimer _inactivityStopwatchTimer;

        // Tempo
        private DateTime _endTime;
        private TimeSpan _remainingTime;

        // Timer per il break
        private DateTime _breakEndTime;
        private TimeSpan _breakRemainingTime;

        // Stato del timer
        private bool _isPaused;               // Pausa manuale del timer principale
        private bool _pausedByInactivity;     // Pausa a causa dell’inattività
        private TimeSpan _inactivityDuration; // Durata dell’inattività

        // Timer break: gestione pausa per attività
        private bool _breakPausedByActivity = false;
        private bool _isBreakActive = false;

        // Costanti di configurazione
        private const double WorkingTimerDurationMinutes = 1;
        private const double BreakTimerDurationMinutes = 1;
        private const int InactivityThresholdSeconds = 15;
        private const int BreakInactivityThresholdSeconds = 10;

        private WindowHelper _windowHelper;

        private PomodoroTimer _currentTimer;
        private UserActivityPomodoroTimerManager _userActivityPomodoroTimerManager;


        #endregion

        public MainWindow()
        {
            this.InitializeComponent();
            //InitializeTimers(); ----------------------------------- UNCOMMENT THIS LINE
            CoordinateTimers();
            _windowHelper = new WindowHelper();
            this.AppWindow.SetIcon("Assets/Square44x44Logo.targetsize-32.png");
            //this.AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
        }

        private void CoordinateTimers()
        {
            // Inizializza i timer e lo stato iniziale.
            _currentTimer = new WorkTimer(WorkingTimerDurationMinutes, timerTextBlock, primaryButton, stopButton);
            _userActivityPomodoroTimerManager = new UserActivityWorkTimerManager(_currentTimer, inactivityTimerTextBlock);
            _currentTimer.TimerCompleted += OnTimerElapsed;
            
        }

        private void OnTimerElapsed(object? sender, TimerCompletedEventArgs e)
        {
            if (e.TimerType == "Work")
            {
                // Visualizza una notifica toast per avvisare l'utente che il timer è scaduto.
                ShowToastNotification();

                Window _mainWindow = _windowHelper.LaunchAndBringToForegroundIfNeeded(this);
                _windowHelper.EnterFullScreen(_mainWindow);

                // Avvia il timer di pausa appropriato
                StartPomodoroTimer("Break");
            }
            else if (e.TimerType == "Break")
            {
                // Dopo una pausa, avvia un nuovo timer di lavoro
                _windowHelper.ExitFullScreen(this);
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
                    _userActivityPomodoroTimerManager = new UserActivityWorkTimerManager(_currentTimer, inactivityTimerTextBlock);
                    break;
                case "Break":
                    _currentTimer = new BreakTimer(BreakTimerDurationMinutes, timerTextBlock, primaryButton, stopButton);
                    _userActivityPomodoroTimerManager = new UserActivityBreakTimerManager(_currentTimer, inactivityTimerTextBlock);
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


        /// <summary>
        /// Inizializza i timer e lo stato iniziale.
        /// </summary>
        private void InitializeTimers()
        {
            _remainingTime = TimeSpan.FromMinutes(WorkingTimerDurationMinutes);
            timerTextBlock.Text = _remainingTime.ToString(@"mm\:ss");

            // Timer principale (ogni 1 secondo)
            _workingTimer = new Timer(1000);
            _workingTimer.Elapsed += OnWorkingTimerElapsed;

            // Timer per il controllo dell’inattività (ogni 1 secondo)
            _inactivityTimer = new Timer(1000);
            _inactivityTimer.Elapsed += OnInactivityTimerElapsed;
            _inactivityTimer.Start();

            // Timer per aggiornare il cronometro di inattività (ogni 1 secondo)
            _inactivityStopwatchTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _inactivityStopwatchTimer.Tick += OnInactivityStopwatchTimerTick;
        }

        #region Eventi UI

        private void PrimaryButton_Click(object sender, RoutedEventArgs e)
        {
            _currentTimer.ClickStartPauseResume();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            _currentTimer.Stop();
            // Esce dalla modalità fullscreen se attivo
            _windowHelper.ExitFullScreen(this);
        }

        #endregion

        #region Timer Principale

        private void OnWorkingTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            _remainingTime = _endTime - DateTime.Now;
            if (_remainingTime <= TimeSpan.Zero)
            {
                _workingTimer.Stop();
                _remainingTime = TimeSpan.Zero;
                DispatcherQueue.TryEnqueue(() =>
                {
                    timerTextBlock.Text = "00:00";
                    primaryButton.IsEnabled = false;
                    stopButton.IsEnabled = false;
                    _inactivityDuration = TimeSpan.Zero;
                    // Gestione della fine del timer e avvio del break
                    HandleWorkingTimerCompletionAsync();
                });
                return;
            }

            // Aggiornamento dell’interfaccia
            DispatcherQueue.TryEnqueue(() =>
                timerTextBlock.Text = _remainingTime.ToString(@"mm\:ss"));
        }

        /// <summary>
        /// Metodo asincrono che visualizza una notifica toast per richiamare l’attenzione dell’utente,
        /// attende un breve ritardo ed avvia il break timer.
        /// </summary>
        private async void HandleWorkingTimerCompletionAsync()
        {

            // Visualizza una notifica toast per avvisare l'utente che il timer è scaduto.
            ShowToastNotification();

            EnsureAppInForeground();
            _windowHelper.LaunchAndBringToForegroundIfNeeded(this);

            // Attende un breve ritardo per permettere l'aggiornamento dell'interfaccia
            await Task.Delay(300);

            // Entra in modalità fullscreen se l'utente clicca la notifica (vedi gestione attivazione)
            // Oppure, si può impostare la logica per entrare in fullscreen direttamente
            _windowHelper.EnterFullScreen(this);

            StartBreakTimer();
        }

        /// <summary>
        /// Ferma il timer principale, resetta il tempo e aggiorna l'interfaccia.
        /// </summary>
        private void StopWorkingTimerAndReset()
        {
            _workingTimer.Stop();
            _remainingTime = TimeSpan.FromMinutes(WorkingTimerDurationMinutes);
            timerTextBlock.Text = _remainingTime.ToString(@"mm\:ss");
            primaryButton.Content = "Start Timer";
            stopButton.IsEnabled = false;
            _isPaused = false;
            _pausedByInactivity = false;
            _inactivityStopwatchTimer.Stop();
        }

        #endregion

        #region Break Timer

        /// <summary>
        /// Avvia il break timer della durata di 3 minuti.
        /// </summary>
        private async void StartBreakTimer()
        {
            _isBreakActive = true; // Disabilita temporaneamente la logica di inattività per il break

            _breakRemainingTime = TimeSpan.FromMinutes(BreakTimerDurationMinutes);
            _breakEndTime = DateTime.Now.Add(_breakRemainingTime);
            _breakTimer = new Timer(1000);
            _breakTimer.Elapsed += OnBreakTimerElapsed;
            _breakTimer.Start();

            // Attende 3 secondi per evitare che un movimento iniziale del cursore metta in pausa il break
            await Task.Delay(3000);
            _isBreakActive = false;
        }

        private void OnBreakTimerElapsed(object sender, ElapsedEventArgs e)
        {
            // Se il break è in pausa per attività, non decrementare il tempo residuo
            if (_breakPausedByActivity)
            {
                return;
            }

            _breakRemainingTime = _breakEndTime - DateTime.Now;
            if (_breakRemainingTime <= TimeSpan.Zero)
            {
                _breakTimer.Stop();
                _breakRemainingTime = TimeSpan.Zero;
                DispatcherQueue.TryEnqueue(() =>
                {
                    timerTextBlock.Text = "Break over";
                    ExitFullScreen();
                    // Reset del timer principale e aggiornamento dell'interfaccia
                    _remainingTime = TimeSpan.FromMinutes(WorkingTimerDurationMinutes);
                    timerTextBlock.Text = _remainingTime.ToString(@"mm\:ss");
                    primaryButton.Content = "Start Timer";
                    primaryButton.IsEnabled = true;
                    stopButton.IsEnabled = false;

                    // Riavvia il timer principale
                    _endTime = DateTime.Now.Add(_remainingTime);
                    _workingTimer.Start();
                    primaryButton.Content = "Pause Timer";
                    stopButton.IsEnabled = true;
                    _inactivityTimer.Start();
                    // Non è necessario riaggiungere l'handler del Tick del cronometro d'inattività.
                });
            }
            else
            {
                DispatcherQueue.TryEnqueue(() =>
                    timerTextBlock.Text = _breakRemainingTime.ToString(@"mm\:ss"));
            }
        }

        #endregion

        #region Inattività

        /// <summary>
        /// Verifica periodicamente l'inattività e gestisce sia il timer principale che il break in base all'attività del cursore.
        /// </summary>
        private void OnInactivityTimerElapsed(object sender, ElapsedEventArgs e)
        {
            TimeSpan idleTime = GetIdleTime();

            // Gestione del break timer
            if (_breakTimer != null && (_breakTimer.Enabled || _breakPausedByActivity))
            {
                // Se il break è appena iniziato, ignora la logica di inattività
                if (_isBreakActive)
                    return;

                if (idleTime < TimeSpan.FromSeconds(BreakInactivityThresholdSeconds))
                {
                    // Se il cursore si muove, metti in pausa il break (se non già in pausa)
                    if (!_breakPausedByActivity)
                    {
                        DispatcherQueue.TryEnqueue(() =>
                        {
                            _breakTimer.Stop();
                            _breakPausedByActivity = true;
                            _inactivityDuration = TimeSpan.Zero;
                            _inactivityStopwatchTimer.Start();
                            inactivityTimerTextBlock.Visibility = Visibility.Visible;
                            // (Opzionale: aggiornare l'interfaccia, es. mostrare "Break paused")
                        });
                    }
                }
                else // idleTime >= BreakInactivityThresholdSeconds
                {
                    // Se il break era in pausa ed ora il cursore è inattivo, riavvia il break
                    if (_breakPausedByActivity)
                    {
                        DispatcherQueue.TryEnqueue(() =>
                        {
                            // Aggiorna il breakEndTime per compensare il tempo in pausa
                            _breakEndTime = DateTime.Now.Add(_breakRemainingTime);
                            _breakTimer.Start();
                            _breakPausedByActivity = false;
                            _inactivityStopwatchTimer.Stop();
                        });
                    }
                }
                return; // Esce dalla gestione dell'inattività per il timer principale
            }

            // Gestione del timer principale
            if (idleTime >= TimeSpan.FromSeconds(InactivityThresholdSeconds) &&
                _workingTimer.Enabled && !_pausedByInactivity && !_isPaused)
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    _remainingTime = _endTime - DateTime.Now;
                    _workingTimer.Stop();
                    _pausedByInactivity = true;
                    _inactivityDuration = TimeSpan.Zero;
                    _inactivityStopwatchTimer.Start();
                    inactivityTimerTextBlock.Visibility = Visibility.Visible;
                    primaryButton.Content = "Resume Timer (inactivity)";
                });
            }
            else if (_pausedByInactivity && idleTime < TimeSpan.FromSeconds(InactivityThresholdSeconds))
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    _endTime = DateTime.Now.Add(_remainingTime);
                    _workingTimer.Start();
                    _pausedByInactivity = false;
                    _inactivityStopwatchTimer.Stop();
                    primaryButton.Content = "Pause Timer";
                });
            }
        }

        private void OnInactivityStopwatchTimerTick(object sender, object e)
        {
            _inactivityDuration = _inactivityDuration.Add(TimeSpan.FromSeconds(1));
            inactivityTimerTextBlock.Text = _inactivityDuration.ToString(@"mm\:ss");
        }


        #endregion

        #region Fullscreen e AppWindow

        /// <summary>
        /// Restituisce l'istanza corrente di AppWindow.
        /// </summary>
        private AppWindow GetAppWindow()
        {
            var hWnd = WindowNative.GetWindowHandle(this);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            return AppWindow.GetFromWindowId(windowId);
        }

        /// <summary>
        /// Mette la finestra in modalità fullscreen.
        /// </summary>
        private void EnterFullScreen()
        {
            AppWindow appWindow = GetAppWindow();
            appWindow.SetPresenter(AppWindowPresenterKind.FullScreen);
        }

        /// <summary>
        /// Esce dalla modalità fullscreen.
        /// </summary>
        private void ExitFullScreen()
        {
            AppWindow appWindow = GetAppWindow();
            appWindow.SetPresenter(AppWindowPresenterKind.Overlapped);
        }

        #endregion

        #region Inattività tramite Win32 API

        [StructLayout(LayoutKind.Sequential)]
        private struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        [DllImport("user32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        /// <summary>
        /// Restituisce il tempo di inattività (dall'ultimo input) come TimeSpan.
        /// </summary>
        private TimeSpan GetIdleTime()
        {
            var lastInputInfo = new LASTINPUTINFO
            {
                cbSize = (uint)Marshal.SizeOf(typeof(LASTINPUTINFO))
            };

            if (!GetLastInputInfo(ref lastInputInfo))
            {
                return TimeSpan.Zero;
            }

            uint idleTimeMillis = ((uint)Environment.TickCount - lastInputInfo.dwTime);
            return TimeSpan.FromMilliseconds(idleTimeMillis);
        }

        #endregion

        #region Gestione della finestra in primo piano

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_RESTORE = 9;

        /// <summary>
        /// Porta l'app in primo piano se non lo è già.
        /// </summary>
        private void EnsureAppInForeground()
        {
            IntPtr hWnd = WindowNative.GetWindowHandle(this);
            IntPtr foregroundHwnd = GetForegroundWindow();

            if (hWnd != foregroundHwnd)
            {
                if (IsIconic(hWnd))
                {
                    ShowWindow(hWnd, SW_RESTORE);
                }
                SetForegroundWindow(hWnd);
            }
        }

        #endregion

        #region Notifiche Toast

        /// <summary>
        /// Mostra una notifica toast per informare l'utente che il timer è scaduto.
        /// L'argomento "action=toastClick" verrà passato all'attivazione della notifica.
        /// </summary>
        private void ShowToastNotification()
        {

            debugTextBlock.Visibility = Visibility.Visible;
            debugTextBlock.Text = "Check app permission for notifications";

            try
            {
                debugTextBlock.Text = debugTextBlock.Text + ", Notification Status: " + ToastNotificationManager.CreateToastNotifier().Setting;
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
                    .AddText("Il timer di lavoro è scaduto! Clicca qui per avviare il break.")
                    .GetToastContent();

                debugTextBlock.Text = debugTextBlock.Text + ", Toast Content Built";

                // Crea la notifica e inviala
                var toast = new ToastNotification(content.GetXml());

                ToastNotificationManager.CreateToastNotifier().Show(toast);

                debugTextBlock.Text = debugTextBlock.Text + ", Toast Sent";
            }
            catch (Exception ex)
            {
                debugTextBlock.Text = debugTextBlock.Text + ", Toast Error: " + ex.Message;
            }
        }

        #endregion
    }
}
