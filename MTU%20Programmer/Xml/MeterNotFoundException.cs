using System;

namespace Xml
{
    public class MeterNotFoundException: Exception
    {
        public MeterNotFoundException()
        {
        }

        public MeterNotFoundException(string message) : base(message)
        {
        }

        public MeterNotFoundException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
