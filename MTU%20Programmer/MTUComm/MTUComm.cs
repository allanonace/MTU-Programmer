using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Lexi.Interfaces;
using MTUComm.actions;
using MTUComm.Exceptions;
using MTUComm.MemoryMap;
using Xml;

using ActionType  = MTUComm.Action.ActionType;
using FIELD       = MTUComm.actions.AddMtuForm.FIELD;
using LogDataType = MTUComm.LogQueryResult.LogDataType;
using ParameterType = MTUComm.Parameter.ParameterType;

namespace MTUComm
{
    public class MTUComm
    {
        #region Constants

        private const int BASIC_READ_1_ADDRESS = 0;
        private const int BASIC_READ_1_DATA    = 32;
        private const int BASIC_READ_2_ADDRESS = 244;
        private const int BASIC_READ_2_DATA    = 1;
        private const int DEFAULT_OVERLAP      = 6;
        private const int DEFAULT_LENGTH_AES   = 16;
        
        private const int WAIT_BTW_TURNOFF     = 500;
        private const int WAIT_BTW_IC          = 1000;

        private const string ERROR_LOADDEMANDCONF = "DemandConfLoadException";
        private const string ERROR_LOADMETER = "MeterLoadException";
        private const string ERROR_LOADMTU = "MtuLoadException";
        private const string ERROR_LOADALARM = "AlarmLoadException";
        private const string ERROR_NOTFOUNDMTU = "MtuNotFoundException";
        private const string ERROR_LOADINTERFACE = "InterfaceLoadException";
        private const string ERROR_LOADGLOBAL = "GlobalLoadException";
        private const string ERROR_NOTFOUNDMETER = "MeterNotFoundException";

        #endregion

        #region Delegates and Events

        public delegate void ReadMtuHandler(object sender, ReadMtuArgs e);
        public event ReadMtuHandler OnReadMtu;

        public delegate void TurnOffMtuHandler(object sender, TurnOffMtuArgs e);
        public event TurnOffMtuHandler OnTurnOffMtu;

        public delegate void TurnOnMtuHandler(object sender, TurnOnMtuArgs e);
        public event TurnOnMtuHandler OnTurnOnMtu;

        public delegate void ReadMtuDataHandler(object sender, ReadMtuDataArgs e);
        public event ReadMtuDataHandler OnReadMtuData;

        public delegate ActionResult AddMtuHandler(object sender, AddMtuArgs e);
        public event AddMtuHandler OnAddMtu;

        public delegate void BasicReadHandler(object sender, BasicReadArgs e);
        public event BasicReadHandler OnBasicRead;

        public delegate void ErrorHandler ();
        public event ErrorHandler OnError;

        public delegate void ProgresshHandler(object sender, ProgressArgs e);
        public event ProgresshHandler OnProgress;

        #endregion

        #region Class Args

        /*
        public class ErrorArgs : EventArgs
        {
            public Exception exception { private set; get; }

            public ErrorArgs () { }

            public ErrorArgs ( Exception e )
            {
                this.exception = e;
            }
        }
        */

        public class ReadMtuArgs : EventArgs
        {
            public AMemoryMap MemoryMap { get; private set; }

            public Mtu MtuType { get; private set; }

            public ReadMtuArgs(AMemoryMap memorymap, Mtu mtype)
            {
                MemoryMap = memorymap;
                MtuType = mtype;
            }
        }

        public class ReadMtuDataArgs : EventArgs
        {
            public LogDataType Status { get; private set; }

            public int TotalEntries { get; private set; }
            public int CurrentEntry { get; private set; }

            public DateTime Start { get; private set; }
            public DateTime End { get; private set; }

            public MTUBasicInfo MtuType { get; private set; }


            public List<LogDataEntry> Entries { get; private set; }

            public ReadMtuDataArgs(LogDataType status, DateTime start, DateTime end, MTUBasicInfo mtype)
            {
                Status = status;
                TotalEntries = 0;
                CurrentEntry = 0;
                MtuType = mtype;
            }

            public ReadMtuDataArgs(LogDataType status, DateTime start, DateTime end, MTUBasicInfo mtype, List<LogDataEntry> entries)
            {
                Status = status;
                TotalEntries = entries.Count;
                CurrentEntry = entries.Count;
                Entries = entries;
                MtuType = mtype;
            }

            public ReadMtuDataArgs(LogDataType status, DateTime start, DateTime end, MTUBasicInfo mtype, int totalEntries, int currentEntry)
            {
                Status = status;
                TotalEntries = totalEntries;
                CurrentEntry = currentEntry;
                MtuType = mtype;
                Start = start;
                End = end;
            }

        }

        public class ProgressArgs : EventArgs
        {
            public int Step { get; private set; }
            public int TotalSteps { get; private set; }
            public string Message { get; private set; }

            public ProgressArgs(int step, int totalsteps)
            {
                Step = step;
                TotalSteps = totalsteps;
                Message = "";
            }

            public ProgressArgs(int step, int totalsteps, string message)
            {
                Step = step;
                TotalSteps = totalsteps;
                Message = message;
            }
        }

        public class TurnOffMtuArgs : EventArgs
        {
            public Mtu Mtu { get; }
            public TurnOffMtuArgs ( Mtu Mtu )
            {
                this.Mtu = Mtu;
            }
        }

        public class TurnOnMtuArgs : EventArgs
        {
            public Mtu Mtu { get; }
            public TurnOnMtuArgs ( Mtu Mtu )
            {
                this.Mtu = Mtu;
            }
        }

        public class AddMtuArgs : EventArgs
        {
            public AMemoryMap MemoryMap { get; private set; }
            public Mtu MtuType { get; private set; }
            public MtuForm Form { get; private set; }
            public AddMtuLog AddMtuLog { get; private set; }

            public AddMtuArgs(AMemoryMap memorymap, Mtu mtype, MtuForm form, AddMtuLog addMtuLog )
            {
                MemoryMap = memorymap;
                MtuType = mtype;
                Form = form;
                AddMtuLog = addMtuLog;
            }
        }

        public class BasicReadArgs : EventArgs
        {
            public BasicReadArgs()
            {
            }
        }

        #endregion

        #region Attributes

        private Lexi.Lexi lexi;
        private Configuration configuration;
        private MTUBasicInfo latest_mtu;
        private Mtu mtu;
        private Boolean isPulse = false;
        private Boolean mtuHasChanged;

        #endregion

        #region Properties

        private AddMtuLog addMtuLog;
        
        #endregion

        #region Initialization

        public MTUComm(ISerial serial, Configuration configuration)
        {
            this.configuration = configuration;
            latest_mtu = new MTUBasicInfo(new byte[BASIC_READ_1_DATA + BASIC_READ_2_DATA]);
            lexi = new Lexi.Lexi(serial, 10000);
        }

