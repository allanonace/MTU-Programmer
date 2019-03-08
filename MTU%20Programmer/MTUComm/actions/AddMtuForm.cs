using System;
using System.Collections.Generic;
using System.Text;
using Xml;
using MTUComm.Exceptions;

using ParameterType = MTUComm.Parameter.ParameterType;

namespace MTUComm.actions
{
    public class AddMtuForm : MtuForm
    {
        public enum FIELD
        {
            NOTHING,
        
            MTU_ID_OLD,
            
            ACCOUNT_NUMBER,
            ACCOUNT_NUMBER_2,
            
            WORK_ORDER,
            WORK_ORDER_2,
            
            ACTIVITY_LOG_ID,
            
            METER_NUMBER,
            METER_NUMBER_2,
            METER_NUMBER_OLD,
            METER_NUMBER_OLD_2,
            
            METER_READING,
            METER_READING_2,
            METER_READING_OLD,
            METER_READING_OLD_2,
            
            METER_WORKING_OLD,
            METER_WORKING_OLD_2,
            REPLACE_METER_REG,
            REPLACE_METER_REG_2,
            
            METER_TYPE,
            METER_TYPE_2,
            
            NUMBER_OF_DIALS,
            NUMBER_OF_DIALS_2,
            DRIVE_DIAL_SIZE,
            DRIVE_DIAL_SIZE_2,
            UNIT_MEASURE,
            UNIT_MEASURE_2,
            
            READ_INTERVAL,
            SNAP_READS,
            TWO_WAY,
            ALARM,
            DEMAND,
            
            GPS_LATITUDE,
            GPS_LONGITUDE,
            GPS_ALTITUDE,
            OPTIONAL_PARAMS,
            FORCE_TIME_SYNC
        }

        private const string PORT_2_SUFIX = "_2";

        public Dictionary<ParameterType,FIELD> IdsAclara =
            new Dictionary<ParameterType,FIELD> ()
            {
                { ParameterType.OldMtuId,             FIELD.MTU_ID_OLD        },
                { ParameterType.AccountNumber,        FIELD.ACCOUNT_NUMBER    },
                { ParameterType.ActivityLogId,        FIELD.ACTIVITY_LOG_ID   },
                { ParameterType.WorkOrder,            FIELD.WORK_ORDER        },
                { ParameterType.MeterType,            FIELD.METER_TYPE        },
                { ParameterType.NumberOfDials,        FIELD.NUMBER_OF_DIALS   },
                { ParameterType.DriveDialSize,        FIELD.DRIVE_DIAL_SIZE   },
                { ParameterType.UnitOfMeasure,        FIELD.UNIT_MEASURE      },
                { ParameterType.SnapRead,             FIELD.SNAP_READS        },
                { ParameterType.Custom,               FIELD.OPTIONAL_PARAMS   },
                { ParameterType.ReadInterval,         FIELD.READ_INTERVAL     },
                { ParameterType.Alarm,                FIELD.ALARM             },
                { ParameterType.ForceTimeSync,        FIELD.FORCE_TIME_SYNC   },
                
                { ParameterType.MeterSerialNumber,    FIELD.METER_NUMBER      },
                { ParameterType.NewMeterSerialNumber, FIELD.METER_NUMBER      },
                { ParameterType.OldMeterSerialNumber, FIELD.METER_NUMBER_OLD  },
                
                { ParameterType.MeterReading,         FIELD.METER_READING     },
                { ParameterType.NewMeterReading,      FIELD.METER_READING     },
                { ParameterType.OldMeterReading,      FIELD.METER_READING_OLD }
            };

