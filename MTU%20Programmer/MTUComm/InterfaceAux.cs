using Xml;
using System.Collections.Generic;

namespace MTUComm
{
    public class InterfaceAux
    {
        public static string GetmemoryMapTypeByMtuId ( Mtu mtu )
        {
            Interface iInfo = GetInterfaceBytMtuId ( mtu );

            return iInfo.Memorymap;
        }

        public static int GetmemoryMapSizeByMtuId ( Mtu mtu )
        {
            Interface iInfo = GetInterfaceBytMtuId ( mtu );

            return iInfo.MemorymapSize;
        }

        private static Interface GetInterfaceBytMtuId ( Mtu mtu )
        {
            Configuration   config        = Configuration.GetInstance ();
            InterfaceConfig xmlInterfaces = config.interfaces;
            bool            meterIdIsNumeric;
            List<string>    portTypes     = mtu.Ports[ 0 ].GetPortTypes ( out meterIdIsNumeric );

            int interfaceIndex;

            // Gas MTUs of family 33xx should use family 31xx32xx memorymap
            if ( ! mtu.Ecoder &&
                 ( mtu.Ports[ 0 ].Type.Contains ( "M" ) ||
                   mtu.Ports[ 0 ].Type.Contains ( "R" ) ||
                   ( meterIdIsNumeric && config.getMeterTypeById ( int.Parse ( portTypes[ 0 ] ) ).Utility.ToLower ().Equals ( "gas" ) ) ) )
            {
                 interfaceIndex = 1; // Family 31xx32xx
            }
            else
            {
                MtuInterface iInfoMtu = xmlInterfaces.MtuInterfaces.Find ( x => x.Id == mtu.Id );

                if ( iInfoMtu == null )
                    throw new MtuNotFoundException("Mtu not found");
                else
                    interfaceIndex = iInfoMtu.Interface;
            }

            Interface iInfo = xmlInterfaces.Interfaces.Find(x => x.Id == interfaceIndex );

            if ( iInfo == null )
                throw new InterfaceNotFoundException("Meter not found");
            
            return iInfo;
        }
    }
}
