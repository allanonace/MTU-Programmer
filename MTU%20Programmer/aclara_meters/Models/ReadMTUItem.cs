using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aclara_meters.Models
{
    public class ReadMTUItem
    {

        public ReadMTUItem(){
            Title = "-";
            Description = "-";
            isMeter = "false";
            isMTU = "true";
            isDetailMeter = "false";

            Title1 = "-";
            Title2 = "-";
            Title3 = "-";

            Description1 = "-";
            Description2 = "-";
            Description3 = "-";

            isDisplayed = "true";
            Height = "60";
        }

        public string Height
        { get; set; }



        public string Title
        { get; set; }

        public string Description
        { get; set; }

        public string isMeter
        { get; set; }

        public string isDisplayed
        { get; set; }

        public string isMTU
        { get; set; }


        public string isDetailMeter
        { get; set; }

        public string Title1
        { get; set; }

        public string Description1
        { get; set; }

        public string Title2
        { get; set; }

        public string Description2
        { get; set; }

        public string Title3
        { get; set; }

        public string Description3
        { get; set; }


        /*
        public string Title 
        { 
            
            get
            {
                if (Title != null)
                    return Title;
                else
                    return "-";
            }

            set
            {
                Title = value;
            }
        }

        public string Description 
        {
            get
            {
                if (Description != null)
                    return Description;
                else
                    return "-";
            }

            set
            {
                Description = value;
            }
        }

        public string isMeter 
        { 
            get
            {
                if (isMeter != null)
                    return isMeter;
                else
                    return "false";
            }

            set
            {
                isMeter = value;
            } 
        }
        public string isMTU 
        { 
            get
            {
                if (isMTU != null)
                    return isMTU;
                else
                    return "true";
            }

            set
            {
                isMTU = value;
            }
        }

        public string Title1 
        { 
            get
            {
                if (Title1 != null)
                    return Title1;
                else
                    return "-";
            }

            set
            {
                Title1 = value;
            }
        }
        public string Description1 
        { 
            get
            {
                if (Description1 != null)
                    return Description1;
                else
                    return "-";
            }

            set
            {
                Title1 = value;
            }
        }

        public string Title2 
        { 
            get{
                if (Title2 != null)
                    return Title2;
                else
                    return "-";
            }

            set
            {
                Title2 = value;
            }
        }

        public string Description2 
        { 
            get
            {
                if (Description2 != null)
                    return Description2;
                else
                    return "-";
            }

            set
            {
                Description2 = value;
            }
        }

        public string Title3 
        { 
            get
            {
                if (Title3 != null)
                    return Title3;
                else
                    return "-";
            }

            set
            {
                Title3 = value;
            }
        }

        public string Description3 
        { 
            get
            {
                if (Description3 != null)
                    return Description3;
                else
                    return "-";
            }

            set
            {
                Description3 = value;
            }
        }

        public string Title4 
        { 
            get
            {
                if (Title4 != null)
                    return Title4;
                else
                    return "-";
            }

            set
            {
                Title4 = value;
            }

        }

        public string Description4 
        { 
            get
            {
                if (Description4 != null)
                    return Description4;
                else
                    return "-";
            }

            set
            {
                Description4 = value;
            } 
        }
        */
    }
}

