using System;
using FileLockedBy.Enums;

namespace FileLockedBy
{
    class Program
    {
        [STAThread]
        static void Main(params string[] args)
        {
            Console.WriteLine("Unlocking...");
            Error result = ConsoleApp.Run(args);
            Console.WriteLine();
            Console.WriteLine(result == 0 ? "Success." : $"Error: {Enum.GetName(typeof(Error), result)}");
            Environment.Exit((int) result);
        }
    }
}