using System;
using System.Threading;
using System.Windows.Forms;

namespace WinCaffeine
{
    static class Program
    {
        static Mutex mutex = new Mutex(true, "{695F514F-3C2D-423F-AABF-528D590CE910}");

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (mutex.WaitOne(TimeSpan.Zero, true))
                Application.Run(new AboutForm());
            else
                MessageBox.Show("An instance of WinCaffeine is already running!", "WinCaffeine", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
