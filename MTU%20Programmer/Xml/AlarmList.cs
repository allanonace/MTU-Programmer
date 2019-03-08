using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Xml
{
    [XmlRoot("Alarms")]
    public class AlarmList
    {
        [XmlElement("FileVersion")]
        public string FileVersion { get; set; }

        [XmlElement("FileDate")]
        public string FileDate { get; set; }
 
        [XmlElement("Customer")]
        public string Customer { get; set; }

        [XmlElement("Alarm")]
        public List<Alarm> Alarms { get; set; }

        public List<Alarm> FindByMtuType(int mtuType)
        {
            List<Alarm> alarms = Alarms.FindAll(x => (x.MTUType == mtuType));
            if (alarms == null)
            {
                throw new AlarmNotFoundException("Alarms not found");
            }
            return alarms;
        }
    }
}
