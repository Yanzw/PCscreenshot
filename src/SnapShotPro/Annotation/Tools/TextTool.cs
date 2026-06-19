using System.Drawing;
using System.Windows.Forms;

namespace SnapShotPro.Annotation.Tools
{
    class TextTool : ITool
    {
        public Color Color    = Color.Red;
        public float FontSize = 16f;

        Point _pos;
        bool _placed;

        // Callback: canvas calls this to get text from user
        public System.Func<Point, string> RequestText;

        public void OnMouseDown(Point p, Graphics g)
        {
            _pos    = p;
            _placed = true;
        }

        public void OnMouseMove(Point p, Graphics g) { }

        public void OnMouseUp(Point p, Graphics g)
        {
            if (!_placed) return;
            _placed = false;

            string text = RequestText != null ? RequestText(_pos) : null;
            if (string.IsNullOrEmpty(text)) return;

            Draw(g, text, _pos);
        }

        public void DrawPreview(Graphics g) { }

        void Draw(Graphics g, string text, Point pos)
        {
            using (var font  = new Font("微软雅黑", FontSize, FontStyle.Regular, GraphicsUnit.Pixel))
            using (var brush = new SolidBrush(Color))
            {
                // shadow for readability
                using (var shadow = new SolidBrush(System.Drawing.Color.FromArgb(120, 0, 0, 0)))
                    g.DrawString(text, font, shadow, pos.X + 1, pos.Y + 1);
                g.DrawString(text, font, brush, pos.X, pos.Y);
            }
        }
    }
}