        #endregion

        #region Launch Actions

        public async void LaunchActionThread ( ActionType type, params object[] args )
        {
            try
            {
                //Gets MTU casci info ( type and id )
                // TODO: Descubrir porque al hacer un segundo basic read en la accion de AddMtu,
                // cuando se pulsa el boton, habiendo sido el primero el que se hace nada mas cargar
                // el formulario, la lectura casca. Ahora mismo esta condicion es para evitar que en
                // la accion AddMtu se haga una segunda lectura basica en modo interactivo pero si que
                // se permite hacer la primera en modo scripting
                if ( type != ActionType.AddMtu ||
                     type == ActionType.AddMtu && args.Length == 1 )
                    this.LoadMtuAndMetersBasicInfo ();

                switch (type)
                {
                    case ActionType.AddMtu:
                    case ActionType.AddMtuAddMeter:
                    case ActionType.AddMtuReplaceMeter:
                    case ActionType.ReplaceMTU:
                    case ActionType.ReplaceMeter:
                    case ActionType.ReplaceMtuReplaceMeter:
                        // Interactive and Scripting
                        if ( args.Length > 1 )
                             await Task.Run(() => Task_AddMtu ( (AddMtuForm)args[0], (string)args[1], (ActionType)args[2] ) );
                        else await Task.Run(() => Task_AddMtu ( (Action)args[0] ) );
                        break;
                    case ActionType.ReadMtu    : await Task.Run(() => Task_ReadMtu()); break;
                    case ActionType.TurnOffMtu : await Task.Run(() => Task_TurnOnOffMtu ( false ) ); break;
                    case ActionType.TurnOnMtu  : await Task.Run(() => Task_TurnOnOffMtu ( true  ) ); break;
                    case ActionType.ReadData   : await Task.Run(() => Task_ReadDataMtu((int)args[0])); break;
                    case ActionType.BasicRead  : await Task.Run(() => Task_BasicRead()); break;
                    case ActionType.MtuInstallationConfirmation: await Task.Run(() => Task_InstallConfirmation()); break;
                    default: break;
                }
            }
            // MTUComm.Exceptions.MtuTypeIsNotFoundException
            catch ( Exception e )
            {
                Errors.LogRemainExceptions ( e );
            
                this.OnError ();
            }
        }

        #endregion

        #region Actions

        public void Task_ReadDataMtu ( int NumOfDays )
        {
            DateTime start = DateTime.UtcNow.Date.Subtract(new TimeSpan(NumOfDays, 0, 0, 0));
            DateTime end = DateTime.UtcNow.Date.AddSeconds(86399);

            lexi.TriggerReadEventLogs(start, end);

            List<LogDataEntry> entries = new List<LogDataEntry>();

            bool last_packet = false;
            while (!last_packet)
            {
                LogQueryResult response = new LogQueryResult(lexi.GetNextLogQueryResult());
                switch (response.Status)
                {
                    case LogDataType.LastPacket:
                        last_packet = true;
                        OnReadMtuData(this, new ReadMtuDataArgs(response.Status, start, end, latest_mtu, entries));
                        break;
                    case LogDataType.Bussy:
                        OnReadMtuData(this, new ReadMtuDataArgs(response.Status, start, end, latest_mtu));
                        Thread.Sleep(100);
                        break;
                    case LogDataType.NewPacket:
                        entries.Add(response.Entry);
                        OnReadMtuData(this, new ReadMtuDataArgs(response.Status, start, end, latest_mtu, response.TotalEntries, response.CurrentEntry));
                        break;

                }
            }
        }

        #region Install Confirmation

        public void Task_InstallConfirmation ()
        {
            this.InstallConfirmation_Logic ();
            this.Task_ReadMtu ();
        }

        private bool InstallConfirmation_Logic ( bool force = false, int time = 0 )
        {
            Global global = this.configuration.global;
        
            // DEBUG
            //this.WriteMtuBit ( 22, 0, false ); // Turn On MTU
        
            // MTU is turned off
            if ( ! force &&
                 this.latest_mtu.Shipbit )
            {
                Errors.LogErrorNowAndContinue ( new MtuIsAlreadyTurnedOffICException () );
                return false;
            }
            
            // MTU does not support two-way or client does not want to perform it
            if ( ! global.TimeToSync ||
                 ! this.mtu.TimeToSync )
            {
                Errors.LogErrorNowAndContinue ( new MtuIsNotTwowayICException () );
                return false;
            }
            
            else
            {
                try
                {
                    MemRegister regICNotSynced = configuration.getFamilyRegister ( this.mtu, "InstallConfirmationNotSynced" );
                    MemRegister regICRequest   = configuration.getFamilyRegister ( this.mtu, "InstallConfirmationRequest"   );

                    Console.WriteLine ( "InstallConfirmation trigger start" );

                    // Reset to fail state the Install Confirmation result
                    // Set bit to true/one, because loop detection will continue while it doesn't change to false/zero
                    uint addressNotSynced = ( uint )regICNotSynced.Address;
                    uint bitSynced        = ( uint )regICNotSynced.Size;
                    this.WriteMtuBit ( addressNotSynced, bitSynced, true );

                    // Set to true/one this flag to request a time sync
                    this.WriteMtuBit ( ( uint )regICRequest.Address, ( uint )regICRequest.Size, true );

                    bool fail;
                    int  count = 1;
                    int  wait  = 5;
                    int  max   = ( int )( global.TimeSyncCountDefault / wait ); // Seconds / Seconds = Rounded max number of iterations
                    
                    do
                    {
                        // Update interface text to look the progress
                        int progress = ( int )Math.Round ( ( decimal )( ( count * 100.0 ) / max ) );
                        OnProgress ( this, new ProgressArgs ( count, max, "Checking IC... "+ progress.ToString() + "%" ) );
                        
                        Thread.Sleep ( wait * 1000 );
                        
                        fail = this.ReadMtuBit ( addressNotSynced, bitSynced );
                    }
                    // is MTU not synced with DCU yet?
                    while ( fail &&
                            ++count <= max );
                    
                    if ( fail )
                        throw new AttemptNotAchievedICException ();
                }
                catch ( Exception e )
                {
                    if ( Errors.IsOwnException ( e ) )
                        Errors.AddError ( e );
                    // Finish
                    else
                    {
                        Errors.LogErrorNowAndContinue ( new PuckCantCommWithMtuException () );
                        return false;
                    }
                
                    // Retry action ( thre times = first plus two replies )
                    if ( time < global.TimeSyncCountRepeat )
                    {
                        Thread.Sleep ( WAIT_BTW_IC );
                        
                        return this.InstallConfirmation_Logic ( force, ++time );
                    }
                    
                    // Finish with error
                    Errors.LogErrorNowAndContinue ( new ActionNotAchievedICException ( ( global.TimeSyncCountRepeat + 1 ) + "" ) );
                    return false;
                }
            }
            return true;
        }

