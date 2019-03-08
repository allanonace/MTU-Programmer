using System;
using System.ComponentModel;
using aclara_meters;
using aclara_meters.iOS;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(ViewCellTransparent), typeof(ViewCellTransparentRenderer))]
namespace aclara_meters.iOS
{
    public class ViewCellTransparentRenderer : ViewCellRenderer
    {
        public override UITableViewCell GetCell(Cell item, UITableViewCell reusableCell, UITableView tv)
        {

            var cell = base.GetCell(item, reusableCell, tv);
            if (cell != null)
            {
                // Disable native cell selection color style - set as *Transparent*
                cell.SelectionStyle = UITableViewCellSelectionStyle.None;

            }
            return cell;
        }
    }
}
