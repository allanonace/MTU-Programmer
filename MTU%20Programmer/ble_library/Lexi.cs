using System;
using System.IO;
using System.Threading;
using Lexi.Interfaces;
using Lexi.Exceptions;

namespace Lexi
{
    /*
    Lexi Protocol Class.
    Contains all methods of Lexi Protocol V2: Read, Write and Operations.
    */
    /// <summary>
    /// <see cref="Lexi.Lexi" /> Protocol Class.
    /// Contains all methods of Lexi Protocol V2: Read, Write and Operations.
    /// </summary>
    public class Lexi
    {
        /// <summary>
        /// Serial port interface used to communicate through Lexi
        /// </summary>
        /// <remarks>User should iplement custom serial that inherist from ISearial</remarks>
        private ISerial m_serial;
        /// <summary>
        /// Timout limit to wait for MTU response.
        /// </summary>
        /// <remarks>Once request is sent, timeot defines the time to wait for a response from MTU</remarks>
        private int m_timeout;

        /// <summary>
        /// Precalculated CRC Table
        /// </summary>
        /// <remarks>This table is used by CRC validation function and it makes CRC calculation fast</remarks>
        static uint[] CRCTable = {0, 4489, 8978, 12955, 17956, 22445, 25910, 29887, 35912, 40385, 44890,
                                  48851, 51820, 56293, 59774, 63735, 4225, 264, 13203, 8730, 22181,
                                  18220, 30135, 25662, 40137, 36160, 49115, 44626, 56045, 52068, 63999,
                                  59510, 8450, 12427, 528, 5017, 26406, 30383, 17460, 21949, 44362,
                                  48323, 36440, 40913, 60270, 64231, 51324, 55797, 12675, 8202, 4753,
                                  792, 30631, 26158, 21685, 17724, 48587, 44098, 40665, 36688, 64495,
                                  60006, 55549, 51572, 16900, 21389, 24854, 28831, 1056, 5545, 10034,
                                  14011, 52812, 57285, 60766, 64727, 34920, 39393, 43898, 47859, 21125,
                                  17164, 29079, 24606, 5281, 1320, 14259, 9786, 57037, 53060, 64991,
                                  60502, 39145, 35168, 48123, 43634, 25350, 29327, 16404, 20893, 9506,
                                  13483, 1584, 6073, 61262, 65223, 52316, 56789, 43370, 47331, 35448,
                                  39921, 29575, 25102, 20629, 16668, 13731, 9258, 5809, 1848, 65487,
                                  60998, 56541, 52564, 47595, 43106, 39673, 35696, 33800, 38273, 42778,
                                  46739, 49708, 54181, 57662, 61623, 2112, 6601, 11090, 15067, 20068,
                                  24557, 28022, 31999, 38025, 34048, 47003, 42514, 53933, 49956, 61887,
                                  57398, 6337, 2376, 15315, 10842, 24293, 20332, 32247, 27774, 42250,
                                  46211, 34328, 38801, 58158, 62119, 49212, 53685, 10562, 14539, 2640,
                                  7129, 28518, 32495, 19572, 24061, 46475, 41986, 38553, 34576, 62383,
                                  57894, 53437, 49460, 14787, 10314, 6865, 2904, 32743, 28270, 23797,
                                  19836, 50700, 55173, 58654, 62615, 32808, 37281, 41786, 45747, 19012,
                                  23501, 26966, 30943, 3168, 7657, 12146, 16123, 54925, 50948, 62879,
                                  58390, 37033, 33056, 46011, 41522, 23237, 19276, 31191, 26718, 7393,
                                  3432, 16371, 11898, 59150, 63111, 50204, 54677, 41258, 45219, 33336,
                                  37809, 27462, 31439, 18516, 23005, 11618, 15595, 3696, 8185, 63375,
                                  58886, 54429, 50452, 45483, 40994, 37561, 33584, 31687, 27214, 22741,
                                  18780, 15843, 11370, 7921, 3960};


