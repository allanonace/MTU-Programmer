using System.Collections.Generic;
using Xml;
using System;
using System.Linq;
using System.Dynamic;
using MTUComm.Exceptions;

namespace MTUComm
{
    public sealed class Errors
    {    
        #region Constants

        private const string ERROR_TITLE = "Controlled Exception";

        private Dictionary<Exception,int> ex2id = 
        new Dictionary<Exception,int> ()
        {
            // Dynamic MemoryMap [ 0xx ]
            //------------------
            // ...
        
            // MTU [ 1xx ]
            //----
            // MTU is not the same as at the beginning of the process
            { new MtuHasChangeBeforeFinishActionException (),   100 },
            // Puck can't write or read to/from MTU
            { new PuckCantCommWithMtuException (),              101 },
            // Puck can't read from MTU after has completed writing process
            { new PuckCantReadFromMtuAfterWritingException (),  102 },
        
            // Meter [ 2xx ]
            //------
            // Mtu can't be recovered using MTU type
            { new MtuTypeIsNotFoundException (),                200 },
            // The MTU does not support selected Meter
            { new MtuNotSupportMeterException (),               201 },
            // The Meter.xml does not contain the Meter type specified with tags NumberOfDials, DriveDialSize and UnitOfMeasure
            { new ScriptingAutoDetectMeterException (),         202 },
            // The script does not contain the tag NumberOfDials needed to select the MTU
            { new NumberOfDialsTagMissingScript (),             203 },
            // The script does not contain the tag DriveDialSize needed to select the MTU
            { new DriveDialSizeTagMissingScript (),             204 },
            // The script does not contain the tag UnitOfMeasure needed to select the MTU
            { new UnitOfMeasureTagMissingScript (),             205 },
            
            // Scripting Parameters [ 3xx ]
            //---------------------
            // Error translating or validating parameters from script/trigger file
            { new ProcessingParamsScriptException (),           300 },
            // Script is only for one port but the MTU has two port and both activated
            { new ScriptForOnePortButTwoEnabledException (),    301 },
            // Script is for two ports but the MTU has one port only or second port is disabled
            { new ScriptForTwoPortsButMtuOnlyOneException (),   302 },
            // Logfile element in the script is empty or contains some invalid character
            { new ScriptLogfileInvalidException (),             303 },
            // Action type specified in the script is empty or is not one of the available options
            { new ScriptActionTypeInvalidException (),          304 },
            // The script file used has not valid structure or format
            { new ScriptWrongStructureException (),             305 },
            // The script file used is empty
            { new ScriptEmptyException (),                      306 },
            
            // Alarm [ 4xx ]
            //------
            // Alarm profile selected for current MTU is not defined in the Alarm.xml file
            { new ScriptingAlarmForCurrentMtuException (),      400 },
            // The  alarm profile "Scripting" for current MTU is not defined in the Alarm.xml file
            { new SelectedAlarmForCurrentMtuException (),       401 },
            
            // Turn Off [ 5xx ]
            //---------
            // Turn off MTU process has failed trying to activated the Ship Bit
            { new AttemptNotAchievedTurnOffException (),        500 },
            // MTU can not be turned off after having tried it several times
            { new ActionNotAchievedTurnOffException (),         501 },
            
            // Install Confirmation [ 6xx ]
            //---------------------
            // The MTU has not support for two-way
            { new MtuIsNotTwowayICException (),                 600 },
            // The MTU is turned off
            { new MtuIsAlreadyTurnedOffICException (),          601 },
            // Installation Confirmation process has failed trying to communicate with the DCU
            { new AttemptNotAchievedICException (),             602 },
            // Installation Confirmation can't be performed after having tried it several times
            { new ActionNotAchievedICException (),              603 },
        };

        #endregion

        #region Attributes

        private static Errors instance;
        
        private Global global;
        private Logger logger;

        private Dictionary<int,Error> errors;
        private List<Error> errorsToLog;
        
        private Error lastError;

        #endregion

        #region Properties

