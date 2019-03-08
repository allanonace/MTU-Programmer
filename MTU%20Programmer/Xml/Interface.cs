using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Xml
{
    public class Interface
    {
        [XmlAttribute("ID")]
        public int Id { get; set; }

        [XmlAttribute("memorymap")]
        public string Memorymap { get; set; }

        [XmlAttribute("memorysize")]
        public int MemorymapSize { get; set; }

        [XmlElement("Action")]
        public List<ActionInterface> Actions { get; set; }


        public ActionInterface GetInterfaceActionType(string action)
        {
            ActionInterface action_interface = Actions.Find(x => x.Type.Equals(action));

            if (action_interface == null)
            {
                throw new ActionInterfaceNotFoundException("Meter not found");
            }

            return action_interface;
        }
    }
}
