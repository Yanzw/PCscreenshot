using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SnapShotPro.Core
{
    static class ScreenCapture
    {
        [DllImport("gdi32.dll")]
        static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int w, int h,
            IntPtr hdcSrc, int xSrc, int ySrc, int rop);

        [DllImport("user32.dll")]
        static extern bool PrintWindow(IntPtr hwnd, IntPtr hdcBlt, uint nFlags);

        [DllImport("user32.dll")]
        static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        const int SRCCOPY = 0x00CC0020;
        const uint PW_CLIENTONLY        = 0x1;
        const uint PW_RENDERFULLCONTENT = 0x2;  // Win8.1+: 渲染 GPU/DirectComposition 内容

        public static Bitmap CaptureRegion(Rectangle region)
        {
            var bmp = new Bitmap(region.Width, region.Height, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bmp))
            {
                IntPtr hdcDest = g.GetHdc();
                IntPtr hdcSrc = NativeMethods.CreateDC("DISPLAY", null, null, IntPtr.Zero);
                try
                {
                    BitBlt(hdcDest, 0, 0, region.Width, region.Height,
                           hdcSrc, region.X, region.Y, SRCCOPY);
                }
                finally
                {
                    NativeMethods.DeleteDC(hdcSrc);
                    g.ReleaseHdc(hdcDest);
                }
            }
            return bmp;
        }

        public static Bitmap CaptureFullScreen()
        {
            var bounds = GetVirtualScreenBounds();
            return CaptureRegion(bounds);
        }

        public static Bitmap CaptureWindow(IntPtr hwnd)
        {
            // 用 DWM 真实边界，去掉 Win10/11 的隐形阴影边框；失败则退回 GetWindowRect
            Rectangle region = GetWindowBounds(hwnd);
            if (region.Width <= 0 || region.Height <= 0)
                return null;

            // 方案一：PrintWindow + PW_RENDERFULLCONTENT（Win8.1+ 支持 GPU 渲染窗口）
            var bmp = new Bitmap(region.Width, region.Height, PixelFormat.Format32bppArgb);
            bool printed = false;
            using (var g = Graphics.FromImage(bmp))
            {
                IntPtr hdcDest = g.GetHdc();
                printed = PrintWindow(hwnd, hdcDest, PW_RENDERFULLCONTENT);
                if (!printed)
                    printed = PrintWindow(hwnd, hdcDest, 0); // 老系统退回旧标志
                g.ReleaseHdc(hdcDest);
            }

            // 方案二：PrintWindow 失败或结果全黑（DWM 合成窗口）时，从屏幕直接抓取
            if (!printed || IsMostlyBlack(bmp))
            {
                bmp.Dispose();
                BringToForeground(hwnd);
                bmp = CaptureRegion(region);
            }

            return bmp;
        }

        // DWM 扩展边界：返回不含隐形阴影的真实可见矩形
        public static Rectangle GetWindowBounds(IntPtr hwnd)
        {
            NativeMethods.RECT rc;
            int hr = NativeMethods.DwmGetWindowAttribute(
                hwnd, NativeMethods.DWMWA_EXTENDED_FRAME_BOUNDS,
                out rc, Marshal.SizeOf(typeof(NativeMethods.RECT)));

            if (hr != 0) // DWM 不可用（如 Win7 经典模式），退回普通边界
                NativeMethods.GetWindowRect(hwnd, out rc);

            return new Rectangle(rc.Left, rc.Top, rc.Right - rc.Left, rc.Bottom - rc.Top);
        }

        static void BringToForeground(IntPtr hwnd)
        {
            // 屏幕抓取前确保目标窗口可见且置顶，避免被遮挡
            if (NativeMethods.IsIconic(hwnd))
                NativeMethods.ShowWindow(hwnd, NativeMethods.SW_RESTORE);
            NativeMethods.SetForegroundWindow(hwnd);
            System.Threading.Thread.Sleep(200); // 等待窗口完成绘制
        }

        // 采样若干像素判断位图是否近乎全黑（PrintWindow 黑屏检测）
        static bool IsMostlyBlack(Bitmap bmp)
        {
            int blackCount = 0, total = 0;
            int stepX = Math.Max(1, bmp.Width  / 20);
            int stepY = Math.Max(1, bmp.Height / 20);
            for (int y = 0; y < bmp.Height; y += stepY)
            {
                for (int x = 0; x < bmp.Width; x += stepX)
                {
                    Color c = bmp.GetPixel(x, y);
                    if (c.R < 8 && c.G < 8 && c.B < 8) blackCount++;
                    total++;
                }
            }
            return total > 0 && blackCount >= total * 0.98;
        }

        public static Rectangle GetVirtualScreenBounds()
        {
            int left = int.MaxValue, top = int.MaxValue;
            int right = int.MinValue, bottom = int.MinValue;
            foreach (Screen s in Screen.AllScreens)
            {
                left   = Math.Min(left,   s.Bounds.Left);
                top    = Math.Min(top,    s.Bounds.Top);
                right  = Math.Max(right,  s.Bounds.Right);
                bottom = Math.Max(bottom, s.Bounds.Bottom);
            }
            return new Rectangle(left, top, right - left, bottom - top);
        }
    }

    static class NativeMethods
    {
        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateDC(string lpszDriver, string lpszDevice,
            string lpszOutput, IntPtr lpInitData);

        [DllImport("gdi32.dll")]
        public static extern bool DeleteDC(IntPtr hdc);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder text, int maxLength);

        [DllImport("user32.dll")]
        public static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr WindowFromPoint(POINT pt);

        [DllImport("user32.dll")]
        public static extern IntPtr GetAncestor(IntPtr hwnd, uint gaFlags);

        [DllImport("dwmapi.dll")]
        public static extern int DwmGetWindowAttribute(IntPtr hwnd, int attr,
            out RECT pvAttribute, int cbAttribute);

        [DllImport("user32.dll")]
        public static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        public const int  GWL_EXSTYLE        = -20;
        public const int  WS_EX_TOOLWINDOW   = 0x00000080;
        public const int DWMWA_EXTENDED_FRAME_BOUNDS = 9;
        public const int SW_RESTORE = 9;

        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left, Top, Right, Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X, Y;
        }

        public const uint GA_ROOT = 2;
    }
}
