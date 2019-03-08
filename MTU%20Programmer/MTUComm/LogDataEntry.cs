using System;
using System.Collections.Generic;
using System.Text;

namespace MTUComm
{
    public class LogDataEntry
    {
        public enum ReadReason
        {
            /// <summary>
            /// Normally scheduled read.
            /// </summary>
            Scheduled,
            /// <summary>
            /// OTA request for a read.
            /// </summary>
            OnDemand,
            /// <summary>
            /// Bit set using the coil interface.
            /// </summary>
            TaskFlag
        }

        public int FormatVersion { get; private set; }
        public DateTime TimeStamp { get; private set; }
        public int ReadInterval { get; private set; }
        public long MeterRead { get; private set; }
        private int Flags;
        public int ErrorStatus { get; private set; }

        public LogDataEntry(byte[] response)
        {

            FormatVersion = response[8];
            int timestamp_seconds = response[9] + (response[10] << 8) +(response[11] << 16) + (response[12] << 24);
            TimeStamp = new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(timestamp_seconds);
            Flags = response[13] + (response[14] << 8);
            ReadInterval = response[15] + (response[16] << 8);
            MeterRead = response[17] + (response[18] << 8) + (response[19] << 16) + (response[20] << 24) + (response[21] << 32);
            ErrorStatus = response[22];
        }

        public int PortNumber
        {
            get
            {
                if ((Flags & 3) == 0)
                {
                    return 1;
                }
                if ((Flags & 3) == 1)
                {
                    return 2;
                }
                return -1;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the read was a daily read.
        /// </summary>
        public bool IsDailyRead => (Flags & 4) != 0;

        /// <summary>
        /// Gets a value indicating whether the read was a top of the hour read.
        /// </summary>
        public bool IsTopOfHourRead => (Flags & 8) != 0;

        /// <summary>
        /// Gets the reason for the reading.
        /// </summary>
        public ReadReason ReasonForRead => (ReadReason)((Flags >> 4) & 7);

        /// <summary>
        /// Gets a value indicating whether the time was synchronized when the reading was taken.
        /// </summary>
        public bool IsSynchronized => (Flags & 0x80) != 0;
    }
}
