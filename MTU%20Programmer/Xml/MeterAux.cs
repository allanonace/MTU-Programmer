using System.Collections.Generic;

namespace Xml
{
    public class MeterAux
    {
        public static bool GetPortTypes ( string portType, out List<string> portTypes )
        {
            portTypes = new List<string>();

            bool isNumeric = int.TryParse(portType, out int portTypeNumber);

            if (isNumeric)
            {
                portTypes.Add(portType); // "3101"
                return true; // numeric
            }
            else if ( IsStringPortType ( portType ) )
                portTypes.Add(portType); // "S4K", "4KL", "GUT", "CH4"

            else if (portType.Contains("|"))
            {
                portTypes.AddRange(portType.Split('|')); // multiple meter types (i.e. "3101|3102|3103")
                return true; // numeric
            }
            else
                foreach (char c in portType)
                    portTypes.Add(c.ToString());

            return false; // string or char
        }

        public static bool IsStringPortType ( string type )
        {
            return type.Equals("S4K") ||
                   type.Equals("4KL") ||
                   type.Equals("GUT") ||
                   type.Equals("CH4");
        }
    }
}
