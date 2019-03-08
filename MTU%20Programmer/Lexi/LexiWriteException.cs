using System;
using System.Collections.Generic;
using System.Text;

namespace Lexi.Exceptions
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks></remarks>
    public class LexiWriteException : Exception
    {

        /// <summary>
        /// 
        /// </summary>
        /// <remarks></remarks>
        private int error_code;

        /// <summary>
        /// Initializes a new instance of the <see cref="Lexi.Exceptions.LexiWriteException" /> class. 
        /// </summary>
        /// <param name="response"></param>
        /// <remarks></remarks>
        public LexiWriteException(byte[] response) : base(getMessge(response[1]))
        {
            error_code = response[1];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <returns>String value that descrives the error identified by error code</returns>
        /// <remarks></remarks>
        private static String getMessge(int code)
        {
            switch (code)
            {
                case 0x31: return "Bad CRC";
                case 0x32: return "Checksum does not equal 0";
                case 0x33: return "Old coil commands no longer supported";
                case 0x34: return "The command is not defined or allocating memory to hold command structure failed";
                case 0x35: return "The command size is too large";
                default: return "Undefined Error";
            }
        }

        /// <summary>
        /// Gets Error code of the Exception:
        /// <list type="bullet">
        /// <item>
        /// <description><b>0x31</b>: COIL_CMD_RX_ERR_CRC - Bad CRC</description>
        /// </item>
        /// <item>
        /// <description><b>0x32</b>: COIL_CMD_RX_ERR_CKSUM - Checksum does not equal 0</description>
        /// </item>
        /// <item>
        /// <description><b>0x33</b>: COIL_CMD_RX_ERR_OLD - Old coil commands no longer supported</description>
        /// </item>
        /// <item>
        /// <description><b>0x34</b>: COIL_CMD_RX_ERR_UNDEF - The command is not defined or allocating memory to hold command structure failed</description>
        /// </item>
        /// <item>
        /// <description><b>0x35</b>: COIL_CMD_SIZE_TOO_BIG - The command size is too large</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <value>The ErrorCode property gest the byte code identifier of the error sent by MTU</value>
        public int ErrorCode
        {
            get { return error_code; }
        }
    }
}
