using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Xml
{
    public class InterfaceParameters
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("display")]
        public string Display { get; set; }

        [XmlAttribute("log")]
        public bool Log { get; set; }

        [XmlAttribute("interface")]
        public bool Interface { get; set; }

        [XmlAttribute("index")]
        public int Index{ get; set; }
        
        [XmlAttribute("global")]
        public string Global { get; set; }

        [XmlAttribute("conditional")]
        public string Conditional { get; set; }

        [XmlAttribute("source")]
        public string Source { get; set; }

        [XmlText]
        public string Value { get; set; }

        [XmlElement("Parameter")]
        public List<InterfaceParameters> Parameters { get; set; }
    }
}
