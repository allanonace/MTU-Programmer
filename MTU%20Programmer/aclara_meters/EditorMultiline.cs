using System;
using Xamarin.Forms;
using System.Collections.Generic;
using System.Text;
namespace aclara_meters
{
    public class EditorMultiline : Editor
    {
        public EditorMultiline()
        {
            this.TextChanged += (sender, e) =>
            {
                this.InvalidateMeasure();
            };
        }
    }
}


