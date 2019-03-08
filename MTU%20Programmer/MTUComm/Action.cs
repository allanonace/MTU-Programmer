using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Lexi.Interfaces;
using MTUComm.actions;
using Xml;

namespace MTUComm
{
    public class Action
    {
        #region Nested class

        private class ConditionObjet
        {
            public String Condition { get; private set; }
            public String Key       { get; private set; }
            public String Value     { get; private set; }

            public ConditionObjet ( string condition, string key, string value )
            {
                if ( ! condition.Equals ( "&" ) &&
                     ! condition.Equals ( "|" ) )
                     Condition = "|";
                else Condition = condition;

                Key   = key;
                Value = value;
            }
        }

        #endregion

        #region Constants

        private const string NET_IDS   = @"[@_a-zA-Z][_a-zA-Z0-9]+";
        private const string REGEX_IFS = @"([&|]?)((" + NET_IDS + @"." + NET_IDS + @")=(" + NET_IDS + @"))";
        //@"([&|]?)(([^&|=]+)=([^&|=#]*))"

        public enum ActionType
        {
            ReadMtu,
            AddMtu,
            ReplaceMTU,
            AddMtuAddMeter,
            AddMtuReplaceMeter,
            ReplaceMtuReplaceMeter,
            ReplaceMeter,
            TurnOffMtu,
            TurnOnMtu,
            ReadData,
            MtuInstallationConfirmation,
            Diagnosis,
            BasicRead
        }

        private Dictionary<ActionType, String> displays = new Dictionary<ActionType, String>()
        {
            {ActionType.ReadMtu,"Read MTU" },
            {ActionType.AddMtu,"Add MTU" },
            {ActionType.ReplaceMTU,"Replace MTU" },
            {ActionType.AddMtuAddMeter,"Add MTU/Meter" },
            {ActionType.AddMtuReplaceMeter,"Add MTU/Replace Meter" },
            {ActionType.ReplaceMtuReplaceMeter,"Replace MTU/Meter" },
            {ActionType.ReplaceMeter,"Replace Meter" },
            {ActionType.TurnOffMtu,"Turn Off MTU" },
            {ActionType.TurnOnMtu,"Turn On MTU" },
            {ActionType.ReadData,"Read Data Log" },
            {ActionType.MtuInstallationConfirmation,"Install Confirmation" },
            {ActionType.Diagnosis, "" }
        };

        private Dictionary<ActionType, String> tag_types = new Dictionary<ActionType, String>()
        {
            {ActionType.ReadMtu,"ReadMTU" },
            {ActionType.AddMtu,"Program MTU" },
            {ActionType.ReplaceMTU,"Program MTU" },
            {ActionType.AddMtuAddMeter,"Program MTU" },
            {ActionType.AddMtuReplaceMeter,"Program MTU" },
            {ActionType.ReplaceMtuReplaceMeter,"Program MTU" },
            {ActionType.ReplaceMeter,"Program MTU" },
            {ActionType.TurnOffMtu,"TurnOffMtu" },
            {ActionType.TurnOnMtu,"TurnOnMTU" },
            {ActionType.ReadData, "Program MTU" },
            {ActionType.MtuInstallationConfirmation,"InstallConfirmation" },
            {ActionType.Diagnosis, "" }
        };

        private Dictionary<ActionType, String> tag_reasons = new Dictionary<ActionType, String>()
        {
            {ActionType.ReadMtu,null },
            {ActionType.AddMtu,"AddMtu" },
            {ActionType.ReplaceMTU,"ReplaceMtu" },
            {ActionType.AddMtuAddMeter,"AddMtuAddMeter" },
            {ActionType.AddMtuReplaceMeter,"AddMtuReplaceMeter" },
            {ActionType.ReplaceMtuReplaceMeter,"ReplaceMtuReplaceMeter" },
            {ActionType.ReplaceMeter,"ReplaceMeter" },
            {ActionType.TurnOffMtu, null },
            {ActionType.TurnOnMtu, null },
            {ActionType.ReadData, "DataRead" },
            {ActionType.MtuInstallationConfirmation,"InstallConfirmation" },
            {ActionType.Diagnosis, "" }
        };