        /// <summary>
        /// An indexer to easiest recover errors without having to use a methods,
        /// doing the structure more logical, associating the class itself with errors
        /// </summary>
        /// <param name="id">Error identifier</param>
        public Error this[ int id ]
        {
            get
            {
                if ( this.errors.ContainsKey ( id ) )
                    return errors[ id ];
                return null;
            }
        }
        
        /// <summary>
        /// Indicates if the error id should be written or only the messages
        /// </summary>
        /// <value><c>true</c> if the client wants to show error IDs in the log; otherwise, <c>false</c></value>
        public static bool ShowId
        {
            get
            {
                return Errors.GetInstance ().global.ErrorId;
            }
        }

        public static Error LastError
        {
            get { return Errors.GetInstance ().lastError; }
        }

        #endregion

        #region Initialization

        private Errors ()
        {
            Configuration config = Configuration.GetInstance ();
        
            this.global      = config.global;
            this.logger      = new Logger ( config );
            this.errors      = new Dictionary<int,Error> ();
            this.errorsToLog = new List<Error> ();
        
            Error[] errorsXml = config.errors;

            if ( errorsXml != null )
                foreach ( Error errorXml in errorsXml )
                    this.errors.Add ( errorXml.Id, errorXml );
                    
            config = null;
        }
        
        private static Errors GetInstance ()
        {
            if ( Errors.instance == null )
                Errors.instance = new Errors ();
            
            return Errors.instance;
        }

        #endregion

        #region Logic

