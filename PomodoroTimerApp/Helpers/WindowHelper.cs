using System;
using System.Runtime.InteropServices;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using WinRT.Interop;

namespace PomodoroTimerApp.Helpers
{
    public class WindowHelper
    {
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_SHOWWINDOW = 0x0040;
        private const uint SWP_NOACTIVATE = 0x0010;
        private static readonly IntPtr HWND_TOP = IntPtr.Zero;
        private IOHelper IOHelper = new IOHelper(); 

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        public bool ShowWindowInForeground(Window window)
        {
            // Bring the window to the foreground... first get the window handle...
            var hwnd = WindowNative.GetWindowHandle(window);

            // Restore window if minimized... requires DLL import above
            ShowWindow(hwnd, 0x00000009);

            //// Activates the window and displays it in its current size and position.
            //ShowWindow(hwnd, 0x00000005);

            // And call SetForegroundWindow... requires DLL import above
            bool bVisible = IsWindowVisible(hwnd);

            IOHelper.PushLogLine(String.Format("DEBUG ------------- Window is visible: {0}", bVisible));

            bool bSetWindowPos = SetWindowPos(hwnd, HWND_TOP, 0, 0, 0, 0,
            SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);


            IOHelper.PushLogLine($"DEBUG ------------- Window position as been set: {bSetWindowPos}");

            try
            {
                // And call SetForegroundWindow... requires DLL import above

                bool bForeground = SetForegroundWindow(hwnd);

                return bForeground;
            }
            catch (Exception e)
            {
                IOHelper.PushLogLine(String.Format("DEBUG ------------- Exception: {0}", e.Message));
                IOHelper.WriteDebugLogs();
                return false;
            }


        }
        // Inside the LaunchAndBringToForegroundIfNeeded method
        public Window LaunchAndBringToForegroundIfNeeded(Window m_window)
        {
            bool bForeground = false;

            if (m_window == null)
            {
                IOHelper.PushLogLine("DEBUG ------------- Window is null");
                m_window = new MainWindow();
                m_window.Activate();

                // Additionally we show using our helper, since if activated via a toast, it doesn't
                // activate the window correctly
                //bForeground = ShowWindowInForeground(m_window);
            }
            else
            {
                IOHelper.PushLogLine("DEBUG ------------- Window is not null");
                bForeground = ShowWindowInForeground(m_window);
                IOHelper.PushLogLine($"DEBUG ------------- Window shown in foreground: {bForeground}");
            }

            // log bForeground
            IOHelper.WriteDebugLogs();


            return m_window;
        }

        /// <summary>
        /// Restituisce l'istanza corrente di AppWindow.
        /// </summary>
        private AppWindow GetAppWindow(Window m_window)
        {
            var hWnd = WindowNative.GetWindowHandle(m_window);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            return AppWindow.GetFromWindowId(windowId);
        }

        /// <summary>
        /// Mette la finestra in modalità fullscreen.
        /// </summary>
        public void EnterFullScreen(Window m_window)
        {
            AppWindow appWindow = GetAppWindow(m_window);
            appWindow.SetPresenter(AppWindowPresenterKind.FullScreen);
        }



    }
}