        #endregion

        #region Turn On|Off

        public void Task_TurnOnOffMtu (
            bool on )
        {
            if ( on )
                 Console.WriteLine ( "TurnOffMtu start" );
            else Console.WriteLine ( "TurnOnMtu start"  );

            // Launchs exception 'ActionNotAchievedTurnOffException'
            if ( this.TurnOnOffMtu_Logic ( on ) )
            {
                if ( on )
                     this.OnTurnOnMtu  ( this, new TurnOnMtuArgs  ( this.mtu ) );
                else this.OnTurnOffMtu ( this, new TurnOffMtuArgs ( this.mtu ) );
            }
        }

        private bool TurnOnOffMtu_Logic (
            bool on,
            int time = 0 )
        {
            try
            {
                MemRegister regShipbit = configuration.getFamilyRegister ( this.mtu, "Shipbit" );
                uint address = ( uint )regShipbit.Address;
                uint bit     = ( uint )regShipbit.Size;
            
                this.WriteMtuBit ( address, bit, ! on );         // Set state of the shipbit
                bool shipbit = this.ReadMtuBit ( address, bit ); // Read written value to verify
                
                // Fail turning off MTU
                if ( shipbit == on )
                    throw new AttemptNotAchievedTurnOffException ();
            }
            // System.IO.IOException = Puck is not well placed or is off
            catch ( Exception e )
            {
                if ( Errors.IsOwnException ( e ) )
                    Errors.AddError ( e );
                // Finish
                else
                {
                    Errors.LogErrorNow ( new PuckCantCommWithMtuException () );
                    return false;
                }
                
                // Retry action ( thre times = first plus two replies )
                if ( time < 2 )
                {
                    Thread.Sleep ( WAIT_BTW_TURNOFF );
                    
                    return this.TurnOnOffMtu_Logic ( on, ++time );
                }
                // Finish with error
                else Errors.LogErrorNow ( new ActionNotAchievedTurnOffException ( "3" ) );
                
                return false;
            }
            return true;
        }
        
        #endregion

        #region Read MTU

        public void Task_ReadMtu ()
        {
            String memory_map_type = configuration.GetMemoryMapTypeByMtuId ( this.mtu );
            int memory_map_size    = configuration.GetmemoryMapSizeByMtuId ( this.mtu );

            byte[] buffer = new byte[1024];

            try
            {
                lexi.Write(64, new byte[] { 1 });
                Thread.Sleep(1000);
    
                System.Buffer.BlockCopy(lexi.Read(0, 255), 0, buffer, 0, 255);
    
                // Check if the MTU is still the same
                if ( this.LoadMtuBasicInfo () )
                    Errors.LogErrorNow ( new MtuHasChangeBeforeFinishActionException () );
    
                if (memory_map_size > 255)
                {
                    System.Buffer.BlockCopy(lexi.Read(256, 64), 0, buffer, 256, 64);
                    System.Buffer.BlockCopy(lexi.Read(318, 2), 0, buffer, 318, 2);
                }
    
                if (memory_map_size > 320)
                {
                    //System.Buffer.BlockCopy(lexi.Read(320, 64), 0, buffer, 320, 64);
                    //System.Buffer.BlockCopy(lexi.Read(384, 64), 0, buffer, 384, 64);
                    //System.Buffer.BlockCopy(lexi.Read(448, 64), 0, buffer, 448, 64);
                    //System.Buffer.BlockCopy(lexi.Read(512, 64), 0, buffer, 512, 64);
                }
    
                if (memory_map_size > 960)
                    System.Buffer.BlockCopy(lexi.Read(960, 64), 0, buffer, 960, 64);
            }
            // MtuHasChangeBeforeFinishActionException
            // System.IO.IOException = Puck is not well placed or is off
            catch ( Exception e )
            {
                // Is not own exception
                if ( ! Errors.IsOwnException ( e ) )
                    Errors.LogErrorNow ( new PuckCantCommWithMtuException () );
                
                // Allow to continue rising the error
                else throw ( e );
            }

            // Generates the memory map with recovered data
            dynamic map = new MemoryMap.MemoryMap ( buffer, memory_map_type );

            // Finish!
            OnReadMtu ( this, new ReadMtuArgs ( map, this.mtu ) );
        }

        #endregion

        #region Write MTU

        private Action truquitoAction;

