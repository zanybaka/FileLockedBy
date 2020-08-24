using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FileLockedBy.Win32.Entities
{
    public class SmartPtr : IDisposable
    {
        private int size;
        private IntPtr pointer = IntPtr.Zero;

        public SmartPtr()
        {
            RuntimeHelpers.PrepareConstrainedRegions();
        }

        public int Size
        {
            get { return size; }
        }

        public IntPtr Pointer
        {
            get { return pointer; }
        }

        #region IDisposable Members

        public void Dispose()
        {
            // CER guarantees that the allocated memory is freed,
            // if an asynchronous exception occurs.
            Marshal.FreeHGlobal(pointer);
        }

        #endregion

        public IntPtr Allocate(int sizeToAllocate)
        {
            this.size = sizeToAllocate;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                // CER guarantees that the address of the allocated
                // memory is actually assigned to pointer if an
                // asynchronous exception occurs.
                pointer = Marshal.AllocHGlobal(sizeToAllocate);
            }
            return pointer;
        }

        public IntPtr ReAllocate(int sizeToAllocate)
        {
            this.size = sizeToAllocate;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                // CER guarantees that the previous allocation is freed,
                // and that the newly allocated memory address is
                // assigned to pointer if an asynchronous exception occurs.
                Marshal.FreeHGlobal(pointer);
                pointer = Marshal.AllocHGlobal(sizeToAllocate);
            }
            return pointer;
        }

        public SmartPtr Clone()
        {
            SmartPtr clone = new SmartPtr();
            clone.Allocate(size);
            byte[] byteArray = new byte[size];
            Marshal.Copy(Pointer, byteArray, 0, size);
            Marshal.Copy(byteArray, 0, clone.Pointer, size);
            return clone;
        }
    }
}