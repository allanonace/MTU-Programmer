using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTUComm
{
    public class Parameter
    {
        public enum ParameterType
        {
            ActivityLogId = 0,
            MeterType = 1,
            UnitOfMeasure = 2,
            Port2Disabled = 3,
            AccountNumber = 4,
            ProvidingHandFactor = 5,
            ReadInterval = 6,
            ForceTimeSync = 7,
            WorkOrder = 8,
            LiveDigits = 9,
            TempReadInterval = 10,
            Alarm = 11,
            MeterSerialNumber = 12,
            NumberOfDials = 13,
            TempReadDays = 14,
            MeterReading = 15,
            DriveDialSize = 16,
            SnapRead = 17,
            OldMtuId = 18,
            OldMeterSerialNumber = 19,
            NewMeterSerialNumber = 20,
            OldMeterReading = 21,
            NewMeterReading = 22,
            DaysOfRead = 23,
            Custom = 24
        };

        private class ParameterDefine {

            public Boolean memory_present;
            public Boolean log_generation;
            public String action_tag;
            public String action_display;

            public ParameterDefine(Boolean present, Boolean generates_log, String tag, String display)
            {
                memory_present = present;
                log_generation = generates_log;
                action_tag = tag;
                action_display = display;
            }
        };

        private Dictionary<ParameterType, ParameterDefine> paremeter_defines = new Dictionary<ParameterType, ParameterDefine>()
        {
            {ParameterType.ActivityLogId, new ParameterDefine(true, true, "ActivityLogId", "") },
            {ParameterType.MeterType, new ParameterDefine(true, true, "Port{0}MeterType", "Meter Type")},
            {ParameterType.UnitOfMeasure, new ParameterDefine(true, true, "UnitOfMeasure", "")},
            {ParameterType.Port2Disabled, new ParameterDefine(true, true, "Port2Disabled", "")},
            {ParameterType.AccountNumber, new ParameterDefine(true, true, "Port{0}AccountNumber", "Service Pt. ID")},
            {ParameterType.ProvidingHandFactor, new ParameterDefine(true, true, "ProvidingHandFactor", "")},
            {ParameterType.ReadInterval, new ParameterDefine(true, true, "ReadInterval", "Read Interval")},
            {ParameterType.ForceTimeSync, new ParameterDefine(true, true, "ForceTimeSync", "")},
            {ParameterType.WorkOrder, new ParameterDefine(true, true, "WorkOrder", "Field Order")},
            {ParameterType.LiveDigits, new ParameterDefine(true, true, "LiveDigits", "")},
            {ParameterType.TempReadInterval, new ParameterDefine(true, true, "TempReadInterval", "")},
            {ParameterType.Alarm, new ParameterDefine(true, true, "Alarm", "")},
            {ParameterType.MeterSerialNumber, new ParameterDefine(true, true, "MeterSerialNumber", "Meter Number")},
            {ParameterType.NumberOfDials, new ParameterDefine(true, true, "NumberOfDials", "")},
            {ParameterType.TempReadDays, new ParameterDefine(true, true, "TempReadDays", "")},
            {ParameterType.MeterReading, new ParameterDefine(true, true, "MeterReading", "Meter Reading")},
            {ParameterType.DriveDialSize, new ParameterDefine(true, true, "DriveDialSize", "")},
            {ParameterType.SnapRead, new ParameterDefine(true, true, "SnapRead", "")},
            {ParameterType.OldMtuId, new ParameterDefine(true, true, "OldMtuId", "Old MTU ID")},
            {ParameterType.OldMeterSerialNumber, new ParameterDefine(true, true, "Port{0}OldMeterSerialNumber", "Old Meter Serial Number")},
            {ParameterType.NewMeterSerialNumber, new ParameterDefine(true, true, "Port{0}NewMeterSerialNumber", "New Meter Serial Number")},
            {ParameterType.OldMeterReading, new ParameterDefine(true, true, "Port{0}OldMeterReading", "Old Meter Reading")},
            {ParameterType.NewMeterReading, new ParameterDefine(true, true, "Port{0}NewMeterReading", "")},
            {ParameterType.DaysOfRead, new ParameterDefine(true, true, "DaysOfRead", "DaysOfRead")},
            {ParameterType.Custom, new ParameterDefine(true, true, "{1}", "{1}")}
        };


        private Boolean has_port = false;
        private int port = 1;

        private String mCustomParameter;
        private String mCustomDisplay = null;

        private ParameterType mParameterType;

        private dynamic mValue;

        private bool optional = false;

        public Parameter ()
        {
            mValue = null;
        }

        public Parameter(ParameterType type, String value)
        {
            mParameterType = type;
            mValue = value;
        }

        public Parameter(ParameterType type, String value, int port)
        {
            if(port == 0)
            {
                port++;
            }
            mParameterType = type;
            mValue = value;
            setPort(port);
        }

        public Parameter(String custom_parameter, String custom_display, dynamic value, bool optional = false )
        {
            mParameterType   = ParameterType.Custom;
            mCustomParameter = custom_parameter;
            mCustomDisplay   = custom_display;
            mValue           = value;
            this.optional    = optional;
        }

        public ParameterType Type
        {
            get
            {
                return mParameterType;
            }
        }


        public int Port
        {
            get
            {
                if (hasPort())
                {
                    return port;
                }
                else
                {
                    return 0;
                }
            }
        }

        public Boolean isInMemoryMap()
        {
            return paremeter_defines[mParameterType].memory_present;
        }

        public void setPort(int port)
        {
            this.port = port;
            has_port = true;
        }

        public Boolean hasPort()
        {
            return has_port;
        }

        public String getLogTag()
        {
            return String.Format(paremeter_defines[mParameterType].action_tag, port, mCustomParameter);
        }

        public String getLogDisplay()
        {
            String display = String.Format(paremeter_defines[mParameterType].action_display, port,  mCustomDisplay);
            if(display.Length > 0)
            {
                return display;
            }

            return null;
        }

        public Boolean doesGenerateLog()
        {
            return paremeter_defines[mParameterType].log_generation;
        }

        public bool Optional
        {
            get
            {
                return this.optional;
            }
        }

        public string CustomParameter
        {
            get
            {
                return mCustomParameter;
            }
        }

        public string CustomDisplay
        {
            get
            {
                return mCustomDisplay;
            }
        }

        public dynamic Value
        {
            get
            {
                //if ( this.mParameterType == ParameterType.MeterReading ||
                //     this.mCustomParameter.Equals ( "MeterReading" ) )


                return mValue;
            }
        }

        public override string ToString()
        {
            return CustomParameter + ": " + Value;
        }
    }

    public static class ParameterListExtension
    {
        private const int PARAMETER_INDEX = 1;

        public static Parameter FindByParamId(
            this List<Parameter> paramList, dynamic paramId, dynamic texts = null)
        {
            if ( texts == null )
                return paramList.Find(x => string.Equals(x.CustomParameter, paramId));
            
            return paramList.Find(x => string.Equals(x.CustomParameter, texts[ paramId ][ PARAMETER_INDEX ]));
        }
    }
}
