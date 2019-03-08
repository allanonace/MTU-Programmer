using System;
using System.Collections.Generic;
using System.Xml.Linq;
using MTUComm.actions;
using Xml;

using FIELD = MTUComm.actions.AddMtuForm.FIELD;
using ActionType = MTUComm.Action.ActionType;

namespace MTUComm
{
    public class AddMtuLog
    {
        public  Logger logger;
        private string user;
        private dynamic form;
        private MTUBasicInfo mtuBasicInfo;
        private string logUri;
        //private Mtu mtu;

        private const string ENABLE  = "Enable";
        private const string DISABLE = "Disable";

        private const string addMtuDisplay = "Add MTU";
        private const string addMtuType = "Program MTU";
        private const string addMtuReason = "AddMtu";

        private const string turnOffDisplay = "Turn Off MTU";
        private const string turnOffType = "TurnOffType";

        private const string turnOnDisplay = "Turn On MTU";
        private const string turnOnType = "TurnOnType";

        private const string readMtuDisplay = "Read MTU";
        private const string readMtuType = "ReadMTU";

        private XDocument doc;
        private XElement  addMtuAction;
        private XElement  turnOffAction;
        private XElement  turnOnAction;
        private XElement  readMtuAction;

        public AddMtuLog(Logger logger, dynamic form, string user, bool isFromScripting )
        {
            this.logger = logger;
            this.form = form;
            this.user = user;
            this.mtuBasicInfo = MtuForm.mtuBasicInfo;
            this.logUri = this.logger.CreateFileIfNotExist ( ! isFromScripting );

            this.addMtuAction  = new XElement("Action");
            this.turnOffAction = new XElement("Action");
            this.turnOnAction  = new XElement("Action");
            this.readMtuAction = new XElement("Action");
        }

        public void LogTurnOff ()
        {
            logger.addAtrribute(this.turnOffAction, "display", turnOffDisplay);
            logger.addAtrribute(this.turnOffAction, "type", turnOffType);

            logger.logParameter(this.turnOffAction, new Parameter("Date", "Date/Time", DateTime.UtcNow.ToString("MM/dd/yyyy HH:mm:ss")));

            if (!string.IsNullOrEmpty(this.user))
                logger.logParameter(this.turnOffAction, new Parameter("User", "User", this.user));

            logger.logParameter(this.turnOffAction, new Parameter("MtuId", "MTU ID", this.mtuBasicInfo.Id));
        }

        public void LogTurnOn ()
        {
            logger.addAtrribute(this.turnOnAction, "display", turnOnDisplay);
            logger.addAtrribute(this.turnOnAction, "type", turnOnType);

            logger.logParameter(this.turnOnAction, new Parameter("Date", "Date/Time", DateTime.UtcNow.ToString("MM/dd/yyyy HH:mm:ss")));

            if (!string.IsNullOrEmpty(this.user))
                logger.logParameter(this.turnOnAction, new Parameter("User", "User", this.user));

            logger.logParameter(this.turnOnAction, new Parameter("MtuId", "MTU ID", this.mtuBasicInfo.Id));
        }

