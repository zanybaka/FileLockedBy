using System;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace FileLockedBy.Registry
{
    public class ExplorerIntegration
    {
        public static void RegisterMenuItem(string app, string path)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new NotSupportedException("The platform is not supported.");
            }

            RegistryKey classes = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\Classes", writable: true);
            RegistryKey asterisk = classes.OpenSubKey("*", writable: true) ?? classes.CreateSubKey("*");
            RegistryKey shell = asterisk.OpenSubKey("shell", writable: true) ?? asterisk.CreateSubKey("shell");
            RegistryKey unlock = shell.CreateSubKey(app, writable: true);
            unlock.SetValue("Icon", $"\"{path}\"");
            RegistryKey command = unlock.CreateSubKey("command");
            command.SetValue("", $"\"{path}\" \"%1\"");
            command.Close();
            unlock.Close();
            shell.Close();
        }

        public static void UnregisterMenuItem(string app)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new NotSupportedException("The platform is not supported.");
            }

            RegistryKey shell = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\Classes\\*\\shell", writable: true);
            shell.DeleteSubKeyTree(app, false);
            shell.Close();
        }
    }
}