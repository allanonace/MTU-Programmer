using System;
using Xml;

using REGISTER_TYPE = MTUComm.MemoryMap.AMemoryMap.REGISTER_TYPE;
using RegType       = MTUComm.MemoryMap.MemoryMap.RegType;

namespace MTUComm.MemoryMap
{
    public class MemoryRegister<T> : IEquatable<MemoryRegister<T>>
    {
        #region Constants

        private enum CUSTOM_TYPE { EMPTY, METHOD, OPERATION, FORMAT }

        #endregion

        #region Attributes

        public Func<T> funcGet;                 // MemoryRegister.Value{get}
        public Func<T> funcGetCustom;           // Only use working dynamically ( IMemoryRegister.Get )
        public Func<byte[]> funcGetByteArray;   // 
        public Action<T> funcSet;               // MemoryRegister.Value{set}
        public Func<dynamic,dynamic> funcSetCustom;         // MemoryRegister.Value{set}
        public Action<string> funcSetString;    // MemoryRegister.Value{set}
        public Action<byte[]> funcSetByteArray; // MemoryRegister.Value{set}
        public string id { get; }
        public string description { get; }
        public RegType valueType { get; }
        public int address { get; }
        public int size { get; }
        public bool write { get; }
        private string custom_Get { get; }
        private string custom_Set { get; }
        public string methodId_Get { get; }
        public string methodId_Set { get; }
        private CUSTOM_TYPE customType_Get;
        private CUSTOM_TYPE customType_Set;
        public bool used;
        public REGISTER_TYPE registerType { get; }

        #endregion

        #region Properties

        // Value size ( number of consecutive bytes ) is also used for bit with bool type
        public int bit { get { return this.size; } }

        public string mathExpression_Get { get { return this.custom_Get; } }
        public string mathExpression_Set { get { return this.custom_Set; } }
        public string format_Get { get { return this.custom_Get; } }
        public string format_Set { get { return this.custom_Set; } }

        #region Custom Get

        private bool _HasCustomMethod_Get
        {
            get { return this.custom_Get.ToLower ().StartsWith ( MemoryMap.METHOD ); }
        }

        private bool _HasCustomMethodId_Get
        {
            get { return this.custom_Get.ToLower ().StartsWith ( MemoryMap.METHOD_KEY ); }
        }

        private bool _HasCustomOperation_Get
        {
            get { return ! this._HasCustomMethod_Get     &&
                           this.valueType < RegType.CHAR &&
                         ! string.IsNullOrEmpty ( this.custom_Get ); }
        }

        private bool _HasCustomFormat_Get
        {
            get { return ! this._HasCustomMethod_Get        &&
                           this.valueType == RegType.STRING &&
                         ! string.IsNullOrEmpty ( this.custom_Get ); }
        }

        // - MemoryMap.CreateProperty_Get<T>
        // - MemoryRegister{Constructor}
        // - IMemoryMap.TryGetMember|Get
        public bool HasCustomMethod_Get
        {
            get { return this.customType_Get == CUSTOM_TYPE.METHOD; }
        }

        // - MemoryMap.CreateProperty_Get<T>
        public bool HasCustomOperation_Get
        {
            get { return this.customType_Get == CUSTOM_TYPE.OPERATION; }
        }

        // - MemoryMap.CreateProperty_Get<T>
        public bool HasCustomFormat_Get
        {
            get { return this.customType_Get == CUSTOM_TYPE.FORMAT; }
        }

        #endregion

        #region Custom Set

        private bool _HasCustomMethod_Set
        {
            get { return this.custom_Set.ToLower ().StartsWith ( MemoryMap.METHOD ); }
        }

        private bool _HasCustomMethodId_Set
        {
            get { return this.custom_Set.ToLower ().StartsWith ( MemoryMap.METHOD_KEY ); }
        }

        private bool _HasCustomOperation_Set
        {
            get { return ! this._HasCustomMethod_Set     &&
                           this.valueType < RegType.CHAR &&
                         ! string.IsNullOrEmpty ( this.custom_Set ); }
        }

        private bool _HasCustomFormat_Set
        {
            get { return ! this._HasCustomMethod_Set        &&
                           this.valueType == RegType.STRING &&
                         ! string.IsNullOrEmpty ( this.custom_Set ); }
        }

        // - MemoryMap.CreateProperty_Set<T>
        // - MemoryRegister{Constructor}
        // - MemoryRegister.Value{set}
        public bool HasCustomMethod_Set
        {
            get { return this.customType_Set == CUSTOM_TYPE.METHOD; }
        }

        // - MemoryMap.CreateProperty_Set<T>
        public bool HasCustomOperation_Set
        {
            get { return this.customType_Set == CUSTOM_TYPE.OPERATION; }
        }

        // - MemoryMap.CreateProperty_Set_String<T>
        public bool HasCustomFormat_Set
        {
            get { return this.customType_Set == CUSTOM_TYPE.FORMAT; }
        }

        #endregion

        // Read and write without processing data, raw info
        public dynamic ValueRaw
        {
            get { return (T)this.funcGet (); }
            set { this.funcSet ( (T)value ); }
        }

