using System;

namespace Xml
{
    public class DemandNotFoundException : Exception
    {
        public DemandNotFoundException()
        {
        }

        public DemandNotFoundException(string message) : base(message)
        {
        }

        public DemandNotFoundException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
