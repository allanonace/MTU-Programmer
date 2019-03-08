using System;
using System.Globalization;

namespace MTUComm.MemoryMap
{
    public partial class MemoryMap : AMemoryMap
    {
        #region Constants

        private const string MIDNIGHT    = "MidNight";
        private const string NOON        = "Noon";
        private const string AM          = " AM";
        private const string PM          = " PM";
        private const string OFF         = "Off";

        private const string STATE_ON    = "ON";
        private const string STATE_OFF   = "OFF";

        private const string YES         = "Yes";
        private const string NO          = "No";

        private const string PCBFORMAT   = "{0:000000000}";
        private const string NTAVAILABLE = "Not Available";

        private const string MTU_SOFTVERSION_LONG = "Version {0:00}.{1:00}.{2:0000}";
        private const string MTU_SOFTVERSION_SMALL = "Version {0:00}";

        private const string MTUVLFORMAT = "0.00 V";

        private const string FWAYFORMAT  = "X8";

        private const string MESAG_FAST  = "Fast";
        private const string MESAG_SLOW  = "Slow";

        private const string HOURS       = " Hrs";
        private const string HOUR        = " Hr";
        private const string MIN         = " Min";

        private const char   ZERO        = '0';
        private const int    INDEX_STATE = 2;
        private const int    PAD_LEFT    = 8;

        private const string ENABLED     = "Enabled";
        private const string DISABLED    = "Disabled";
        private const string TRIGGERED   = "Triggered";

        private const string NTCONFIRMED = "NOT CONFIRMED";
        private const string CONFIRMED   = "CONFIRMED";

        private const int    ECODE_METER = 0xFF;
        private const int    ECODE_DIGIT = 0xFE;
        private const int    ECODE_OVER  = 0xFD;
        private const int    ECODE_PURGE = 0xFC;
        private const string ERROR_METER = "ERROR - Check Meter";
        private const string ERROR_DIGIT = "ERROR - Bad Digits";
        private const string ERROR_OVER  = "ERROR - Delta Overflow";
        private const string ERROR_PURGE = "ERROR - Readings Purged";

        private const string CASE_00     = "00";
        private const string CASE_01     = "01";
        private const string CASE_10     = "10";

        private const string CASE_00_BFS = "No reverse Flow Event in last 35 days";
        private const string CASE_01_BFS = "Small Reverse Flow Event in last 35 days";
        private const string CASE_10_BFS = "Large Reverse Flow Event in last 35 days";

        private const string CASE_00_LKD = "Less than 50 15-minute intervals";
        private const string CASE_01_LKD = "Between 50 and 95 15-minute intervals";
        private const string CASE_10_LKD = "Greater than 96 15-minute intervals";

        private const string CASE_000    = "000";
        private const string CASE_001    = "001";
        private const string CASE_010    = "010";
        private const string CASE_011    = "011";
        private const string CASE_100    = "100";
        private const string CASE_101    = "101";
        private const string CASE_110    = "110";

        private const string CASE_000_TX = "0";
        private const string CASE_001_TX = "1-2";
        private const string CASE_010_TX = "3-7";
        private const string CASE_011_TX = "8-14";
        private const string CASE_100_TX = "15-21";
        private const string CASE_101_TX = "22-34";
        private const string CASE_110_TX = "35 (ALL)";
        private const string CASE_NOFLOW = " days of no consumption";
        private const string CASE_LEAK   = " days of leak detection";

        #endregion

        #region Overloads

        public string DailySnap_Get (MemoryOverload<string> MemoryOverload, dynamic MemoryRegisters)
        {
            int timeDiff = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Hours;
            int curTime = MemoryRegisters.DailyGMTHourRead.Value + timeDiff;

            if ( curTime < 0 )
                curTime = 24 + curTime;

            if      ( curTime ==  0 ) return MIDNIGHT;
            else if ( curTime <= 11 ) return curTime + AM;
            else if ( curTime == 12 ) return NOON;
            else if ( curTime >  12 &&
                      curTime <  24 ) return ( curTime - 12 ) + PM;
            else return OFF;
        }

        public string MtuStatus_Get (MemoryOverload<string> MemoryOverload, dynamic MemoryRegisters)
        {
            return MemoryRegisters.Shipbit.Value ? STATE_OFF : STATE_ON;
        }

