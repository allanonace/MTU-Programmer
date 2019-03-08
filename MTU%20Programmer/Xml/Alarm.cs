using System;
using System.Xml.Serialization;

namespace Xml
{
    public class Alarm
    {
        private const int DEF_CUTWIRE = 0;
        private const int DEF_OVERLAP = 6;
    
        public Alarm ()
        {
            this.CutAlarmCable              = false;
            this.CutWireAlarmImm            = false;
            this.CutWireDelaySetting        = DEF_CUTWIRE;
            //this.DcuUrgentAlarm           = ¿?
            this.ECoderDaysNoFlow           = false;
            this.ECoderDaysOfLeak           = false;
            this.ECoderLeakDetectionCurrent = false;
            this.ECoderReverseFlow          = false;
            //this.ImmediateAlarmTransmit   = ¿?
            //this.ImmediateTransmit        = ¿?
            this.InsufficientMemory         = false;
            this.InsufficientMemoryImm      = false;
            this.InterfaceTamper            = true;
            this.InterfaceTamperImm         = false;
            //this.IntervalData             = ¿?
            this.LastGasp                   = false;
            this.LastGaspImm                = false;
            this.Magnetic                   = true;
            this.Overlap                    = DEF_OVERLAP; // [1-11]
            this.RegisterCover              = true;
            this.ReverseFlow                = true;
            this.SerialComProblem           = false;
            this.SerialComProblemImm        = false;
            this.SerialCutWire              = false;
            this.SerialCutWireImm           = false;
            this.Tilt                       = true;
            this.TamperPort1                = false;
            this.TamperPort2                = false;
            this.TamperPort1Imm             = false;
            this.TamperPort2Imm             = false;
        }

        #region Elements

        [XmlAttribute("MTUType")]
        public int MTUType { get; set; }

        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlElement("IntervalData")]
        public bool IntervalData { get; set; }

        [XmlIgnore]
        public byte CutWireDelaySetting { get; set; }
        
        [XmlElement("CutWireDelaySetting")]
        public string CutWireDelaySetting_AllowEmptyField
        {
            get { return this.CutWireDelaySetting.ToString (); }
            set
            {
                if ( ! string.IsNullOrEmpty ( value ) )
                {
                    byte v;
                    if ( byte.TryParse ( value, out v ) )
                         this.CutWireDelaySetting = v;
                    else this.CutWireDelaySetting = DEF_CUTWIRE;
                }
                else this.CutWireDelaySetting = DEF_CUTWIRE;
            }
        }
        
        [XmlIgnore]
        public int Overlap { get; set; }
        
        [XmlElement("Overlap")]
        public string Overlap_AllowEmptyField
        {
            get { return this.Overlap.ToString (); }
            set
            {
                if ( ! string.IsNullOrEmpty ( value ) )
                {
                    int v;
                    if ( int.TryParse ( value, out v ) )
                         this.Overlap = v;
                    else this.Overlap = DEF_OVERLAP;
                }
                else this.Overlap = DEF_OVERLAP;
            }
        }

        #region Tampers

        [XmlElement("CutAlarmCable")]
        public bool CutAlarmCable { get; set; }
        
        [XmlElement("CutWireAlarmImm")]
        public bool CutWireAlarmImm { get; set; }
        
        [XmlElement("DcuUrgentAlarm")]
        public bool DcuUrgentAlarm { get; set; }

        [XmlElement("ECoderDaysNoFlow")]
        public bool ECoderDaysNoFlow { get; set; }

        [XmlElement("ECoderDaysOfLeak")]
        public bool ECoderDaysOfLeak { get; set; }

        [XmlElement("ECoderLeakDetectionCurrent")]
        public bool ECoderLeakDetectionCurrent { get; set; }

        [XmlElement("ECoderReverseFlow")]
        public bool ECoderReverseFlow { get; set; }

        [XmlElement("ImmediateAlarmTransmit")]
        public bool ImmediateAlarmTransmit { get; set; }

        [XmlElement("ImmediateTransmit")]
        public bool ImmediateTransmit { get; set; }

        [XmlElement("InsufficientMemory")]
        public bool InsufficientMemory { get; set; }

        [XmlElement("InsufficientMemoryImm")]
        public bool InsufficientMemoryImm { get; set; }
        
        [XmlElement("InterfaceTamper")]
        public bool InterfaceTamper { get; set; }

        [XmlElement("InterfaceTamperImm")]
        public bool InterfaceTamperImm { get; set; }

        [XmlElement("LastGasp")]
        public bool LastGasp { get; set; }

        [XmlElement("LastGaspImm")]
        public bool LastGaspImm { get; set; }

        [XmlElement("Magnetic")]
        public bool Magnetic { get; set; }
        
        [XmlElement("RegisterCover")]
        public bool RegisterCover { get; set; }
        
        [XmlElement("ReverseFlow")]
        public bool ReverseFlow { get; set; }
        
        [XmlElement("SerialComProblem")]
        public bool SerialComProblem { get; set; }

        [XmlElement("SerialComProblemImm")]
        public bool SerialComProblemImm { get; set; }

        [XmlElement("SerialCutWire")]
        public bool SerialCutWire { get; set; }

        [XmlElement("SerialCutWireImm")]
        public bool SerialCutWireImm { get; set; }
        
        [XmlElement("Tilt")]
        public bool Tilt { get; set; }

        [XmlElement("TamperPort1")]
        public bool TamperPort1 { get; set; }

        [XmlElement("TamperPort2")]
        public bool TamperPort2 { get; set; }

        [XmlElement("TamperPort1Imm")]
        public bool TamperPort1Imm { get; set; }

        [XmlElement("TamperPort2Imm")]
        public bool TamperPort2Imm { get; set; }

        #endregion

        #endregion
    }
}
