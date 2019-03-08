using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Xml
{
    [XmlRoot("InterfaceConfig")]
    public class InterfaceConfig
    {
        [XmlElement("MtuInterface")]
        public List<MtuInterface> MtuInterfaces { get; set; }

        [XmlElement("Interface")]
        public List<Interface> Interfaces { get; set; }

        public ActionInterface GetInterfaceByMtuIdAndAction(int mtuid, string action)
        {
            MtuInterface mtu = MtuInterfaces.Find(x => x.Id == mtuid);
            if (mtu == null)
            {
                throw new MtuNotFoundException("Mtu not found");
            }

            Interface mtu_interface = Interfaces.Find(x => x.Id == mtu.Interface);

            if (mtu_interface == null)
            {
                throw new InterfaceNotFoundException("Meter not found");
            }

            ActionInterface action_interface = mtu_interface.GetInterfaceActionType(action);

            if (action_interface == null)
            {
                throw new ActionInterfaceNotFoundException("Meter not found");
            }

            return action_interface;
        }
    }
}