        public string ReadInterval_Get (MemoryOverload<string> MemoryOverload, dynamic MemoryRegisters)
        {
            return TimeFormatter(MemoryRegisters.ReadIntervalMinutes.Value);
        }

        public string XmitInterval_Get (MemoryOverload<string> MemoryOverload, dynamic MemoryRegisters)
        {
            return TimeFormatter ( MemoryRegisters.ReadIntervalMinutes.Value * ( 12 - MemoryRegisters.MessageOverlapCount.Value ) );
        }

        public string PCBNumber_Get (MemoryOverload<string> MemoryOverload, dynamic MemoryRegisters)
        {
            string tempString = string.Empty;

            //ASCII RANGE FOR PCBSupplierCode
            if ( MemoryRegisters.PCBSupplierCode.Value >= 65 &&
                 MemoryRegisters.PCBSupplierCode.Value <= 90 )
                tempString = tempString + Convert.ToChar(MemoryRegisters.PCBSupplierCode.Value) + "-";

            if ( MemoryRegisters.PCBCoreNumber.Value >= 0 )
                tempString = tempString + string.Format ( PCBFORMAT, MemoryRegisters.PCBCoreNumber.Value );

            if ( MemoryRegisters.PCBProductRevision.Value >= 65 &&
                 MemoryRegisters.PCBProductRevision.Value <= 90 )
                tempString = tempString + "-" + Convert.ToChar(MemoryRegisters.PCBProductRevision.Value);

            string result = ( string.IsNullOrEmpty ( tempString ) ) ? NTAVAILABLE : tempString;

            return result;
        }

        public string Encryption_Get (MemoryOverload<string> MemoryOverload, dynamic MemoryRegisters)
        {
            return ( MemoryRegisters.Encrypted.Value ) ? YES : NO;
        }

        public string MtuVoltageBattery_Get (MemoryOverload<string> MemoryOverload, dynamic MemoryRegisters)
        {
            return ((MemoryRegisters.MtuMiliVoltageBattery.Value * 1.0) / 1000).ToString( MTUVLFORMAT );
        }

        public string P1ReadingError_Get (MemoryOverload<string> MemoryOverload, dynamic MemoryRegisters)
        {
            return TranslateErrorCodes(MemoryRegisters.P1ReadingErrorCode.Value);
        }

        public string P2ReadingError_Get (MemoryOverload<string> MemoryOverload, dynamic MemoryRegisters)
        {
            return TranslateErrorCodes(MemoryRegisters.P2ReadingErrorCode.Value);
        }

        public string InterfaceTamperStatus_Get (MemoryOverload<string> MemoryOverload, dynamic MemoryRegisters)
        {
            return GetTemperStatus(MemoryRegisters.P1InterfaceAlarm.Value, MemoryRegisters.ProgrammingCoilInterfaceTamper.Value);
        }

        public string TiltTamperStatus_Get (MemoryOverload<string> MemoryOverload, dynamic MemoryRegisters)
        {
            return GetTemperStatus(MemoryRegisters.P1TiltAlarm.Value, MemoryRegisters.TiltTamper.Value);
        }

        public string MagneticTamperStatus_Get (MemoryOverload<string> MemoryOverload, dynamic MemoryRegisters)
        {
            return GetTemperStatus(MemoryRegisters.P1MagneticAlarm.Value, MemoryRegisters.MagneticTamper.Value);
        }

        public string RegisterCoverTamperStatus_Get (MemoryOverload<string> MemoryOverload, dynamic MemoryRegisters)
        {
            return GetTemperStatus(MemoryRegisters.P1RegisterCoverAlarm.Value, MemoryRegisters.RegisterCoverTamper.Value);
        }

        public string ReverseFlowTamperStatus_Get (MemoryOverload<string> MemoryOverload, dynamic MemoryRegisters)
        {
            return GetTemperStatus(MemoryRegisters.P1ReverseFlowAlarm.Value, MemoryRegisters.ReverseFlowTamper.Value);
        }

        public string FastMessagingMode_Get (MemoryOverload<string> MemoryOverload, dynamic MemoryRegisters)
        {
            return ( MemoryRegisters.Fast2Way.Value ) ? MESAG_FAST : MESAG_SLOW;
        }

        public string LastGasp_Get (MemoryOverload<string> MemoryOverload, dynamic MemoryRegisters)
        {
            return ( MemoryRegisters.LastGaspTamper.Value ) ? ENABLED : TRIGGERED;
        }

