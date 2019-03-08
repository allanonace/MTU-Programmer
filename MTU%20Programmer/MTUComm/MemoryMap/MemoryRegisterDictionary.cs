using System;
using System.Collections.Generic;

using RegType = MTUComm.MemoryMap.MemoryMap.RegType;

namespace MTUComm.MemoryMap
{
    public class MemoryRegisterDictionary
    {
        Dictionary<RegType,dynamic> dictionary;

        public MemoryRegisterDictionary ()
        {
            this.dictionary = new Dictionary<RegType,dynamic> ();
            this.dictionary.Add ( RegType.INT,    new List<MemoryRegister<int   >> () );
            this.dictionary.Add ( RegType.UINT,   new List<MemoryRegister<uint  >> () );
            this.dictionary.Add ( RegType.ULONG,  new List<MemoryRegister<ulong >> () );
            this.dictionary.Add ( RegType.BOOL,   new List<MemoryRegister<bool  >> () );
            this.dictionary.Add ( RegType.CHAR,   new List<MemoryRegister<char  >> () );
            this.dictionary.Add ( RegType.STRING, new List<MemoryRegister<string>> () );
        }

        public void AddElement<T> ( MemoryRegister<T> register )
        {
            switch ( Type.GetTypeCode ( typeof ( T ) ) )
            {
                case TypeCode.Int32  : this.dictionary[ RegType.INT    ].Add ( register ); break;
                case TypeCode.UInt32 : this.dictionary[ RegType.UINT   ].Add ( register ); break;
                case TypeCode.UInt64 : this.dictionary[ RegType.ULONG  ].Add ( register ); break;
                case TypeCode.Boolean: this.dictionary[ RegType.BOOL   ].Add ( register ); break;
                case TypeCode.Char   : this.dictionary[ RegType.CHAR   ].Add ( register ); break;
                case TypeCode.String : this.dictionary[ RegType.STRING ].Add ( register ); break;
            }
        }

        public List<MemoryRegister<int>> GetElements_Int ()
        {
            return this.dictionary[ RegType.INT ];
        }

        public List<MemoryRegister<uint>> GetElements_UInt ()
        {
            return this.dictionary[ RegType.UINT ];
        }

        public List<MemoryRegister<ulong>> GetElements_ULong ()
        {
            return this.dictionary[ RegType.ULONG ];
        }

        public List<MemoryRegister<bool>> GetElements_Bool ()
        {
            return this.dictionary[ RegType.BOOL ];
        }

        public List<MemoryRegister<char>> GetElements_Char ()
        {
            return this.dictionary[ RegType.CHAR ];
        }

        public List<MemoryRegister<string>> GetElements_String ()
        {
            return this.dictionary[ RegType.STRING ];
        }

        public List<dynamic> GetAllElements ()
        {
            List<dynamic> list = new List<dynamic> ();
            list.AddRange ( this.dictionary[ RegType.INT    ] );
            list.AddRange ( this.dictionary[ RegType.UINT   ] );
            list.AddRange ( this.dictionary[ RegType.ULONG  ] );
            list.AddRange ( this.dictionary[ RegType.BOOL   ] );
            list.AddRange ( this.dictionary[ RegType.CHAR   ] );
            list.AddRange ( this.dictionary[ RegType.STRING ] );

            return list;
        }
    }
}
