using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ActionType = MTUComm.Action.ActionType;

namespace aclara_meters.Models
{
    public class PageItem
    {
        public string Title { get; set; }
        public string Icon { get; set; }
        public ActionType TargetType { get; set; }
    }
}