        private void Task_AddMtu ( Action addMtuAction )
        {
            truquitoAction     = addMtuAction;
            Parameter[] ps     = addMtuAction.GetParameters ();
            dynamic     form   = new AddMtuForm ( this.mtu );
            Global      global = form.global;
            form.usePort2      = false;
            bool scriptUseP2   = false;
            
            List<Meter> meters;
            List<string> portTypes;
            Meter meterPort1 = null;
            Meter meterPort2 = null;
            
            MemRegister regP2Status = configuration.getFamilyRegister ( this.mtu, "P2StatusFlag" );
            bool port2IsActivated = ReadMtuBit ( ( uint )regP2Status.Address, ( uint )regP2Status.Size );

            // Recover parameters from script and translante from Aclara nomenclature to our own
            foreach ( Parameter parameter in ps )
            {
                // Launchs exception 'TranslatingParamsScriptException'
                form.AddParameterTranslatingAclaraXml ( parameter );
                
                if ( parameter.Port == 2 )
                    form.usePort2 = true;
            }

            scriptUseP2    = form.usePort2;
            form.usePort2 &= this.mtu.TwoPorts;

            #region Auto-detect Meters

            // Script is for one port but MTU has two and second is enabled
            if ( ! scriptUseP2    &&
                 port2IsActivated && // Return true in a one port 138 MTU
                 this.mtu.TwoPorts ) // and for that reason I have to check also this
                Errors.LogErrorNow ( new ScriptForOnePortButTwoEnabledException () );
            
            // Script is for two ports but MTU has not second port or is disabled
            else if ( scriptUseP2 &&
                      ! port2IsActivated )
                Errors.LogErrorNow ( new ScriptForTwoPortsButMtuOnlyOneException () );

            // Auto-detect Meter
            bool isAutodetectMeter = false;
            if ( ! form.ContainsParameter ( FIELD.METER_TYPE ) )
            {
                // Missing tags
                if ( ! form.ContainsParameter ( FIELD.NUMBER_OF_DIALS ) )
                    Errors.AddError ( new NumberOfDialsTagMissingScript () );
                
                if ( ! form.ContainsParameter ( FIELD.DRIVE_DIAL_SIZE ) )
                    Errors.AddError ( new DriveDialSizeTagMissingScript () );
                    
                if ( ! form.ContainsParameter ( FIELD.UNIT_MEASURE ) )
                    Errors.AddError ( new UnitOfMeasureTagMissingScript () );
                
                // Log errors but not finish yet
                Errors.LogRegisteredErrors ();
            
                if ( form.ContainsParameter ( FIELD.NUMBER_OF_DIALS ) &&
                     form.ContainsParameter ( FIELD.DRIVE_DIAL_SIZE ) &&
                     form.ContainsParameter ( FIELD.UNIT_MEASURE    ) )
                {
                    isAutodetectMeter = true;
                
                    meters = configuration.meterTypes.FindByDialDescription (
                        int.Parse ( form.NumberOfDials.Value ),
                        int.Parse ( form.DriveDialSize.Value ),
                        form.UnitOfMeasure.Value,
                        this.mtu.Flow );
    
                    // At least one Meter was found
                    if ( meters.Count > 0 )
                        form.AddParameter ( FIELD.METER_TYPE, ( meterPort1 = meters[ 0 ] ).Id.ToString () );
                    // No meter was found using the selected parameters
                    else
                        Errors.LogErrorNow ( new ScriptingAutoDetectMeterException () );
                }
                // Script does not contain some of the needed tags ( NumberOfDials,... )
                else Errors.LogErrorNow ( new ScriptingAutoDetectMeterException () );
            }
            // Check if the selected Meter exists and current MTU support it
            else
            {
                meterPort1 = configuration.getMeterTypeById ( int.Parse ( form.Meter.Value ) );
                portTypes  = this.mtu.Ports[ 0 ].GetPortTypes ();
                
                // Is not valid Meter ID
                if ( meterPort1.IsEmpty )
                    Errors.LogErrorNow ( new MtuTypeIsNotFoundException () );
                
                // Current MTU does not support selected Meter
                else if ( ! portTypes.Contains ( form.Meter.Value ) && // By Meter Id = Numeric
                          ! portTypes.Contains ( meterPort1.Type ) )   // By Type = Chars
                    Errors.LogErrorNow ( new MtuNotSupportMeterException () );
            }

            if ( this.mtu.TwoPorts &&
                 port2IsActivated )
            {
                if ( ! form.ContainsParameter ( FIELD.METER_TYPE_2 ) )
                {
                    // Missing tags
                    if ( ! form.ContainsParameter ( FIELD.NUMBER_OF_DIALS_2 ) )
                        Errors.AddError ( new NumberOfDialsTagMissingScript (), 2 );
                    
                    if ( ! form.ContainsParameter ( FIELD.DRIVE_DIAL_SIZE_2 ) )
                        Errors.AddError ( new DriveDialSizeTagMissingScript (), 2 );
                        
                    if ( ! form.ContainsParameter ( FIELD.UNIT_MEASURE_2 ) )
                        Errors.AddError ( new UnitOfMeasureTagMissingScript (), 2 );
                
                    // Log errors but not finish yet
                    Errors.LogRegisteredErrors ();
                
                    if ( form.ContainsParameter ( FIELD.NUMBER_OF_DIALS_2 ) &&
                         form.ContainsParameter ( FIELD.DRIVE_DIAL_SIZE_2 ) &&
                         form.ContainsParameter ( FIELD.UNIT_MEASURE_2    ) )
                    {
                        meters = configuration.meterTypes.FindByDialDescription (
                            int.Parse ( form.NumberOfDials_2.Value ),
                            int.Parse ( form.DriveDialSize_2.Value ),
                            form.UnitOfMeasure_2.Value,
                            this.mtu.Flow );
                        
                        // At least one Meter was found
                        if ( meters.Count > 0 )
                            form.AddParameter ( FIELD.METER_TYPE_2, ( meterPort2 = meters[ 0 ] ).Id.ToString () );
                        // No meter was found using the selected parameters
                        else
                            Errors.LogErrorNow ( new ScriptingAutoDetectMeterException (), 2 );
                    }
                    // Script does not contain some of the needed tags ( NumberOfDials,... )
                    else Errors.LogErrorNow ( new ScriptingAutoDetectMeterException (), 2 );
                }
                // Check if the selected Meter exists and current MTU support it
                else
                {
                    meterPort2 = configuration.getMeterTypeById ( int.Parse ( form.Meter_2.Value ) );
                    portTypes  = this.mtu.Ports[ 1 ].GetPortTypes ();
                    
                    // Is not valid Meter ID
                    if ( meterPort2.IsEmpty )
                        Errors.LogErrorNow ( new MtuTypeIsNotFoundException (), 2 );
                    
                    // Current MTU does not support selected Meter
                    else if ( ! portTypes.Contains ( form.Meter_2.Value ) && // By Meter Id = Numeric
                              ! portTypes.Contains ( meterPort2.Type ) )     // By Type = Chars
                        Errors.LogErrorNow ( new MtuNotSupportMeterException (), 2 );
                }
            }

            #endregion

            #region Validation

            dynamic Empty = new Func<string,bool> ( ( value ) =>
                                    string.IsNullOrEmpty ( value ) );

            dynamic EmptyNum = new Func<string,bool> ( ( value ) =>
                                    string.IsNullOrEmpty ( value ) || ! Validations.IsNumeric ( value ) );

            // Value equals to maximum length
            dynamic NoEqNum = new Func<string,int,bool> ( ( value, maxLength ) =>
                                ! Validations.NumericText ( value, maxLength ) );
                                
            dynamic NoEqTxt = new Func<string,int,bool> ( ( value, maxLength ) =>
                                ! Validations.Text ( value, maxLength ) );

            // Value equals or lower to maximum length
            dynamic NoELNum = new Func<string,int,bool> ( ( value, maxLength ) =>
                                ! Validations.NumericText ( value, maxLength, 1, true, true, false ) );
                                
            dynamic NoELTxt = new Func<string,int,bool> ( ( value, maxLength ) =>
                                ! Validations.Text ( value, maxLength, 1, true, true, false ) );
        
            // Validate each parameter and remove those that are not going to be used
            string msgError = string.Empty;
            foreach ( KeyValuePair<FIELD,Parameter> item in form.RegisteredParamsByField )
            {
                FIELD type = item.Key;
                Parameter parameter = item.Value;
            
                bool   fail  = false;
                string value = parameter.Value.ToString ();
            
                // Validates each parameter before continue with the action
                switch ( type )
                {
                    case FIELD.ACTIVITY_LOG_ID:
                    fail = EmptyNum ( value );
                    break;
                
                    case FIELD.ACCOUNT_NUMBER:
                    case FIELD.ACCOUNT_NUMBER_2:
                    fail = NoEqNum ( value, global.AccountLength );
                    break;
                    
                    case FIELD.WORK_ORDER:
                    case FIELD.WORK_ORDER_2:
                    fail = NoELTxt ( value, global.WorkOrderLength );
                    
                    // Do not use
                    if ( ! fail &&
                         ! global.WorkOrderRecording )
                        if ( parameter.Port == 0 )
                             form.RemoveParameter ( FIELD.WORK_ORDER   );
                        else form.RemoveParameter ( FIELD.WORK_ORDER_2 );
                    break;
                    
                    case FIELD.MTU_ID_OLD:
                    fail = NoEqNum ( value, global.MtuIdLength );
                    
                    // Do not use
                    if ( ! fail &&
                         addMtuAction.type != ActionType.ReplaceMTU &&
                         addMtuAction.type != ActionType.ReplaceMtuReplaceMeter )
                        form.RemoveParameter ( FIELD.MTU_ID_OLD );
                    break;
                    
                    case FIELD.METER_NUMBER:
                    case FIELD.METER_NUMBER_2:
                    case FIELD.METER_NUMBER_OLD:
                    case FIELD.METER_NUMBER_OLD_2:
                    fail = NoELTxt ( value, global.MeterNumberLength );
                    
                    // Do not use
                    if ( ! fail &&
                         ! global.UseMeterSerialNumber )
                    {
                        if ( parameter.Port == 0 )
                        {
                            switch ( parameter.Type )
                            {
                                case ParameterType.MeterSerialNumber:
                                case ParameterType.NewMeterSerialNumber:
                                form.RemoveParameter ( FIELD.METER_NUMBER );
                                break;
                                
                                case ParameterType.OldMeterSerialNumber:
                                form.RemoveParameter ( FIELD.METER_NUMBER_OLD );
                                break;
                            }
                        }
                        else
                        {
                            switch ( parameter.Type )
                            {
                                case ParameterType.MeterSerialNumber:
                                case ParameterType.NewMeterSerialNumber:
                                form.RemoveParameter ( FIELD.METER_NUMBER_2 );
                                break;
                                
                                case ParameterType.OldMeterSerialNumber:
                                form.RemoveParameter ( FIELD.METER_NUMBER_OLD_2 );
                                break;
                            }
                        }
                    }
                    break;
                    
                    case FIELD.METER_READING:
                    case FIELD.METER_READING_2:
                    if ( ! isAutodetectMeter )
                    {
                        // If necessary fill left to 0's up to LiveDigits
                        if ( parameter.Port == 0 )
                             value = meterPort1.FillLeftLiveDigits ( value );
                        else value = meterPort2.FillLeftLiveDigits ( value );
                    }
                    else
                    {
                        if ( parameter.Port == 0 )
                        {
                            if ( ! ( fail = meterPort1.NumberOfDials <= -1 || 
                                            NoELNum ( value, meterPort1.NumberOfDials ) ) )
                            {
                                // Apply mask and fill left to 0's up to LiveDigits
                                value = meterPort1.FillLeftNumberOfDials ( value );
                                value = meterPort1.ApplyReadingMask ( value );
                            }
                            else break;
                        }
                        else
                        {
                            if ( ! ( fail = meterPort2.NumberOfDials <= -1 ||
                                            NoELNum ( value, meterPort2.NumberOfDials ) ) )
                            {
                                // Apply mask and fill left to 0's up to LiveDigits
                                value = meterPort2.FillLeftNumberOfDials ( value );
                                value = meterPort2.ApplyReadingMask ( value );
                            }
                            else break;
                        }
                    }
                    
                    if ( parameter.Port == 0 )
                         fail = NoEqNum ( value, meterPort1.LiveDigits );
                    else fail = NoEqNum ( value, meterPort2.LiveDigits );
                    break;
                    
                    case FIELD.METER_READING_OLD:
                    case FIELD.METER_READING_OLD_2:
                    fail = NoELNum ( value, 12 );
                    
                    // Do not use
                    if ( ! fail &&
                         ! global.OldReadingRecording )
                    {
                        if ( parameter.Port == 0 )
                             form.RemoveParameter ( FIELD.METER_READING_OLD   );
                        else form.RemoveParameter ( FIELD.METER_READING_OLD_2 );
                    }
                    break;
                    
                    case FIELD.METER_TYPE:
                    case FIELD.METER_TYPE_2:
                    fail = Empty ( value );
                    break;
                    
                    case FIELD.READ_INTERVAL:
                    List<string> readIntervalList;
                    if ( MtuForm.mtuBasicInfo.version >= global.LatestVersion )
                    {
                        readIntervalList = new List<string>()
                        {
                            "24 Hours",
                            "12 Hours",
                            "8 Hours",
                            "6 Hours",
                            "4 Hours",
                            "3 Hours",
                            "2 Hours",
                            "1 Hour",
                            "30 Min",
                            "20 Min",
                            "15 Min"
                        };
                    }
                    else
                    {
                        readIntervalList = new List<string>()
                        {
                            "1 Hour",
                            "30 Min",
                            "20 Min",
                            "15 Min"
                        };
                    }
                    
                    // TwoWay MTU reading interval cannot be less than 15 minutes
                    if ( ! this.mtu.TimeToSync )
                    {
                        readIntervalList.AddRange ( new string[]{
                            "10 Min",
                            "5 Min"
                        });
                    }
                    
                    value = value.ToLower ()
                                 .Replace ( "hr", "hour" )
                                 .Replace ( "h", "H" );
                    fail = Empty ( value ) || ! readIntervalList.Contains ( value );
                    break;
                    
                    case FIELD.SNAP_READS:
                    fail = EmptyNum ( value );
                    
                    // Do not use
                    if ( ! fail &&
                         ( ! global.AllowDailyReads ||
                           ! mtu.DailyReads ) )
                        form.RemoveParameter ( FIELD.SNAP_READS );
                    break;
                    
                    //case FIELD.LIVE_DIGITS:
                    //fail = NoELNum ( value, 10 );
                    //break;
                    
                    case FIELD.NUMBER_OF_DIALS:
                    case FIELD.NUMBER_OF_DIALS_2:
                    case FIELD.DRIVE_DIAL_SIZE:
                    case FIELD.DRIVE_DIAL_SIZE_2:
                    fail = EmptyNum ( value );
                    break;
                    
                    case FIELD.UNIT_MEASURE:
                    case FIELD.UNIT_MEASURE_2:
                    fail = Empty ( value );
                    break;
                    
                    case FIELD.FORCE_TIME_SYNC:
                    bool.TryParse ( value, out fail );
                    fail = ! fail;
                    break;
                }

                if ( fail )
                {
                    fail = false;
                    
                    string typeStr = ( form.Texts as Dictionary<FIELD,string[]> )[ type ][ 2 ];
                    
                    if ( ! msgError.Contains ( typeStr ) )
                        msgError += ( ( ! string.IsNullOrEmpty ( msgError ) ) ? ", " : string.Empty ) + typeStr;
                }
            }

            if ( ! string.IsNullOrEmpty ( msgError ) )
            {
                int index;
                if ( ( index = msgError.LastIndexOf ( ',' ) ) > -1 )
                    msgError = msgError.Substring ( 0, index ) +
                               " and" +
                               msgError.Substring ( index + 1 );

                Errors.LogErrorNow ( new ProcessingParamsScriptException ( msgError ) );
            }

            #endregion

            #region Auto-detect Alarm

            // Auto-detect scripting Alarm profile
            List<Alarm> alarms = configuration.alarms.FindByMtuType ( (int)MtuForm.mtuBasicInfo.Type );
            if ( alarms.Count > 0 )
            {
                Alarm alarm = alarms.Find ( a => string.Equals ( a.Name.ToLower (), "scripting" ) );
                if ( alarm != null &&
                     form.mtu.RequiresAlarmProfile )
                    form.AddParameter ( FIELD.ALARM, alarm );
                    
                // For current MTU does not exist "Scripting" profile inside Alarm.xml
                else Errors.LogErrorNow ( new ScriptingAlarmForCurrentMtuException () );
            }

            #endregion

            this.Task_AddMtu ( form, addMtuAction.user, addMtuAction.type, true );
        }

