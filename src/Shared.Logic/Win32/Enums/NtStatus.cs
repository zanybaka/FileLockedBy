namespace FileLockedBy.Win32.Enums
{
    internal enum NtStatus
    {
        Success = 0x00000000,
        BufferOverflow = unchecked((int) 0x80000005L),
        InfoLengthMismatch = unchecked((int) 0xC0000004L)
    }
}