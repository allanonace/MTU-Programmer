using System;

namespace MTUComm.MemoryMap
{
    public class MemoryOverloadsAreReadOnly : Exception
    {
        public MemoryOverloadsAreReadOnly()
        {
        }

        public MemoryOverloadsAreReadOnly(string message) : base(message)
        {
        }

        public MemoryOverloadsAreReadOnly(string message, Exception inner) : base(message, inner)
        {
        }
    }
}