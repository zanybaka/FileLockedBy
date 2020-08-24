using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using FileLockedBy.Enums;
using FileLockedBy.Win32.Entities;
using FileLockedBy.Win32.Processes;
using FileLockedBy.Win32.Structures;
using FileLockedBy.Win32.System;

namespace FileLockedBy
{
    internal static class ConsoleApp
    {
        public static Error Run(params string[] args)
        {
            if (args == null || args.Length != 1)
            {
                return Error.InvalidArguments;
            }
            string fileName = args[0];
            if (!File.Exists(fileName))
            {
                return Error.TargetFileNotFound;
            }

            string[] exclusionProcesses = File.ReadAllLines("ExclusionProcesses.txt");
            string[] exclusionFolders = File.ReadAllLines("ExclusionFolders.txt");
            Process currentProcess = Process.GetCurrentProcess();
            Process[] processes = Process
                .GetProcesses()
                .Where(x =>
                    x.HandleCount > 0 &&
                    !exclusionProcesses.Contains(x.ProcessName.ToLowerInvariant()))
                .ToArray();
            Console.WriteLine($"Found {processes.Length} processes.");
            using (SmartPtr sptr = SystemInformation.GetSystemHandleInformation())
            {
                var information = (SystemHandlesInformation)Marshal.PtrToStructure(sptr.Pointer, typeof(SystemHandlesInformation));
                int handleCount = information.Count;
                //int handleCount = Marshal.ReadInt32(sptr.Pointer);
                for (int i = 0; i < processes.Length; i++)
                {
                    Process process = processes[i];
                    if (!ProcessHelper.IsCurrentUserProcess(process.Id))
                    {
                        continue;
                    }

                    if (exclusionFolders.Any(y => process.MainModule.FileName.ToLowerInvariant().StartsWith(y)))
                    {
                        continue;
                    }

                    Console.Write("#");
                    var infoEnumerator = ProcessHelper.GetCurrentProcessOpenFilesEnumerator(process.Id, sptr, handleCount);
                    while (infoEnumerator.MoveNext())
                    {
                        FileHandleInfo current = infoEnumerator.Current;
                        if (string.Compare(fileName, current.FileSystemInfo.FullName, StringComparison.OrdinalIgnoreCase) != 0)
                        {
                            continue;
                        }

                        Console.WriteLine();
                        Console.WriteLine($"Found!: {process.ProcessName} -> {process.MainModule.FileName}");

                        return ProcessHelper.CloseHandle(process, current, currentProcess);
                    }
                }
            }

            return Error.HandleNotFound;
        }
    }
}