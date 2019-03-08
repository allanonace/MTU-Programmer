using System.Xml.Serialization;

namespace Xml
{
    [XmlRoot("Registers")]
    public class MemRegisterList
    {
        [XmlElement("Register")]
        public MemRegister[] Registers { get; set; }

        [XmlElement("Overload",IsNullable=true)]
        public MemOverload[] Overloads { get; set; }
    }
}