        public void LogAddMtu ( bool isFromScripting = false )
        {
            Mtu     mtu    = form.mtu;
            Global  global = form.global;
            dynamic map    = form.map;
            string  temp   = string.Empty;

            bool isReplaceMeter = form.actionType == ActionType.ReplaceMeter           ||
                                  form.actionType == ActionType.ReplaceMtuReplaceMeter ||
                                  form.actionType == ActionType.AddMtuReplaceMeter;
            bool isReplaceMtu   = form.actionType == ActionType.ReplaceMTU ||
                                  form.actionType == ActionType.ReplaceMtuReplaceMeter;

            #region General

            logger.addAtrribute ( this.addMtuAction, "display", addMtuDisplay );
            logger.addAtrribute ( this.addMtuAction, "type",    addMtuType    );
            logger.addAtrribute ( this.addMtuAction, "reason",  addMtuReason  );

            logger.logParameter ( this.addMtuAction, new Parameter("Date", "Date/Time", DateTime.UtcNow.ToString("MM/dd/yyyy HH:mm:ss")));

            if ( ! string.IsNullOrEmpty ( this.user ) )
                logger.logParameter(this.addMtuAction, new Parameter("User", "User", this.user ) );

            if ( isReplaceMtu &&
                 form.ContainsParameter ( FIELD.MTU_ID_OLD ) )
                logger.logParameter ( this.addMtuAction, form.OldMtuId );

            logger.logParameter ( this.addMtuAction, new Parameter ( "MtuId",   "MTU ID",   this.mtuBasicInfo.Id   ) );
            logger.logParameter ( this.addMtuAction, new Parameter ( "MtuType", "MTU Type", this.mtuBasicInfo.Type ) );
            logger.logParameter ( this.addMtuAction, form.ReadInterval );

            bool   useDailyReads    = ( global.AllowDailyReads && mtu.DailyReads );
            string dailyReads       = ( useDailyReads ) ? form.SnapReads.Value : "Disable";
            string dailyGmtHourRead = ( useDailyReads ) ? form.SnapReads.Value : "Disable";
            logger.logParameter(this.addMtuAction, new Parameter("DailyGMTHourRead", "GMT Daily Reads", dailyGmtHourRead));
            logger.logParameter(this.addMtuAction, new Parameter("DailyReads", "Daily Reads", dailyReads));

            if ( mtu.FastMessageConfig )
                logger.logParameter ( this.addMtuAction, form.TwoWay );

            // Related to F12WAYRegister1XX registers
            string afc = ( mtu.TimeToSync &&
                           global.AFC &&
                           map.MtuSoftVersion >= 19 ) ? "Set" : "Off";
            logger.logParameter ( this.addMtuAction, new Parameter ( "AFC", "AFC", afc ) );

            #endregion

            #region Port 1

            Meter meter = ( ! isFromScripting ) ?
                ( Meter )form.Meter.Value :
                Configuration.GetInstance().getMeterTypeById ( Convert.ToInt32 ( ( string )form.Meter.Value ) );

            XElement port = new XElement("Port");
            logger.addAtrribute(port, "display", "Port 1");
            logger.addAtrribute(port, "number", "1");

            logger.logParameter ( port, form.AccountNumber );

            if ( global.WorkOrderRecording )
                logger.logParameter ( port, form.WorkOrder );

            if ( isReplaceMeter )
            {
                if ( global.UseMeterSerialNumber )
                    logger.logParameter ( port, form.MeterNumberOld );
                
                if ( global.MeterWorkRecording )
                    logger.logParameter ( port, form.OldMeterWorking );
                
                if ( global.OldReadingRecording )
                    logger.logParameter ( port, form.MeterReadingOld );
                
                if ( global.RegisterRecording )
                    logger.logParameter ( port, form.ReplaceMeterRegister );
                
                if ( global.AutoRegisterRecording )
                {
                    temp = ( string.Equals ( form.MeterNumber, form.MeterNumberOld ) ) ?
                             "Register head change" : "Meter change";
                    logger.logParameter ( port, new Parameter ( "MeterRegisterAutoStatus", temp, "Meter Register Auto Status" ) );
                }
            }

            string meterType = string.Format("({0}) {1}", meter.Id, meter.Display);
            logger.logParameter ( port, new Parameter("MeterType", "Meter Type", meterType));
            logger.logParameter ( port, new Parameter("MeterTypeId", "Meter Type ID", meter.Id.ToString()));
            logger.logParameter ( port, new Parameter("MeterVendor", "Meter Vendor", meter.Vendor));
            logger.logParameter ( port, new Parameter("MeterModel", "Meter Model", meter.Model));
            
            if ( global.UseMeterSerialNumber )
                logger.logParameter ( port, form.MeterNumber );
            
            logger.logParameter ( port, form.MeterReading );
            
            logger.logParameter ( port, new Parameter("PulseHi","Pulse Hi Time", meter.PulseHiTime.ToString ().PadLeft ( 2, '0' ) ) );
            logger.logParameter ( port, new Parameter("PulseLo","Pulse Low Time", meter.PulseLowTime.ToString ().PadLeft ( 2, '0' ) ) );

            this.addMtuAction.Add(port);

            #endregion

            #region Port 2

            if ( form.usePort2 )
            {
                Meter meter2 = ( ! isFromScripting ) ?
                    ( Meter )form.Meter_2.Value :
                    Configuration.GetInstance().getMeterTypeById ( Convert.ToInt32 ( ( string )form.Meter_2.Value ) );

                port = new XElement ( "Port");
                logger.addAtrribute ( port, "display", "Port 2" );
                logger.addAtrribute ( port, "number", "2" );

                logger.logParameter ( port, form.AccountNumber_2 );

                if ( global.WorkOrderRecording )
                    logger.logParameter ( port, form.WorkOrder_2 );

                if ( isReplaceMeter )
                {
                    if ( global.UseMeterSerialNumber )
                        logger.logParameter ( port, form.MeterNumberOld_2 );

                    if ( global.MeterWorkRecording )
                        logger.logParameter ( port, form.OldMeterWorking_2 );
                    
                    if ( global.OldReadingRecording )
                        logger.logParameter ( port, form.MeterReadingOld_2 );
                    
                    if ( global.RegisterRecording )
                        logger.logParameter ( port, form.ReplaceMeterRegister_2 );
                    
                    if ( global.AutoRegisterRecording )
                    {
                        temp = ( string.Equals ( form.MeterNumber_2, form.MeterNumberOld_2 ) ) ?
                                 "Register head change" : "Meter change";
                        logger.logParameter ( port, new Parameter ( "MeterRegisterAutoStatus", temp, "Meter Register Auto Status" ) );
                    }
                }
                
                string meterType2 = string.Format("({0}) {1}", meter2.Id, meter2.Display);
                logger.logParameter ( port, new Parameter("MeterType", "Meter Type", meterType2));
                logger.logParameter ( port, new Parameter("MeterTypeId", "Meter Type ID", meter2.Id.ToString()));
                logger.logParameter ( port, new Parameter("MeterVendor", "Meter Vendor", meter2.Vendor));
                logger.logParameter ( port, new Parameter("MeterModel", "Meter Model", meter2.Model));
                
                if ( global.UseMeterSerialNumber )
                    logger.logParameter ( port, form.MeterNumber_2 );
                    
                logger.logParameter ( port, form.MeterReading_2 );

                logger.logParameter ( port, new Parameter("PulseHi","Pulse Hi Time", meter2.PulseHiTime.ToString ().PadLeft ( 2, '0' ) ) );
                logger.logParameter ( port, new Parameter("PulseLo","Pulse Low Time", meter2.PulseLowTime.ToString ().PadLeft ( 2, '0' ) ) );

                this.addMtuAction.Add(port);
            }

            #endregion

            #region Alarms

            if ( mtu.RequiresAlarmProfile )
            {
                Alarm alarms = (Alarm)form.Alarm.Value;
                if ( alarms != null )
                {
                    XElement alarmSelection = new XElement("AlarmSelection");
                    logger.addAtrribute(alarmSelection, "display", "Alarm Selection");

                    string alarmConfiguration = alarms.Name;
                    logger.logParameter(alarmSelection, new Parameter("AlarmConfiguration", "Alarm Configuration Name", alarmConfiguration));

                    string immediateAlarmTransmit = "False";
                    if (alarms.ImmediateAlarmTransmit)
                    {
                        immediateAlarmTransmit = "True";
                    }
                    logger.logParameter(alarmSelection, new Parameter("ImmediateAlarm", "Immediate Alarm Transmit", immediateAlarmTransmit));

                    string urgentAlarm = "False";
                    if (alarms.DcuUrgentAlarm)
                    {
                        urgentAlarm = "True";
                    }
                    logger.logParameter(alarmSelection, new Parameter("UrgentAlarm", "DCU Urgent Alarm Transmit", urgentAlarm));

                    string overlap = alarms.Overlap.ToString();
                    logger.logParameter(alarmSelection, new Parameter("Overlap", "Message Overlap", overlap));

                    if ( mtu.MagneticTamper )
                        logger.logParameter ( alarmSelection, new Parameter("MagneticTamper", "Magnetic Tamper",
                                              ( alarms.Magnetic ) ? ENABLE : DISABLE ));

                    if ( mtu.RegisterCoverTamper )
                        logger.logParameter ( alarmSelection, new Parameter("RegisterCoverTamper", "Register Cover Tamper",
                                              ( alarms.RegisterCover ) ? ENABLE : DISABLE ));

                    if ( mtu.TiltTamper )
                        logger.logParameter( alarmSelection, new Parameter("TiltTamper", "Tilt Tamper",
                                             ( alarms.Tilt ) ? ENABLE : DISABLE ));

                    if ( mtu.ReverseFlowTamper )
                    {
                        logger.logParameter ( alarmSelection, new Parameter("ReverseFlow", "Reverse Flow Tamper",
                                              ( alarms.ReverseFlow ) ? ENABLE : DISABLE ));
                        logger.logParameter(alarmSelection, new Parameter("FlowDirection", "Flow Direction", meter.Flow.ToString() ));
                    }

                    if ( mtu.InterfaceTamper)
                        logger.logParameter ( alarmSelection, new Parameter("InterfaceTamper", "Interface Tamper",
                                              ( alarms.InterfaceTamper ) ? ENABLE : DISABLE ));

                    this.addMtuAction.Add(alarmSelection);
                }
            }

            #endregion

            // TODO (encoders)
            #region Demands

            if ( mtu.MtuDemand )
            {
                XElement demandConf = new XElement("DemandConfiguration");
                logger.addAtrribute(demandConf, "display", "Demand Configuration");
                logger.logParameter(demandConf, new Parameter("ConfigurationName", "Configuration Name", "Default")); // TODO: replace real value
                logger.logParameter(demandConf, new Parameter("MtuNumLowPriorityMsg", "Mtu Num Low Priority Msg", "2")); // TODO: replace real value
                logger.logParameter(demandConf, new Parameter("MtuPrimaryWindowInterval", "Mtu Primary WindowInterval", "180")); // TODO: replace real value
                logger.logParameter(demandConf, new Parameter("MtuWindowAStart", "Mtu Window A Start", "0")); // TODO: replace real value
                logger.logParameter(demandConf, new Parameter("MtuWindowBStart", "Mtu Window B Start", "0")); // TODO: replace real value
                logger.logParameter(demandConf, new Parameter("MtuPrimaryWindowIntervalB", "Mtu Primary WindowInterval B", "3600")); // TODO: replace real value
                logger.logParameter(demandConf, new Parameter("MtuPrimaryWindowOffset", "Mtu Primary Window Offset", "51")); // TODO: replace real value
                this.addMtuAction.Add(demandConf);
            }

            #endregion

            #region Misc/Optional

            if ( form.ContainsParameter ( FIELD.GPS_LATITUDE  ) &&
                 form.ContainsParameter ( FIELD.GPS_LONGITUDE ) &&
                 form.ContainsParameter ( FIELD.GPS_ALTITUDE  ) )
            {
                //logger.logParameter ( this.addMtuAction, form.GPS_LATITUDE  );
                //logger.logParameter ( this.addMtuAction, form.GPS_LONGITUDE );
                //logger.logParameter ( this.addMtuAction, form.GPS_ALTITUDE  );

                logger.logParameter(this.addMtuAction, new Parameter("GPS_Y", "Lat", form.GPSLat.Value ));
                logger.logParameter(this.addMtuAction, new Parameter("GPS_X", "Long", form.GPSLon.Value ));
                logger.logParameter(this.addMtuAction, new Parameter("Altitude", "Elevation", form.GPSAlt.Value ));
            }

            if ( ! ( form.OptionalParams.Value is string ) )
            {
                List<Parameter> optionalParams = (List<Parameter>)form.OptionalParams.Value;

                if (optionalParams != null)
                    foreach (Parameter p in optionalParams)
                        logger.logParameter(this.addMtuAction, p);
            }

            #endregion
        }

