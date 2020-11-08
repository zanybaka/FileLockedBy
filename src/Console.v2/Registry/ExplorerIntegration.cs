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

            void Register(RegistryKey registryKey)
            {
                using RegistryKey shell  = registryKey.OpenSubKey("shell", writable: true) ?? registryKey.CreateSubKey("shell");
                using RegistryKey unlock = shell.CreateSubKey(app, writable: true);
                unlock.SetValue("Icon", $"\"{path}\"");
                using RegistryKey command = unlock.CreateSubKey("command");
                command.SetValue("", $"\"{path}\" \"%1\"");
            }

            using RegistryKey classes = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\Classes", writable: true);
            using RegistryKey asterisk = classes.OpenSubKey("*", writable: true) ?? classes.CreateSubKey("*");
            Register(asterisk);
            using RegistryKey directory = classes.OpenSubKey("Directory", writable: true) ?? classes.CreateSubKey("Directory");
            Register(directory);
        }

        public static void UnregisterMenuItem(string app)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new NotSupportedException("The platform is not supported.");
            }

            using RegistryKey shellFiles = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\Classes\\*\\shell", writable: true);
            shellFiles?.DeleteSubKeyTree(app, throwOnMissingSubKey: false);

            using RegistryKey shellDirectory = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\Classes\\Directory\\shell", writable: true);
            shellDirectory?.DeleteSubKeyTree(app, throwOnMissingSubKey: false);
        }
    }
}