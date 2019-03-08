using System;

namespace MTUComm.MemoryMap
{
    public class MemoryRegisterNotAllowWrite : Exception
    {
        public MemoryRegisterNotAllowWrite()
        {
        }

        public MemoryRegisterNotAllowWrite(string message) : base(message)
        {
        }

        public MemoryRegisterNotAllowWrite(string message, Exception inner) : base(message, inner)
        {
        }
    }
}