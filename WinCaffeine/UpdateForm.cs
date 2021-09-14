using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace WinCaffeine
{
    public partial class UpdateForm : Form
    {
        public UpdateForm()
        {
            InitializeComponent();

            Load += UpdateForm_Load;
        }

        private void UpdateForm_Load(object sender, EventArgs e)
        {
            var fvi = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            var Version = fvi.ProductVersion;
            webBrowser1.ObjectForScripting = new ObjectForScripting();
            webBrowser1.Navigate("http://wincaffeine.jonaskohl.de/update.php?embedded=true&current_version=" + Version);
        }
    }

    [ComVisible(true)]
    public class ObjectForScripting
    {
        public void openDefault(string path)
        {
            var s = "\"" + Regex.Replace(path, @"(\\+)$", @"$1$1") + "\"";
            Process.Start("explorer", s);
        }
    }

}