        /// <summary>
        /// Initializes a new instance of the <see cref="Lexi.Lexi" /> class.
        /// </summary>
        /// <param name="serial"></param>
        /// <param name="timeout"></param>
        public Lexi(ISerial serial, int timeout)
        {
            m_serial = serial;
            m_timeout = timeout;

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Lexi.Lexi" /> class. 
        /// </summary>
        /// <remarks></remarks>
        public Lexi()
        {
            //set default read wait to response timeout to 400ms
            m_timeout = 400;
        }


        /// <summary>
        /// Adds two integers and returns the result.
        /// </summary>
        /// <param name="addres">A double precision number.</param>
        /// <param name="data">A double precision number.</param>
        /// <returns>
        /// The sum of two integers.
        /// </returns>
        /// <example>
        ///   <code><![CDATA[
        /// lx.Read(0, 1)
        /// ]]></code>
        /// </example>
        /// <seealso cref="Write(byteaddres,byte[]data)" />
        /// <seealso cref="Write(ISerialserial,UInt32addres,byte[]data,inttimeout)" />
        /// <exception cref="System.ArgumentNullException">Thrown when no Serial is
        /// defines</exception>
        /// <exception cref="System.TimeoutException">Thrown response Timeout is
        /// reached</exception>
        /// <exception cref="System.IO.InvalidDataException">Thrown when response data or
        /// CRC is not valid</exception>
        /// <see cref="Read(ISerial serial, UInt32 addres, uint data, int timeout)" />
        public byte[] Read(UInt32 addres, uint data)
        {
            if (m_serial == null)
            {
                throw new ArgumentNullException("No Serial interface defined");
            }

            return Read(m_serial, addres, data, m_timeout);
        }

        /// <summary>
        /// Request to read bytes from memory map
        /// </summary>
        /// <param name="serial">A double precision number.</param>
        /// <param name="addres">A double precision number.</param>
        /// <param name="data">A double precision number.</param>
        /// <param name="timeut">A double precision number.</param>
        /// <returns>
        /// Memory Map values from read address request
        /// </returns>
        /// <example>
        /// <code>
        /// lx.Read(new USBSerial("COM5"), 0, 1, 400)
        /// </code>
        /// </example>
        /// <exception cref="System.TimeoutException">Thrown response Timeout is reached </exception>
        /// <exception cref="System.IO.InvalidDataException">Thrown when response data or CRC is not valid </exception>
        /// See <see cref="Read(UInt32 addres, uint data)"/> to add doubles.
        /// <seealso cref="Write(byte addres, byte[] data)"/>
        /// <seealso cref="Write(ISerial serial, UInt32 addres, byte[] data, int timeout)"/>
        public byte[] Read(ISerial serial, UInt32 addres, uint data, int timeout)
        {
            /* +------------+----------+----------------------------------------------------------+
             * | Byte Index |  Field   |                          Notes                           |
             * +------------+----------+----------------------------------------------------------+
             * |          0 | Header   | This field is always 0x25                                |
             * |          1 | Command  | 0x80, 0x82, 0x84, 0x86 as detailed in section 4.2 above  |
             * |          2 | Address  | Address of first byte to read                            |
             * |          3 | Size     | Number of bytes to read                                  |
             * |          4 | Checksum | 2’s complement of sum: header + command + address + data |
             * +------------+----------+----------------------------------------------------------+ 
             * 
             * Command
             * -------
             * block 0 addres >= 0 & < 256 = 0x80 => 128
             * block 1: addres >= 256 & < 512 = 0x82 => 130
             * block 2:addres >= 512 & < 768 = 0x84 => 132
             * block 3:addres >= 768 = 0x86 => 134
             */
            uint read_comand = (uint)(128 + ((addres / 256)*2));

            /* Address
             * -------
             * addres then become offset from 256 block
             * Example: Addres 600 --> block 2 --> offset 600 - 512 = 88;
             */
            uint start_addres = (uint)(addres - ((addres / 256) * 256));

            //generate payload woth header, command, addres and data
            byte[] payload = new byte[]{
                0x25, // header
                (byte)read_comand,
                (byte)start_addres,
                (byte)data
            };

            //Addes checksum to payload
            byte[] stream = checkSum(payload);

            //Send Lexi Read command thought "serial port"
            serial.Write(stream, 0, stream.Length);


            //define response buffer size
            byte[] rawBuffer = new byte[2];
            int response_offest = 0;
            if (serial.isEcho()) {response_offest = stream.Length;}
            Array.Resize(ref rawBuffer, (int)(rawBuffer.Length + data + response_offest));


            // whait till the response buffer data is available or timeout limit is reached
            long timeout_limit = DateTimeOffset.Now.ToUnixTimeMilliseconds() + (timeout);
            while (serial.BytesToRead() < rawBuffer.Length)
            {
                if(DateTimeOffset.Now.ToUnixTimeMilliseconds() > timeout_limit)
                {
                    throw new TimeoutException();
                }
                Thread.Sleep(10);
            }

            //read the response buffer
            serial.Read(rawBuffer, 0, (int)(rawBuffer.Length));

            /*
             * +------------+-------+---------------+--------------------------------------------------------+
             * | Byte Index | Field |   Value(s)    |                         Notes                          |
             * +------------+-------+---------------+--------------------------------------------------------+
             * | 0..N-1     | Data  | Read from RAM | N = number of bytes specified in Data field of Request |
             * | N..N+1     | CRC   | Calculated    | CRC over all data bytes.  See section 7.2              |
             * +------------+-------+---------------+--------------------------------------------------------+
             * 
             */

            //Get response body from RAW response (clean echo response)
            byte[] response= new byte[2];
            Array.Resize(ref response, (int)(response.Length + data));
            Array.Copy(rawBuffer, response_offest, response, 0, response.Length);

            //Validare CRC ane get Response BUDY
            return validateReadResponse(response,data);

        }

        public void Write(byte addres, byte[] data)
        {
            if (m_serial == null)
            {
                throw new ArgumentNullException("No Serial interface defined");
            }

            Write(m_serial, addres, data, m_timeout);
        }

        public void Write(ISerial serial, UInt32 addres, byte[] data, int timeout)
        {
            /*
             * +------------+----------+----------------------------------------------------------+
             * | Byte Index |  Field   |                          Notes                           |
             * +------------+----------+----------------------------------------------------------+
             * | 0          | Header   | This field is always 0x25                                |
             * | 1          | Command  | 0x81, 0x83, 0x85, 0x87 as detailed in section 4.2 above  |
             * | 2          | Address  | Address of first byte to write                           |
             * | 3          | Size     | Number of bytes to write                                 |
             * | 4          | Checksum | 2’s complement of sum: header + command + address + data |
             * | 5..4+N     | Data     | N = number of bytes specified in Data/Size field above   |
             * | 6+N..7+N   | CRC      | See section 7.2                                          |
             * +------------+----------+----------------------------------------------------------+
             * 
             * Command
             * -------
             * block 0 addres >= 0 & < 256 = 0x81 => 129
             * block 1: addres >= 256 & < 512 = 0x83 => 131
             * block 2:addres >= 512 & < 768 = 0x85 => 133
             * block 3:addres >= 768 = 0x87 => 135
             */

            uint write_comand = (uint)(129 + ((addres / 256) * 2));

            /* Address
             * -------
             * addres then become offset from 256 block
             * Example: Addres 600 --> block 2 --> offset 600 - 512 = 88;
             */
            uint start_addres = (uint)(addres - ((addres / 256) * 256));

            byte[] payload = new byte[]{
                0x25, // header
                (byte)write_comand,
                (byte)start_addres,
                (byte)data.Length
            };

            //Addes checksum to payload
            byte[] header = checkSum(payload);

            byte[] stream = new byte[0];
            Array.Resize(ref stream, (int)(header.Length + data.Length));

            Array.Copy(header, 0, stream, 0, header.Length);
            Array.Copy(data, 0, stream, header.Length, data.Length);


            byte[] crc_bytes = BitConverter.GetBytes(calcCRC(data, data.Length));

            Array.Resize(ref stream, (int)(header.Length + data.Length+2));
            stream[stream.Length - 1] = crc_bytes[1];
            stream[stream.Length - 2] = crc_bytes[0];

            serial.Write(stream, 0, stream.Length);

            byte[] rawBuffer = new byte[2];
            int response_offest = 0;
            if (serial.isEcho()) { response_offest = stream.Length; }
            Array.Resize(ref rawBuffer, (int)(response_offest + 2));

            long timeout_limit = DateTimeOffset.Now.ToUnixTimeMilliseconds() + (timeout);
            while (serial.BytesToRead() < rawBuffer.Length-1)
            {
                if (DateTimeOffset.Now.ToUnixTimeMilliseconds() > timeout_limit)
                {
                    throw new TimeoutException();
                }
                Thread.Sleep(10);
            }

            serial.Read(rawBuffer, 0, (int)(rawBuffer.Length));

            byte[] response = new byte[2];
            Array.Copy(rawBuffer, response_offest, response, 0, response.Length);

            if (response[0] != 0x06)
            {
                throw new LexiWriteException(response);
            }
        }

        static byte[] checkSum(byte[] data)
        {
            int chksum = 0;

            for (int i = 0; i < data.Length; i++)
                chksum += data[i];
            chksum = (chksum ^ 0xff) + 1;
            if (chksum < 0)
                chksum += 256;

            Array.Resize(ref data, data.Length + 1);
            data[data.Length - 1] = Convert.ToByte(chksum & 0x00ff); ;

            return data;
        }

        static uint calcCRC(byte[] data, int len)
        {
            uint accum = 0xffff;

            for (int i = 0; i < len; i++)
                accum = (accum >> 8) ^ CRCTable[(accum ^ data[i]) & 0x00ff];
            return accum;
        }

        private byte[] validateReadResponse(byte[] response, uint response_length)
        {
            if(!((response.Length-2) == response_length))
            {
                throw new InvalidDataException("");
           
            }


            byte[] crc = new byte[2];
            byte[] response_body = new byte[0];
            Array.Resize(ref response_body, (int)(response.Length - 2));

            Array.Copy(response, response.Length - 2, crc, 0, crc.Length);
            Array.Copy(response, 0, response_body, 0, response_body.Length);

            if (calcCRC(response_body, response_body.Length)!= BitConverter.ToUInt16(crc,0))
            {
                throw new InvalidDataException("Bad CRC");
            }

            return response_body;

        }

        /// <summary>
        /// Gets or sets .
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public int Timout
        {
            get
            {
                return m_timeout;
            }

            set
            {
                m_timeout = value;
            }
        }
    }
}
