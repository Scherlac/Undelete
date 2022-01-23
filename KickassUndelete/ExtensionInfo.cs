using Microsoft.Win32;
using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace KickassUndelete {
	public class ExtensionInfo {
		public Icon Image { get; private set; }
		public string SystemName { get; private set; }
		public string FriendlyName { get; private set; }
		public bool UnrecognisedExtension { get; private set; }
		public ExtensionInfo(string extension) {
			SystemName = extension;
			FriendlyName = extension;
			UnrecognisedExtension = true;
#if !MONO
			if (!string.IsNullOrEmpty(extension)) {
				RegistryKey rkRoot = Registry.ClassesRoot;
				RegistryKey extkey = rkRoot.OpenSubKey(extension);
				if (extkey != null) {
					object keyPath = extkey.GetValue("");
					if (keyPath != null) {
						RegistryKey rkFriendlyName = rkRoot.OpenSubKey(keyPath.ToString());
						if (rkFriendlyName != null) {
							object friendlyName = rkFriendlyName.GetValue("");
							if (friendlyName != null) {
								FriendlyName = friendlyName.ToString();
								UnrecognisedExtension = false;
							}
							rkFriendlyName.Close();
						}

						string defaultIcon = keyPath.ToString() + "\\DefaultIcon";
						RegistryKey rkFileIcon = rkRoot.OpenSubKey(defaultIcon);
						if (rkFileIcon != null) {
							object iconPath = rkFileIcon.GetValue("");
							if (iconPath != null) {
								string fileParam = iconPath.ToString().Replace("\"", "");
								Image = ExtractIconFromFile(fileParam);
							}
							rkFileIcon.Close();
						}
					}
					extkey.Close();
				}
				rkRoot.Close();
			}
#endif
		}

#if !MONO
		[DllImport("shell32.dll", EntryPoint = "ExtractIconA",
				CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
		private static extern IntPtr ExtractIcon
				(int hInst, string lpszExeFileName, int nIconIndex);
#endif

		public static Icon ExtractIconFromFile(string fileAndParam) {
#if !MONO
			try {
				int commaIndex = fileAndParam.IndexOf(",");
				if (commaIndex > 0) {
					string filename = fileAndParam.Substring(0, commaIndex);
					int index = int.Parse(fileAndParam.Substring(commaIndex + 1));
					//Gets the handle of the icon.
					IntPtr lIcon = ExtractIcon(0, filename, index);

					//Gets the real icon.
					return Icon.FromHandle(lIcon);
				}
			} catch (Exception exc) { Console.WriteLine(exc); }
#endif
			return null;
		}
	}
}
