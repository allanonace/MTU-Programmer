using System;
using System.Collections.Generic;
using System.Text;

namespace Xml
{
    class InterfaceLoadException : Exception
    {
        public InterfaceLoadException()
        {
        }

        public InterfaceLoadException(string message) : base(message)
        {
        }

        public InterfaceLoadException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
