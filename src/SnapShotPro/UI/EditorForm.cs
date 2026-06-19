using System;
using System.Drawing;
using System.Windows.Forms;
using SnapShotPro.Annotation;
using SnapShotPro.Core;
using SnapShotPro.Utils;

namespace SnapShotPro.UI
{
    class EditorForm : Form
    {
        AnnotationCanvas _canvas;
        Panel  _toolbar;
        Label  _statusLabel;
        Settings _settings;

        // toolbar buttons
        Button _btnArrow, _btnRect, _btnText, _btnMosaic;
        Button _btnUndo, _btnRedo;
        Button _btnColor;
        Button _btnSave, _btnCopy, _btnClose;
        Panel  _colorSwatch;

        Color _currentColor = Color.Red;

        public EditorForm(Settings settings)
        {
            _settings = settings;
            InitUI();
        }

        void InitUI()
        {
            Text            = "SnapShot Pro - 标注编辑器";
            FormBorderStyle = FormBorderStyle.Sizable;
            StartPosition   = FormStartPosition.CenterScreen;
            MinimumSize     = new Size(400, 300);
            BackColor       = Color.FromArgb(40, 40, 40);
            ShowInTaskbar   = true;

            var appIcon = AppIcon.Load(32);
            if (appIcon != null) Icon = appIcon;

            // ── toolbar ──────────────────────────────────────────
            _toolbar = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 44,
                BackColor = Color.FromArgb(55, 55, 55),
                Padding   = new Padding(4)
            };

            _btnArrow  = MakeToolBtn("箭头",   0);
            _btnRect   = MakeToolBtn("矩形",   1);
            _btnText   = MakeToolBtn("文字",   2);
            _btnMosaic = MakeToolBtn("马赛克", 3);
            // "马赛克" 为三个字，加宽按钮避免被截断为 "马赛"
            _btnMosaic.Width = 72;

            // separator
            var sep = new Label { Width = 1, Height = 30, BackColor = Color.Gray,
                                  Top = 7, Left = 4 * 70 + 8 };

            _btnUndo = MakeToolBtn("撤销", 5);
            _btnRedo = MakeToolBtn("重做", 6);

            _colorSwatch = new Panel
            {
                Size      = new Size(22, 22),
                BackColor = _currentColor,
                Top       = 11,
                // place to the right of the 重做 (redo) button: redo Left(412) + Width(62) + gap(10)
                Left      = _btnRedo.Left + _btnRedo.Width + 10,
                Cursor    = Cursors.Hand,
                BorderStyle = BorderStyle.FixedSingle
            };
            _colorSwatch.Click += PickColor;

            _btnSave  = MakeActionBtn("保存",  0);
            _btnCopy  = MakeActionBtn("复制",  1);
            _btnClose = MakeActionBtn("关闭",  2);

            _toolbar.Controls.AddRange(new Control[]
            {
                _btnArrow, _btnRect, _btnText, _btnMosaic,
                sep,
                _btnUndo, _btnRedo,
                _colorSwatch,
                _btnSave, _btnCopy, _btnClose
            });

            // ── status bar ───────────────────────────────────────
            _statusLabel = new Label
            {
                Dock      = DockStyle.Bottom,
                Height    = 22,
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.Silver,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(6, 0, 0, 0),
                Text      = "就绪"
            };

            // ── canvas scroll container ──────────────────────────
            var scroll = new Panel
            {
                Dock       = DockStyle.Fill,
                AutoScroll = true,
                BackColor  = Color.FromArgb(30, 30, 30)
            };

            _canvas = new AnnotationCanvas
            {
                Location = new Point(8, 8)
            };
            _canvas.ImageChanged += (s, e) => UpdateStatus();
            scroll.Controls.Add(_canvas);

            Controls.AddRange(new Control[] { scroll, _toolbar, _statusLabel });

            // ── keyboard shortcuts ───────────────────────────────
            KeyPreview = true;
            KeyDown   += OnKeyDown;

            // ── default tool ─────────────────────────────────────
            SelectTool(_btnArrow, ToolType.Arrow);
        }

        // ── public API ───────────────────────────────────────────

        public void LoadCapture(Bitmap bmp)
        {
            _canvas.LoadImage(bmp);

            // size form to fit image + chrome
            int chromeW = Width  - ClientSize.Width;
            int chromeH = Height - ClientSize.Height;
            int w = Math.Min(bmp.Width  + 16 + chromeW, Screen.PrimaryScreen.WorkingArea.Width  - 40);
            int h = Math.Min(bmp.Height + 16 + _toolbar.Height + _statusLabel.Height + chromeH,
                             Screen.PrimaryScreen.WorkingArea.Height - 40);
            Size = new Size(w, h);
            CenterToScreen();
            UpdateStatus();
        }

