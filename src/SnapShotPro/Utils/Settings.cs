using System;
using System.IO;
using System.Windows.Forms;
using SnapShotPro.Core;

namespace SnapShotPro.Utils
{
    class Settings
    {
        static readonly string IniPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "SnapShotPro.ini");

        public string SaveFolder   = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        public string ImageFormat  = "png";
        public bool   AutoCopy     = false;

        // 颜色选择器中的自定义颜色（ColorDialog.CustomColors，BGR 整数数组）
        public int[]  CustomColors = new int[0];

        // 全局热键（可自定义），默认 Alt+A / Alt+W / Alt+F
        public HotkeyConfig HotkeyRegion     = new HotkeyConfig(false, true, false, false, Keys.A);
        public HotkeyConfig HotkeyWindow     = new HotkeyConfig(false, true, false, false, Keys.W);
        public HotkeyConfig HotkeyFullscreen = new HotkeyConfig(false, true, false, false, Keys.F);

        public static Settings Load()
        {
            var s = new Settings();
            if (!File.Exists(IniPath)) return s;

            foreach (string line in File.ReadAllLines(IniPath))
            {
                int eq = line.IndexOf('=');
                if (eq < 0) continue;
                string key = line.Substring(0, eq).Trim();
                string val = line.Substring(eq + 1).Trim();

                switch (key)
                {
                    case "SaveFolder":  s.SaveFolder  = val; break;
                    case "ImageFormat": s.ImageFormat = val; break;
                    case "AutoCopy":    s.AutoCopy    = val == "1"; break;
                    case "CustomColors": s.CustomColors = ParseColors(val); break;
                    case "HotkeyRegion":
                        s.HotkeyRegion = HotkeyConfig.Parse(val, s.HotkeyRegion); break;
                    case "HotkeyWindow":
                        s.HotkeyWindow = HotkeyConfig.Parse(val, s.HotkeyWindow); break;
                    case "HotkeyFullscreen":
                        s.HotkeyFullscreen = HotkeyConfig.Parse(val, s.HotkeyFullscreen); break;
                }
            }
            return s;
        }

        public void Save()
        {
            File.WriteAllLines(IniPath, new[]
            {
                "SaveFolder="       + SaveFolder,
                "ImageFormat="      + ImageFormat,
                "AutoCopy="         + (AutoCopy ? "1" : "0"),
                "CustomColors="     + SerializeColors(CustomColors),
                "HotkeyRegion="     + HotkeyRegion.Serialize(),
                "HotkeyWindow="     + HotkeyWindow.Serialize(),
                "HotkeyFullscreen=" + HotkeyFullscreen.Serialize(),
            });
        }

        // 把颜色数组序列化为逗号分隔的整数字符串
        static string SerializeColors(int[] colors)
        {
            if (colors == null || colors.Length == 0) return "";
            return string.Join(",", Array.ConvertAll(colors, c => c.ToString()));
        }

        // 解析逗号分隔的整数字符串为颜色数组
        static int[] ParseColors(string val)
        {
            if (string.IsNullOrWhiteSpace(val)) return new int[0];
            string[] parts = val.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var list = new System.Collections.Generic.List<int>(parts.Length);
            foreach (string p in parts)
                if (int.TryParse(p.Trim(), out int c)) list.Add(c);
            return list.ToArray();
        }
    }
}
