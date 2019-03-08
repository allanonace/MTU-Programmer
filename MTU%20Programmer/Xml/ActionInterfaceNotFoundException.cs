using System;
using System.Collections.Generic;
using System.Text;

namespace Xml
{
    class ActionInterfaceNotFoundException : Exception
    {
        public ActionInterfaceNotFoundException()
        {
        }

        public ActionInterfaceNotFoundException(string message) : base(message)
        {
        }

        public ActionInterfaceNotFoundException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
