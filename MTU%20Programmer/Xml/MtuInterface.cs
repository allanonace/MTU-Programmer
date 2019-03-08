using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Xml
{
    public class MtuInterface
    {
        [XmlAttribute("ID")]
        public int Id { get; set; }

        [XmlAttribute("interface")]
        public int Interface { get; set; }
    }
}
