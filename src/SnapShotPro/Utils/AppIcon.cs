using System;
using System.Drawing;
using System.Reflection;

namespace SnapShotPro.Utils
{
    /// <summary>
    /// 从内嵌资源加载应用图标（app.ico），按需返回指定尺寸。
    /// </summary>
    static class AppIcon
    {
        const string ResourceName = "SnapShotPro.app.ico";

        /// <summary>加载最接近指定尺寸的图标帧；失败时返回 null。</summary>
        public static Icon Load(int size)
        {
            try
            {
                var asm = Assembly.GetExecutingAssembly();
                using (var stream = asm.GetManifestResourceStream(ResourceName))
                {
                    if (stream == null) return null;
                    return new Icon(stream, new Size(size, size));
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