        // Elements array
        // 0. Parameter ID
        // 1. Custom parameter
        // 2. Custom display
        public Dictionary<FIELD, string[]> Texts =
            new Dictionary<FIELD, string[]>()
            {
                #region Service Port ID = Account Number = Functl Loctn
                {
                    FIELD.ACCOUNT_NUMBER,
                    new string[]
                    {
                        "AccountNumber",
                        "AccountNumber",
                        "Service Port ID"
                    }
                },
                {
                    FIELD.ACCOUNT_NUMBER_2,
                    new string[]
                    {
                        "AccountNumber_2",
                        "AccountNumber",
                        "Service Port ID"
                    }
                },
                #endregion
                #region Field Order = Work Order
                {
                    FIELD.WORK_ORDER,
                    new string[]
                    {
                        "WorkOrder",
                        "WorkOrder",
                        "Work Order"
                    }
                },
                {
                    FIELD.WORK_ORDER_2,
                    new string[]
                    {
                        "WorkOrder_2",
                        "WorkOrder",
                        "Work Order"
                    }
                },
                #endregion
                #region Old MTU ID
                {
                    FIELD.MTU_ID_OLD,
                    new string[]
                    {
                        "OldMtuId",
                        "OldMtuID",
                        "Old MTU ID"
                    }
                },
                #endregion
                #region Activity Log ID
                {
                    FIELD.ACTIVITY_LOG_ID,
                    new string[]
                    {
                        "ActivityLogId",
                        "ActivityLogID",
                        "Activity Log ID"
                    }
                },
                #endregion
                #region Meter Serial Number
                {
                    FIELD.METER_NUMBER,
                    new string[]
                    {
                        "MeterNumber",
                        "NewMeterSerialNumber",
                        "New Meter Serial Number"
                    }
                },
                {
                    FIELD.METER_NUMBER_2,
                    new string[]
                    {
                        "MeterNumber_2",
                        "NewMeterSerialNumber",
                        "New Meter Serial Number"
                    }
                },
                {
                    FIELD.METER_NUMBER_OLD,
                    new string[]
                    {
                        "MeterNumberOld",
                        "OldMeterSerialNumber",
                        "Old Meter Serial Number"
                    }
                },
                {
                    FIELD.METER_NUMBER_OLD_2,
                    new string[]
                    {
                        "MeterNumberOld_2",
                        "OldMeterSerialNumber",
                        "Old Meter Serial Number"
                    }
                },
                #endregion
                #region Initial Reading = Meter Reading
                {
                    FIELD.METER_READING,
                    new string[]
                    {
                        "MeterReading",
                        "MeterReading",
                        "Meter Reading"
                    }
                },
                {
                    FIELD.METER_READING_2,
                    new string[]
                    {
                        "MeterReading_2",
                        "MeterReading",
                        "Meter Reading"
                    }
                },
                {
                    FIELD.METER_READING_OLD,
                    new string[]
                    {
                        "MeterReadingOld",
                        "OldMeterReading",
                        "Old Meter Reading"
                    }
                },
                {
                    FIELD.METER_READING_OLD_2,
                    new string[]
                    {
                        "MeterReadingOld_2",
                        "OldMeterReading",
                        "Old Meter Reading"
                    }
                },
                #endregion
                #region Meter Type ( Meter ID )
                {
                    FIELD.METER_TYPE,
                    new string[]
                    {
                        "Meter",
                        "SelectedMeterId",
                        "Selected Meter ID"
                    }
                },
                {
                    FIELD.METER_TYPE_2,
                    new string[]
                    {
                        "Meter_2",
                        "SelectedMeterId",
                        "Selected Meter ID"
                    }
                },
                #endregion

                #region Old Meter Working
                {
                    FIELD.METER_WORKING_OLD,
                    new string[]
                    {
                        "OldMeterWorking",
                        "OldMeterWorking",
                        "Old Meter Working"
                    }
                },
                {
                    FIELD.METER_WORKING_OLD_2,
                    new string[]
                    {
                        "OldMeterWorking_2",
                        "OldMeterWorking",
                        "Old Meter Working"
                    }
                },
                #endregion
                #region Replace Meter|Register
                {
                    FIELD.REPLACE_METER_REG,
                    new string[]
                    {
                        "ReplaceMeterRegister",
                        "ReplaceMeterRegister",
                        "Replace Meter/Register"
                    }
                },
                {
                    FIELD.REPLACE_METER_REG_2,
                    new string[]
                    {
                        "ReplaceMeterRegister_2",
                        "ReplaceMeterRegister",
                        "Replace Meter/Register"
                    }
                },
                #endregion

                #region Number of Dials
                {
                    FIELD.NUMBER_OF_DIALS,
                    new string[]
                    {
                        "NumberOfDials",
                        "NumberOfDials",
                        "Number of Dials"
                    }
                },
                {
                    FIELD.NUMBER_OF_DIALS_2,
                    new string[]
                    {
                        "NumberOfDials_2",
                        "NumberOfDials",
                        "Number of Dials"
                    }
                },
                #endregion
                #region Drive Dial Size
                {
                    FIELD.DRIVE_DIAL_SIZE,
                    new string[]
                    {
                        "DriveDialSize",
                        "DriveDialSize",
                        "Drive Dial Size"
                    }
                },
                {
                    FIELD.DRIVE_DIAL_SIZE_2,
                    new string[]
                    {
                        "DriveDialSize_2",
                        "DriveDialSize",
                        "Drive Dial Size"
                    }
                },
                #endregion
                #region Unit of Measure
                {
                    FIELD.UNIT_MEASURE,
                    new string[]
                    {
                        "UnitOfMeasure",
                        "UnitOfMeasure",
                        "Unit of Measure"
                    }
                },
                {
                    FIELD.UNIT_MEASURE_2,
                    new string[]
                    {
                        "UnitOfMeasure_2",
                        "UnitOfMeasure",
                        "Unit of Measure"
                    }
                },
                #endregion

                #region Read Interval
                {
                    FIELD.READ_INTERVAL,
                    new string[]
                    {
                        "ReadInterval",
                        "ReadInterval",
                        "Read Interval"
                    }
                },
                #endregion
                #region Snap Reads
                {
                    FIELD.SNAP_READS,
                    new string[]
                    {
                        "SnapReads",
                        "SnapReads",
                        "Snap Reads"
                    }
                },
                #endregion
                #region 2Way
                {
                    FIELD.TWO_WAY,
                    new string[]
                    {
                        "TwoWay",
                        "Fast-2-Way",
                        "Fast Message Config"
                    }
                },
                #endregion
                #region Alarm
                {
                    FIELD.ALARM,
                    new string[]
                    {
                        "Alarm",
                        "Alarms",
                        "Alarms"
                    }
                },
                #endregion
                #region Demand
                {
                    FIELD.DEMAND,
                    new string[]
                    {
                        "Demand",
                        "Demands",
                        "Demands"
                    }
                },
                #endregion
                #region GPS
                {
                    FIELD.GPS_LATITUDE,
                    new string[]
                    {
                        "GPSLat",
                        "GPS_Y",
                        "Lat"
                    }
                },
                {
                    FIELD.GPS_LONGITUDE,
                    new string[]
                    {
                        "GPSLon",
                        "GPS_X",
                        "Long"
                    }
                },
                {
                    FIELD.GPS_ALTITUDE,
                    new string[]
                    {
                        "GPSAlt",
                        "Altitude",
                        "Elevation"
                    }
                },
                #endregion
                #region Optional Parameters
                {
                    FIELD.OPTIONAL_PARAMS,
                    new string[]
                    {
                        "OptionalParams",
                        "OptionalParams",
                        "OptionalParams"
                    }
                },
                #endregion
                #region Force TimeSync -> Install Confirmation
                {
                    FIELD.FORCE_TIME_SYNC,
                    new string[]
                    {
                        "ForceTimeSync",
                        "ForceTimeSync",
                        "Force TimeSync"
                    }
                }
                #endregion
            };

