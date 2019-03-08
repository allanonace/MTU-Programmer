using System;

namespace Xml
{
    public class AlarmNotFoundException: Exception
    {
        public AlarmNotFoundException()
        {
        }

        public AlarmNotFoundException(string message) : base(message)
        {
        }

        public AlarmNotFoundException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
