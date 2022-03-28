using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace UplayerWindowsDemo
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private IntPtr _intPtr;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            SourceInitialized += OnSourceInitialized;
        }
        private void OnSourceInitialized(object sender, EventArgs e)
        {
            var handle = (PresentationSource.FromVisual(this) as HwndSource).Handle;
            var exstyle = User32.GetWindowLong(handle, GWL_EXSTYLE);
            User32.SetWindowLong(handle, GWL_EXSTYLE, new IntPtr(exstyle.ToInt32() | WS_EX_NOACTIVATE));
        }
        public const int WS_EX_NOACTIVATE = 0x08000000;
        public const int GWL_EXSTYLE = -20;

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Interval = TimeSpan.FromSeconds(3);
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Start();
            _intPtr = (PresentationSource.FromVisual(this) as HwndSource).Handle;
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            var aboveWindows = AppWindows.GetAllBelowWindows(_intPtr);
            var windowInfos = aboveWindows.Where(I => I.IsVisible && !I.IsMinimized && I.Bounds.Width > 1 && I.Bounds.Height > 1
                                                      && I.Hwnd != _intPtr).ToList();
            //忽略系统应用窗口
            windowInfos = windowInfos.Where(i => !AppWindows.IsIgnoreSystemWindow(i.ClassName)).ToList();
            //忽略系统后台运行窗口
            windowInfos = windowInfos.Where(i =>
            {
                if (!AppWindows.IsSystemBackgroundWindow(i.ClassName))
                {
                    return true;
                }
                return !AppWindows.IsInvisibleSystemBackgroundWindow(i.Hwnd);
            }).ToList();
            OutputTextBlock.Text = string.Join(",",
                windowInfos.Select(i =>
                    $"{i.Title},{i.ClassName},IsMinimized:{i.IsMinimized},IsVisible:{i.IsVisible}\r\n"));
        }

    }
}
