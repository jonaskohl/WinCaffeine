using System.Runtime.InteropServices;

namespace System.Windows.Forms
{
    public class LinkLabelEx : LinkLabel
    {
        private const int IDC_HAND = 32649;
        private const int WM_SETCURSOR = 32;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

        [DllImport("user32.dll")]
        public static extern int SetCursor(IntPtr hCursor);

        //private static readonly Cursor SystemHandCursor = new Cursor(LoadCursor(IntPtr.Zero, IDC_HAND));
        private static readonly IntPtr Hwnd_IDC_HAND = LoadCursor(IntPtr.Zero, IDC_HAND);

        protected override void WndProc(ref Message msg)
        {
            if (msg.Msg == WM_SETCURSOR)
            {
                SetCursor(Hwnd_IDC_HAND);

                msg.Result = IntPtr.Zero;
                return;
            }

            base.WndProc(ref msg);
        }

        //protected override void OnMouseMove(MouseEventArgs e)
        //{
        //    base.OnMouseMove(e);

        //    // If the base class decided to show the ugly hand cursor
        //    if (OverrideCursor == Cursors.Hand)
        //    {
        //        // Show the system hand cursor instead
        //        OverrideCursor = SystemHandCursor;
        //    }
        //}
    }
}
