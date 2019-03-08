using System;
using aclara_meters;
using aclara_meters.Droid;
using Android.Content;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Android.Graphics.Drawables;
using Android.Views;
using View = Android.Views.View;

[assembly: ExportRenderer(typeof(ViewCellTransparent), typeof(ViewCellTransparentRenderer))]
namespace aclara_meters.Droid
{
    public class ViewCellTransparentRenderer : ViewCellRenderer
    {
       
        protected override View GetCellCore(Cell item, View convertView, ViewGroup parent, Context context)
        {
            var cell = base.GetCellCore(item, convertView, parent, context);
            var listView = parent as Android.Widget.ListView;

            if (listView != null)
            {
                // Disable native cell selection color style - set as *Transparent*
                listView.SetSelector(Android.Resource.Color.Transparent);
                listView.CacheColorHint = Android.Graphics.Color.Transparent;
            }

            return cell;
        }
    }
}
