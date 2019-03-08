using System;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using MTUComm.MemoryMap;
using Xml;
using MTUComm.Exceptions;

namespace MTUComm
{
    public sealed class Validations
    {
        #region Constants

        private const string PREFIX_AUX     = "_AllowEmptyField";
        private const string EXCEP_NAME_INI = "#";
        private const string EXCEP_NAME_SGN = "_";
        private const string EXCEP_VALIDATE = "Deserializing XML has failed due to validate required fields [ " + EXCEP_NAME_SGN + " ]";

        #endregion

        #region Tests

        public static bool IsNumeric ( dynamic value )
        {
            if ( string.IsNullOrEmpty ( value.ToString () ) )
                return false;
            
            else if ( value is Int32  ||
                      value is UInt32 ||
                      value is UInt64 )
                return true;

            // Validation for other numeric types
            string chars = value.ToString ().Trim ();
            chars = chars.Replace ( ",", string.Empty )
                         .Replace ( ".", string.Empty )
                         .Replace ( "-", string.Empty );

            // NOTA: No funciona con numeros negativos
            return chars.All ( c => char.IsDigit ( c ) );
        }

        public static bool IsNumeric<T> ( dynamic value )
        {
            bool ok1 = IsNumeric ( value );

            // String conversion for used types
            if ( ! ok1 && value is string )
            {
                if ( string.IsNullOrEmpty ( value ) )
                    return false;

                string valueString = ( string )( object )value;
                switch ( Type.GetTypeCode( typeof(T)) )
                {
                    case TypeCode.Int32:
                        int valueInt = 0;
                        if ( ! int.TryParse ( valueString, out valueInt ) )
                            return false;
                        return true;
                    
                    case TypeCode.UInt32:
                        uint valueUInt = 0;
                        if ( ! uint.TryParse ( valueString, out valueUInt ) )
                            return false;
                        return true;

                    case TypeCode.UInt64:
                        ulong valueULong = 0;
                        if ( ! ulong.TryParse ( valueString, out valueULong ) )
                            return false;
                        return true;
                }
            }

            return ok1;
        }

        public static bool NumericBytesLimit<T> ( dynamic value, int numBytes )
        {
            if ( ! IsNumeric<T> ( value ) )
                return false;

            bool isString = ( value is string );

            switch ( Type.GetTypeCode( typeof(T)) )
            {
                #region Int

                case TypeCode.Int32:
                    int valueInt = 0;
                    if ( isString )
                    {
                        if ( ! int.TryParse ( value, out valueInt ) )
                            return false;
                    }
                    else
                    {
                        try
                        {
                            valueInt = Convert.ToInt32 ( value );
                        }
                        catch ( Exception e )
                        {
                            return false;
                        }
                    }
                    
                    int iLimit = 0;
                    try
                    {
                        iLimit = Convert.ToInt32 ( Math.Pow ( 2, numBytes * 8 ) );
                    }
                    // Launchs error when result is bigger than ulong upper limit
                    catch ( Exception e )
                    {
                        iLimit = int.MaxValue;
                    }

                    return ( valueInt < iLimit );

                #endregion
                #region UInt

                case TypeCode.UInt32:
                    uint valueUInt = 0;
                    if ( isString )
                    {
                        if ( ! uint.TryParse ( value, out valueUInt ) )
                            return false;
                    }
                    else
                    {
                        try
                        {
                            valueUInt = Convert.ToUInt32 ( value );
                        }
                        catch ( Exception e )
                        {
                            return false;
                        }
                    }

                    uint uLimit = 0;
                    try
                    {
                        uLimit = Convert.ToUInt32 ( Math.Pow ( 2, numBytes * 8 ) );
                    }
                    // Launchs error when result is bigger than ulong upper limit
                    catch ( Exception e )
                    {
                        uLimit = uint.MaxValue;
                    }

                    return ( valueUInt < uLimit );

                #endregion
                #region ULong

                case TypeCode.UInt64:
                    ulong valueULong = 0;
                    if ( isString )
                    {
                        if ( ! ulong.TryParse ( ( string )value, out valueULong ) )
                            return false;
                    }
                    else
                    {
                        try
                        {
                            valueULong = Convert.ToUInt64 ( value );
                        }
                        catch ( Exception e )
                        {
                            return false;
                        }
                    }

                    ulong ulLimit = 0;
                    try
                    {
                        ulLimit = Convert.ToUInt64 ( Math.Pow ( 2, numBytes * 8 ) );
                    }
                    // Launchs error when result is bigger than ulong upper limit
                    catch ( Exception e )
                    {
                        ulLimit = ulong.MaxValue;
                    }

                    return ( valueULong < ulLimit );

                #endregion
            }
            return false;
        }

