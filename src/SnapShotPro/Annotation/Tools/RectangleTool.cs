using System.Drawing;

namespace SnapShotPro.Annotation.Tools
{
    class RectangleTool : ITool
    {
        Point _start, _end;
        bool _drawing;
        public Color Color     = Color.Red;
        public float Thickness = 2f;

        public void OnMouseDown(Point p, Graphics g) { _start = p; _end = p; _drawing = true; }
        public void OnMouseMove(Point p, Graphics g) { if (_drawing) _end = p; }

        public void OnMouseUp(Point p, Graphics g)
        {
            if (!_drawing) return;
            _end = p;
            _drawing = false;
            Draw(g);
        }

        public void DrawPreview(Graphics g)
        {
            if (_drawing) Draw(g);
        }

        void Draw(Graphics g)
        {
            Rectangle r = MakeRect(_start, _end);
            if (r.Width < 2 || r.Height < 2) return;
            using (var pen = new Pen(Color, Thickness))
                g.DrawRectangle(pen, r);
        }

        static Rectangle MakeRect(Point a, Point b)
        {
            return new Rectangle(
                System.Math.Min(a.X, b.X),
                System.Math.Min(a.Y, b.Y),
                System.Math.Abs(b.X - a.X),
                System.Math.Abs(b.Y - a.Y));
        }
    }
}