        public string InsufficentMemory_Get (MemoryOverload<string> MemoryOverload, dynamic MemoryRegisters)
        {
            return ( MemoryRegisters.InsufficentMemoryTamper.Value ) ? ENABLED : TRIGGERED;
        }

        public string P1Status_Get (MemoryOverload<string> MemoryOverload, dynamic MemoryRegisters)
        {
            return GetPortStatus(MemoryRegisters.P1StatusFlag.Value);
        }

        public string P2Status_Get (MemoryOverload<string> MemoryOverload, dynamic MemoryRegisters)
        {
            return GetPortStatus(MemoryRegisters.P2StatusFlag.Value);
        }

        public string F12WAYRegister1_Get(MemoryOverload<string> MemoryOverload, dynamic MemoryRegisters)
        {
            return HEX_PREFIX + MemoryRegisters.F12WAYRegister1Int.Value.ToString ( FWAYFORMAT );
        }

        public string F12WAYRegister10_Get(MemoryOverload<string> MemoryOverload, dynamic MemoryRegisters)
        {
            return HEX_PREFIX + MemoryRegisters.F12WAYRegister10Int.Value.ToString ( FWAYFORMAT );
        }

        public string F12WAYRegister14_Get(MemoryOverload<string> MemoryOverload, dynamic MemoryRegisters)
        {
            return HEX_PREFIX + MemoryRegisters.F12WAYRegister14Int.Value.ToString ( FWAYFORMAT );
        }

        public string Frequency1Way_Get(MemoryOverload<string> MemoryOverload, dynamic MemoryRegisters)
        {
            return String.Format(new CultureInfo("en-us"), "{0:0.000}",(MemoryRegisters.Frequency1WayHz.Value / 1000000.0));
        }

        public string Frequency2WayTx_Get(MemoryOverload<string> MemoryOverload, dynamic MemoryRegisters)
        {
            return String.Format(new CultureInfo("en-us"), "{0:0.000}", (MemoryRegisters.Frequency2WayTxHz.Value / 1000000.0));
        }

        public string Frequency2WayRx_Get(MemoryOverload<string> MemoryOverload, dynamic MemoryRegisters)
        {
            return String.Format(new CultureInfo("en-us"), "{0:0.000}", (MemoryRegisters.Frequency2WayRxHz.Value / 1000000.0));
        }

        public string InstallConfirmationStatus_Get(MemoryOverload<string> MemoryOverload, dynamic MemoryRegisters)
        {
            return ( MemoryRegisters.InstallConfirmationNotSynced.Value ) ? NTCONFIRMED : CONFIRMED;
        }

        public int MtuSoftVersion_Get ( MemoryOverload<int> memoryOverload, dynamic MemoryRegisters )
        {
            if ( MemoryRegisters.MtuSoftVersionNew.Value == 255 )
                return MemoryRegisters.MtuSoftVersionLegacy.Value;
            return MemoryRegisters.MtuSoftVersionNew.Value;
        }

        public string MtuSoftVersionString_Get ( MemoryOverload<string> memoryOverload, dynamic MemoryRegisters )
        {
            int mtuSoftVersion = MemoryRegisters.MtuSoftVersion.Value;
        
            if ( mtuSoftVersion == 254 )
                return string.Format ( MTU_SOFTVERSION_LONG,
                    MemoryRegisters.MtuSoftRevYear    .Value,
                    MemoryRegisters.MtuSoftRevMonth   .Value,
                    MemoryRegisters.MtuSoftBuildNumber.Value );
            
            return string.Format ( MTU_SOFTVERSION_SMALL, mtuSoftVersion );
        }

        public int MtuSoftVersion342x_Get ( MemoryOverload<int> memoryOverload, dynamic MemoryRegisters )
        {
            return MemoryRegisters.MtuSoftFormatFlag.Value;
        }

        public string MtuSoftVersionString342x_Get ( MemoryOverload<string> MemoryOverload, dynamic MemoryRegisters )
        {
            return string.Format ( MTU_SOFTVERSION_LONG, 
                MemoryRegisters.MtuSoftVersionMajor.Value,
                MemoryRegisters.MtuSoftVersionMinor.Value,
                MemoryRegisters.MtuSoftBuildNumber .Value );
        }

        #endregion

        #region Registers

