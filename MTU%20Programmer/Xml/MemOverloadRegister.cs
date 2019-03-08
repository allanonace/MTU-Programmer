using System.Xml.Serialization;

namespace Xml
{
    public sealed class MemOverloadRegister
    {
        [XmlText]
        public string Id { get; set; }
    }
}
