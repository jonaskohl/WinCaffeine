using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Security.Principal;
using System.Text;

namespace WinCaffeine
{
    public class DarkModeWatcher
    {
		private const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
		private const string RegistryValueName = "SystemUsesLightTheme";

		public event EventHandler DarkModeChanged;
		public bool IsDarkMode => GetWindowsTheme();

		public DarkModeWatcher()
        {
			WatchTheme();
        }

		private void WatchTheme()
		{
			var currentUser = WindowsIdentity.GetCurrent();
			string query = string.Format(
				CultureInfo.InvariantCulture,
				@"SELECT * FROM RegistryValueChangeEvent WHERE Hive = 'HKEY_USERS' AND KeyPath = '{0}\\{1}' AND ValueName = '{2}'",
				currentUser.User.Value,
				RegistryKeyPath.Replace(@"\", @"\\"),
				RegistryValueName);

			try
			{
				var watcher = new ManagementEventWatcher(query);
				watcher.EventArrived += (sender, args) =>
				{
					bool newWindowsTheme = GetWindowsTheme();
					DarkModeChanged?.Invoke(this, EventArgs.Empty);
				};

				// Start listening for events
				watcher.Start();
			}
			catch (Exception)
			{
				// This can fail on Windows 7
			}

			bool initialTheme = GetWindowsTheme();
		}

		private static bool GetWindowsTheme()
		{
			using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath))
			{
				object registryValueObject = key?.GetValue(RegistryValueName);
				if (registryValueObject == null)
					return false;

				int registryValue = (int)registryValueObject;

				return registryValue <= 0;
			}
		}
	}
}
