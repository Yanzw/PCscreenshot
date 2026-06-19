using System;
using System.Drawing;

namespace SnapShotPro.Annotation.Tools
{
    class ArrowTool : ITool
    {
        Point _start, _end;
        bool _drawing;
        public Color Color     = Color.Red;
        public float Thickness = 6f;

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
            if (_start == _end) return;

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            double dx  = _end.X - _start.X;
            double dy  = _end.Y - _start.Y;
            double len = Math.Sqrt(dx * dx + dy * dy);
            if (len < 1) return;

            // arrowhead size scales with thickness, clamped so short arrows still look right
            float headLen   = (float)Math.Min(Thickness * 6 + 8, len);
            float headWidth = headLen * 0.6f;

            double angle = Math.Atan2(dy, dx);
            double cos = Math.Cos(angle), sin = Math.Sin(angle);

            // tip of the arrow
            PointF tip = new PointF(_end.X, _end.Y);

            // base center of the triangle (pulled back from the tip along the shaft)
            PointF baseCenter = new PointF(
                (float)(_end.X - headLen * cos),
                (float)(_end.Y - headLen * sin));

            // two wings of the triangle (perpendicular to the shaft)
            PointF wing1 = new PointF(
                (float)(baseCenter.X - headWidth / 2 * sin),
                (float)(baseCenter.Y + headWidth / 2 * cos));
            PointF wing2 = new PointF(
                (float)(baseCenter.X + headWidth / 2 * sin),
                (float)(baseCenter.Y - headWidth / 2 * cos));

            using (var pen   = new Pen(Color, Thickness))
            using (var brush = new SolidBrush(Color))
            {
                // shaft stops at the base of the head so the line doesn't poke through
                g.DrawLine(pen, _start, baseCenter);
                g.FillPolygon(brush, new[] { tip, wing1, wing2 });
            }
        }
    }
}
