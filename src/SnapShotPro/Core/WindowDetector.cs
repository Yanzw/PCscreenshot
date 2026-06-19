using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SnapShotPro.Core
{
    class WindowInfo
    {
        public IntPtr Handle;
        public Rectangle Bounds;
        public string Title;
    }

    static class WindowDetector
    {
        public static WindowInfo GetWindowAtPoint(Point screenPoint)
        {
            var pt = new NativeMethods.POINT { X = screenPoint.X, Y = screenPoint.Y };
            IntPtr hwnd = NativeMethods.WindowFromPoint(pt);
            if (hwnd == IntPtr.Zero)
                return null;

            // Walk up to root window
            IntPtr root = NativeMethods.GetAncestor(hwnd, NativeMethods.GA_ROOT);
            if (root != IntPtr.Zero)
                hwnd = root;

            // 用 DWM 真实边界（去掉隐形阴影），与 CaptureWindow 截取范围保持一致
            Rectangle bounds = ScreenCapture.GetWindowBounds(hwnd);
            if (bounds.Width <= 0 || bounds.Height <= 0)
                return null;

            var sb = new System.Text.StringBuilder(256);
            NativeMethods.GetWindowText(hwnd, sb, 256);

            return new WindowInfo
            {
                Handle = hwnd,
                Bounds = bounds,
                Title = sb.ToString()
            };
        }

        public static List<WindowInfo> GetVisibleWindows()
        {
            var list = new List<WindowInfo>();
            NativeMethods.EnumWindows((hwnd, lParam) =>
            {
                if (!NativeMethods.IsWindowVisible(hwnd))
                    return true;

                // 跳过工具窗口和无标题的系统外壳窗口（如桌面、托盘宿主）
                int exStyle = NativeMethods.GetWindowLong(hwnd, NativeMethods.GWL_EXSTYLE);
                if ((exStyle & NativeMethods.WS_EX_TOOLWINDOW) != 0)
                    return true;
                if (NativeMethods.IsIconic(hwnd))
                    return true;

                Rectangle bounds = ScreenCapture.GetWindowBounds(hwnd);
                if (bounds.Width <= 0 || bounds.Height <= 0)
                    return true;

                var sb = new System.Text.StringBuilder(256);
                NativeMethods.GetWindowText(hwnd, sb, 256);

                list.Add(new WindowInfo
                {
                    Handle = hwnd,
                    Bounds = bounds,
                    Title = sb.ToString()
                });
                return true;
            }, IntPtr.Zero);
            return list;
        }

        // 从缓存的窗口列表（Z 序，顶在前）中查找包含指定点的最上层窗口，
        // 排除指定句柄（用于排除截图遮罩自身），避免 WindowFromPoint 命中遮罩。
        public static WindowInfo FindWindowAt(Point screenPoint, List<WindowInfo> windows, IntPtr exclude)
        {
            foreach (var w in windows)
            {
                if (w.Handle == exclude) continue;
                if (w.Bounds.Contains(screenPoint))
                    return w;
            }
            return null;
        }
    }
}
