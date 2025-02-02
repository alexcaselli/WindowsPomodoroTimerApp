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

// Aggiungi i namespace per la gestione del fullscreen
using WinRT.Interop;
using static Microsoft.UI.Win32Interop; // Modifica qui
using Microsoft.UI.Windowing;

namespace PomodoroTimerApp
{
    /// <summary>
    /// Una finestra che può essere usata da sola o navigata all’interno di un Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        // Timer per il countdown principale
        private Timer timer;
        // Timer per controllare l'inattività del cursore
        private Timer inactivityTimer;
        // Timer per aggiornare il cronometro di inattività
        private DispatcherTimer inactivityStopwatchTimer;

        // Timer per il break di 3 minuti
        private Timer breakTimer;

        private DateTime endTime;
        private TimeSpan remainingTime;

        // Variabili per il break
        private DateTime breakEndTime;
        private TimeSpan breakRemainingTime;

        private bool isPaused;               // Indica se il timer principale è stato messo in pausa manualmente
        private bool pausedByInactivity;     // Indica se il timer principale è stato messo in pausa per inattività
        private TimeSpan inactivityDuration; // Durata dell'inattività

        // Costanti di configurazione
        private const int WorkingTimerDurationMinutes = 1;
        private const int BreakTimerDurationMinutes = 1;
        private const int InactivityThresholdSeconds = 15;
        private const int BreakInactivityThresholdSeconds = 10;

        // Flag per indicare se il break timer è stato messo in pausa a causa dell'attività del cursore
        private bool breakPausedByActivity = false;
        private bool isBreakActive = false;

        public MainWindow()
        {
            this.InitializeComponent();
            remainingTime = TimeSpan.FromMinutes(WorkingTimerDurationMinutes);

            // Timer principale (ogni 1 secondo)
            timer = new Timer(1000);
            timer.Elapsed += Timer_Elapsed;
            timerTextBlock.Text = remainingTime.ToString(@"mm\:ss");

            // Timer per il controllo dell'inattività (ogni 1 secondo)
            inactivityTimer = new Timer(1000);
            inactivityTimer.Elapsed += InactivityTimer_Elapsed;
            inactivityTimer.Start();

            // Timer per aggiornare il cronometro di inattività (ogni 1 secondo)
            inactivityStopwatchTimer = new DispatcherTimer();
            inactivityStopwatchTimer.Interval = TimeSpan.FromSeconds(1);
            inactivityStopwatchTimer.Tick += InactivityStopwatchTimer_Tick;
        }

        private void myButton_Click(object sender, RoutedEventArgs e)
        {
            if (timer.Enabled)
            {
                // Se il timer principale è in esecuzione, distinguiamo tra pausa manuale e pausa per inattività.
                if (isPaused || pausedByInactivity)
                {
                    // Riprendi il timer solo se era stato messo in pausa manualmente.
                    // Se era in pausa per inattività, il ripristino avverrà automaticamente al movimento del cursore.
                    if (isPaused)
                    {
                        endTime = DateTime.Now.Add(remainingTime);
                        timer.Start();
                        isPaused = false;
                        myButton.Content = "Pause Timer";
                    }
                }
                else
                {
                    // Pausa manuale del timer principale
                    timer.Stop();
                    remainingTime = endTime - DateTime.Now;
                    isPaused = true;
                    myButton.Content = "Resume Timer";
                }
            }
            else
            {
                // Avvia il timer principale (se era fermo)
                endTime = DateTime.Now.Add(remainingTime);
                timer.Start();
                myButton.Content = "Pause Timer";
                stopButton.IsEnabled = true;
                inactivityTimer.Start();
            }
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            // Stop e reset del timer principale
            timer.Stop();
            remainingTime = TimeSpan.FromMinutes(WorkingTimerDurationMinutes);
            timerTextBlock.Text = remainingTime.ToString(@"mm\:ss");
            myButton.Content = "Start Timer";
            stopButton.IsEnabled = false;
            isPaused = false;
            pausedByInactivity = false;
            inactivityStopwatchTimer.Stop();

            // Se il break era in corso, fermalo
            if (breakTimer != null)
            {
                breakTimer.Stop();
            }
            // Esce dal fullscreen se attivo
            ExitFullScreen();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            remainingTime = endTime - DateTime.Now;
            if (remainingTime <= TimeSpan.Zero)
            {
                // Il timer principale è terminato: fermiamo il timer e avviamo il break
                timer.Stop();
                remainingTime = TimeSpan.Zero;
                DispatcherQueue.TryEnqueue(() =>
                {
                    timerTextBlock.Text = "00:00";
                    myButton.IsEnabled = false;
                    stopButton.IsEnabled = false;
                    inactivityDuration = TimeSpan.Zero;

                    // Porta l'app in primo piano (se non lo è) e avvia la sequenza per il break
                    HandleTimerCompletionAsync();
                });
                return;
            }

            DispatcherQueue.TryEnqueue(() =>
                timerTextBlock.Text = remainingTime.ToString(@"mm\:ss"));
        }