        #endregion

        #region Events and Delegates

        public delegate void ActionProgresshHandler(object sender, ActionProgressArgs e);
        public event ActionProgresshHandler OnProgress;

        public delegate void ActionFinishHandler(object sender, ActionFinishArgs e);
        public event ActionFinishHandler OnFinish;

        public delegate void ActionErrorHandler ();
        public event ActionErrorHandler OnError;

        #endregion

        #region Args

        public class ActionProgressArgs : EventArgs
        {
            public int Step { get; private set; }
            public int TotalSteps { get; private set; }
            public string Message { get; private set; }

            public ActionProgressArgs(int step, int totalsteps)
            {
                Step = step;
                TotalSteps = totalsteps;
                Message = "";
            }

            public ActionProgressArgs(int step, int totalsteps, string message)
            {
                Step = step;
                TotalSteps = totalsteps;
                Message = message;
            }
        }

        public class ActionFinishArgs : EventArgs
        {
            public ActionResult Result { get; private set; }
            public AddMtuLog FormLog;

            public ActionFinishArgs(ActionResult result )
            {
                Result = result;
            }
        }

        public class ActionErrorArgs : EventArgs
        {
            public ActionErrorArgs () { }

            public int Status { get; private set; }

            public String Message { get; private set; }

            public ActionErrorArgs(int status, String message)
            {
                Status = status;
                Message = message;
            }

            public ActionErrorArgs(String message)
            {
                Status = -1;
                Message = message;
            }
        }

        #endregion

        #region Attributes

        public static Mtu currentMtu;

        public MTUComm comm { get; private set; }
        public ActionType type { get; }
        private List<Parameter> mparameters = new List<Parameter>();
        private Boolean canceled = false;
        public  String user { get; private set; }
        public  Logger logger;
        private Configuration configuration;
        private List<Action> sub_actions = new List<Action>();
        public  int order = 0;
        //public Func<object, object, object> OnFinish;

        #endregion

        #region Properties

        public String DisplayText
        {
            get { return displays[this.type]; }
        }

        public String LogText
        {
            get { return tag_types[this.type]; }
        }

        public String Reason
        {
            get { return tag_reasons[this.type]; }
        }

        #endregion

        #region Initialization

        public Action(Configuration config, ISerial serial, ActionType type, String user = "", String outputfile = "")
        {
            // outputfile = new FileInfo ( outputfile ).Name; // NO
            // System.IO.Path.GetFileName(outputfile)); // NO

            configuration = config;
            logger = new Logger(config, outputfile.Substring(outputfile.LastIndexOf('\\') + 1) ); 
            comm = new MTUComm(serial, config);
            this.type = type;
            this.user = user;
            comm.OnError += Comm_OnError;
        }

        #endregion

        #region Parameters

        public void AddParameter (Parameter parameter)
        {
            mparameters.Add(parameter);
        }

        public void AddParameter ( MtuForm form )
        {
            Parameter[] addMtuParams = form.GetParameters ();
            foreach ( Parameter parameter in addMtuParams )
                mparameters.Add (parameter);
        }

        public Parameter[] GetParameters()
        {
            return mparameters.ToArray();
        }

        public Parameter GetParameterByTag(string tag, int port = -1)
        {
            return mparameters.Find(x => x.getLogTag().Equals(tag) && ( port == -1 || x.Port == port ) );
        }

        #endregion

        #region Actions

        public void AddActions(Action action)
        {
            sub_actions.Add(action);
        }

        public Action[] GetSubActions()
        {
            return sub_actions.ToArray();
        }

        #endregion

        #region Validation

