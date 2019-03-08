using System.Xml.Serialization;

namespace Xml
{
    [XmlRoot("Errors")]
    public class ErrorList
    {
        [XmlElement("Error")]
        public Error[] List { get; set; }
    }
}
