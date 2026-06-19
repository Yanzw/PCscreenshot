using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace SnapShotPro.Core
{
    static class ClipboardHelper
    {
        public static void CopyImage(Bitmap bmp)
        {
            Clipboard.SetImage(bmp);
        }
    }
}
