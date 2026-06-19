using System;
using System.Drawing;
using System.Windows.Forms;
using SnapShotPro.Core;
using SnapShotPro.Utils;

namespace SnapShotPro.UI
{
    class TrayApp : IDisposable
    {
        readonly NotifyIcon    _tray;
        readonly HotkeyManager _hotkeys;
        readonly Settings      _settings;
        readonly SelectionOverlay _overlay;

        // 缓存三个截图菜单项，以便快捷键变更后刷新其显示标签
        MenuItem _miRegion, _miWindow, _miFullscreen;

        public TrayApp()
        {
            _settings = Settings.Load();
            _overlay  = new SelectionOverlay();

            _overlay.RegionSelected += OnCaptureReady;
            _overlay.WindowSelected += OnCaptureReady;

            _tray = new NotifyIcon
            {
                Text    = "SnapShot Pro",
                Icon    = BuildIcon(),
                Visible = true,
                ContextMenu = BuildMenu()
            };
            _tray.DoubleClick += (s, e) => StartRegion();

            _hotkeys = new HotkeyManager();
            RegisterHotkeys(showBalloon: true);
        }

        // 按当前设置注册三个全局热键；可在设置变更后重复调用
        void RegisterHotkeys(bool showBalloon)
        {
            _hotkeys.UnregisterAll();

            bool a = _hotkeys.Register(_settings.HotkeyRegion,     StartRegion);
            bool w = _hotkeys.Register(_settings.HotkeyWindow,     StartWindow);
            bool f = _hotkeys.Register(_settings.HotkeyFullscreen, StartFullscreen);

            RefreshMenuLabels();

            if (!a || !w || !f)
                _tray.ShowBalloonTip(3000, "SnapShot Pro",
                    "部分热键注册失败，可能与其他软件冲突，请在设置中更换。", ToolTipIcon.Warning);
            else if (showBalloon)
                _tray.ShowBalloonTip(2000, "SnapShot Pro",
                    "已启动\n" + _settings.HotkeyRegion.ToDisplayString() + " 区域  "
                    + _settings.HotkeyWindow.ToDisplayString() + " 窗口  "
                    + _settings.HotkeyFullscreen.ToDisplayString() + " 全屏", ToolTipIcon.Info);
        }

        // ── capture triggers ─────────────────────────────────────

        void StartRegion()
        {
            _overlay.StartCapture(OverlayMode.Region);
        }

        void StartWindow()
        {
            _overlay.StartCapture(OverlayMode.Window);
        }

        void StartFullscreen()
        {
            Bitmap bmp = ScreenCapture.CaptureFullScreen();
            OnCaptureReady(bmp);
        }

        void OnCaptureReady(Bitmap bmp)
        {
            if (bmp == null) return;

            if (_settings.AutoCopy)
                ClipboardHelper.CopyImage(bmp);

            var editor = new EditorForm(_settings);
            editor.LoadCapture(bmp);
            editor.FormClosed += (s, e) => bmp.Dispose();
            editor.Show();
        }

        // ── tray menu ────────────────────────────────────────────

        ContextMenu BuildMenu()
        {
            var menu = new ContextMenu();

            _miRegion     = menu.MenuItems.Add("区域截图", (s, e) => StartRegion());
            _miWindow     = menu.MenuItems.Add("窗口截图", (s, e) => StartWindow());
            _miFullscreen = menu.MenuItems.Add("全屏截图", (s, e) => StartFullscreen());
            menu.MenuItems.Add("-");
            menu.MenuItems.Add("延时 2 秒", (s, e) => StartDelayed(2));
            menu.MenuItems.Add("延时 5 秒", (s, e) => StartDelayed(5));
            menu.MenuItems.Add("-");
            menu.MenuItems.Add("快捷键设置...", (s, e) => OpenSettings());
            menu.MenuItems.Add("-");
            menu.MenuItems.Add("退出", (s, e) =>
            {
                _tray.Visible = false;
                Application.Exit();
            });

            RefreshMenuLabels();
            return menu;
        }

        // 把菜单项标签更新为当前快捷键，如 "区域截图  (Alt+A)"
        void RefreshMenuLabels()
        {
            if (_miRegion == null) return;
            _miRegion.Text     = "区域截图  (" + _settings.HotkeyRegion.ToDisplayString() + ")";
            _miWindow.Text     = "窗口截图  (" + _settings.HotkeyWindow.ToDisplayString() + ")";
            _miFullscreen.Text = "全屏截图  (" + _settings.HotkeyFullscreen.ToDisplayString() + ")";
        }

        void OpenSettings()
        {
            using (var dlg = new SettingsForm(_settings))
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    // 设置已保存，按新快捷键重新注册
                    RegisterHotkeys(showBalloon: false);
                    _tray.ShowBalloonTip(2000, "SnapShot Pro",
                        "快捷键已更新。", ToolTipIcon.Info);
                }
            }
        }

        void StartDelayed(int seconds)
        {
            var timer = new Timer { Interval = seconds * 1000 };
            int remaining = seconds;

            // countdown balloon
            _tray.ShowBalloonTip(seconds * 1000, "延时截图",
                remaining + " 秒后截图...", ToolTipIcon.Info);

            timer.Tick += (s, e) =>
            {
                timer.Stop();
                timer.Dispose();
                StartRegion();
            };
            timer.Start();
        }

        // ── icon (drawn programmatically, no resource file needed) ──

        static Icon BuildIcon()
        {
            // 优先使用内嵌的应用图标；加载失败时回退到程序绘制的简单图标
            var icon = AppIcon.Load(16);
            if (icon != null) return icon;

            using (var bmp = new Bitmap(16, 16))
            using (var g   = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                using (var pen = new Pen(Color.DeepSkyBlue, 1.5f))
                {
                    g.DrawRectangle(pen, 1, 1, 13, 13);
                    g.DrawLine(pen, 5, 8, 11, 8);
                    g.DrawLine(pen, 8, 5, 8, 11);
                }
                return Icon.FromHandle(bmp.GetHicon());
            }
        }

        public void Dispose()
        {
            _hotkeys.Dispose();
            _overlay.Dispose();
            _tray.Visible = false;
            _tray.Dispose();
        }
    }
}
