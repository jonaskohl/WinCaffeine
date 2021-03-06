using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Win32;

namespace WinCaffeine
{
    public partial class AboutForm : Form
    {
        private const string SETTINGS_KEY = @"Software\Jonas Kohl\WinCaffeine";

        public enum IconStyle
        {
            SystemDefault,
            Luna,
            Aero,
            AeroLegacy,
            Fluent
        }

        enum IconStyleInternal
        {
            luna = 1,
            aero = 2,
            aeroLegacy = 3,
            fluent = 4
        }

        struct IconStyleMapping
        {
            public IconStyle Style { get; set; }
            public string Caption { get; set; }
            public string ID { get; set; }

            public override string ToString()
            {
                return Caption;
            }
        }

        static readonly IconStyleMapping[] IconStyleMappings = new[]
        {
            new IconStyleMapping() { ID = "{00000000-0000-0000-0000-000000000000}", Style = IconStyle.SystemDefault, Caption = "System default" },
            new IconStyleMapping() { ID = "{19A33D0A-C955-4A1B-BB7C-0B6BD45163B2}", Style = IconStyle.Luna, Caption = "Luna" },
            new IconStyleMapping() { ID = "{41EF04D8-946B-49B0-80C8-BD819156D19F}", Style = IconStyle.Aero, Caption = "Aero" },
            new IconStyleMapping() { ID = "{B46BBF54-AF48-4724-BB53-738E4D95610C}", Style = IconStyle.AeroLegacy, Caption = "Aero (Legacy)" },
            new IconStyleMapping() { ID = "{1002D7DF-5D42-46BB-B427-DE74BB7FECEA}", Style = IconStyle.Fluent, Caption = "Fluent" }
        };

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern uint SetThreadExecutionState(uint esFlags);

        const uint ES_AWAYMODE_REQUIRED = 0x00000040;
        const uint ES_CONTINUOUS = 0x80000000;
        const uint ES_DISPLAY_REQUIRED = 0x80000002;
        const uint ES_SYSTEM_REQUIRED = 0x80000001;

        private bool IsWin10 => Environment.OSVersion.Version.Major >= 10;
        private bool IsWinXP => Environment.OSVersion.Version.Major < 6;
        private IconStyleMapping GetSelectedIconStyleMapping => comboBox1.SelectedItem != null ? (IconStyleMapping)comboBox1?.SelectedItem : IconStyleMappings[0];
        private IconStyleInternal iconStyle
        {
            get
            {
                var s = GetSelectedIconStyleMapping.Style;
                switch (s)
                {
                    case IconStyle.Luna:
                        return IconStyleInternal.luna;
                    case IconStyle.Aero:
                        return IconStyleInternal.aero;
                    case IconStyle.AeroLegacy:
                        return IconStyleInternal.aeroLegacy;
                    case IconStyle.Fluent:
                        return IconStyleInternal.fluent;
                    case IconStyle.SystemDefault:
                    default:
                        return IsWin10 ? IconStyleInternal.fluent : (
                            IsWinXP ? IconStyleInternal.luna : IconStyleInternal.aero
                        );
                }
            }
        }

        private IconStyleInternal GetIconStyleInternal(IconStyle style)
        {
            switch (style)
            {
                case IconStyle.Luna:
                    return IconStyleInternal.luna;
                case IconStyle.Aero:
                    return IconStyleInternal.aero;
                case IconStyle.AeroLegacy:
                    return IconStyleInternal.aeroLegacy;
                case IconStyle.Fluent:
                    return IconStyleInternal.fluent;
                case IconStyle.SystemDefault:
                default:
                    return IconStyleInternal.luna;
            }
        }

        private DarkModeWatcher watcher;
        private string Version;

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
            Shown += new EventHandler(Form1_Shown);
            var fvi = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            Version = fvi.ProductVersion;
            label1.Text = string.Format(label1.Text,
                fvi.ProductVersion,
                fvi.LegalCopyright
            );
            var s = LoadSettings();
            checkBox1.Checked = Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Run", "WinCaffeine", null) != null;
            checkBox1.CheckedChanged += CheckBox1_CheckedChanged;
            comboBox1.DisplayMember = "Caption";
            comboBox1.ValueMember = "Style";
            comboBox1.DataSource = IconStyleMappings;
            comboBox1.SelectedIndex = s.ContainsKey("IconType") ? GetStyleMapping((string)s["IconType"]) : 0;
            comboBox1.SelectionChangeCommitted += ComboBox1_SelectionChangeCommitted;
            timer1.Enabled = s.ContainsKey("Active") ? (int)s["Active"] == 1 : false;
            UpdateNotifyIcon();
        }