        private void Task_AddMtu (
            dynamic form,
            string user,
            ActionType actionType = ActionType.AddMtu,
            bool isFromScripting = false )
        {
            Mtu    mtu    = form.mtu;
            Global global = form.global;
        
            this.mtu    = mtu;
            form.actionType = actionType;

            try
            {
                Logger logger = ( ! isFromScripting ) ? new Logger ( this.configuration ) : truquitoAction.logger;
                addMtuLog = new AddMtuLog ( logger, form, user, isFromScripting );

                #region Turn Off MTU

                this.TurnOnOffMtu_Logic ( false );
                addMtuLog.LogTurnOff();

                #endregion

                #region Add MTU

                // Prepare memory map
                String memory_map_type = configuration.GetMemoryMapTypeByMtuId ( this.mtu ); //( int )MtuForm.mtuBasicInfo.Type );
                int    memory_map_size = configuration.GetmemoryMapSizeByMtuId ( this.mtu ); //( int )MtuForm.mtuBasicInfo.Type );

                byte[] memory = new byte[ memory_map_size ];
                dynamic map = new MemoryMap.MemoryMap ( memory, memory_map_type );
                
                form.map = map;

                #region Account Number

                map.P1MeterId = form.AccountNumber.Value;
                if ( form.usePort2 &&
                     form.ContainsParameter ( FIELD.ACCOUNT_NUMBER_2 ) )
                    map.P2MeterId = form.AccountNumber_2.Value;

                #endregion

                #region Meter Type

                Meter selectedMeter  = null;
                Meter selectedMeter2 = null;
                   
                if ( ! isFromScripting )
                     selectedMeter = (Meter)form.Meter.Value;
                else selectedMeter = this.configuration.getMeterTypeById ( Convert.ToInt32 ( ( string )form.Meter.Value ) );
                map.P1MeterType = selectedMeter.Id;

                if ( form.usePort2 &&
                     form.ContainsParameter ( FIELD.METER_TYPE_2 ) )
                {
                    if ( ! isFromScripting )
                         selectedMeter2 = (Meter)form.Meter_2.Value;
                    else selectedMeter2 = this.configuration.getMeterTypeById ( Convert.ToInt32 ( ( string )form.Meter_2.Value ) );
                    map.P2MeterType = selectedMeter2.Id;
                }

                #endregion

                #region Initial Reading = Meter Reading

                string p1readingStr = "0";
                string p2readingStr = "0";

                if ( form.ContainsParameter ( FIELD.METER_READING ) )
                {
                    if ( ! isFromScripting ) // No mask
                         p1readingStr = form.MeterReading.Value;
                    else p1readingStr = selectedMeter.FillLeftLiveDigits ( form.MeterReading.Value );
                    
                    ulong p1reading = ( ! string.IsNullOrEmpty ( p1readingStr ) ) ? Convert.ToUInt64 ( ( p1readingStr ) ) : 0;
    
                    map.P1Reading = p1reading / ( ( selectedMeter.HiResScaling <= 0 ) ? 1 : selectedMeter.HiResScaling );
                }
                
                if ( form.usePort2 &&
                     form.ContainsParameter ( FIELD.METER_READING_2 ) )
                {
                    if ( ! isFromScripting ) // No mask
                         p2readingStr = form.MeterReading_2.Value;
                    else p2readingStr = selectedMeter2.FillLeftLiveDigits ( form.MeterReading_2.Value );
                    
                    ulong p2reading = ( ! string.IsNullOrEmpty ( p2readingStr ) ) ? Convert.ToUInt64 ( ( p2readingStr ) ) : 0;
    
                    map.P2Reading = p2reading / ( ( selectedMeter2.HiResScaling <= 0 ) ? 1 : selectedMeter2.HiResScaling );
                }

                #endregion

                #region Reading Interval

                if ( global.IndividualReadInterval )
                {
                    // If not present in scripted mode, set default value to one/1 hour
                    map.ReadIntervalMinutes = ( form.ContainsParameter ( FIELD.READ_INTERVAL ) ) ?
                                                form.ReadInterval.Value : "1 Hr";
                }

                #endregion

                #region Overlap count

                map.MessageOverlapCount = DEFAULT_OVERLAP;
                if ( form.usePort2 )
                    map.P2MessageOverlapCount = DEFAULT_OVERLAP;

                #endregion

                #region Alarm

                if ( mtu.RequiresAlarmProfile )
                {
                    Alarm alarms = (Alarm)form.Alarm.Value;
                    if ( alarms != null )
                    {
                        /*
                        try
                        {
                        if ( mtu.CutWireDelaySetting )
                            map.xxx = alarms.CutWireDelaySetting;

                        if ( mtu.GasCutWireAlarm )
                            map.xxx = alarms.LastGasp;

                        if ( mtu.GasCutWireAlarmImm )
                            map.xxx = alarms.LastGaspImm;
                            
                        if ( mtu.InsufficentMemory )
                            map.xxx = alarms.InsufficientMemory;
                            
                        if ( mtu.InsufficentMemoryImm )
                            map.xxx = alarms.InsufficientMemoryImm;
                            
                        if ( mtu.InterfaceTamper )
                            map.P1InterfaceAlarm = alarms.InterfaceTamper;

                        if ( mtu.InterfaceTamperImm )
                            map.xxx = alarms.InterfaceTamperImm;
                            
                        if ( mtu.LastGasp )
                            map.xxx = alarms.LastGasp;

                        if ( mtu.LastGaspImm )
                            map.xxx = alarms.LastGaspImm;
                        
                        if ( mtu.MagneticTamper )
                            map.P1MagneticAlarm = alarms.Magnetic;
                            
                        if ( mtu.RegisterCoverTamper )
                            map.P1RegisterCoverAlarm = alarms.RegisterCover;
                            
                        if ( mtu.ReverseFlowTamper )
                            map.P1ReverseFlowAlarm = alarms.ReverseFlow;
                            
                        if ( mtu.SerialComProblem )
                            map.xxx = alarms.SerialComProblem;
                            
                        if ( mtu.SerialComProblemImm )
                            map.xxx = alarms.SerialComProblemImm;
                            
                        if ( mtu.SerialCutWire )
                            map.xxx = alarms.SerialCutWire;
                            
                        if ( mtu.SerialCutWireImm )
                            map.xxx = alarms.SerialCutWireImm;
                            
                        if ( mtu.TamperPort1 )
                            map.xxx = alarms.TamperPort1;
                            
                        if ( mtu.TamperPort2 )
                            map.xxx = alarms.TamperPort2;
                        
                        if ( mtu.TamperPort1Imm )
                            map.xxx = alarms.TamperPort1Imm;
                        
                        if ( mtu.TamperPort2Imm )
                            map.xxx = alarms.TamperPort2Imm;
                            
                        if ( mtu.TiltTamper )
                            map.P1TiltAlarm = alarms.Tilt;

    
                    
                        // P1ImmediateAlarm
                        map.P1ImmediateAlarm = alarms.ImmediateAlarmTransmit;
                    
                        // P1UrgentAlarm
                        map.P1UrgentAlarm = alarms.DcuUrgentAlarm;
                        
                        // Escribir directamente
                        // - DcuAlarm
                        // - ImmediateAlarm
                    
                        // Cut wire alarm
                        // Only for MTU Types: 144, 146, 148 and 154
                        if ( mtu.GasCutWireAlarm )
                            map.P1CutWireAlarm = alarms.LastGasp;
                    
                        
                        
                        // Message overlap count
                        // Number of new readings to take before transmit
                        map.MessageOverlapCount = alarms.Overlap;
                        
                        // 
                        map.xxx = alarms.CutWireDelaySetting;

                        }
                        catch ( Exception e )
                        {

                        }
                        */
                    }
                    // No alarm profile was selected before launch the action
                    else Errors.LogErrorNow ( new SelectedAlarmForCurrentMtuException () );
                }

                #endregion

                #region Encryption

                // Only encrypt the key if MTU.SpecialSet tag is true
                if ( mtu.SpecialSet )
                {
                    try
                    { 
                        MemoryRegister<string> eKey = map[ "EncryptionKey" ];

                    RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider ();
                    byte[] aesKey = new byte[ eKey.size ]; // DEFAULT_LENGTH_AES ];
                    rng.GetBytes ( aesKey );
                    map.EncryptionKey = aesKey;
                    for ( int i = 0; i < 15; i++ )
                        if ( aesKey[ i ] != memory[ 256 + i ] )
                            throw new Exception ( "AES key does not match" );

                    }
                    catch ( Exception e )
                    {

                    }

                    // Encrypted
                    // EncryptionIndex
                }

                #endregion

                #region Frequencies

                if ( mtu.TimeToSync &&
                     global.AFC &&
                     map.MtuSoftVersion >= 19 )
                {
                    map.F12WAYRegister1Int  = global.F12WAYRegister1;
                    map.F12WAYRegister10Int = global.F12WAYRegister10;
                    map.F12WAYRegister14Int = global.F12WAYRegister14;
                }

                #endregion

                // fast message (not in pulse)
                // encoder digits to drop (not in pulse)

                // Write changes into MTU
                WriteMtuModifiedRegisters( map );
                addMtuLog.LogAddMtu ( isFromScripting );

                #endregion

                #region Turn On MTU

                this.TurnOnOffMtu_Logic ( true );
                addMtuLog.LogTurnOn();

                #endregion

                #region Install Confirmation

                // After TurnOn has to be performed an InstallConfirmation
                // if certain tags/registers are validated/true
                if ( global.TimeToSync && // Indicates that is a two-way MTU and enables TimeSync request
                     mtu.TimeToSync    && // Indicates that is a two-way MTU and enables TimeSync request
                     mtu.OnTimeSync    && // MTU can be force during installation to perform a TimeSync/IC
                     // If script contains ForceTimeSync, use it but if not use value from Global
                     ( ! form.ContainsParameter ( FIELD.FORCE_TIME_SYNC ) &&
                       global.ForceTimeSync ||
                       form.ContainsParameter ( FIELD.FORCE_TIME_SYNC ) &&
                       form.ForceTimeSync ) )
                {
                    // Force to execute Install Confirmation avoiding problems
                    // with MTU shipbit, because MTU is just turned on
                    this.InstallConfirmation_Logic ( true );
                }

                #endregion

                #region Read MTU

                lexi.Write(64, new byte[] { 1 });
                Thread.Sleep(1000);

                byte[] buffer = new byte[1024];

                System.Buffer.BlockCopy(lexi.Read(0, 255), 0, buffer, 0, 255);
               
                if (memory_map_size > 255)
                {
                    System.Buffer.BlockCopy(lexi.Read(256, 64), 0, buffer, 256, 64);
                    System.Buffer.BlockCopy(lexi.Read(318, 2), 0, buffer, 318, 2);
                }
                if (memory_map_size > 320)
                {
                    //System.Buffer.BlockCopy(lexi.Read(320, 64), 0, buffer, 320, 64);
                    //System.Buffer.BlockCopy(lexi.Read(384, 64), 0, buffer, 384, 64);
                    //System.Buffer.BlockCopy(lexi.Read(448, 64), 0, buffer, 448, 64);
                    //System.Buffer.BlockCopy(lexi.Read(512, 64), 0, buffer, 512, 64);
                }
                if (memory_map_size > 960)
                {
                    System.Buffer.BlockCopy(lexi.Read(960, 64), 0, buffer, 960, 64);
                }

                MemoryMap.MemoryMap readMemoryMap = new MemoryMap.MemoryMap(buffer, memory_map_type);

                List<string> diff = new List<string>(map.GetModifiedRegistersDifferences(readMemoryMap));
                if (diff.Count > 1 || (diff.Count == 1 && !diff.Contains("EncryptionKey")))
                {
                    // ERROR
                }
                else
                {
                    // OK
                }

                //if ( ! isFromScripting )
                    //form.logger.fixed_name = "";

                #endregion

                // Generate log to show on device screen
                AddMtuArgs addMtuArgs = new AddMtuArgs ( readMemoryMap, mtu, form, addMtuLog );
                this.OnAddMtu ( this, addMtuArgs );

                //ActionResult result = this.OnAddMtu ( this, addMtuArgs );
                //addMtuLog.LogReadMtu(result);

                // Generate xml log file and save on device
                //addMtuLog.Save ();
            }
            catch ( Exception e )
            {
                this.OnError ();
            }
        }