        private bool ValidateCondition ( string condition, dynamic map, Mtu mtu, String port = "", dynamic actionParams = null )
        {
            if ( condition == null )
                return true;

            try
            {
                List<ConditionObjet> conditions = new List<ConditionObjet> ();

                MatchCollection matches = Regex.Matches ( condition, REGEX_IFS, RegexOptions.Compiled );
                foreach ( Match m in matches.Cast<Match> ().ToList () )
                    conditions.Add (
                        new ConditionObjet (
                            Uri.UnescapeDataString ( m.Groups[ 1 ].Value ),     // & or |
                            Uri.UnescapeDataString ( m.Groups[ 3 ].Value ),     // Class.Property
                            Uri.UnescapeDataString ( m.Groups[ 4 ].Value ) ) ); // Value

                int finalResult = 0;

                Global global = Configuration.GetInstance ().GetGlobal ();

                foreach ( ConditionObjet item in conditions )
                {
                    string value  = string.Empty;
                    int    result = 0;

                    string[] member   = item.Key.Split ( new char[]{ '.' } ); // Class.Property
                    string   property = member[ 1 ];

                    switch ( member[ 0 ] )
                    {
                        case "Port":
                            // P1 or P2
                            value = port;
                            break;
                        case "Action":
                            // User, Date or Type
                            value = GetProperty ( property );
                            break;
                        case "MeterType":
                            break;
                        case "MtuType":
                            // Reflection over Mtu class
                            value = mtu.GetProperty ( property );
                            break;
                        case "ActionParams":
                            value = actionParams.GetType().GetProperty ( member[ 0 ] )
                                .GetValue ( actionParams, null ).ToString();
                            break;
                        case "Global":
                            value = global.GetType ().GetProperty ( member[ 0 ] )
                                .GetValue ( global, null ).ToString();
                            break;
                        default: // MemoryMap
                            // Recover register from MTU memory map
                            // Some registers have port sufix but other not
                            if ( ! string.IsNullOrEmpty ( port ) &&
                                 map.ContainsMember ( port + property ) )
                                value = map.GetProperty ( port + property ).Value.ToString ();
                            // Try to recover register without port prefix
                            else if ( map.ContainsMember ( property ) )
                                value = map.GetProperty ( property ).Value.ToString ();
                            break;
                    }

                    // Compare property value with condition value
                    if ( ! string.IsNullOrEmpty ( value ) &&
                         value.ToLower ().Equals ( item.Value.ToLower () ))
                        result = 1;

                    // Concatenate conditions results
                    // If one validate, pass
                    if ( item.Condition.Equals ( "|" ) )
                        finalResult = finalResult + result;
                    // All conditions have to validate
                    else if ( item.Condition.Equals ( "&" ) )
                        finalResult = finalResult * result;
                }

                return ( finalResult > 0 );
            }
            catch ( Exception e )
            {
                Console.WriteLine ( e.Message + "\r\n" + e.StackTrace );
            }

            return true;
        }

        #endregion

        #region Gets

        public String GetProperty ( string name )
        {
            switch (name)
            {
                case "User": return user;
                case "Date": return DateTime.UtcNow.ToString("MM/dd/yyyy HH:mm:ss");
                case "Type": return type.ToString();
                default    : return "";
            }
        }

        public String GetResultXML ( ActionResult result )
        {
            if (type == ActionType.AddMtu)
                return comm.GetResultXML();
            else
                return logger.logReadResultString(this, result);
        }

        #endregion

        #region Execution

