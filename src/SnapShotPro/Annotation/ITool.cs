using System.Drawing;
using System.Windows.Forms;

namespace SnapShotPro.Annotation
{
    enum ToolType { Arrow, Rectangle, Text, Mosaic }

    interface ITool
    {
        void OnMouseDown(Point p, Graphics g);
        void OnMouseMove(Point p, Graphics g);
        void OnMouseUp(Point p, Graphics g);
        void DrawPreview(Graphics g);
    }
}
