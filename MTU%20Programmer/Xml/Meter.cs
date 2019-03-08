using System;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace Xml
{
    public class Meter
    {
        public bool IsEmpty
        {
            get { return this.LiveDigits <= 0; }
        }

        [XmlAttribute("ID")]
        public int Id { get; set; }

        [XmlElement("Display")]
        public string Display { get; set; }
 
        [XmlElement("Type")]
        public string Type { get; set; }

        [XmlElement("MeterMask")]
        public string MeterMask { get; set; }

        [XmlElement("Utility")]
        public string Utility { get; set; }

        [XmlElement("Vendor")]
        public string Vendor { get; set; }

        [XmlElement("Model")]
        public string Model { get; set; }

        [XmlElement("LiveDigits")]
        public int LiveDigits { get; set; }

        [XmlElement("DummyDigits")]
        public int DummyDigits { get; set; }

        [XmlElement("PaintedDigits")]
        public int PaintedDigits { get; set; }

        [XmlElement("LeadingDummy")]
        public int LeadingDummy { get; set; }

        [XmlElement("Scale")]
        public int Scale { get; set; }

        [XmlElement("Prescaler")]
        public int Prescaler { get; set; }

        [XmlElement("ImmediateAlarmTransmit")]
        public int ImmediateAlarmTransmit { get; set; }

        [XmlElement("DcuUrgentAlarm")]
        public int DcuUrgentAlarm { get; set; }

        [XmlElement("ExternalTamper")]
        public int ExternalTamper { get; set; }

        [XmlElement("InternalTamper")]
        public int InternalTamper { get; set; }

        [XmlElement("ProvingHandFactor")]
        public int ProvingHandFactor { get; set; }

        [XmlElement("WdtPrescalerFollowingEdge")]
        public int WdtPrescalerFollowingEdge { get; set; }

        [XmlElement("MinimumPulseLength")]
        public int MinimumPulseLength { get; set; }

        [XmlElement("EdgePolarity")]
        public int EdgePolarity { get; set; }

        [XmlElement("ReadingType")]
        public int ReadingType { get; set; }

        [XmlElement("EncoderType")]
        public int EncoderType { get; set; }

        [XmlElement("PulseLowTime")]
        public int PulseLowTime { get; set; }

        [XmlElement("PulseHiTime")]
        public int PulseHiTime { get; set; }

        [XmlElement("HiResScaling")]
        public decimal HiResScaling { get; set; }

        [XmlElement("PHF")]
        public int PHF { get; set; }

        [XmlElement("MtuMode")]
        public int MtuMode { get; set; }

        [XmlElement("Flow")]
        public int Flow { get; set; }

        [XmlIgnore]
        public int NumberOfDials {
            get
            {
                Match match = Regex.Match ( this.Display,
                               @"(\w+) (\d+)D PF(\d+) (\w+)",
                               RegexOptions.IgnoreCase | RegexOptions.Singleline |
                               RegexOptions.CultureInvariant | RegexOptions.Compiled );
                               
                if ( match.Success )
                    return int.Parse ( match.Groups[2].Value );
                return -1;
            }
        }

        [XmlIgnore]
        public int DriveDialSize
        {
            get
            {
                Match match = Regex.Match(this.Display,
                               @"(\w+) (\d+)D PF(\d+) (\w+)",
                               RegexOptions.IgnoreCase | RegexOptions.Singleline |
                               RegexOptions.CultureInvariant | RegexOptions.Compiled);
                if (match.Success)
                {
                    return int.Parse ( match.Groups[3].Value );
                }
                return -1;
            }
        }

        [XmlIgnore]
        public string UnitOfMeasure
        {
            get
            {
                Match match = Regex.Match(this.Display,
                               @"(\w+) (\d+)D PF(\d+) (\w+)",
                               RegexOptions.IgnoreCase | RegexOptions.Singleline |
                               RegexOptions.CultureInvariant | RegexOptions.Compiled);
                if (match.Success)
                {
                    return match.Groups[4].Value;
                }
                return string.Empty;
            }
        }

        public String GetProperty(String Name)
        {
            return this.GetType().GetProperty(Name).GetValue(this, null).ToString();
        }
        
        public string ApplyReadingMask (
            string value )
        {
            if ( ! string.IsNullOrEmpty ( this.MeterMask ) &&
                 value.Length < this.LiveDigits )
            {
                string mask  = this.MeterMask.ToLower ();
                int    index = mask.IndexOfAny ( new Char[] { 'x' } );
                if ( index >= 0 )
                {
                    string leadingRead  = mask.Substring ( 0, index );
                    string trailingRead = mask.Substring ( index + 1 );
                    value = leadingRead + value + trailingRead;
                }
            }
            
            return value;
        }

        public string FillLeftLiveDigits (
            string value )
        {
            return value.PadLeft ( this.LiveDigits, '0' );
        }
        
        public string FillLeftNumberOfDials (
            string value )
        {
            return value.PadLeft ( this.NumberOfDials, '0' );
        }
    }
}
