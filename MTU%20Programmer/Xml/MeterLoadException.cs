using System;

namespace Xml
{
    public class MeterLoadException: Exception
    {
        public MeterLoadException()
        {
        }

        public MeterLoadException(string message) : base(message)
        {
        }

        public MeterLoadException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