        #endregion

        public string GetResultXML ()
        {
            return addMtuLog.ToString ();
        }

        public void Task_BasicRead ()
        {
            BasicReadArgs args = new BasicReadArgs();
            OnBasicRead(this, args);
        }

        #endregion

        #region Write to MTU

        public void WriteMtuModifiedRegisters ( MemoryMap.MemoryMap map )
        {
            List<dynamic> modifiedRegisters = map.GetModifiedRegisters ().GetAllElements ();
            foreach ( dynamic r in modifiedRegisters )
                this.WriteMtuRegister ( ( uint )r.address, map.memory, ( uint )r.size );

            modifiedRegisters.Clear ();
            modifiedRegisters = null;
        }

        public void WriteMtuRegister(uint address, byte[] memory, uint length)
        {
            byte[] tmp = new byte[ length ];
            Array.Copy ( memory, address, tmp, 0, length );

            Console.WriteLine ( "Write subpart: Addr {0} | Value {1} | Length {2}", address, BitConverter.ToString(tmp), length );

            lexi.Write ( address, tmp );
        }

        public void WriteMtuRegister ( uint address, byte[] values )
        {
            Console.WriteLine ( "Write values: Addr {0} | Value {1} | Length {2}", address, BitConverter.ToString(values), values.Length );

            lexi.Write ( address, values );
        }

