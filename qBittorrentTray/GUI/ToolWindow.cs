using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace qBittorrentTray.GUI
{
    public class ToolWindow : Window
    {
        // Prep stuff needed to remove close button on window
        private const int GWL_STYLE = -16;

        private const int WS_SYSMENU = 0x80000;

        public ToolWindow()
        {
            Loaded += ToolWindow_Loaded;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private void ToolWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Code to remove close box from window
            var hwnd = new WindowInteropHelper(this).Handle;
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
        }
    }
}