        public void Run(MtuForm mtuForm = null)
        {
            if (canceled)
            {
                throw new Exception("Canceled Action can not be Executed");
            }
            else
            {
                List<object> parameters = new List<object>();

                switch (type)
                {
                    case ActionType.ReadMtu:
                        comm.OnReadMtu += Comm_OnReadMtu;
                        break;

                    case ActionType.AddMtu:
                    case ActionType.AddMtuAddMeter:
                    case ActionType.AddMtuReplaceMeter:
                    case ActionType.ReplaceMTU:
                    case ActionType.ReplaceMeter:
                    case ActionType.ReplaceMtuReplaceMeter:
                        comm.OnAddMtu   += Comm_OnAddMtu;
                        comm.OnProgress += Comm_OnProgress;
                        // Interactive and Scripting
                        if (mtuForm != null)
                             parameters.AddRange(new object[] { (AddMtuForm)mtuForm, this.user, type });
                        else parameters.Add(this);
                        break;

                    case ActionType.TurnOffMtu:
                        comm.OnTurnOffMtu += Comm_OnTurnOffMtu;
                        break;

                    case ActionType.TurnOnMtu:
                        comm.OnTurnOnMtu += Comm_OnTurnOnMtu;
                        break;

                    case ActionType.MtuInstallationConfirmation:
                        comm.OnReadMtu += Comm_OnReadMtu;
                        comm.OnProgress += Comm_OnProgress;
                        break;

                    case ActionType.ReadData:
                        Parameter param = mparameters.Find(x => (x.Type == Parameter.ParameterType.DaysOfRead));
                        if (param == null)
                        {
                            this.OnError (); //this, new ActionErrorArgs("Days Of Read parameter Not Defined or Invalid"));
                            break;
                        }
                        int DaysOfRead = 0;
                        if (!Int32.TryParse(param.Value, out DaysOfRead) || DaysOfRead <= 0)
                        {
                            this.OnError (); //this, new ActionErrorArgs("Days Of Read parameter Invalid"));
                            break;
                        }
                        comm.OnReadMtuData += Comm_OnReadMtuData;
                        parameters.Add(DaysOfRead);
                        break;

                    case ActionType.BasicRead:
                        comm.OnBasicRead += Comm_OnBasicRead;
                        break;
                }

                // Is more easy to control one point of invokation
                // than N, one for each action/new task to launch
                comm.LaunchActionThread(type, parameters.ToArray());
            }
        }

        public void Cancel(string cancelReason = "410 DR Defective Register")
        {
            canceled = true;
            logger.logCancel ( this, "User Cancelled", cancelReason );
        }

        #endregion

        #region OnEvents

        private void Comm_OnProgress(object sender, MTUComm.ProgressArgs e)
        {
            try
            {
                OnProgress(this, new ActionProgressArgs(e.Step, e.TotalSteps, e.Message));
            }
            catch (Exception pe)
            {

            }
        }

        private void Comm_OnError ()
        {
            this.OnError ();
        }

        private void Comm_OnReadMtuData(object sender, MTUComm.ReadMtuDataArgs e)
        {
            ActionProgressArgs args;
            switch (e.Status)
            {
                case LogQueryResult.LogDataType.Bussy:
                    args = new ActionProgressArgs(0, 0);
                    OnProgress(this, args);
                    break;
                case LogQueryResult.LogDataType.NewPacket:
                    args = new ActionProgressArgs(e.CurrentEntry, e.TotalEntries);
                    OnProgress(this, args);
                    break;
                case LogQueryResult.LogDataType.LastPacket:
                    Mtu mtu_type = configuration.GetMtuTypeById((int)e.MtuType.Type);
                    ActionResult result = ReadMTUData(e.Start, e.End, e.Entries, e.MtuType, mtu_type);
                    logger.logReadDataResult(this, result, mtu_type);
                    ActionFinishArgs f_args = new ActionFinishArgs(null);
                    OnFinish(this, f_args);
                    break;
            }
        }

        private void Comm_OnReadMtu(object sender, MTUComm.ReadMtuArgs e)
        {
            currentMtu = e.MtuType;
        
            ActionResult result = CreateActionResultUsingInterface ( e.MemoryMap, e.MtuType );
            logger.logReadResult ( this, result, e.MtuType );
            ActionFinishArgs args = new ActionFinishArgs ( result );

            OnFinish ( this, args );
        }

        private void Comm_OnTurnOffMtu ( object sender, MTUComm.TurnOffMtuArgs e )
        {
            ActionResult result = getBasciInfoResult();
            logger.logTurnOffResult ( this, e.Mtu );
            ActionFinishArgs args = new ActionFinishArgs ( result );

            OnFinish ( this, args );
        }

        private void Comm_OnTurnOnMtu(object sender, MTUComm.TurnOnMtuArgs e)
        {
            ActionResult result = getBasciInfoResult();
            logger.logTurnOffResult ( this, e.Mtu );
            ActionFinishArgs args = new ActionFinishArgs ( result );

            OnFinish ( this, args );
        }