        public T ReadMtuRegister<T> ( uint address, uint length )
        {
            byte value = ( lexi.Read ( address, length ) )[ 0 ];

            return ( T )( object )value;
        }

        public bool ReadMtuBit ( uint address, uint bit )
        {
            byte value = ( lexi.Read ( address, 1 ) )[ 0 ];

            return ( ( ( value >> ( int )bit ) & 1 ) == 1 );
        }

        public void WriteMtuBit ( uint address, uint bit, bool active )
        {
            this.WriteMtuBitAndVerify ( address, bit, active, false );
        }

        public bool WriteMtuBitAndVerify ( uint address, uint bit, bool active, bool verify = true )
        {
            // Read current value
            byte systemFlags = ( lexi.Read ( address, 1 ) )[ 0 ];

            // Modify bit and write to MTU
            if ( active )
                 systemFlags = ( byte ) ( systemFlags |    1 << ( int )bit   );
            else systemFlags = ( byte ) ( systemFlags & ~( 1 << ( int )bit ) );
            
            lexi.Write ( address, new byte[] { systemFlags } );

            // Read new written value to verify modification
            if ( verify )
            {
                byte valueWritten = ( lexi.Read ( address, 1 ) )[ 0 ];
                return ( ( ( valueWritten >> ( int )bit ) & 1 ) == ( ( active ) ? 1 : 0 ) );
            }

            // Without verification
            return true;
        }

