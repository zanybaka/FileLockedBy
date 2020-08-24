using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using FileLockedBy.Win32.Entities;
using FileLockedBy.Win32.Enums;
using FileLockedBy.Win32.Structures;

namespace FileLockedBy.Win32.System
{
    internal static class HandleInformation
    {
        private static readonly Dictionary<string, SystemHandleType> handleTypeTokens
            = new Dictionary<string, SystemHandleType>();

        static HandleInformation()
        {
            //handleTypeTokens.Add("", SystemHandleType.Unknown);
            handleTypeTokens.Add("", SystemHandleType.Type);
            handleTypeTokens.Add("Directory", SystemHandleType.Directory);
            handleTypeTokens.Add("SymbolicLink", SystemHandleType.SymbolicLink);
            handleTypeTokens.Add("Token", SystemHandleType.Token);
            handleTypeTokens.Add("Process", SystemHandleType.Process);
            handleTypeTokens.Add("Thread", SystemHandleType.Thread);
            handleTypeTokens.Add("Unknown7", SystemHandleType.Unknown7);
            handleTypeTokens.Add("Event", SystemHandleType.Event);
            handleTypeTokens.Add("EventPair", SystemHandleType.EventPair);
            handleTypeTokens.Add("Mutant", SystemHandleType.Mutant);
            handleTypeTokens.Add("Unknown11", SystemHandleType.Unknown11);
            handleTypeTokens.Add("Semaphore", SystemHandleType.Semaphore);
            handleTypeTokens.Add("Timer", SystemHandleType.Timer);
            handleTypeTokens.Add("Profile", SystemHandleType.Profile);
            handleTypeTokens.Add("WindowStation", SystemHandleType.WindowStation);
            handleTypeTokens.Add("Desktop", SystemHandleType.Desktop);
            handleTypeTokens.Add("Section", SystemHandleType.Section);
            handleTypeTokens.Add("Key", SystemHandleType.Key);
            handleTypeTokens.Add("Port", SystemHandleType.Port);
            handleTypeTokens.Add("WaitablePort", SystemHandleType.WaitablePort);
            handleTypeTokens.Add("Unknown21", SystemHandleType.Unknown21);
            handleTypeTokens.Add("Unknown22", SystemHandleType.Unknown22);
            handleTypeTokens.Add("Unknown23", SystemHandleType.Unknown23);
            handleTypeTokens.Add("Unknown24", SystemHandleType.Unknown24);
            //OB_TYPE_CONTROLLER,
            //OB_TYPE_DEVICE,
            //OB_TYPE_DRIVER,
            handleTypeTokens.Add("IoCompletion", SystemHandleType.IoCompletion);
            handleTypeTokens.Add("File", SystemHandleType.File);
        }

        public static bool GetHandleType(IntPtr handle, int processId, out SystemHandleType handleType)
        {
            string token = GetHandleTypeToken(handle, processId);
            if (handleTypeTokens.ContainsKey(token))
            {
                handleType = handleTypeTokens[token];
                return true;
            }
            handleType = SystemHandleType.Unknown;
            return false;
        }

        private static string GetHandleTypeToken(IntPtr handle, int processId)
        {
            Process currentProcess = Process.GetCurrentProcess();

            bool remote = (processId != currentProcess.Id);

            SafeProcessHandle processHandle = null;

            SafeObjectHandle objectHandle = null;

            try
            {
                if (remote)
                {
                    processHandle = NativeMethods.OpenProcess(ProcessAccessRights.DuplicateHandle, true, processId);

                    if (NativeMethods.DuplicateHandle(processHandle.DangerousGetHandle(), handle, currentProcess.Handle,
                                                      out objectHandle, 0, false,
                                                      DuplicateHandleOptions.SameAccess))
                    {
                        handle = objectHandle.DangerousGetHandle();
                    }
                }
                return GetHandleTypeToken(handle);
            }

            finally
            {
                if (remote)
                {
                    if (processHandle != null)
                    {
                        processHandle.Close();
                    }

                    if (objectHandle != null)
                    {
                        objectHandle.Close();
                    }
                }
            }
        }

        private static string GetHandleTypeToken(IntPtr handle)
        {
            int length;
            NativeMethods.NtQueryObject(
                handle, ObjectInformationClass.ObjectTypeInformation, IntPtr.Zero, 0,
                out length
                );

            using (SmartPtr sptr = new SmartPtr())
            {
                sptr.Allocate(length);
                if (NativeMethods.NtQueryObject(
                        handle, ObjectInformationClass.ObjectTypeInformation,
                        sptr.Pointer, length, out length
                        ) == NtStatus.Success
                    )
                {
                    ObjectTypeInformation oti =
                        (ObjectTypeInformation) Marshal.PtrToStructure(sptr.Pointer, typeof (ObjectTypeInformation));
                    UnicodeString unicodeType = oti.Name;
                    string typeName = unicodeType.GetValue();
                    return typeName;
                }
            }
            return string.Empty;
        }
    }
}