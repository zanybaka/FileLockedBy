namespace FileLockedBy.Enums
{
    public enum Error
    {
        None = 0,
        InvalidArguments = -1,
        TargetFileNotFound = -2,
        InvalidPid = -3,
        DuplicateHandleFails = -4,
        GetHandleInformationFails = -5,
        HandleProtectedFromClosed = -6,
        HandleNotFound = -7,
    }
}