        /// <summary>
        /// Metodo asincrono che porta l'app in primo piano (se necessario), attende un breve ritardo, 
        /// entra in modalità fullscreen e avvia il break timer.
        /// </summary>
        private async void HandleTimerCompletionAsync()
        {
            EnsureAppInForeground();
            // Attendi 300ms per consentire al sistema di aggiornare lo stato della finestra
            await Task.Delay(300);
            EnterFullScreen();
            StartBreakTimer();
        }

        /// <summary>
        /// Avvia il break timer della durata di 3 minuti.
        /// </summary>
        private async void StartBreakTimer()
        {
            // Disabilita temporaneamente il controllo dell'inattività per il break
            isBreakActive = true; // Aggiungi una variabile bool isBreakActive a livello di classe


            breakRemainingTime = TimeSpan.FromMinutes(BreakTimerDurationMinutes);
            breakEndTime = DateTime.Now.Add(breakRemainingTime);
            breakTimer = new Timer(1000);
            breakTimer.Elapsed += BreakTimer_Elapsed;
            breakTimer.Start();

            await Task.Delay(3000); // Attendi 3 secondi per evitare che un movimento iniziale del cursore metta in pausa il break
            isBreakActive = false;
        }


        private void BreakTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // Se il break timer è in pausa per attività, non decrementiamo il tempo residuo
            if (breakPausedByActivity)
            {
                return;
            }

