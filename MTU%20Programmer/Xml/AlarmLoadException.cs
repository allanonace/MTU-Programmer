using System;

namespace Xml
{
    public class AlarmLoadException: Exception
    {
        public AlarmLoadException()
        {
        }

        public AlarmLoadException(string message) : base(message)
        {
        }

        public AlarmLoadException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
