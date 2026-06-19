using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace SnapShotPro.Annotation.Tools
{
    class MosaicTool : ITool
    {
        public int BlockSize = 12;
        Point _start, _end;
        bool _drawing;

        // Reference to the annotation bitmap so we can read pixels during drag
        public Bitmap SourceBitmap;

        public void OnMouseDown(Point p, Graphics g)
        {
            _start = p; _end = p; _drawing = true;
            ApplyMosaic(g, _start, _end);
        }

        public void OnMouseMove(Point p, Graphics g)
        {
            if (!_drawing) return;
            _end = p;
            ApplyMosaic(g, _start, _end);
        }

        public void OnMouseUp(Point p, Graphics g)
        {
            if (!_drawing) return;
            _end = p; _drawing = false;
            ApplyMosaic(g, _start, _end);
        }

        public void DrawPreview(Graphics g) { }

        void ApplyMosaic(Graphics g, Point a, Point b)
        {
            if (SourceBitmap == null) return;

            Rectangle region = new Rectangle(
                Math.Min(a.X, b.X), Math.Min(a.Y, b.Y),
                Math.Abs(b.X - a.X), Math.Abs(b.Y - a.Y));

            if (region.Width < BlockSize || region.Height < BlockSize) return;

            // Clamp to bitmap bounds
            region.X = Math.Max(0, Math.Min(region.X, SourceBitmap.Width  - 1));
            region.Y = Math.Max(0, Math.Min(region.Y, SourceBitmap.Height - 1));
            region.Width  = Math.Min(region.Width,  SourceBitmap.Width  - region.X);
            region.Height = Math.Min(region.Height, SourceBitmap.Height - region.Y);

            for (int y = region.Y; y < region.Y + region.Height; y += BlockSize)
            {
                for (int x = region.X; x < region.X + region.Width; x += BlockSize)
                {
                    int bw = Math.Min(BlockSize, region.Right  - x);
                    int bh = Math.Min(BlockSize, region.Bottom - y);
                    Color avg = AverageColor(SourceBitmap, x, y, bw, bh);
                    using (var brush = new SolidBrush(avg))
                        g.FillRectangle(brush, x, y, bw, bh);
                }
            }
        }

        static Color AverageColor(Bitmap bmp, int x, int y, int w, int h)
        {
            long r = 0, gr = 0, b = 0;
            int count = 0;
            for (int py = y; py < y + h; py++)
            {
                for (int px = x; px < x + w; px++)
                {
                    Color c = bmp.GetPixel(px, py);
                    // 以白色为底色合成：透明像素按白色计入，避免马赛克块发黑
                    float a = c.A / 255f;
                    r  += (int)(c.R * a + 255 * (1 - a));
                    gr += (int)(c.G * a + 255 * (1 - a));
                    b  += (int)(c.B * a + 255 * (1 - a));
                    count++;
                }
            }
            if (count == 0) return Color.White;
            return Color.FromArgb((int)(r / count), (int)(gr / count), (int)(b / count));
        }
    }
}
