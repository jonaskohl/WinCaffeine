using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Reflection;

namespace WinCaffeine
{
    public partial class AboutForm : Form
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern uint SetThreadExecutionState(uint esFlags);

        const uint ES_AWAYMODE_REQUIRED = 0x00000040;
        const uint ES_CONTINUOUS = 0x80000000;
        const uint ES_DISPLAY_REQUIRED = 0x80000002;
        const uint ES_SYSTEM_REQUIRED = 0x80000001;

        private bool IsWin10 => Environment.OSVersion.Version.Major >= 10;
        private bool IsWinXP => Environment.OSVersion.Version.Major < 6;

        private DarkModeWatcher watcher;

        public AboutForm()
        {
            InitializeComponent();
            if (IsWin10)
            {
                watcher = new DarkModeWatcher();
                watcher.DarkModeChanged += Watcher_DarkModeChanged;
            }
            Opacity = 0;
            notifyIcon1.ContextMenu = contextMenu1;
            UpdateNotifyIcon();
            Shown += new EventHandler(Form1_Shown);
            var fvi = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            label1.Text = string.Format(label1.Text,
                fvi.ProductVersion,
                fvi.LegalCopyright
            );
        }

        private void Watcher_DarkModeChanged(object sender, EventArgs e)
        {
            UpdateNotifyIcon();
        }

        void Form1_Shown(object sender, EventArgs e)
        {
            Hide();
            Opacity = 1;
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ToggleActivated();
            }
        }

        private void ToggleActivated()
        {
            timer1.Enabled = !timer1.Enabled;
            if (timer1.Enabled)
                RefreshWakeLock();
            else
                DisableWakeLock();

            UpdateNotifyIcon();
        }

        private void UpdateNotifyIcon()
        {
            var isDark = watcher?.IsDarkMode ?? false;
            var isCaffeinated = timer1.Enabled;
            notifyIcon1.Icon =
                IsWin10 ? (
                    isDark ? (
                        isCaffeinated ? Properties.Resources.fluent_on_w : Properties.Resources.fluent_off_w
                    ) : (
                        isCaffeinated ? Properties.Resources.fluent_on_b : Properties.Resources.fluent_off_b
                    )
                ) : (
                    IsWinXP ? (
                        isCaffeinated ? Properties.Resources.luna_on : Properties.Resources.luna_off
                    ) : (
                        isCaffeinated ? Properties.Resources.coffee_full : Properties.Resources.coffee_empty
                    )
                );

            notifyIcon1.Text = "WinCaffeine\r\nCurrent status: " + (timer1.Enabled ? "Stay awake" : "Allow sleep");
            Icon = notifyIcon1.Icon;
        }

        private void DisableWakeLock()
        {
            SetThreadExecutionState(ES_CONTINUOUS);
        }

        private void RefreshWakeLock()
        {
            SetThreadExecutionState(ES_CONTINUOUS | ES_DISPLAY_REQUIRED);
        }

        private void menuItem1_Click(object sender, EventArgs e)
        {
            Show();
        }

        private void menuItem3_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            RefreshWakeLock();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Hide();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://wincaffeine.jonaskohl.de/");
        }
    }
}
