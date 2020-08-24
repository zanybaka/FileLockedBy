using System.Runtime.InteropServices;

namespace FileLockedBy.Win32.Structures
{
    /// <summary>
    /// http://www.jasinskionline.com/TechnicalWiki/(X(1)S(42yxxr55sbwjm4yfppfvg355))/GENERIC_MAPPING-WinApi-Struct.ashx?HL=generic,mapping
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct GenericMapping
    {
        public int GenericRead;
        public int GenericWrite;
        public int GenericExecute;
        public int GenericAll;
    }
}