            breakRemainingTime = breakEndTime - DateTime.Now;
            if (breakRemainingTime <= TimeSpan.Zero)
            {
                breakTimer.Stop();
                breakRemainingTime = TimeSpan.Zero;
                DispatcherQueue.TryEnqueue(() =>
                {
                    timerTextBlock.Text = "Break over";
                    // Esce dalla modalità fullscreen
                    ExitFullScreen();
                    // Reset del timer principale e aggiornamento dell'interfaccia
                    remainingTime = TimeSpan.FromMinutes(WorkingTimerDurationMinutes);
                    timerTextBlock.Text = remainingTime.ToString(@"mm\:ss");
                    myButton.Content = "Start Timer";
                    myButton.IsEnabled = true;
                    stopButton.IsEnabled = false;

                    // Fai ripartire il timer principale
                    endTime = DateTime.Now.Add(remainingTime);
                    timer.Start();
                    myButton.Content = "Pause Timer";
                    stopButton.IsEnabled = true;
                    inactivityTimer.Start();
                });
            }
            else
            {
                DispatcherQueue.TryEnqueue(() =>
                    timerTextBlock.Text = breakRemainingTime.ToString(@"mm\:ss"));
            }
        }

        /// <summary>
        /// Verifica periodicamente l'inattività del sistema.
        /// Se il break timer è attivo, viene applicata la logica:
        ///    - Se il cursore viene mosso (idleTime inferiore a BreakInactivityThresholdSeconds), il break timer viene messo in pausa.
        ///    - Se il break timer era in pausa e il cursore non viene mosso per oltre la soglia, il break timer viene riavviato.
        /// Altrimenti, se il timer principale è attivo, viene applicata la logica esistente.
        /// </summary>
        private void InactivityTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var idleTime = GetIdleTime();


            // Se il break timer è in esecuzione oppure è stato messo in pausa per attività
            if (breakTimer != null && (breakTimer.Enabled || breakPausedByActivity))
            {
                // Se il break è appena iniziato e isBreakActive è true, ignora la logica di inattività
                if (isBreakActive)
                    return;

                if (idleTime < TimeSpan.FromSeconds(BreakInactivityThresholdSeconds))
                {
                    // Il cursore si sta muovendo: se il break non è già in pausa, mettilo in pausa
                    if (!breakPausedByActivity)
                    {
                        DispatcherQueue.TryEnqueue(() =>
                        {
                            breakTimer.Stop();
                            breakPausedByActivity = true;
                            inactivityDuration = TimeSpan.Zero;
                            inactivityStopwatchTimer.Start();
                            inactivityTimerTextBlock.Visibility = Visibility.Visible;

                            // (Opzionale: aggiornamento interfaccia, es. mostrare "Break paused")
                        });
                    }
                }
                else // idleTime >= BreakInactivityThresholdSeconds
                {
                    // Se il break era in pausa e il cursore è inattivo da oltre la soglia, riavvialo
                    if (breakPausedByActivity)
                    {
                        DispatcherQueue.TryEnqueue(() =>
                        {
                            // Aggiorna il breakEndTime per compensare il tempo in pausa
                            breakEndTime = DateTime.Now.Add(breakRemainingTime);
                            breakTimer.Start();
                            breakPausedByActivity = false;
                            inactivityStopwatchTimer.Stop();
                        });
                    }
                }
                return; // Esce: non eseguire la logica per il timer principale
            }

            // Logica per il timer principale (già esistente)
            if (idleTime >= TimeSpan.FromSeconds(InactivityThresholdSeconds) && timer.Enabled && !pausedByInactivity && !isPaused)
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    remainingTime = endTime - DateTime.Now;
                    timer.Stop();
                    pausedByInactivity = true;
                    inactivityDuration = TimeSpan.Zero;
                    inactivityStopwatchTimer.Start();
                    inactivityTimerTextBlock.Visibility = Visibility.Visible;
                    myButton.Content = "Resume Timer (inactivity)";
                });
            }
            else if (pausedByInactivity && idleTime < TimeSpan.FromSeconds(InactivityThresholdSeconds))
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    endTime = DateTime.Now.Add(remainingTime);
                    timer.Start();
                    pausedByInactivity = false;
                    inactivityStopwatchTimer.Stop();
                    //inactivityTimerTextBlock.Visibility = Visibility.Collapsed;
                    myButton.Content = "Pause Timer";
                });
            }
        }

        private void InactivityStopwatchTimer_Tick(object sender, object e)
        {
            inactivityDuration = inactivityDuration.Add(TimeSpan.FromSeconds(1));
            inactivityTimerTextBlock.Text = $"{inactivityDuration.ToString(@"mm\:ss")}";
        }

        #region Funzioni per la gestione del fullscreen

        /// <summary>
        /// Mette la finestra in modalità fullscreen.
        /// </summary>
        private void EnterFullScreen()
        {
            var hWnd = WindowNative.GetWindowHandle(this);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = AppWindow.GetFromWindowId(windowId);
            // Imposta il presenter a FullScreen
            appWindow.SetPresenter(AppWindowPresenterKind.FullScreen);
        }

        /// <summary>
        /// Esce dalla modalità fullscreen.
        /// </summary>
        private void ExitFullScreen()
        {
            var hWnd = WindowNative.GetWindowHandle(this);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = AppWindow.GetFromWindowId(windowId);
            // Imposta il presenter a Overlapped (finestra normale)
            appWindow.SetPresenter(AppWindowPresenterKind.Overlapped);
        }

        #endregion

        #region Funzioni per il rilevamento dell'inattività (Win32 API)

        [StructLayout(LayoutKind.Sequential)]
        struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        [DllImport("user32.dll")]
        static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        /// <summary>
        /// Restituisce il tempo di inattività (dall'ultimo input) come TimeSpan.
        /// </summary>
        private TimeSpan GetIdleTime()
        {
            LASTINPUTINFO lastInputInfo = new LASTINPUTINFO();
            lastInputInfo.cbSize = (uint)Marshal.SizeOf(typeof(LASTINPUTINFO));
            if (!GetLastInputInfo(ref lastInputInfo))
            {
                return TimeSpan.Zero;
            }
            uint idleTimeMillis = ((uint)Environment.TickCount - lastInputInfo.dwTime);
            return TimeSpan.FromMilliseconds(idleTimeMillis);
        }

        #endregion

        #region Funzioni per gestire il posizionamento in primo piano

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
        /// Verifica se la finestra dell'applicazione è in primo piano; se non lo è, la porta in primo piano.
        /// </summary>
        private void EnsureAppInForeground()
        {
            var hWnd = WindowNative.GetWindowHandle(this);
            IntPtr foregroundHwnd = GetForegroundWindow();

            // Se la nostra finestra non è in primo piano, la porta in primo piano
            if (hWnd != foregroundHwnd)
            {
                // Se la finestra è minimizzata, la ripristina
                if (IsIconic(hWnd))
                {
                    ShowWindow(hWnd, SW_RESTORE);
                }
                SetForegroundWindow(hWnd);
            }
        }

        #endregion
    }
}
