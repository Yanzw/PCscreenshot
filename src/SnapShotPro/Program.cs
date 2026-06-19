using System;
using System.Windows.Forms;

namespace SnapShotPro
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using (var trayApp = new UI.TrayApp())
            {
                Application.Run();
            }
        }
    }
}