        private int GetStyleMapping(string iconType)
        {
            IconStyleMapping mapping = IconStyleMappings[0];
            foreach (var m in IconStyleMappings)
            {
                if (m.ID == iconType)
                {
                    mapping = m;
                    break;
                }
            }
            return Array.IndexOf(IconStyleMappings, mapping);
        }

        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

            if (checkBox1.Checked)
                rk.SetValue("WinCaffeine", Application.ExecutablePath);
            else
                rk.DeleteValue("WinCaffeine", false);
        }

        private void ComboBox1_SelectionChangeCommitted(object sender, EventArgs e)
        {
            UpdateNotifyIcon();
            SaveSettings();
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
            SaveSettings();
        }

        private void UpdateNotifyIcon()
        {
            var isDark = watcher?.IsDarkMode ?? false;
            var isCaffeinated = timer1.Enabled;
            notifyIcon1.Icon = GetIcon(iconStyle, isCaffeinated, isDark);

            notifyIcon1.Text = "WinCaffeine\r\nCurrent status: " + (timer1.Enabled ? "Stay awake" : "Allow sleep");
            Icon = notifyIcon1.Icon;
        }

        private Icon GetIcon(IconStyleInternal style, bool isCaffeinated, bool isDark)
        {
            return  style == IconStyleInternal.fluent ? (
                        isDark ? (
                            isCaffeinated ? Properties.Resources.fluent_on_w : Properties.Resources.fluent_off_w
                        ) : (
                            isCaffeinated ? Properties.Resources.fluent_on_b : Properties.Resources.fluent_off_b
                        )
                    ) : (
                        style == IconStyleInternal.luna ? (
                            isCaffeinated ? Properties.Resources.luna_on : Properties.Resources.luna_off
                        ) : (
                            style == IconStyleInternal.aeroLegacy ? (
                                isCaffeinated ? Properties.Resources.coffee_full : Properties.Resources.coffee_empty
                            ) : (
                                isCaffeinated ? Properties.Resources.aero_on : Properties.Resources.aero_off
                            )
                        )
                    );
        }

        private string GetIconName(IconStyleInternal style, bool isCaffeinated, bool isDark)
        {
            return style == IconStyleInternal.fluent ? (
                        isDark ? (
                            isCaffeinated ? "fluent_on_w" : "fluent_off_w"
                        ) : (
                            isCaffeinated ? "fluent_on_b" : "fluent_off_b"
                        )
                    ) : (
                        style == IconStyleInternal.luna ? (
                            isCaffeinated ? "luna_on" : "luna_off"
                        ) : (
                            style == IconStyleInternal.aeroLegacy ? (
                                isCaffeinated ? "coffee_full" : "coffee_empty"
                            ) : (
                                isCaffeinated ? "aero_on" : "aero_off"
                            )
                        )
                    );
        }

        private void SaveSettings()
        {
            using (var key = Registry.CurrentUser.CreateSubKey(SETTINGS_KEY))
            {
                key.SetValue("IconType", GetSelectedIconStyleMapping.ID);
                key.SetValue("Active", timer1.Enabled ? 1 : 0, RegistryValueKind.DWord);
            }
        }

        private Dictionary<string, object> LoadSettings()
        {
            var d = new Dictionary<string, object>();
            using (var key = Registry.CurrentUser.CreateSubKey(SETTINGS_KEY))
                foreach (var n in key.GetValueNames())
                    d[n] = key.GetValue(n);
            return d;
        }

        private void DisableWakeLock()
        {
            SetThreadExecutionState(ES_CONTINUOUS);
        }

        private void RefreshWakeLock()
        {
            SetThreadExecutionState(ES_CONTINUOUS | ES_DISPLAY_REQUIRED | ES_SYSTEM_REQUIRED);
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

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            //Process.Start("http://wincaffeine.jonaskohl.de/update.php?current_version=" + Version);
            using (var f = new UpdateForm())
                f.ShowDialog(this);
        }

        private void comboBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            e.DrawFocusRectangle();

            const int ICOSIZE = 16;

            Icon ico = null;
            var selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

            if (e.Index > 0)
                ico = IconHelper.GetIconFromEmbeddedResource(GetIconName(GetIconStyleInternal(IconStyleMappings[e.Index].Style), true, selected), new Size(ICOSIZE, ICOSIZE));

            if (ico != null)
            {
                e.Graphics.DrawIcon(ico, new Rectangle(
                    e.Bounds.Left,
                    e.Bounds.Top,
                    16,
                    16
                ));
                ico.Dispose();
            }

            using (var b = new SolidBrush(e.ForeColor))
            e.Graphics.DrawString(comboBox1.Items[e.Index].ToString(), e.Font, b, new Point(
                    e.Bounds.Left + 18,
                    e.Bounds.Top + 2
                ));
        }
    }
}
