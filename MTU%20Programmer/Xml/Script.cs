using System;
using System.Collections.Generic;
using System.Xml.Serialization;
namespace Xml
{
    [XmlRoot("MtuScript")]
    public class Script
    {
        [XmlElement("userName")]
        public string UserName { get; set; }

        [XmlElement("logFile")]
        public string LogFile { get; set; }

        [XmlElement("action")]
        public List<ScriptAction> Actions { get; set; }
    }
}
