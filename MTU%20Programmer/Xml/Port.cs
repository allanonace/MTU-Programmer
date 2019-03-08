using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace Xml
{
    public class Port
    {
        [XmlAttribute("Number")]
        public int Number { get; set; }

        [XmlElement("Description")]
        public string Description { get; set; }
 
        [XmlElement("Type")]
        public string Type { get; set; }

        [XmlElement("Tamper")]
        public string TamperSerialize { get; set; }

        [XmlIgnore]
        public byte Tamper
        {
            get
            {
                byte tamper = 0;
                if (TamperSerialize.StartsWith("int"))
                {
                    tamper = 4;
                }
                else if (TamperSerialize.StartsWith("ext"))
                {
                    tamper = 8;
                }
                else
                {
                    // do nothing
                }
                return tamper;
            }
        }

        public String GetProperty(String Name)
        {
            return this.GetType().GetProperty(Name).GetValue(this, null).ToString();
        }

        public List<string> GetPortTypes ( out bool isNumeric )
        {
            List<string> types;
            isNumeric = MeterAux.GetPortTypes ( this.Type, out types );

            return types;
        }
        
        public List<string> GetPortTypes ()
        {
            bool isNumeric;
            return GetPortTypes ( out isNumeric );
        }
    }
}
