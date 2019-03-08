using System;
using System.Collections.Generic;
using System.Text;

namespace MTUComm
{
    public class MTUBasicInfo
    {

        private uint mtu_type;
        private uint mtu_id;

        private Boolean mEncoder = true;

        private int shipbit;

        private int p1enabled;
        private int p2enabled;
        
        public  int version { get; }

        private PortType[] ports = new PortType[] { PortType.TYPE_NONE, PortType.TYPE_NONE }; //Currently max 2 ports

        public enum PortType
        {
            TYPE_NONE,
            TYPE_E,
            TYPE_RW,
            TYPE_M,
            TYPE_NUM,
            TYPE_S4K,
            TYPE_4KL,
            TYPE_GUT,
            TYPE_R,
            TYPE_G,
            TYPE_P,
            TYPE_T,
            TYPE_W,
            TYPE_I,
            TYPE_K,
            TYPE_L,
            TYPE_B,
            TYPE_CH4
        }

        public enum Model
        {
            A,
            C,
            D,
            D2,
            F,
            Z,
            MET
        }

        public MTUBasicInfo(byte[] buffer)
        {
            mtu_type = buffer[0];

            byte[] id_stream = new byte[4];
            Array.Copy ( buffer, 6, id_stream, 0, 4 );

            mtu_id = BitConverter.ToUInt32(id_stream, 0);

            byte mask = 1;
            shipbit = buffer[22];
            shipbit &= mask;

            p1enabled = buffer[28];
            p1enabled &= 1;

            p2enabled = buffer[28];
            p2enabled &= 2;
            
            // If new soft version is equal to 255, use old soft version register
            this.version = ( buffer[ 32 ] == 255 ) ? buffer[ 1 ] : buffer[ 32 ];
        }

        public bool P1Enabled
        {
            get
            {
                return (p1enabled > 0);
            }
        }

        public bool P2Enabled
        {
            get
            {
                return (p2enabled > 0);
            }
        }

        public bool Shipbit
        {
            get
            {
                return (shipbit > 0) ? true : false;
            }
        }

        public uint Type
        {
            get
            {
                return mtu_type;
            }
        }

        public uint Id
        {
            get
            {
                return mtu_id;
            }
        }

        private PortType portStringToType(String portype)
        {
            switch (portype)
            {
                case "E":
                    return PortType.TYPE_E;
                case "RW":
                    return PortType.TYPE_RW;
                case "M":
                    return PortType.TYPE_M;
                case "S4K":
                    return PortType.TYPE_S4K;
                case "4KL":
                    return PortType.TYPE_4KL;
                case "GUT":
                    return PortType.TYPE_GUT;
                case "R":
                    return PortType.TYPE_R;
                case "G":
                    return PortType.TYPE_G;
                case "P":
                    return PortType.TYPE_P;
                case "T":
                    return PortType.TYPE_T;
                case "W":
                    return PortType.TYPE_W;
                case "I":
                    return PortType.TYPE_I;
                case "K":
                    return PortType.TYPE_K;
                case "L":
                    return PortType.TYPE_L;
                case "B":
                    return PortType.TYPE_B;
                case "CH4":
                    return PortType.TYPE_CH4;
                default:
                    try
                    {
                        UInt32.Parse(portype);
                        return PortType.TYPE_NUM;
                    }
                    catch (Exception e)
                    {
                        return PortType.TYPE_NONE;
                    }
                    
            }
        }

        public void setPortType(int port_number, string port_type)
        {

            ports[port_number] = portStringToType(port_type);

            foreach(PortType type in ports){
                if(type != PortType.TYPE_E)
                {
                    mEncoder = false;
                }
            }
        }

        public Boolean isEncoder
        {
            get { return mEncoder; }
        }

    }
}