        // ── toolbar helpers ──────────────────────────────────────

        Button MakeToolBtn(string label, int index)
        {
            bool isRight = index >= 5;
            int x = isRight
                ? ClientSize.Width - (8 - index) * 70
                : index * 70 + 4;

            // We'll reflow in Resize; for now just lay them out
            var btn = new Button
            {
                Text      = label,
                Size      = new Size(62, 30),
                Top       = 7,
                Left      = index * 68 + 4,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(70, 70, 70),
                Cursor    = Cursors.Hand
            };
            btn.FlatAppearance.BorderColor = Color.FromArgb(90, 90, 90);
            return btn;
        }

        Button MakeActionBtn(string label, int rightIndex)
        {
            var btn = new Button
            {
                Text      = label,
                Size      = new Size(62, 30),
                Top       = 7,
                Anchor    = AnchorStyles.Top | AnchorStyles.Right,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(70, 100, 70),
                Cursor    = Cursors.Hand
            };
            btn.FlatAppearance.BorderColor = Color.FromArgb(90, 120, 90);

            // right-anchored positions set in Layout
            _toolbar.Layout += (s, e) =>
            {
                btn.Left = _toolbar.Width - (3 - rightIndex) * 68 - 4;
            };

            if (label == "保存")  btn.Click += (s, e) => DoSave();
            if (label == "复制")  btn.Click += (s, e) => DoCopy();
            if (label == "关闭")  btn.Click += (s, e) => Close();

            return btn;
        }

        void SelectTool(Button sender, ToolType type)
        {
            _canvas.SetTool(type);
            _canvas.DrawColor = _currentColor;

            foreach (Control c in _toolbar.Controls)
            {
                if (c is Button b && b != _btnSave && b != _btnCopy && b != _btnClose
                                  && b != _btnUndo && b != _btnRedo)
                    b.BackColor = Color.FromArgb(70, 70, 70);
            }
            sender.BackColor = Color.FromArgb(0, 120, 215);
        }

        void PickColor(object s, EventArgs e)
        {
            using (var dlg = new ColorDialog { Color = _currentColor, FullOpen = true })
            {
                // 载入上次保存的自定义颜色
                if (_settings.CustomColors != null && _settings.CustomColors.Length > 0)
                    dlg.CustomColors = _settings.CustomColors;

                var result = dlg.ShowDialog(this);

                // 无论确定或取消，都保存用户新增的自定义颜色
                _settings.CustomColors = dlg.CustomColors;
                _settings.Save();

                if (result == DialogResult.OK)
                {
                    _currentColor          = dlg.Color;
                    _colorSwatch.BackColor = dlg.Color;
                    _canvas.DrawColor      = dlg.Color;
                }
            }
        }

        void OnKeyDown(object s, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.Z) { _canvas.Undo(); e.Handled = true; }
            if (e.Control && e.KeyCode == Keys.Y) { _canvas.Redo(); e.Handled = true; }
            if (e.Control && e.KeyCode == Keys.S) { DoSave();       e.Handled = true; }
            if (e.Control && e.KeyCode == Keys.C) { DoCopy();       e.Handled = true; }
        }

        void DoSave()
        {
            using (var bmp = _canvas.GetComposite())
            {
                string path = FileHelper.SaveAs(bmp, _settings.SaveFolder);
                if (path != null)
                {
                    _settings.SaveFolder = System.IO.Path.GetDirectoryName(path);
                    _settings.Save();
                    _statusLabel.Text = "已保存: " + path;
                }
            }
        }

        void DoCopy()
        {
            using (var bmp = _canvas.GetComposite())
                ClipboardHelper.CopyImage(bmp);
            _statusLabel.Text = "已复制到剪贴板";
        }

        void UpdateStatus()
        {
            _statusLabel.Text = "工具就绪  |  Ctrl+Z 撤销  Ctrl+S 保存  Ctrl+C 复制";
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            // wire tool buttons after controls created
            _btnArrow.Click  += (s, _) => SelectTool(_btnArrow,  ToolType.Arrow);
            _btnRect.Click   += (s, _) => SelectTool(_btnRect,   ToolType.Rectangle);
            _btnText.Click   += (s, _) => SelectTool(_btnText,   ToolType.Text);
            _btnMosaic.Click += (s, _) => SelectTool(_btnMosaic, ToolType.Mosaic);
            _btnUndo.Click   += (s, _) => _canvas.Undo();
            _btnRedo.Click   += (s, _) => _canvas.Redo();
        }
    }
}