        // Recover bytes without processing data, raw info
        public byte[] ValueByteArray
        {
            get { return this.funcGetByteArray (); }
        }

        // Use custom methods if them are registered
        public dynamic Value
        {
            // Register Value property always returns value from byte array without any modification
            // This behaviour is mandatory to be able to use original value inside custom get methods,
            // avoiding to create infinite loop: Custom Get -> Value -> Custom Get -> Value...
            get
            {
                // If register has not customized get method, use normal/direct get raw value
                if ( ! this.HasCustomMethod_Get )
                    return this.ValueRaw; // Invokes funGet method internally

                return this.funcGetCustom ();
            }
            set
            {
                // Register with read and write
                if ( this.write )
                {
                    // Method will modify passed value before set in byte array
                    // If XML custom field is "method" or "method:id"
                    if ( this.HasCustomMethod_Set )
                        value = this.funcSetCustom ( value );

                    // Try to set string value after read form control
                    // If XML custom field is...
                    if ( value is string )
                        this.funcSetString(value); 

                    // Try to set string but using byte array ( AES )
                    else if ( value is byte[] )
                        this.funcSetByteArray(value);

                    // Try to set value of waited type
                    // If XML custom field is a math expression ( e.g. _val_ * 2 / 5 )
                    else
                        this.ValueRaw = value;
                }

                // Register is readonly
                else
                {
                    Console.WriteLine ( "Set " + id + ": Error - Can't write to this register" );

                    if ( ! MemoryMap.isUnityTest )
                        throw new MemoryRegisterNotAllowWrite ( MemoryMap.EXCEP_SET_READONLY + ": " + id );
                }
            }
        }

        public string ValueWithXMask ( string xMask, int digits )
        {
            string value = this.Value.ToString ();

            // Ejemplo: num 1234 mask X00 digits 6
            // 1. 4 < 6
            // 2. 6 - 4 == 3 - 1
            if ( value.Length < digits &&
                 digits - value.Length == xMask.Length - 1 )
            {
                value = xMask.Substring ( 1, xMask.Length - 1 ) + value;
            }

            throw new Exception ();
        }

        #endregion

        #region Initialization

        public MemoryRegister () { }

        public MemoryRegister (
            string id,
            RegType type,
            string description,
            int address,
            int size = MemRegister.DEF_SIZE,
            bool write = MemRegister.DEF_WRITE,
            string custom_Get = "",
            string custom_Set = "" )
        {
            this.id           = id;
            this.valueType    = type;
            this.description  = description;
            this.address      = address;
            this.size         = size;
            this.write        = write;
            this.custom_Get   = custom_Get.Replace ( " ", string.Empty );
            this.custom_Set   = custom_Set.Replace ( " ", string.Empty );
            this.registerType = REGISTER_TYPE.REGISTER;

            // Custom Get
            if      ( this._HasCustomMethod_Get    ) this.customType_Get = CUSTOM_TYPE.METHOD;
            else if ( this._HasCustomOperation_Get ) this.customType_Get = CUSTOM_TYPE.OPERATION;
            else if ( this._HasCustomFormat_Get    ) this.customType_Get = CUSTOM_TYPE.FORMAT;
            else                                     this.customType_Get = CUSTOM_TYPE.EMPTY;

            if ( this.HasCustomMethod_Get )
            {
                if ( this._HasCustomMethodId_Get )
                     this.methodId_Get = this.custom_Get.Substring ( MemoryMap.METHOD_KEY.Length );
                else this.methodId_Get = this.id + MemoryMap.METHOD_SUFIX_GET;
            }

            // Custom Set
            if      ( this._HasCustomMethod_Set    ) this.customType_Set = CUSTOM_TYPE.METHOD;
            else if ( this._HasCustomOperation_Set ) this.customType_Set = CUSTOM_TYPE.OPERATION;
            else if ( this._HasCustomFormat_Set    ) this.customType_Set = CUSTOM_TYPE.FORMAT;
            else                                     this.customType_Set = CUSTOM_TYPE.EMPTY;

            if ( this.HasCustomMethod_Set )
            {
                if ( this._HasCustomMethodId_Set )
                     this.methodId_Set = this.custom_Set.Substring ( MemoryMap.METHOD_KEY.Length );
                else this.methodId_Set = this.id + MemoryMap.METHOD_SUFIX_SET;
            }
        }

        #endregion

        #region Compare

        public bool Equals ( MemoryRegister<T> other )
        {
            if ( other == null )
                return false;

            bool ok_id          = string.Equals ( this.id, other.id );
            bool ok_description = string.Equals ( this.description, other.description );
            bool ok_address     = ( this.address == other.address );
            bool ok_size        = ( this.size    == other.size    );
            bool ok_write       = ( this.write   == other.write   );
            bool ok_value       = object.Equals ( this.ValueRaw, other.ValueRaw );

            return ok_id          &&
                   ok_description &&
                   ok_address     &&
                   ok_size        &&
                   ok_write       &&
                   ok_value;
        }

        #endregion
    }
}