        public void LogReadMtu(ActionResult result)
        {
            logger.addAtrribute(this.readMtuAction, "display", readMtuDisplay);
            logger.addAtrribute(this.readMtuAction, "type", readMtuType);

            /*logger.logParameter(this.readMtuAction, new Parameter("Date", "Date/Time", DateTime.UtcNow.ToString("MM/dd/yyyy HH:mm:ss")));

            if (!string.IsNullOrEmpty(this.user))
            {
                logger.logParameter(this.readMtuAction, new Parameter("User", "User", this.user));
            }*/

            InterfaceParameters[] parameters = Configuration.GetInstance().getLogInterfaceFields( form.mtu.Id, "ReadMTU");
            foreach (InterfaceParameters parameter in parameters)
            {
                if (parameter.Name == "Port")
                {
                    ActionResult[] ports = result.getPorts();
                    for (int i = 0; i < ports.Length; i++)
                    {
                        logger.logPort(i, this.readMtuAction, ports[i], parameter.Parameters.ToArray());
                    }
                }
                else
                {
                    logger.logComplexParameter(this.readMtuAction, result, parameter);
                }
            }
        }

        public void Save ()
        {
            this.addMtuAction.Add ( this.turnOffAction );
            this.addMtuAction.Add ( this.turnOnAction  );
            this.addMtuAction.Add ( this.readMtuAction );

            this.doc = XDocument.Load ( logUri );
            XElement mtus = doc.Root.Element ( "Mtus" );
            mtus.Add ( this.addMtuAction );

            string resultStr = doc.ToString ();

            doc.Save ( logUri );
        }

        public override string ToString ()
        {
            if ( this.doc == null )
                this.Save ();

            return doc.ToString ();         
        }
    }
}