        public static bool NumericTypeLimit<T> ( dynamic value )
        {
            if ( ! IsNumeric<T> ( value ) )
                return false;

            bool isString = ( value is string );

            switch ( Type.GetTypeCode( typeof(T)) )
            {
                #region Int

                case TypeCode.Int32:
                    int valueInt = 0;
                    if ( isString )
                    {
                        if ( ! int.TryParse ( value, out valueInt ) )
                            return false;
                    }
                    else
                    {
                        try
                        {
                            valueInt = Convert.ToInt32 ( value );
                        }
                        catch ( Exception e )
                        {
                            return false;
                        }
                    }
                    return ( valueInt >= Int32.MinValue &&
                             valueInt <= Int32 .MaxValue );

                #endregion
                #region UInt

                case TypeCode.UInt32:
                    uint valueUInt = 0;
                    if ( isString )
                    {
                        if ( ! uint.TryParse ( value, out valueUInt ) )
                            return false;
                    }
                    else
                    {
                        try
                        {
                            valueUInt = Convert.ToUInt32 ( value );
                        }
                        catch ( Exception e )
                        {
                            return false;
                        }
                    }
                    return ( valueUInt >= UInt32.MinValue &&
                             valueUInt <= UInt32 .MaxValue );

                #endregion
                #region ULong

                case TypeCode.UInt64:
                    ulong valueULong = 0;
                    if ( isString )
                    {
                        if ( ! ulong.TryParse ( value, out valueULong ) )
                            return false;
                    }
                    else
                    {
                        try
                        {
                            valueULong = Convert.ToUInt64 ( value );
                        }
                        catch ( Exception e )
                        {
                            return false;
                        }
                    }
                    return ( valueULong >= UInt64.MinValue &&
                             valueULong <= UInt64.MaxValue );

                #endregion
            }
            return false;
        }

        public static bool TextLength (
            string value,
            int  maxLength,
            int  minLength    = 0,
            bool maxInclusive = true,
            bool minInclusive = true )
        {
            if ( string.IsNullOrEmpty ( value ) )
                return false;

            int length = value.Length;
            return ( ( ! minInclusive && length >  minLength ||
                         minInclusive && length >= minLength ) &&
                     ( ! maxInclusive && length <  maxLength ||
                         maxInclusive && length <= maxLength ) );
        }

        public static T DeserializeXml<T> ( TextReader reader )
        {
            Type typeXml  = typeof ( T );
            T deserialize = ( T )new XmlSerializer ( typeXml ).Deserialize ( reader );

            try
            {
                DeserializeXml_Logic ( deserialize, typeXml.GetProperties () );
            }
            catch ( Exception e )
            {
                // Capture validation error with element name and launch a new exception
                throw new MemoryMapXmlValidationException ( EXCEP_VALIDATE.Replace ( EXCEP_NAME_SGN, e.Message ) );
            }
            return deserialize;
        }

