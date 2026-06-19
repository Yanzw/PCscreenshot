using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SnapShotPro.Core
{
    class HotkeyManager : IDisposable
    {
        [DllImport("user32.dll")]
        static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        const uint MOD_ALT   = 0x0001;
        const uint MOD_CTRL  = 0x0002;
        const uint MOD_SHIFT = 0x0004;
        const int  WM_HOTKEY = 0x0312;

        readonly HotkeyWindow _window;
        readonly Dictionary<int, Action> _handlers = new Dictionary<int, Action>();
        int _nextId = 1;

        public HotkeyManager()
        {
            _window = new HotkeyWindow();
            _window.HotkeyPressed += OnHotkeyPressed;
        }

        public bool Register(Keys key, bool alt, bool ctrl, bool shift, Action handler)
        {
            return Register(new HotkeyConfig(ctrl, alt, shift, false, key), handler);
        }

        public bool Register(HotkeyConfig cfg, Action handler)
        {
            if (cfg == null || !cfg.IsValid)
                return false;

            int id = _nextId++;
            if (!RegisterHotKey(_window.Handle, id, cfg.ModifierFlags, (uint)cfg.Key))
                return false;

            _handlers[id] = handler;
            return true;
        }

        // 注销所有已注册热键（用于设置变更后重新注册）
        public void UnregisterAll()
        {
            foreach (int id in _handlers.Keys)
                UnregisterHotKey(_window.Handle, id);
            _handlers.Clear();
        }

        void OnHotkeyPressed(int id)
        {
            Action handler;
            if (_handlers.TryGetValue(id, out handler))
                handler();
        }

        public void Dispose()
        {
            foreach (int id in _handlers.Keys)
                UnregisterHotKey(_window.Handle, id);
            _handlers.Clear();
            _window.Dispose();
        }

        class HotkeyWindow : NativeWindow, IDisposable
        {
            public event Action<int> HotkeyPressed;

            public HotkeyWindow()
            {
                CreateHandle(new CreateParams());
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == WM_HOTKEY)
                    HotkeyPressed?.Invoke(m.WParam.ToInt32());
                base.WndProc(ref m);
            }

            public void Dispose()
            {
                DestroyHandle();
            }
        }
    }
}
