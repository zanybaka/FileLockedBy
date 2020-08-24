namespace FileLockedBy.Win32.Enums
{
    internal enum SystemHandleType
    {
        Unknown = 0,
        Type = 1,
        Directory,
        SymbolicLink,
        Token,
        Process,
        Thread,
        Unknown7,
        Event,
        EventPair,
        Mutant,
        Unknown11,
        Semaphore,
        Timer,
        Profile,
        WindowStation,
        Desktop,
        Section,
        Key,
        Port,
        WaitablePort,
        Unknown21,
        Unknown22,
        Unknown23,
        Unknown24,
        //OB_TYPE_CONTROLLER,
        //OB_TYPE_DEVICE,
        //OB_TYPE_DRIVER,
        IoCompletion,
        File
    } ;
}