        // TO-DO: Valida bien la falta de registros de los Overload, pero solo devuelve bien el error
        // cuando no hay ningun elemento register listado, porque si se indica uno vacio ( <Register/> )
        // el mensaje de error resultante devuelto es "Value cannot be null"
        private static bool DeserializeXml_Logic ( dynamic instance, PropertyInfo[] psInfo, string index = EXCEP_NAME_INI )
        {
            Type typeAtrArray   = typeof ( XmlArrayAttribute );
            Type typeAtrElement = typeof ( XmlElementAttribute );
            Type typeAtrText    = typeof ( XmlTextAttribute );
            Type typeInstance   = instance.GetType ();

            XmlArrayAttribute   atrArray;
            XmlElementAttribute atrElement;
            XmlTextAttribute    atrText;
            foreach ( PropertyInfo pinfo in psInfo )
            {
                string name  = pinfo.Name;
                object value = pinfo.GetValue ( instance );

                // [XmlArray("...")]
                if ( ( atrArray = ( XmlArrayAttribute )pinfo.GetCustomAttribute ( typeAtrArray ) ) != null )
                {
                    Array ar = ( Array )value;

                    if ( ! atrArray.IsNullable &&
                         ( ar == null || ar.Length == 0 || ar.GetValue ( 0 ) == null ) )
                        throw new Exception ( name + " " + index + " | Array" );

                    int i = 0;
                    foreach ( var element in ar )
                        DeserializeXml_Logic (
                            element,
                            element.GetType().GetProperties (),
                            index + ( ( string.Equals ( index, EXCEP_NAME_INI ) ) ? string.Empty : EXCEP_NAME_SGN ) + ++i );
                }

                // [XmlElement("...")]
                else if ( ( atrElement = ( XmlElementAttribute )pinfo.GetCustomAttribute ( typeAtrElement ) ) != null )
                {
                    // NOTE: "value is Array" not works
                    bool isArray = pinfo.PropertyType.IsArray;

                    // Element not nullable and without value ( empty or null )
                    if ( ! atrElement.IsNullable )
                    {
                        if ( value is string )
                        {
                            // Empty string elements
                            if ( string.IsNullOrEmpty ( ( string )value ) )
                                throw new Exception ( name + " " + index + " | String" );

                            // Int elements with string auxiliary method to allow correct deserialization
                            if ( string.Equals ( value, MemRegister.ERROR_STR ) )
                                throw new Exception ( name + " " + index + " | Int" );

                            // Bool elements with string auxiliary method to allow correct deserialization
                            if ( name.Contains ( PREFIX_AUX ) &&
                                 typeInstance.GetProperty (
                                    ( name = name.Substring ( 0, name.IndexOf ( PREFIX_AUX ) ) ) )
                                 .GetValue ( instance ) == MemRegister.ERROR_VAL )
                                throw new Exception ( name + " " + index + " | Bool" );
                        }

                        // Empty array
                        else if ( isArray &&
                                  ( value == null || ( ( Array )value ).Length == 0 ) )
                                throw new Exception ( name + " " + index + " | Array" );
                    }

                    // Element is an array
                    if ( isArray )
                    {
                        int i = 0;
                        foreach ( var element in ( Array )value )
                            DeserializeXml_Logic (
                                element,
                                element.GetType().GetProperties (),
                                index + ( ( string.Equals ( index, EXCEP_NAME_INI ) ) ? string.Empty : EXCEP_NAME_SGN ) + ++i );
                    }
                }

                // [XmlText]
                else if ( ( atrText = ( XmlTextAttribute )pinfo.GetCustomAttribute ( typeAtrText ) ) != null )
                {
                    if ( string.IsNullOrEmpty ( ( string )value ) )
                        throw new Exception ( name + " " + index + " | Text" );
                }
            }

            return true;
        }

        public static bool NumericText (
            string value,
            int  maxLength,
            int  minLength    = 1,
            bool maxInclusive = true,
            bool minInclusive = true,
            bool equalsLength = true,
            bool validateNumericLength = false )
        {
            if ( value is null )
                return false;

            string valueClean = value.ToString ().Trim ()
                                    .Replace ( ",", string.Empty )
                                    .Replace ( ".", string.Empty )
                                    .Replace ( "-", string.Empty );

            bool okTextLength = TextLength ( valueClean, maxLength, minLength, maxInclusive, minInclusive );
            bool okNumeric    = IsNumeric ( valueClean );
            bool okEquals     = ( ! equalsLength || valueClean.Length == maxLength );

            return okTextLength && okNumeric && okEquals;
        }
        
        public static bool Text (
            string value,
            int maxLength,
            int minLength = 1,
            bool maxInclusive = true,
            bool minInclusive = true,
            bool equalsLength = true )
        {
            if ( value is null )
                return false;
                
            string valueClean = value.ToString ().Trim ();
                
            bool okTextLength = TextLength ( valueClean, maxLength, minLength, maxInclusive, minInclusive );
            bool okEquals     = ( ! equalsLength || valueClean.Length == maxLength );

            return okTextLength && okEquals;
        }

        #endregion
    }
}
