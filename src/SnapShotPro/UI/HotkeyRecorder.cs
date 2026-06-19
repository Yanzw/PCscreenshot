using System;
using System.Drawing;
using System.Windows.Forms;
using SnapShotPro.Core;

namespace SnapShotPro.UI
{
    // 一个只读文本框，聚焦后按下任意组合键即被录制为热键。
    // 只接受"修饰键 + 主键"形式；单独的修饰键不会被确定为最终值。
    class HotkeyRecorder : TextBox
    {
        HotkeyConfig _value = new HotkeyConfig();

        public HotkeyConfig Value
        {
            get { return _value; }
            set
            {
                _value = value ?? new HotkeyConfig();
                Text = _value.ToDisplayString();
            }
        }

        public HotkeyRecorder()
        {
            ReadOnly  = true;
            BackColor = Color.White;
            Cursor    = Cursors.Hand;
            Text      = "(点击后按下快捷键)";
            ShortcutsEnabled = false;
        }

        protected override void OnEnter(EventArgs e)
        {
            base.OnEnter(e);
            Text = "请按下快捷键...";
            BackColor = Color.FromArgb(230, 244, 255);
        }

        protected override void OnLeave(EventArgs e)
        {
            base.OnLeave(e);
            BackColor = Color.White;
            Text = _value.ToDisplayString();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // 拦截所有按键（包括 Tab、方向键），避免焦点跳走或触发默认行为
            if (Focused)
            {
                CaptureKey(keyData);
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        void CaptureKey(Keys keyData)
        {
            Keys key   = keyData & Keys.KeyCode;
            bool ctrl  = (keyData & Keys.Control) == Keys.Control;
            bool alt   = (keyData & Keys.Alt)     == Keys.Alt;
            bool shift = (keyData & Keys.Shift)   == Keys.Shift;

            // Esc 清空当前录制
            if (key == Keys.Escape)
            {
                _value = new HotkeyConfig();
                Text = "(未设置)";
                return;
            }

            // 修饰键本身不是主键，仅显示进行中的组合
            if (key == Keys.ControlKey || key == Keys.Menu ||
                key == Keys.ShiftKey   || key == Keys.LWin || key == Keys.RWin)
            {
                var partial = new HotkeyConfig(ctrl, alt, shift, false, Keys.None);
                Text = partial.ToDisplayString() + "...";
                return;
            }

            var cfg = new HotkeyConfig(ctrl, alt, shift, false, key);
            if (cfg.IsValid)
            {
                _value = cfg;
                Text = cfg.ToDisplayString();
            }
            else
            {
                // 没有修饰键的单键不允许作为全局热键
                Text = "需包含 Ctrl/Alt/Shift";
            }
        }
    }
}