        public bool usePort2;
        private Dictionary<FIELD,Parameter> dictionary;

        public Dictionary<FIELD,Parameter> RegisteredParamsByField
        {
            get { return this.dictionary; }
        }

        public AddMtuForm ( Mtu mtu ) : base ( mtu )
        {
            this.dictionary = new Dictionary<FIELD,Parameter> ();
        }

        public void AddParameter ( FIELD fieldType, dynamic value )
        {
            string[] texts = Texts[ fieldType ];
            Parameter param = AddParameter ( texts[ 0 ], texts[ 1 ], texts[ 2 ], value ); // base method
            this.dictionary.Add ( fieldType, param );
        }

        public void AddParameterTranslatingAclaraXml ( Parameter parameter )
        {
            ParameterType typeAclara;
            FIELD typeOwn = FIELD.NOTHING;

            string nameTypeAclara = parameter.Type.ToString ();

            // Translate aclara tag/id to us
            if ( ! Enum.TryParse<ParameterType> ( nameTypeAclara, out typeAclara ) )
                Errors.LogErrorNow ( new ProcessingParamsScriptException () );
            else
            {
                if ( IdsAclara.ContainsKey ( typeAclara ) )
                    typeOwn = IdsAclara[ typeAclara ];
                else
                    return;

                // If is for port two, find the correct enum element adding two ( "_2" ) as sufix
                if ( parameter.Port == 2 )
                    Enum.TryParse<FIELD> ( typeOwn.ToString () + PORT_2_SUFIX, out typeOwn );
            }

            this.AddParameter ( typeOwn, parameter.Value );
        }

        public Parameter FindById ( FIELD field_type )
        {
            string[] texts = Texts[field_type];
            return base.FindParameterById(texts[0]);
        }

        public bool ContainsParameter ( FIELD fieldType )
        {
            return base.ContainsParameter ( Texts[ fieldType ][ 0 ] );
        }
        
        public void RemoveParameter ( FIELD fieldType )
        {
            base.RemoveParameter ( Texts[ fieldType ][ 0 ] );
            this.dictionary.Remove ( fieldType );
        }
    }
}