        private ActionResult Comm_OnAddMtu(object sender, MTUComm.AddMtuArgs e)
        {
            ActionResult result = CreateActionResultUsingInterface(e.MemoryMap, e.MtuType, e.Form );
            ActionFinishArgs args = new ActionFinishArgs(result);

            e.AddMtuLog.LogReadMtu(result);

            // Generate xml log file and save on device
            e.AddMtuLog.Save ();
            
            args.FormLog = e.AddMtuLog;

            OnFinish(this, args);
            return result;
        }

        private void Comm_OnBasicRead(object sender, MTUComm.BasicReadArgs e)
        {
            ActionResult result = new ActionResult();
            ActionFinishArgs args = new ActionFinishArgs(result);
            OnFinish(this, args);
        }

        #endregion

        #region Reads

        private ActionResult ReadMTUData ( DateTime start, DateTime end, List<LogDataEntry> Entries, MTUBasicInfo mtuInfo, Mtu mtu )
        {
            ActionResult result = new ActionResult();

            string log_path = logger.logReadDataResultEntries(mtuInfo.Id.ToString("d15"), start, end, Entries);

            InterfaceParameters[] parameters = configuration.getAllInterfaceFields(mtu.Id, "DataRead");
            foreach (InterfaceParameters parameter in parameters)
            {
                if (parameter.Name.Equals("Port"))
                {
                    for (int i = 0; i < mtu.Ports.Count; i++)
                    {
                        foreach (InterfaceParameters port_parameter in parameter.Parameters)
                        {
                            if (port_parameter.Source != null && port_parameter.Source.StartsWith("ActionParams"))
                            {
                                Parameter sel_parameter = GetParameterByTag(port_parameter.Name, i + 1);
                                if (sel_parameter != null)
                                {
                                    result.AddParameter(new Parameter(port_parameter.Name, port_parameter.Display, sel_parameter.Value));
                                }
                            }

                        }


                    }
                }
                else
                {
                    try
                    {

                        if (parameter.Source != null && parameter.Source.StartsWith("ActionParams"))
                        {
                            Parameter sel_parameter = GetParameterByTag(parameter.Name);
                            if (sel_parameter != null)
                            {
                                result.AddParameter(new Parameter(parameter.Name, parameter.Display, sel_parameter.Value));
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message + "\r\n" + e.StackTrace);
                    }
                }

            }

            result.AddParameter(new Parameter("ReadRequest", "Number Read Request Days", ""));
            result.AddParameter(new Parameter("ReadResult", "Read Result", "Number of Reads " + Entries.Count.ToString() + " for Selected Period From " + start.ToString("dd/MM/yyyy") + " 0:00:00 Till " + end.ToString("dd/MM/yyyy") + " 23:59:59"));
            result.AddParameter(new Parameter("ReadResultFile", "Read Result File", log_path));

            return result;
        }

        private ActionResult CreateActionResultUsingInterface (
            dynamic map = null,
            Mtu mtutype = null,
            MtuForm form = null,
            dynamic actionParams = null,
            string actionType = "ReadMTU" )
        {
            InterfaceParameters[] parameters = configuration.getAllInterfaceFields ( mtutype.Id, actionType );

            ActionResult result = new ActionResult ();
            foreach ( InterfaceParameters parameter in parameters )
            {
                if ( parameter.Name.Equals ( "MtuVoltageBattery" ) )
                {

                }

                if ( parameter.Name.Equals ( "Port" ) )
                    for ( int i = 0; i < mtutype.Ports.Count; i++ )
                        result.addPort ( ReadPort ( i, parameter.Parameters.ToArray (), map, mtutype ) );
                else
                {
                    try
                    {
                        if ( ValidateCondition ( parameter.Conditional, map, mtutype, "", actionParams ) )
                        {
                            //if ( parameter.Source == null )
                            //    parameter.Source = "";

                            string sourceWhere    = string.Empty;
                            string sourceProperty = string.Empty;

                            if ( ! string.IsNullOrEmpty ( parameter.Source ) )
                            {
                                string[] sources = parameter.Source.Split(new char[] { '.' });
                                sourceWhere      = sources[ 0 ];
                                sourceProperty   = sources[ 1 ];
                            }

                            Parameter paramToAdd = null;
                            switch ( sourceWhere )
                            {
                                case "Action":
                                    string action_property_value = GetProperty ( sourceProperty );
                                    if ( action_property_value != null )
                                        paramToAdd = new Parameter ( parameter.Name, parameter.Display, action_property_value );
                                    break;
                                case "MeterType":
                                    break;
                                case "MtuType":
                                    paramToAdd = new Parameter ( parameter.Name, parameter.Display, mtutype.GetProperty( sourceProperty ));
                                    break;
                                case "MemoryMap":
                                    paramToAdd = new Parameter ( sourceProperty, parameter.Display, map.GetProperty( sourceProperty ).Value.ToString());
                                    break;
                                case "Form":
                                    if ( form.ContainsParameter ( sourceProperty ) )
                                        paramToAdd = form.GetParameter ( sourceProperty );
                                    break;
                                case "ActionParams":
                                    paramToAdd = new Parameter ( parameter.Name, parameter.Display,
                                                                 actionParams.GetType().GetProperty ( sourceProperty )
                                                                    .GetValue ( actionParams, null ).ToString() );
                                    break;
                                default:
                                    try
                                    {
                                        paramToAdd = new Parameter(parameter.Name, parameter.Display, map.GetProperty(parameter.Name).Value.ToString());
                                    }
                                    catch (Exception e)
                                    {
                                        paramToAdd = null;
                                    }
                                   
                                    break;
                            }

                            if ( paramToAdd != null )
                                result.AddParameter ( paramToAdd );
                            else
                            {

                            }
                        }

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message + "\r\n" + e.StackTrace);
                    }
                }

            }
            return result;
        }

