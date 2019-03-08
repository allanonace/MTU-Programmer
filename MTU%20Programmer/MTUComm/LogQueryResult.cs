using System;
using System.Collections.Generic;
using System.Text;

namespace MTUComm
{
    public class LogQueryResult
    {
        public enum LogDataType
        {
            Bussy,
            NewPacket,
            LastPacket,
        }

        private LogDataType mStatus;
        private LogDataEntry mEntry;

        public int TotalEntries { get; private set; }
        public int CurrentEntry { get; private set; }


        public LogQueryResult(byte[] response)
        {
            if (response[1] == 1)
            {
                if (response[2] == 1)
                {
                    mStatus = LogDataType.LastPacket;
                }
                else
                {
                    mStatus = LogDataType.Bussy;
                }
                TotalEntries = 0;
                CurrentEntry = 0;

            }
            else
            {
                mStatus = LogDataType.NewPacket;
                TotalEntries = response[3] + (response[4] << 8);
                CurrentEntry = response[5] + (response[6] << 8);

                mEntry = new LogDataEntry(response);
            }
        }

        public LogDataType Status
        {
            get
            {
                return mStatus;
            }
        }

        public LogDataEntry Entry
        {
            get
            {
                return mEntry;
            }
        }
    }
}