        /// <summary>
        /// Register a new error to be written into the log after be recovered using GetErrorsToLog
        /// </summary>
        /// <returns><c>true</c>, if error was added, <c>false</c> otherwise.</returns>
        /// <param name="id">Error identifier</param>
        private bool AddErrorById (
            int id,
            Exception e,
            int portIndex )
        {
            // Error ID exists and is not registered already
            if ( this[ id ] != null )
            {
                Error error = ( Error )this[ id ].Clone ();
                error.Port  = portIndex;
                error.Exception = e;
            
                this.errorsToLog.Add ( error );
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Register .NET errors trying to found some error that match
        /// </summary>
        /// <param name="e">.NET exception</param>
        private Error AddErrorByException (
            Exception e,
            int portIndex )
        {
            Type typeExp = e.GetType ();
            
            // Own exception
            if ( this.ex2id.Any ( item => item.Key.GetType () == typeExp ) )
            {
                int id = this.ex2id.Single ( item => item.Key.GetType () == typeExp ).Value;
                
                this.AddErrorById ( id, e, portIndex );
            }
            // .NET exception
            else
            {
                Error error = ( Error )this.TryToTranslateDotNet ( e ).Clone ();
                error.Port  = portIndex;
                error.Exception = e;
            
                this.errorsToLog.Add ( error );
            }
                
            return ( this.lastError = this.errorsToLog.Last () );
        }

        /// <summary>
        /// Returns all registered errors and by default clear list after that
        /// </summary>
        /// <returns>The errors to log.</returns>
        /// <param name="clearList">If set to <c>true</c> clear list of registered errors</param>
        private Error[] _GetErrorsToLog (
            bool clearList = true )
        {
            Error[] errors = new Error[ this.errorsToLog.Count ];
            Array.Copy ( this.errorsToLog.ToArray (), errors, this.errorsToLog.Count );
            
            if ( clearList )
                this._ClearList ();
            
            return errors;
        }
        
        private Error TryToTranslateDotNet (
            Exception e )
        {
            dynamic dynException = new ExpandoObject ();
            dynException.Message = e.Message;
            dynException.HResult = int.Parse ( e.HResult.ToString ( "X" ) );
        
            int idTranslated = this.GetIdForDotNetError ( dynException );
        
            // The exception has translation in Error.xml
            if ( idTranslated > -1 )
                return this[ idTranslated ];
            
            // Register the .Net exception directly, without error ID
            return new Error ( dynException.Message );
        }

        private int GetIdForDotNetError (
            dynamic e ) // e is an Exception
        {
            if ( this.IsRegisteredDotNetError ( e ) )
                return this.errors.Single ( item => item.Value.DotNetId == e.HResult ).Value.Id;
            return -1;
        }
        
        private bool IsRegisteredDotNetError (
           dynamic e ) // e is an Exception
        {
            return e.HResult > -1 &&
                   this.errors.Any ( item => item.Value.DotNetId == e.HResult );
        }
        
        /// <summary>
        /// Write an error into the log right now, without have to invoke AddError method
        /// Usually used outside actions logic, for example trying to detect and connect with a puck
        /// </summary>
        /// <param name="e">Exception launched</param>
        private void _LogErrorNow (
            Exception e,
            int portIndex )
        {
            Error error = this.AddErrorByException ( e, portIndex );
            
            PageLinker.ShowAlert ( ERROR_TITLE, error );
            
            this.logger.LogError ();
        }
        
        /// <summary>
        /// Write errors registered using AddError method
        /// Usually used when performing an action
        /// </summary>
        private void _LogRegisteredErrors (
            bool forceException,
            Exception e )
        {
            if ( this.errorsToLog.Count > 0 )
            {
                //Error lastError = ( Error )this.errorsToLog[ this.errorsToLog.Count - 1 ].Clone ();
                Exception lastException = ( e != null ) ? e : this.errorsToLog[ this.errorsToLog.Count - 1 ].Exception;
                
                if ( forceException )
                    PageLinker.ShowAlert ( ERROR_TITLE, this.lastError );
                
                this.logger.LogError ();

                if ( forceException )
                    throw lastException;
            }
        }

        /// <summary>
        /// Clears the list of registered errors
        /// Usually no need to use this method because is already used by default for GetErrorsToLog
        /// </summary>
        private void _ClearList ()
        {
            this.errorsToLog.Clear ();
        }

        private bool IsLastExceptionUsed (
            Exception e )
        {
            return ( this.lastError != null &&
                     this.lastError.Exception == e );
        }

        private static void LaunchException (
            Exception e,
            bool forceException )
        {
            if ( forceException )
                throw e;
        }

        #endregion

        #region Direct Singleton

        public static Error[] GetErrorsToLog (
            bool clearList = true )
        {
            return Errors.GetInstance ()._GetErrorsToLog ( clearList );
        }
        
        public static void AddError (
            Exception e,
            int portIndex = 1 )
        {
            Errors.GetInstance ().AddErrorByException ( e, portIndex );
        }
        
        public static void LogErrorNow (
            Exception e,
            int portIndex = 1,
            bool forceException = true )
        {
            Errors.GetInstance ()._LogErrorNow ( e, portIndex );
            Errors.LaunchException ( e, forceException );
        }
        
        /// <summary>
        /// Only log registered errors and shows error message/pop-up of the last,
        /// without launching the exception, allowing to continue executing process logic
        /// </summary>
        /// <param name="e">Exception that represents the last error happened</param>
        /// <param name="portindex">Index of MTU port associated to the error</param>
        public static void LogErrorNowAndContinue (
            Exception e,
            int portindex = 1 )
        {
            LogErrorNow ( e, portindex, false );
        }

        public static void LogRegisteredErrors (
            bool forceException = false,
            Exception e = null )
        {
            Errors.GetInstance ()._LogRegisteredErrors ( forceException, e );
        }

        /// <summary>
        /// Both options will log all registered exceptions that remain, but in
        /// the first case, previously the last exception launched will be added
        /// </summary>
        /// <param name="e">Exception</param>
        public static void LogRemainExceptions (
            Exception e )
        {
            // Last exception was not added yet
            if ( ! Errors.GetInstance ().IsLastExceptionUsed ( e ) )
                Errors.LogErrorNow ( e );
            
            // Last exception was already added
            else
                Errors.LogRegisteredErrors (); // ! ( e is OwnExceptionsBase ) );
        }
        
        public static bool IsOwnException (
            Exception e )
        {
            return ( e.GetType ().IsSubclassOf ( typeof( OwnExceptionsBase ) ) );
        }

        #endregion
    }
}
