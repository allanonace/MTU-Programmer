using System;
using System.Collections.Generic;
using System.Text;

namespace Xml
{
    public class InterfaceNotFoundException : Exception
    {
        public InterfaceNotFoundException()
        {
        }

        public InterfaceNotFoundException(string message) : base(message)
        {
        }

        public InterfaceNotFoundException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
