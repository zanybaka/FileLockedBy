using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using FileLockedBy.Enums;
using FileLockedBy.Registry;
using FileLockedBy.Win32.Entities;
using FileLockedBy.Win32.Processes;
using FileLockedBy.Win32.Structures;
using FileLockedBy.Win32.System;

namespace FileLockedBy
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Unlocker";
            Process currentProcess = Process.GetCurrentProcess();
            string fullPath = currentProcess.MainModule.FileName;
            string fileName = Path.GetFileName(currentProcess.MainModule.FileName);

            bool suppressUserInput = false;
            if (args.Length == 2)
            {
                suppressUserInput = args[1].ToLowerInvariant() == "-s";
            }
            else if (args.Length != 1)
            {
                Console.WriteLine($"Usage: {fileName} <file_to_unlock> [-s] - unlocks the file");
                Console.WriteLine($"Usage: {fileName} register [-s]         - integrate into Explorer");
                Console.WriteLine($"Usage: {fileName} unregister [-s]       - unintegrate from Explorer");
                Console.WriteLine($"-s     {new string(' ', fileName.Length + 22)} - Optional parameter. Suppress user input");
                return;
            }

            if (args[0].ToLowerInvariant() == "register")
            {
                ExplorerIntegration.RegisterMenuItem("Unlocker", fullPath);
                Console.WriteLine("Registered.");
                if (!suppressUserInput) Console.ReadKey();
                return;
            }

            if (args[0].ToLowerInvariant() == "unregister")
            {
                ExplorerIntegration.UnregisterMenuItem("Unlocker");
                Console.WriteLine("Unregistered.");
                if (!suppressUserInput) Console.ReadKey();
                return;
            }

            Console.WriteLine("Unlocking begin...");
            bool found = false;
            string path = args[0];
            var processes = Unlocker.FindLockerProcesses(path);
            foreach (RM_PROCESS_INFO info in processes)
            {
                Console.WriteLine($"Closing handle locked by {info.strAppName}...");
                using (SmartPtr sptr = SystemInformation.GetSystemHandleInformation())
                {
                    var information = (SystemHandlesInformation)Marshal.PtrToStructure(sptr.Pointer, typeof(SystemHandlesInformation));
                    int handleCount = information.Count;
                    var process = Process.GetProcessById(info.Process.dwProcessId);
                    var infoEnumerator = ProcessHelper.GetCurrentProcessOpenFilesEnumerator(info.Process.dwProcessId, sptr, handleCount);
                    while (infoEnumerator.MoveNext())
                    {
                        FileHandleInfo current = infoEnumerator.Current;
                        if (string.Compare(path, current.FileSystemInfo.FullName, StringComparison.OrdinalIgnoreCase) != 0) continue;
                        Console.WriteLine($"Found! {process.ProcessName} -> {process.MainModule.FileName}");
                        found = true;
                        var result = ProcessHelper.CloseHandle(process, current, currentProcess);
                        Console.WriteLine(result == 0 ? "Success." : $"Error: {Enum.GetName(typeof(Error), result)}");
                    }
                }
            }
            if (!found) Console.WriteLine("Nothing found. Can't unlock or the file is already unlocked.");
            Console.WriteLine("End.");
            if (!suppressUserInput) Console.ReadKey();
        }
    }
}
