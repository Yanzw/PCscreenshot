using System;
using System.Drawing;
using System.Windows.Forms;
using SnapShotPro.Core;
using SnapShotPro.Utils;

namespace SnapShotPro.UI
{
    // 快捷键设置对话框：编辑三个全局热键并校验冲突。
    class SettingsForm : Form
    {
        readonly Settings _settings;
        HotkeyRecorder _recRegion, _recWindow, _recFullscreen;

        public SettingsForm(Settings settings)
        {
            _settings = settings;
            InitUI();
        }

        void InitUI()
        {
            Text            = "快捷键设置";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition   = FormStartPosition.CenterScreen;
            MaximizeBox     = false;
            MinimizeBox     = false;
            ClientSize      = new Size(360, 220);

            AddRow("区域截图", 20, out _recRegion,     _settings.HotkeyRegion);
            AddRow("窗口截图", 60, out _recWindow,     _settings.HotkeyWindow);
            AddRow("全屏截图", 100, out _recFullscreen, _settings.HotkeyFullscreen);

            var tip = new Label
            {
                Text      = "点击输入框后按下组合键；Esc 清空。需含 Ctrl/Alt/Shift。",
                Location  = new Point(20, 138),
                Size      = new Size(330, 30),
                ForeColor = Color.Gray
            };

            var btnOk = new Button
            {
                Text     = "保存",
                Location = new Point(170, 178),
                Size     = new Size(80, 30)
            };
            btnOk.Click += OnSave;

            var btnCancel = new Button
            {
                Text         = "取消",
                Location     = new Point(260, 178),
                Size         = new Size(80, 30),
                DialogResult = DialogResult.Cancel
            };

            AcceptButton = btnOk;
            CancelButton = btnCancel;
            Controls.AddRange(new Control[] { tip, btnOk, btnCancel });
        }

        void AddRow(string label, int y, out HotkeyRecorder rec, HotkeyConfig value)
        {
            var lbl = new Label
            {
                Text      = label,
                Location  = new Point(20, y + 3),
                Size      = new Size(80, 22),
                TextAlign = ContentAlignment.MiddleLeft
            };
            rec = new HotkeyRecorder
            {
                Location = new Point(110, y),
                Size     = new Size(230, 24),
                Value    = value
            };
            Controls.Add(lbl);
            Controls.Add(rec);
        }

        void OnSave(object sender, EventArgs e)
        {
            var r = _recRegion.Value;
            var w = _recWindow.Value;
            var f = _recFullscreen.Value;

            if (!r.IsValid || !w.IsValid || !f.IsValid)
            {
                MessageBox.Show(this, "每个快捷键都需要包含至少一个修饰键和一个主键。",
                    "无效的快捷键", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 校验三者互不相同
            string sr = r.ToDisplayString(), sw = w.ToDisplayString(), sf = f.ToDisplayString();
            if (sr == sw || sr == sf || sw == sf)
            {
                MessageBox.Show(this, "三个快捷键不能相同，请重新设置。",
                    "快捷键冲突", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _settings.HotkeyRegion     = r;
            _settings.HotkeyWindow     = w;
            _settings.HotkeyFullscreen = f;
            _settings.Save();

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
