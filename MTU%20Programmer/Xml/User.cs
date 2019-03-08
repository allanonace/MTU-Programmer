using System.Xml.Serialization;

namespace Xml
{
    public class User
    {
        public User ()
        {
            this.Encrypted = true;
        }

        public User (
            string name,
            string pass )
        {
            this.Name = name;
            this.Pass = pass;
        }

        [XmlElement("Name")]
        public string Name { get; set; }
        
        [XmlElement("Pass")]
        public string Pass { get; set; }
        
        [XmlElement("Encrypted")]
        public bool Encrypted { get; set; }
    }
}