        public int ReadIntervalMinutes_Set ( MemoryRegister<int> MemoryRegister, dynamic inputValue )
        {
            string[] readIntervalArray = ((string)inputValue).Split(' ');
            string readIntervalStr = readIntervalArray[0].ToLower ();
            string timeUnit = readIntervalArray[1].ToLower ();
            int timeIntervalMins = Int32.Parse(readIntervalStr);

            if ( timeUnit.StartsWith ( "hour" ) ||
                 timeUnit.StartsWith ( "hr"   ) )
                timeIntervalMins = timeIntervalMins * 60;

            return timeIntervalMins;
        }

        // Use with <CustomGet>method:ULongToBcd</CustomGet>
        public ulong BcdToULong ( MemoryRegister<ulong> MemoryRegister )
        {
            byte[] bytes  = MemoryRegister.ValueByteArray;
            string outNum = string.Empty;
            
            foreach ( byte b in bytes )
                outNum += b.ToString ( "X" );
            outNum = outNum.TrimEnd ( new char[] { 'F' } );

            outNum = outNum
                .Replace ( "A", "10" )
                .Replace ( "B", "11" )
                .Replace ( "C", "12" )
                .Replace ( "D", "13" )
                .Replace ( "E", "14" )
                .Replace ( "F", "15" );

            ulong a = ulong.Parse ( outNum );
            
            return a;
        }

        // Use with <CustomSet>method:ULongToBcd</CustomSet>
        public byte[] ULongToBcd ( MemoryRegister<ulong> MemoryRegister, dynamic inputValue )
        {
            if ( inputValue is string )
                return this.ULongToBcd_Logic ( inputValue, MemoryRegister.size );
            return this.ULongToBcd_Logic ( inputValue.ToString (), MemoryRegister.size );
        }

        // Convert hexadecimal number to integer value
        // Use with <CustomSet>method:HexToInt</CustomSet>
        public int HexToInt ( MemoryRegister<int> MemoryRegister, dynamic inputValue )
        {
            if ( inputValue is string ) // Removes 0x prefix
                 return int.Parse ( inputValue.Substring ( 2 ), NumberStyles.HexNumber );
            else return ( int ) inputValue;
        }

        #endregion

        #region e-Coder

        public string BackFlowState_Get (MemoryOverload<string> MemoryOverload, dynamic MemoryRegisters)
        {
            string reply = string.Empty;
            string param = Convert.ToString ( MemoryRegisters.FlowState.Value,INDEX_STATE )
                .PadLeft(PAD_LEFT,ZERO)
                .Substring(6);
            switch (param)
            {
                case CASE_00: reply = CASE_00_BFS; break;
                case CASE_01: reply = CASE_01_BFS; break;
                case CASE_10: reply = CASE_10_BFS; break;
            }
            return reply;
        }

        public string DaysOfNoFlow_Get (MemoryOverload<string> MemoryOverload, dynamic MemoryRegisters)
        {
            string reply = string.Empty;
            string param = Convert.ToString ( MemoryRegisters.FlowState.Value,INDEX_STATE )
                .PadLeft(PAD_LEFT,ZERO)
                .Substring(3,3);
            switch (param)
            {
                case CASE_000: reply = CASE_000_TX; break;
                case CASE_001: reply = CASE_001_TX; break;
                case CASE_010: reply = CASE_010_TX; break;
                case CASE_011: reply = CASE_011_TX; break;
                case CASE_100: reply = CASE_100_TX; break;
                case CASE_101: reply = CASE_101_TX; break;
                case CASE_110: reply = CASE_110_TX; break;
            }
            return reply + CASE_NOFLOW;
        }

        public string LeakDetection_Get (MemoryOverload<string> MemoryOverload, dynamic MemoryRegisters)
        {
            string reply = string.Empty;
            string param = Convert.ToString ( MemoryRegisters.LeakState.Value,INDEX_STATE )
                .PadLeft(PAD_LEFT,ZERO)
                .Substring(5,2);
            switch (param)
            {
                case CASE_00: reply = CASE_00_LKD; break;
                case CASE_01: reply = CASE_01_LKD; break;
                case CASE_10: reply = CASE_10_LKD; break;
            }
            return reply;
        }

