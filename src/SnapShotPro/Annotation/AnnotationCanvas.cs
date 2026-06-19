using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using SnapShotPro.Annotation.Tools;

namespace SnapShotPro.Annotation
{
    class AnnotationCanvas : Control
    {
        Bitmap _original;
        Bitmap _annotation;     // transparent overlay
        Bitmap _preview;        // composite for display
        readonly AnnotationHistory _history = new AnnotationHistory();

        ITool _activeTool;
        ToolType _toolType = ToolType.Arrow;
        public Color DrawColor  = Color.Red;
        public float Thickness  = 2f;
        public float FontSize   = 32f;

        public event EventHandler ImageChanged;

        public AnnotationCanvas()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.DoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
            Cursor = Cursors.Cross;
        }

        public void LoadImage(Bitmap bmp)
        {
            _original?.Dispose();
            _annotation?.Dispose();
            _preview?.Dispose();
            _history.Clear();

            _original   = new Bitmap(bmp);
            _annotation = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format32bppArgb);
            _preview    = null;

            Size = bmp.Size;
            _history.Push(_annotation);
            Invalidate();
        }

        public void SetTool(ToolType type)
        {
            _toolType = type;
            _activeTool = null;
        }

        public void Undo()
        {
            var prev = _history.Undo();
            if (prev == null) return;
            _annotation.Dispose();
            _annotation = prev;
            Invalidate();
        }

        public void Redo()
        {
            var next = _history.Redo();
            if (next == null) return;
            _annotation.Dispose();
            _annotation = next;
            Invalidate();
        }

        public Bitmap GetComposite()
        {
            var result = new Bitmap(_original.Width, _original.Height, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(result))
            {
                g.DrawImage(_original, 0, 0);
                g.DrawImage(_annotation, 0, 0);
            }
            return result;
        }

        // ── painting ─────────────────────────────────────────────

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_original == null) return;
            BuildPreview();
            e.Graphics.DrawImage(_preview, 0, 0);
            _activeTool?.DrawPreview(e.Graphics);
        }

        void BuildPreview()
        {
            if (_preview == null || _preview.Size != _original.Size)
            {
                _preview?.Dispose();
                _preview = new Bitmap(_original.Width, _original.Height, PixelFormat.Format32bppArgb);
            }
            using (var g = Graphics.FromImage(_preview))
            {
                g.DrawImage(_original, 0, 0);
                g.DrawImage(_annotation, 0, 0);
            }
        }

        // ── mouse ─────────────────────────────────────────────────

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            _activeTool = CreateTool();

            using (var g = Graphics.FromImage(_annotation))
                _activeTool.OnMouseDown(e.Location, g);
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_activeTool == null || e.Button != MouseButtons.Left) return;
            using (var g = Graphics.FromImage(_annotation))
                _activeTool.OnMouseMove(e.Location, g);
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (_activeTool == null) return;
            using (var g = Graphics.FromImage(_annotation))
                _activeTool.OnMouseUp(e.Location, g);

            _history.Push(_annotation);
            _activeTool = null;
            Invalidate();
            ImageChanged?.Invoke(this, EventArgs.Empty);
        }

        // ── tool factory ──────────────────────────────────────────

        ITool CreateTool()
        {
            switch (_toolType)
            {
                case ToolType.Arrow:
                    return new ArrowTool { Color = DrawColor, Thickness = Thickness * 3f };

                case ToolType.Rectangle:
                    return new RectangleTool { Color = DrawColor, Thickness = Thickness };

                case ToolType.Text:
                    var tt = new TextTool { Color = DrawColor, FontSize = FontSize };
                    tt.RequestText = pt => AskText();
                    return tt;

                case ToolType.Mosaic:
                    return new MosaicTool { SourceBitmap = _annotation };

                default:
                    return new ArrowTool();
            }
        }

        string AskText()
        {
            using (var dlg = new TextInputDialog())
            {
                if (dlg.ShowDialog(FindForm()) == DialogResult.OK)
                    return dlg.InputText;
            }
            return null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _original?.Dispose();
                _annotation?.Dispose();
                _preview?.Dispose();
                _history.Clear();
            }
            base.Dispose(disposing);
        }
    }

    // ── minimal text input dialog ─────────────────────────────────

    class TextInputDialog : Form
    {
        readonly TextBox _box = new TextBox();
        public string InputText => _box.Text;

        public TextInputDialog()
        {
            Text            = "输入文字";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition   = FormStartPosition.CenterParent;
            ClientSize      = new Size(340, 120);
            MaximizeBox     = false;
            MinimizeBox     = false;

            _box.Location   = new Point(15, 15);
            _box.Size       = new Size(310, 30);
            _box.Font       = new Font("微软雅黑", 14f);

            var ok = new Button
            {
                Text         = "确定",
                DialogResult = DialogResult.OK,
                Location     = new Point(125, 70),
                Size         = new Size(90, 32)
            };

            AcceptButton = ok;
            Controls.AddRange(new Control[] { _box, ok });
        }
    }
}
