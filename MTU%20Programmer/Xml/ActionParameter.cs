using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;

namespace Xml
{
    public class ActionParameter
    {
        [XmlAttribute]
        public int Port { get; set; }

        [XmlText]
        public string Value { get; set; }

    }
}
