using System;
using System.Collections.Generic;
using System.Text;

namespace Xml
{
    public class MemoryMapNotFoundException : Exception
    {
        public MemoryMapNotFoundException()
        {
        }

        public MemoryMapNotFoundException(string message) : base(message)
        {
        }

        public MemoryMapNotFoundException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
