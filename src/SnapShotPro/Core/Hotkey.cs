using System;
using System.Text;
using System.Windows.Forms;

namespace SnapShotPro.Core
{
    // 一个可序列化的快捷键组合：修饰键 + 主键。
    // 用于持久化到 INI、在设置界面显示，以及注册全局热键。
    class HotkeyConfig
    {
        public bool Ctrl;
        public bool Alt;
        public bool Shift;
        public bool Win;
        public Keys Key;

        public HotkeyConfig() { }

        public HotkeyConfig(bool ctrl, bool alt, bool shift, bool win, Keys key)
        {
            Ctrl = ctrl; Alt = alt; Shift = shift; Win = win; Key = key;
        }

        public bool IsValid
        {
            // 主键不能为空，且至少有一个修饰键（避免误触发）
            get { return Key != Keys.None && (Ctrl || Alt || Shift || Win); }
        }

        // 供 RegisterHotKey 使用的修饰键位标志
        public uint ModifierFlags
        {
            get
            {
                uint m = 0;
                if (Alt)   m |= 0x0001; // MOD_ALT
                if (Ctrl)  m |= 0x0002; // MOD_CONTROL
                if (Shift) m |= 0x0004; // MOD_SHIFT
                if (Win)   m |= 0x0008; // MOD_WIN
                return m;
            }
        }

        // 人类可读形式，如 "Ctrl+Alt+A"
        public string ToDisplayString()
        {
            if (Key == Keys.None && !Ctrl && !Alt && !Shift && !Win)
                return "(未设置)";

            var sb = new StringBuilder();
            if (Ctrl)  sb.Append("Ctrl+");
            if (Alt)   sb.Append("Alt+");
            if (Shift) sb.Append("Shift+");
            if (Win)   sb.Append("Win+");
            if (Key != Keys.None) sb.Append(KeyName(Key));
            return sb.ToString().TrimEnd('+');
        }

        // 序列化为 INI 存储字符串（与 ToDisplayString 同格式，便于回读）
        public string Serialize()
        {
            return ToDisplayString();
        }

        public static HotkeyConfig Parse(string s, HotkeyConfig fallback)
        {
            if (string.IsNullOrEmpty(s) || s == "(未设置)")
                return fallback;

            var cfg = new HotkeyConfig();
            string[] parts = s.Split('+');
            foreach (string raw in parts)
            {
                string p = raw.Trim();
                if (p.Length == 0) continue;

                switch (p.ToLower())
                {
                    case "ctrl":    cfg.Ctrl  = true; break;
                    case "alt":     cfg.Alt   = true; break;
                    case "shift":   cfg.Shift = true; break;
                    case "win":     cfg.Win   = true; break;
                    default:
                        Keys k;
                        if (TryParseKey(p, out k)) cfg.Key = k;
                        break;
                }
            }
            return cfg.IsValid ? cfg : fallback;
        }

        // 友好的按键名（A-Z 直接显示字母，数字键去掉 D 前缀）
        static string KeyName(Keys k)
        {
            if (k >= Keys.A && k <= Keys.Z) return k.ToString();
            if (k >= Keys.D0 && k <= Keys.D9) return ((char)('0' + (k - Keys.D0))).ToString();
            if (k >= Keys.F1 && k <= Keys.F12) return k.ToString();
            if (k >= Keys.NumPad0 && k <= Keys.NumPad9)
                return "Num" + (k - Keys.NumPad0);
            return k.ToString();
        }

        static bool TryParseKey(string s, out Keys key)
        {
            key = Keys.None;
            if (s.Length == 1 && s[0] >= '0' && s[0] <= '9')
            {
                key = Keys.D0 + (s[0] - '0');
                return true;
            }
            if (s.StartsWith("Num") && s.Length == 4 && char.IsDigit(s[3]))
            {
                key = Keys.NumPad0 + (s[3] - '0');
                return true;
            }
            try
            {
                key = (Keys)Enum.Parse(typeof(Keys), s, true);
                return true;
            }
            catch { return false; }
        }
    }
}
