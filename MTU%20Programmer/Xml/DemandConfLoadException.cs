using System;

namespace Xml
{
    public class DemandConfLoadException: Exception
    {
        public DemandConfLoadException()
        {
        }

        public DemandConfLoadException(string message) : base(message)
        {
        }

        public DemandConfLoadException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
