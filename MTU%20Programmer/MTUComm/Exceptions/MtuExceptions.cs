using System;

namespace MTUComm.Exceptions
{
    public class MtuTypeIsNotFoundException : OwnExceptionsBase
    {
        public MtuTypeIsNotFoundException () { }
        public MtuTypeIsNotFoundException ( string message ) : base ( message ) { }
    }
    
    public class MtuNotSupportMeterException : OwnExceptionsBase
    {
        public MtuNotSupportMeterException () { }
        public MtuNotSupportMeterException ( string message ) : base ( message ) { }
    }
    
    public class MtuHasChangeBeforeFinishActionException : OwnExceptionsBase
    {
        public MtuHasChangeBeforeFinishActionException () { }
        public MtuHasChangeBeforeFinishActionException ( string message ) : base ( message ) { }
    }
    
    public class PuckCantCommWithMtuException : OwnExceptionsBase
    {
        public PuckCantCommWithMtuException () { }
        public PuckCantCommWithMtuException ( string message ) : base ( message ) { }
    }
    
    public class PuckCantReadFromMtuAfterWritingException : OwnExceptionsBase
    {
        public PuckCantReadFromMtuAfterWritingException () { }
        public PuckCantReadFromMtuAfterWritingException ( string message ) : base ( message ) { }
    }
    
    public class AttemptNotAchievedTurnOffException : OwnExceptionsBase
    {
        public AttemptNotAchievedTurnOffException () { }
        public AttemptNotAchievedTurnOffException ( string message ) : base ( message ) { }
    }
    
    public class ActionNotAchievedTurnOffException : OwnExceptionsBase
    {
        public ActionNotAchievedTurnOffException () { }
        public ActionNotAchievedTurnOffException ( string message ) : base ( message ) { }
    }
    
    public class ScriptingAlarmForCurrentMtuException : OwnExceptionsBase
    {
        public ScriptingAlarmForCurrentMtuException () { }
        public ScriptingAlarmForCurrentMtuException ( string message ) : base ( message ) { }
    }
    
    public class SelectedAlarmForCurrentMtuException : OwnExceptionsBase
    {
        public SelectedAlarmForCurrentMtuException () { }
        public SelectedAlarmForCurrentMtuException ( string message ) : base ( message ) { }
    }
    
    public class NumberOfDialsTagMissingScript : OwnExceptionsBase
    {
        public NumberOfDialsTagMissingScript () { }
        public NumberOfDialsTagMissingScript ( string message ) : base ( message ) { }
    }
    
    public class DriveDialSizeTagMissingScript : OwnExceptionsBase
    {
        public DriveDialSizeTagMissingScript () { }
        public DriveDialSizeTagMissingScript ( string message ) : base ( message ) { }
    }
    
    public class UnitOfMeasureTagMissingScript : OwnExceptionsBase
    {
        public UnitOfMeasureTagMissingScript () { }
        public UnitOfMeasureTagMissingScript ( string message ) : base ( message ) { }
    }
    
    public class ScriptingAutoDetectMeterException : OwnExceptionsBase
    {
        public ScriptingAutoDetectMeterException () { }
        public ScriptingAutoDetectMeterException ( string message ) : base ( message ) { }
    }
    
    public class ProcessingParamsScriptException : OwnExceptionsBase
    {
        public ProcessingParamsScriptException () { }
        public ProcessingParamsScriptException ( string message ) : base ( message ) { }
    }
    
    public class ScriptForOnePortButTwoEnabledException : OwnExceptionsBase
    {
        public ScriptForOnePortButTwoEnabledException () { }
        public ScriptForOnePortButTwoEnabledException ( string message ) : base ( message ) { }
    }
    
    public class ScriptForTwoPortsButMtuOnlyOneException : OwnExceptionsBase
    {
        public ScriptForTwoPortsButMtuOnlyOneException () { }
        public ScriptForTwoPortsButMtuOnlyOneException ( string message ) : base ( message ) { }
    }
    
    public class ScriptLogfileInvalidException : OwnExceptionsBase
    {
        public ScriptLogfileInvalidException () { }
        public ScriptLogfileInvalidException ( string message ) : base ( message ) { }
    }
    
    public class ScriptActionTypeInvalidException : OwnExceptionsBase
    {
        public ScriptActionTypeInvalidException () { }
        public ScriptActionTypeInvalidException ( string message ) : base ( message ) { }
    }
    
    public class ScriptEmptyException : OwnExceptionsBase
    {
        public ScriptEmptyException () { }
        public ScriptEmptyException ( string message ) : base ( message ) { }
    }
    
    public class ScriptWrongStructureException : OwnExceptionsBase
    {
        public ScriptWrongStructureException () { }
        public ScriptWrongStructureException ( string message ) : base ( message ) { }
    }
    
    public class MtuIsNotTwowayICException : OwnExceptionsBase
    {
        public MtuIsNotTwowayICException () { }
        public MtuIsNotTwowayICException ( string message ) : base ( message ) { }
    }
    
    public class MtuIsAlreadyTurnedOffICException : OwnExceptionsBase
    {
        public MtuIsAlreadyTurnedOffICException () { }
        public MtuIsAlreadyTurnedOffICException ( string message ) : base ( message ) { }
    }
    
    public class ActionNotAchievedICException : OwnExceptionsBase
    {
        public ActionNotAchievedICException () { }
        public ActionNotAchievedICException ( string message ) : base ( message ) { }
    }
    
    public class AttemptNotAchievedICException : OwnExceptionsBase
    {
        public AttemptNotAchievedICException () { }
        public AttemptNotAchievedICException ( string message ) : base ( message ) { }
    }
}
