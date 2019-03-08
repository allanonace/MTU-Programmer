using System;

namespace MTUComm.Exceptions
{
    public class OwnExceptionsBase : Exception
    {
        private string message;
        public override string Message { get { return message; }  }

        public OwnExceptionsBase () { }
        
        public OwnExceptionsBase ( string message )
        {
            this.message = message;
        }
    }
}
