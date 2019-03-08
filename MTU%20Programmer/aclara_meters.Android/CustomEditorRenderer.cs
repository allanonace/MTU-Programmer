using System;
using aclara_meters;
using aclara_meters.Droid;
using Android.Content;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Android.Graphics.Drawables;

[assembly: ExportRenderer(typeof(Editor), typeof(CustomEntryRenderer))]
namespace aclara_meters.Droid
{
    public class CustomEntryRenderer : EntryRenderer {
        public CustomEntryRenderer(Context context) : base(context)
        {
            
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e) {
            base.OnElementChanged(e);
            if (Control != null)
            {
                Control.Background = new ColorDrawable(Android.Graphics.Color.Transparent);
                Control.SetPadding(10, 10, 10, 10);
            }
      
        }

    }
}