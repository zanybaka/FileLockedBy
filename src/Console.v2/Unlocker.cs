using System;
using System.Linq;
using FileLockedBy.Win32.System;

namespace FileLockedBy
{
    public class Unlocker
    {
        private static uint MAX_LOCK_PROCESS_COUNT = 10;

        public static RM_PROCESS_INFO[] FindLockerProcesses(string[] files)
        {
            int handle;
            if (RestartManagerNativeMethods.RmStartSession(out handle, 0, strSessionKey: Guid.NewGuid().ToString()) != RmResult.ERROR_SUCCESS)
                throw new Exception("Could not begin session. Unable to determine file lockers.");

            try
            {
                string[] resources = files.ToArray();

                if (RestartManagerNativeMethods.RmRegisterResources(handle, (uint)resources.LongLength, resources, 0, null, 0, null) != RmResult.ERROR_SUCCESS)
                    throw new Exception("Could not register resource.");

                uint arraySize = MAX_LOCK_PROCESS_COUNT;
                RmResult result;
                do
                {
                    var array = new RM_PROCESS_INFO[arraySize];
                    uint arrayCount;
                    RM_REBOOT_REASON lpdwRebootReasons;
                    result = RestartManagerNativeMethods.RmGetList(handle, out arrayCount, ref arraySize, array, out lpdwRebootReasons);
                    if (result == RmResult.ERROR_SUCCESS)
                    {
                        Array.Resize(ref array, (int)arrayCount);
                        return array;
                    }
                    else if (result == RmResult.ERROR_MORE_DATA)
                    {
                        arraySize = arrayCount;
                    }
                    else
                    {
                        throw new Exception("Could not list processes locking resource. Failed to get size of result.");
                    }
                } while (result != RmResult.ERROR_SUCCESS);
            }
            finally
            {
                RestartManagerNativeMethods.RmEndSession(handle);
            }
            return new RM_PROCESS_INFO[0];
        }
    }
}