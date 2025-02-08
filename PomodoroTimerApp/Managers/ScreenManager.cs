using Microsoft.UI.Xaml;
using Microsoft.UI.Windowing;
using WinRT.Interop;
using static Microsoft.UI.Win32Interop;

namespace PomodoroTimerApp.Managers
{
    /// <summary>
    /// Gestisce il passaggio in e dallla modalità fullscreen.
    /// </summary>
    public class ScreenManager
    {
        private readonly Window _window;

        public ScreenManager(Window window)
        {
            _window = window;
        }

        private AppWindow GetAppWindow()
        {
            var hWnd = WindowNative.GetWindowHandle(_window);
            var windowId = GetWindowIdFromWindow(hWnd);
            return AppWindow.GetFromWindowId(windowId);
        }

        public void EnterFullScreen()
        {
            AppWindow appWindow = GetAppWindow();
            appWindow.SetPresenter(AppWindowPresenterKind.FullScreen);
        }

        public void ExitFullScreen()
        {
            AppWindow appWindow = GetAppWindow();
            appWindow.SetPresenter(AppWindowPresenterKind.Overlapped);
        }
    }
}
