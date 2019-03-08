using System;

namespace Xml
{
    public class MtuLoadException: Exception
    {
        public MtuLoadException()
        {
        }

        public MtuLoadException(string message) : base(message)
        {
        }

        public MtuLoadException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
