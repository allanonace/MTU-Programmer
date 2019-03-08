using System;

namespace Xml
{
    public class MtuNotFoundException: Exception
    {
        public MtuNotFoundException()
        {
        }

        public MtuNotFoundException(string message) : base(message)
        {
        }

        public MtuNotFoundException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