        private ActionResult ReadPort ( int indexPort, InterfaceParameters[] parameters, dynamic map, Mtu mtu )
        {
            ActionResult result   = new ActionResult ();
            Port         portType = mtu.Ports[ indexPort ];

            // Meter Serial Number
            int meterId = map.GetProperty ( "P" + ( indexPort + 1 ) + "MeterType" ).Value;

            // Port has installed a Meter
            if ( meterId != 0 )
            {
                Meter Metertype = configuration.getMeterTypeById ( meterId );
                
                // Meter type not found in database
                if ( Metertype.Type == "NOTFOUND" )
                {
                    //logger.LogError("No valid meter types were found for MTU type " + Metertype.Id);
                }

                // Iterate all parameters for this port
                foreach ( InterfaceParameters parameter in parameters )
                {
                    if ( parameter.Name.Equals ( "MeterReading" ) )
                    {
                        if (ValidateCondition(parameter.Conditional, map, mtu, "P" + (indexPort + 1)))
                        {
                            string meter_reading_error = map.GetProperty("P" + (indexPort + 1) + "ReadingError").Value.ToString();
                            if (meter_reading_error.Length < 1)
                            {
                                ulong meter_reading = 0;
                                try
                                {
                                    meter_reading = map.GetProperty("P" + (indexPort + 1) + "Reading").Value;
                                }
                                catch (Exception e) { }

                                ulong tempReadingVal = 0;
                                if (mtu.PulseCountOnly)
                                {
                                    tempReadingVal = meter_reading * (ulong)Metertype.HiResScaling;
                                }
                                else
                                {
                                    tempReadingVal = meter_reading;
                                }


                                String tempReading = tempReadingVal.ToString();
                                if (Metertype.LiveDigits < tempReading.Length)
                                {
                                    tempReading = tempReading.Substring(tempReading.Length - Metertype.LiveDigits - (tempReading.IndexOf('.') > -1 ? 1 : 0));
                                }
                                else
                                {
                                    tempReading = tempReading.PadLeft(Metertype.LiveDigits, '0');
                                }
                                if (Metertype.LeadingDummy > 0) // KG 12/08/2008
                                    tempReading = tempReading.PadLeft(tempReading.Length + Metertype.LeadingDummy, configuration.useDummyDigits() ? 'X' : '0');
                                if (Metertype.DummyDigits > 0)  // KG 12/08/2008
                                    tempReading = tempReading.PadRight(tempReading.Length + Metertype.DummyDigits, configuration.useDummyDigits() ? 'X' : '0');
                                if (Metertype.Scale > 0 && tempReading.IndexOf(".") == -1) // 8.12.2011 KG add for F1 Pulse
                                    tempReading = tempReading.Insert(tempReading.Length - Metertype.Scale, ".");
                                if (Metertype.PaintedDigits > 0 && configuration.useDummyDigits()) // KG 12/08/2008
                                    tempReading = tempReading.PadRight(tempReading.Length + Metertype.PaintedDigits, '0').Insert(tempReading.Length, " - ");

                                if (tempReading == "")
                                {
                                    tempReading = "INVALID";
                                }
                                result.AddParameter(new Parameter(parameter.Name, parameter.Display, tempReading));
                            }
                            else
                            {
                                result.AddParameter(new Parameter(parameter.Name, parameter.Display, meter_reading_error));
                            }
                        }

                    }
                    else
                    {
                        try
                        {
                            if (ValidateCondition(parameter.Conditional, map, mtu, "P" + (indexPort + 1)))
                            {
                                if (parameter.Source == null)
                                {
                                    parameter.Source = "";
                                }
                                string val = null;
                                string property_type = "";
                                string property_name = "";

                                if (parameter.Source.Contains(".") && parameter.Source.Length >= 3)
                                {
                                    property_type = parameter.Source.Split(new char[] { '.' })[0];
                                    property_name = parameter.Source.Split(new char[] { '.' })[1];
                                }

                                string name = parameter.Name;

                                switch (property_type)
                                {
                                    case "PortType":
                                        val = portType.GetProperty(property_name);
                                        break;
                                    case "MeterType":
                                        val = Metertype.GetProperty(property_name);
                                        break;
                                    case "MtuType":
                                        val = mtu.GetProperty(property_name);
                                        break;
                                    case "MemoryMap":
                                        val = map.GetProperty("P" + (indexPort + 1) + property_name).Value.ToString();
                                        name = property_name;
                                        break;
                                    default:

                                        try
                                        {
                                            val = map.GetProperty("P" + (indexPort + 1) + parameter.Name).Value.ToString();
                                        }catch (Exception e)
                                        {
                                            val = null;
                                        }

                                        break;

                                }
                                if (!string.IsNullOrEmpty(val))
                                {
                                    result.AddParameter(new Parameter(name, parameter.Display, val));
                                }

                            }

                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message + "\r\n" + e.StackTrace);
                        }
                    }

                }

            }
            // Port has not installed a Meter
            else
            {
                result.AddParameter(new Parameter("Status", "Status", "Not Installed"));
                result.AddParameter(new Parameter("MeterTypeId", "Meter Type ID", "000000000"));
                result.AddParameter(new Parameter("MeterReading", "Meter Reading", "Bad Reading"));
                /*
                result.AddParameter(new Parameter("MeterType", "Meter Type", "Not Installed"));
                result.AddParameter(new Parameter("MeterTypeId", "Meter Type ID", meterid.ToString()));
                result.AddParameter(new Parameter("AcctNumber", "Service Pt. ID", "000000000"));
                result.AddParameter(new Parameter("MeterReading", "Meter Reading", "Bad Reading"));
                */
            }

            return result;
        }

        private ActionResult getBasciInfoResult ()
        {
            ActionResult result = new ActionResult ();
            MTUBasicInfo basic  = comm.GetBasicInfo ();
            
            result.AddParameter(new Parameter("Date", "Date/Time", GetProperty("Date")));
            result.AddParameter(new Parameter("User", "User", GetProperty("User")));
            result.AddParameter(new Parameter("MtuId", "MTU ID", basic.Id));
    
            return result;
        }

        #endregion
    }
}
