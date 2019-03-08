using System;

namespace Xml
{
    public class GlobalLoadException: Exception
    {
        public GlobalLoadException()
        {
        }

        public GlobalLoadException(string message) : base(message)
        {
        }

        public GlobalLoadException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
