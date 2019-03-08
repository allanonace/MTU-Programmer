using System.Xml.Serialization;

namespace Xml
{
    [XmlRoot("Users")]
    public class UserList
    {
        [XmlElement("User")]
        public User[] List { get; set; }
    }
}
