using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Xml
{
    public class ActionInterface
    {
        [XmlAttribute("type")]
        public string Type { get; set; }

        [XmlElement("Parameter")]
        public List<InterfaceParameters> Parameters { get; set; }


        public InterfaceParameters[] getLogInterfaces()
        {
            List<InterfaceParameters> parameters = Parameters.FindAll(x => x.Log == true);
            //parameters.Sort(new Comparison<InterfaceParameters>((x, y) => x.Index - y.Index));
            return parameters.ToArray();
        }

        public InterfaceParameters[] getUserInterfaces()
        {
            List<InterfaceParameters> parameters = Parameters.FindAll(x => x.Interface == true);
            //parameters.Sort(new Comparison<InterfaceParameters>((x, y) => x.Index - y.Index));
            return parameters.ToArray();
        }

        public InterfaceParameters[] getAllInterfaces()
        {
            //Parameters.Sort(new Comparison<InterfaceParameters>((x, y) => x.Index - y.Index));
            return Parameters.ToArray();
        }

    }
}
