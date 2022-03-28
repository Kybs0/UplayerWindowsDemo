using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace UplayerWindowsDemo
{
    /// <summary>
    /// 包含枚举当前用户空间下所有窗口的方法。
    /// </summary>
    public class AppWindows
    {
        public static string[] IgnoredWin10WindowClasses =
        {
            "Progman",
            "Shell_TrayWnd",
            "EdgeUiInputTopWndClass",
            "NativeHWNDHost",
            "VisualStudioGlowWindow",
            "WorkerW",
        };
        public static bool IsIgnoreSystemWindow(string className)
        {
            return IgnoredWin10WindowClasses.Any(i=> className.Contains(i));
        }
        public static string[] Win10BackgroundAppWindowClasses =
        {
            "ApplicationFrameWindow",
            "Windows.UI.Core.CoreWindow",
        };
        public static bool IsSystemBackgroundWindow(string className)
        {
            return Win10BackgroundAppWindowClasses.Contains(className);
        }

        public static bool IsInvisibleSystemBackgroundWindow(IntPtr hwnd)
        {
            bool value;
            var size = Marshal.SizeOf(typeof(bool));
            var ret = User32.DwmGetWindowAttribute(hwnd, User32.DWMWINDOWATTRIBUTE.Cloaked, out value, size);
            if (ret != 0)
            {
                value = false;
            }

            return value;
        }
        /// <summary>
        /// 查找当前用户空间下所有符合条件的窗口（仅查找顶层窗口）。如果不指定条件，将返回所有窗口。
        /// </summary>
        /// <param name="match">过滤窗口的条件。</param>
        /// <returns>找到的所有窗口信息。</returns>
        public static IReadOnlyList<WindowInfo> FindAll(Predicate<WindowInfo> match = null)
        {
            var windowList = new List<WindowInfo>();
            User32.EnumWindows(OnWindowEnum, 0);
            return match == null ? windowList : windowList.FindAll(match);

            bool OnWindowEnum(IntPtr hWnd, int lparam)
            {
                // 仅查找顶层窗口。
                if (User32.GetParent(hWnd) == IntPtr.Zero)
                {
                    var windowDetail = GetWindowDetail(hWnd);
                    // 添加到已找到的窗口列表。
                    windowList.Add(windowDetail);
                }

                return true;
            }
        }

        private static WindowInfo GetWindowDetail(IntPtr hWnd)
        {
            // 获取窗口类名。
            var lpString = new StringBuilder(512);
            User32.GetClassName(hWnd, lpString, lpString.Capacity);
            var className = lpString.ToString();

            // 获取窗口标题。
            var lptrString = new StringBuilder(512);
            User32.GetWindowText(hWnd, lptrString, lptrString.Capacity);
            var title = lptrString.ToString().Trim();

            // 获取窗口可见性。
            var isVisible = User32.IsWindowVisible(hWnd);

            // 获取窗口位置和尺寸。
            User32.LPRECT rect = default;
            User32.GetWindowRect(hWnd, ref rect);
            var bounds = new Rect(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);

            return new WindowInfo(hWnd, className, title, isVisible, bounds);
        }
        public static List<WindowInfo> GetAllAboveWindows(IntPtr hwnd)
        {
            var windowInfos = new List<WindowInfo>();
            var intPtr = User32.GetWindow(hwnd, 3);
            if (intPtr == IntPtr.Zero)
            {
                return windowInfos;
            }
            var windowDetail = GetWindowDetail(intPtr);
            windowInfos.AddRange(GetAllAboveWindows(intPtr));
            windowInfos.Add(windowDetail);
            return windowInfos;
        }
        public static List<WindowInfo> GetAllBelowWindows(IntPtr hwnd)
        {
            var windowInfos = new List<WindowInfo>();
            var intPtr = User32.GetWindow(hwnd, 2);
            if (intPtr == IntPtr.Zero)
            {
                return windowInfos;
            }
            var windowDetail = GetWindowDetail(intPtr);
            windowInfos.AddRange(GetAllBelowWindows(intPtr));
            windowInfos.Add(windowDetail);
            return windowInfos;
        }

    }
}
