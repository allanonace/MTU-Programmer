using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace aclara_meters.XML
{
    public class XmlElementList
    {
        [XmlRoot(ElementName = "user")]
        public class user
        {
            [XmlAttribute(AttributeName = "name")]
            public string name { get; set; }
            [XmlAttribute(AttributeName = "pass")]
            public string pass { get; set; }
        }

        [XmlRoot(ElementName = "Users")]
        public class Users
        {
            [XmlElement(ElementName = "user")]
            public List<user> user { get; set; }
        }

       
    }

}