        #endregion

        // NO PARECE USARSE
        public byte[] ReadComplete ( byte addr, uint length )
        {
            byte[] tmp = new byte[length];
            uint maxReadBytes = 255;
            uint readsNumber = length / maxReadBytes;
            uint additionalBytes = length % maxReadBytes;

            for (uint i = 0; i < readsNumber; i++)
            {
                uint currentAddr = i * maxReadBytes;
                Array.Copy(lexi.Read(currentAddr, maxReadBytes), 0, tmp, currentAddr, maxReadBytes);
            }

            if (additionalBytes > 0)
            {
                uint currentAddr = readsNumber * maxReadBytes;
                Array.Copy(lexi.Read(currentAddr, additionalBytes), 0, tmp, currentAddr, additionalBytes);
            }

            return tmp;
        }

        #region AuxiliaryFunctions
        
        private void LoadMtuAndMetersBasicInfo ()
        {
            if ( this.LoadMtuBasicInfo () )
            {
                MtuForm.SetBasicInfo ( latest_mtu );
                
                // Launchs exception 'MtuTypeIsNotFoundException'
                this.mtu = configuration.GetMtuTypeById ( ( int )this.latest_mtu.Type );
                
                if ( this.mtuHasChanged )
                {
                    for ( int i = 0; i < this.mtu.Ports.Count; i++ )
                        latest_mtu.setPortType ( i, this.mtu.Ports[ i ].Type );
                    
                    if ( latest_mtu.isEncoder ) { }
                }
            }
        }

        private bool LoadMtuBasicInfo (
            bool isAfterWriting = false )
        {
            List<byte> finalRead = new List<byte> ();
        
            try
            {
                byte[] firstRead  = lexi.Read ( BASIC_READ_1_ADDRESS, BASIC_READ_1_DATA );
                byte[] secondRead = lexi.Read ( BASIC_READ_2_ADDRESS, BASIC_READ_2_DATA );
                finalRead.AddRange ( firstRead  );
                finalRead.AddRange ( secondRead );
            }
            // System.IO.IOException = Puck is not well placed or is off
            catch ( Exception e )
            {
                if ( ! isAfterWriting )
                     Errors.LogErrorNow ( new PuckCantCommWithMtuException () );
                else Errors.LogErrorNow ( new PuckCantReadFromMtuAfterWritingException () );
            }

            MTUBasicInfo mtu_info = new MTUBasicInfo ( finalRead.ToArray () );
            this.mtuHasChanged = ( latest_mtu.Id   == 0 ||
                                   latest_mtu.Type == 0 ||
                                   mtu_info.Id     != latest_mtu.Id ||
                                   mtu_info.Type   != latest_mtu.Type );
            
            latest_mtu = mtu_info;
            
            return this.mtuHasChanged;
        }

        public MTUBasicInfo GetBasicInfo ()
        {
            return this.latest_mtu;
        }

        private byte GetByteSettingOnlyOneBit ( int bit )
        {
            BitArray bits = new BitArray ( bit + 1 );
            
            if ( bit > 0 )
                for ( int i = 0; i < bit; i++ )
                    bits[ i ] = false;
                    
            bits[ bit ] = true;
            
            // Left to right: e.g. 4 is not 100 but false false true
            
            byte[] result = new byte[ 1 ];
            bits.CopyTo ( result, 0 );
            
            return result[ 0 ];
        }

        #endregion
    }
}