        public string DaysOfLeak_Get (MemoryOverload<string> MemoryOverload, dynamic MemoryRegisters)
        {
            string reply = string.Empty;
            string param = Convert.ToString ( MemoryRegisters.LeakState.Value,INDEX_STATE )
                .PadLeft(PAD_LEFT,ZERO)
                .Substring(2, 3);
            switch (param)
            {
                case CASE_000: reply = CASE_000_TX; break;
                case CASE_001: reply = CASE_001_TX; break;
                case CASE_010: reply = CASE_010_TX; break;
                case CASE_011: reply = CASE_011_TX; break;
                case CASE_100: reply = CASE_100_TX; break;
                case CASE_101: reply = CASE_101_TX; break;
                case CASE_110: reply = CASE_110_TX; break;
            }
            return reply + CASE_LEAK;
        }

        #endregion

        #region AuxiliaryFunctions

        private string TranslateErrorCodes (int encoderErrorcode)
        {
            if (encoderErrorcode == ECODE_METER) return ERROR_METER;
            if (encoderErrorcode == ECODE_DIGIT) return ERROR_DIGIT;
            if (encoderErrorcode == ECODE_OVER ) return ERROR_OVER;
            if (encoderErrorcode == ECODE_PURGE) return ERROR_PURGE;
            return string.Empty;
        }

        private string TimeFormatter ( int time )
        {
            switch ( time )
            {
                case 2880: return 48 + HOURS;
                case 2160: return 36 + HOURS;
                case 1440: return 24 + HOURS;
                case 720 : return 12 + HOURS;
                case 480 : return 8  + HOURS;
                case 360 : return 6  + HOURS;
                case 240 : return 4  + HOURS;
                case 180 : return 3  + HOURS;
                case 120 : return 2  + HOURS;
                case 90  : return 1  + HOUR + " " + 30 + MIN;
                case 60  : return 1  + HOUR;
                case 30  : return 30 + MIN;
                case 15  : return 15 + MIN;
                case 10  : return 10 + MIN;
                case 5   : return 5  + MIN;
                default: // KG 3.10.2010 add HR-Min calc:
                    if  ( time % 60 == 0 ) return ( time / 60 ).ToString() + HOURS;
                    else if ( time <  60 ) return ( time % 60 ).ToString() + MIN;
                    else if ( time < 120 ) return ( time / 60 ).ToString() + HOUR + " " + (time % 60).ToString() + MIN;
                    else return ( time / 60 ).ToString() + HOURS + " " + (time % 60).ToString() + MIN;
            }
        }

        private string GetTemperStatus (bool alarm, bool temper)
        {
            if ( alarm )
                return ( temper ) ? TRIGGERED : ENABLED;
            return DISABLED;
        }

        private string GetPortStatus (bool status)
        {
            return ( status ) ? ENABLED : DISABLED;
        }

        /*
        private ulong BcdToULong_Logic ( ulong valueInBCD )
        {
            // Define powers of 10 for the BCD conversion routines.
            ulong powers = 1;
            ulong outNum = 0;
            byte tempNum;

            for (int offset = 0; offset < 7; offset++)
            {
                tempNum = (byte)((valueInBCD >> offset * 8) & 0xff);
                if ((tempNum & 0x0f) > 9)
                {
                    break;
                }
                outNum += (ulong)(tempNum & 0x0f) * powers;
                powers *= 10;
                if ((tempNum >> 4) > 9)
                {
                    break;
                }
                outNum += (ulong)(tempNum >> 4) * powers;
                powers *= 10;
            }

            return outNum;
        }
        */

        public byte[] ULongToBcd_Logic ( string value, int size )
        {
            var convertedBytes = new byte[ size ];
            var strNumber      = value;
            var currentNumber  = string.Empty;

            for ( var i = 0; i < size; i++ )
                convertedBytes[i] = 0xff;

            for ( var i = 0; i < strNumber.Length; i++ )
            {
                currentNumber += strNumber[i];

                if (i == strNumber.Length - 1 && i % 2 == 0)
                {
                    convertedBytes[i / 2] = 0xf;
                    convertedBytes[i / 2] |= (byte)((int.Parse(currentNumber) % 10) << 4);
                }

                if (i % 2 == 0) continue;
                var v = int.Parse(currentNumber);
                convertedBytes[(i - 1) / 2] = (byte) (v % 10);
                convertedBytes[(i - 1) / 2] |= (byte)((v / 10) << 4);
                currentNumber = string.Empty;
            }

            return convertedBytes;
        }

        #endregion
    }
}
