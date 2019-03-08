using System;

namespace MTUComm.Exceptions
{
    public class CustomMethodNotExistException : OwnExceptionsBase
    {
        public CustomMethodNotExistException () { }
        public CustomMethodNotExistException ( string message ) : base ( message ) { }
    }
    
    public class MemoryMapParseXmlException : OwnExceptionsBase
    {
        public MemoryMapParseXmlException () { }
        public MemoryMapParseXmlException ( string message ) : base ( message ) { }
    }
    
    public class MemoryMapXmlValidationException : OwnExceptionsBase
    {
        public MemoryMapXmlValidationException () { }
        public MemoryMapXmlValidationException ( string message ) : base ( message ) { }
    }
    
    public class MemoryRegisterNotExistException : OwnExceptionsBase
    {
        public MemoryRegisterNotExistException () { }
        public MemoryRegisterNotExistException ( string message ) : base ( message ) { }
    }
    
    public class OverloadEmptyCustomException : OwnExceptionsBase
    {
        public OverloadEmptyCustomException () { }
        public OverloadEmptyCustomException ( string message ) : base ( message ) { }
    }
    
    public class SetMemoryFormatException : OwnExceptionsBase
    {
        public SetMemoryFormatException () { }
        public SetMemoryFormatException ( string message ) : base ( message ) { }
    }
    
    public class SetMemoryTypeLimitException : OwnExceptionsBase
    {
        public SetMemoryTypeLimitException () { }
        public SetMemoryTypeLimitException ( string message ) : base ( message ) { }
    }
}
