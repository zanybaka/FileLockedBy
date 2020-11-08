using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        private static bool SuppressUserInput = false;

        static void Main(string[] args)
        {
            Console.Title = "Unlocker";
            Process currentProcess = Process.GetCurrentProcess();
            string  fullPath       = currentProcess.MainModule.FileName;
            string  fileName       = Path.GetFileName(currentProcess.MainModule.FileName);

            if (args.Length == 2)
            {
                SuppressUserInput = args[1].ToLowerInvariant() == "-s";
            }
            else if (args.Length != 1)
            {
                Console.WriteLine($"Usage: {fileName} <file_to_unlock> [-s] - unlocks the file/directory");
                Console.WriteLine($"Usage: {fileName} register [-s]         - integrate into Explorer");
                Console.WriteLine($"Usage: {fileName} unregister [-s]       - unintegrate from Explorer");
                Console.WriteLine($"-s     {new string(' ', fileName.Length + 22)} - Optional parameter. Suppress user input");
                return;
            }

            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;

            if (args[0].ToLowerInvariant() == "register")
            {
                ExplorerIntegration.RegisterMenuItem("Unlocker", fullPath);
                Console.WriteLine("Registered.");
                if (!SuppressUserInput) Console.ReadKey();
                return;
            }

            if (args[0].ToLowerInvariant() == "unregister")
            {
                ExplorerIntegration.UnregisterMenuItem("Unlocker");
                Console.WriteLine("Unregistered.");
                if (!SuppressUserInput) Console.ReadKey();
                return;
            }

            bool found;
            Console.WriteLine("Unlocking begin...");
            string path = args[0];
            if (File.GetAttributes(path).HasFlag(FileAttributes.Directory))
            {
                if (!Path.EndsInDirectorySeparator(path))
                {
                    path += Path.DirectorySeparatorChar;
                }

                string[] files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
                found = UnlockFiles(files, currentProcess);
            }
            else
            {
                found = UnlockFile(path, currentProcess);
            }

            if (!found) Console.WriteLine("Nothing found. Can't unlock or the file/directory is already unlocked.");
            Console.WriteLine("End.");
            if (!SuppressUserInput) Console.ReadKey();
        }

        private static bool UnlockFile(string file, Process currentProcess)
        {
            return UnlockFiles(new[] { file }, currentProcess);
        }

        private static bool UnlockFiles(string[] files, Process currentProcess)
        {
            bool found     = false;
            var  processes = Unlocker.FindLockerProcesses(files);

            foreach (RM_PROCESS_INFO info in processes)
            {
                Console.WriteLine($"Closing handle locked by {info.strAppName}...");
                using (SmartPtr sptr = SystemInformation.GetSystemHandleInformation())
                {
                    var information = (SystemHandlesInformation) Marshal.PtrToStructure(sptr.Pointer, typeof(SystemHandlesInformation));
                    int handleCount = information.Count;
                    var process = Process.GetProcessById(info.Process.dwProcessId);
                    var infoEnumerator = ProcessHelper.GetCurrentProcessOpenFilesEnumerator(info.Process.dwProcessId, sptr, handleCount);
                    Dictionary<string, bool> skip = new Dictionary<string, bool>();
                    while (infoEnumerator.MoveNext())
                    {
                        FileHandleInfo current = infoEnumerator.Current;
                        skip.TryGetValue(current.FileSystemInfo.FullName, out bool skipped);
                        if (skipped
                            || files.All(file => string.Compare(file, current.FileSystemInfo.FullName, StringComparison.OrdinalIgnoreCase) != 0))
                        {
                            continue;
                        }

                        Console.WriteLine(
                            $"Found locked file {current.FileSystemInfo.FullName}! {process.ProcessName} -> {process.MainModule.FileName}");
                        found                                 = true;
                        skip[current.FileSystemInfo.FullName] = true;
                        var result = ProcessHelper.CloseHandle(process, current, currentProcess);
                        Console.WriteLine(result == 0 ? "Success." : $"Error: {Enum.GetName(typeof(Error), result)}");
                    }
                }
            }

            return found;
        }

        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.IsTerminating && !SuppressUserInput)
            {
                Console.WriteLine(e.ExceptionObject.ToString());
                Console.ReadKey();
            }
        }
    }
}