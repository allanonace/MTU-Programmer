using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Xml
{
    [XmlRoot("MtuTypes")]
    public class MtuTypes
    {
        [XmlElement("FileVersion")]
        public string FileVersion { get; set; }

        [XmlElement("FileDate")]
        public string FileDate { get; set; }

        [XmlElement("Mtu")]
        public List<Mtu> Mtus { get; set; }

        public Mtu FindByMtuId(int mtuId)
        {
            return Mtus.Find ( x => x.Id == mtuId );
        }
    }
}
