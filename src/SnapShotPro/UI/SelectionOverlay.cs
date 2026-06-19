using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using SnapShotPro.Core;

namespace SnapShotPro.UI
{
    enum OverlayMode { Region, Window }

    class SelectionOverlay : Form
    {
        OverlayMode _mode;
        Point  _start;
        Point  _current;
        bool   _dragging;

        Bitmap _screenShot;         // full virtual-screen grab shown as background
        Rectangle _virtualBounds;

        // Window-mode: highlighted window
        WindowInfo _hoveredWindow;
        System.Collections.Generic.List<WindowInfo> _windowCache;

        public event Action<Bitmap> RegionSelected;
        public event Action<Bitmap> WindowSelected;

        public SelectionOverlay()
        {
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar   = false;
            TopMost         = true;
            DoubleBuffered  = true;
            Cursor          = Cursors.Cross;

            KeyPreview  = true;
            KeyDown    += (s, e) => { if (e.KeyCode == Keys.Escape) Cancel(); };
        }

        public void StartCapture(OverlayMode mode)
        {
            _mode     = mode;
            _dragging = false;
            _hoveredWindow = null;

            // 窗口模式：在遮罩显示前枚举一次可见窗口（此时列表里还没有遮罩自己）
            _windowCache = mode == OverlayMode.Window
                ? WindowDetector.GetVisibleWindows()
                : null;

            // grab the whole virtual screen before showing overlay
            _virtualBounds = ScreenCapture.GetVirtualScreenBounds();
            _screenShot?.Dispose();
            _screenShot = ScreenCapture.CaptureRegion(_virtualBounds);

            Bounds = _virtualBounds;
            Show();
            Activate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;

            // draw screen background
            if (_screenShot != null)
                g.DrawImage(_screenShot, 0, 0);

            // dark overlay
            using (var overlay = new SolidBrush(Color.FromArgb(100, 0, 0, 0)))
                g.FillRectangle(overlay, ClientRectangle);

            if (_mode == OverlayMode.Region)
                DrawRegionSelection(g);
            else
                DrawWindowHighlight(g);
        }

        void DrawRegionSelection(Graphics g)
        {
            if (!_dragging) return;

            Rectangle sel = GetSelectionRect();
            if (sel.Width < 2 || sel.Height < 2) return;

            // restore original pixels inside selection
            if (_screenShot != null)
                g.DrawImage(_screenShot, sel, sel, GraphicsUnit.Pixel);

            // selection border
            using (var pen = new Pen(Color.FromArgb(255, 0, 174, 255), 1.5f))
                g.DrawRectangle(pen, sel);

            // size label
            string label = sel.Width + " × " + sel.Height;
            using (var font  = new Font("Segoe UI", 9f))
            using (var brush = new SolidBrush(Color.White))
            using (var bg    = new SolidBrush(Color.FromArgb(160, 0, 0, 0)))
            {
                SizeF sz  = g.MeasureString(label, font);
                var   lrc = new RectangleF(sel.X + 2, sel.Y - sz.Height - 4, sz.Width + 4, sz.Height + 2);
                if (lrc.Y < 0) lrc.Y = sel.Y + 2;
                g.FillRectangle(bg, lrc);
                g.DrawString(label, font, brush, lrc.Location);
            }
        }

        void DrawWindowHighlight(Graphics g)
        {
            if (_hoveredWindow == null) return;

            // translate screen rect to overlay-local coords
            Rectangle r = _hoveredWindow.Bounds;
            r.Offset(-_virtualBounds.X, -_virtualBounds.Y);

            if (_screenShot != null)
                g.DrawImage(_screenShot, r,
                    new Rectangle(r.X, r.Y, r.Width, r.Height), GraphicsUnit.Pixel);

            using (var pen = new Pen(Color.FromArgb(255, 0, 174, 255), 2f))
                g.DrawRectangle(pen, r);
        }

        // ── mouse ────────────────────────────────────────────────

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            if (_mode == OverlayMode.Window)
            {
                ConfirmWindow();
                return;
            }

            _start    = e.Location;
            _current  = e.Location;
            _dragging = true;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            _current = e.Location;

            if (_mode == OverlayMode.Window)
            {
                Point screen = new Point(
                    e.X + _virtualBounds.X,
                    e.Y + _virtualBounds.Y);
                // 从遮罩显示前缓存的列表里查找，并排除遮罩自身的句柄
                _hoveredWindow = WindowDetector.FindWindowAt(screen, _windowCache, Handle);
            }

            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (_mode != OverlayMode.Region || !_dragging) return;
            _current  = e.Location;
            _dragging = false;
            ConfirmRegion();
        }

        protected override void OnDoubleClick(EventArgs e)
        {
            if (_mode == OverlayMode.Region && !_dragging)
                ConfirmRegion();
        }

        // ── confirm / cancel ─────────────────────────────────────

        void ConfirmRegion()
        {
            Rectangle sel = GetSelectionRect();
            if (sel.Width < 4 || sel.Height < 4) { Cancel(); return; }

            // translate to screen coords
            Rectangle screenSel = new Rectangle(
                sel.X + _virtualBounds.X,
                sel.Y + _virtualBounds.Y,
                sel.Width, sel.Height);

            Hide();
            Bitmap bmp = ScreenCapture.CaptureRegion(screenSel);
            RegionSelected?.Invoke(bmp);
        }

        void ConfirmWindow()
        {
            if (_hoveredWindow == null) { Cancel(); return; }
            Hide();
            Bitmap bmp = ScreenCapture.CaptureWindow(_hoveredWindow.Handle);
            WindowSelected?.Invoke(bmp);
        }

        void Cancel()
        {
            _dragging = false;
            Hide();
        }

        Rectangle GetSelectionRect()
        {
            return new Rectangle(
                Math.Min(_start.X, _current.X),
                Math.Min(_start.Y, _current.Y),
                Math.Abs(_current.X - _start.X),
                Math.Abs(_current.Y - _start.Y));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _screenShot?.Dispose();
            base.Dispose(disposing);
        }

        // prevent ALT+F4 from closing overlay unintentionally
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            { e.Cancel = true; Hide(); }
            else base.OnFormClosing(e);
        }
    }
}
