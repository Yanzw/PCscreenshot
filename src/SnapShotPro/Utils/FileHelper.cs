using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace SnapShotPro.Utils
{
    static class FileHelper
    {
        public static string Save(Bitmap bmp, string folder, string format)
        {
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string ts   = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string name = "Screenshot_" + ts + "." + format.ToLower();
            string path = Path.Combine(folder, name);

            ImageFormat fmt;
            switch (format.ToLower())
            {
                case "jpg":
                case "jpeg": fmt = ImageFormat.Jpeg; break;
                case "bmp":  fmt = ImageFormat.Bmp;  break;
                default:     fmt = ImageFormat.Png;  break;
            }

            bmp.Save(path, fmt);
            return path;
        }

        public static string SaveAs(Bitmap bmp, string initialDir)
        {
            using (var dlg = new SaveFileDialog())
            {
                dlg.Title            = "保存截图";
                dlg.Filter           = "PNG图片|*.png|JPEG图片|*.jpg|BMP图片|*.bmp";
                dlg.FilterIndex      = 1;
                dlg.InitialDirectory = initialDir;
                dlg.FileName         = "Screenshot_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    string ext = Path.GetExtension(dlg.FileName).TrimStart('.').ToLower();
                    Save(bmp, Path.GetDirectoryName(dlg.FileName), ext);
                    return dlg.FileName;
                }
            }
            return null;
        }
    }
}
