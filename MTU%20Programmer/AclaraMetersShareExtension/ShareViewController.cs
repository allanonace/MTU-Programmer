using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Linq;
using Foundation;
using MobileCoreServices;
using Social;
using UIKit;
using System.Threading;
using System.Net;

namespace AclaraMetersShareExtension
{
    public partial class ShareViewController : SLComposeServiceViewController
    {
        NSExtensionContext extensionContext;
        bool enviado;

        protected ShareViewController(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void DidReceiveMemoryWarning()
        {
            // Releases the view if it doesn't have a superview.
            base.DidReceiveMemoryWarning();

            // Release any cached data, images, etc that aren't in use.
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            extensionContext = this.ExtensionContext;
            // Do any additional setup after loading the view.
            NSExtensionItem[] outputItem = extensionContext.InputItems;
            NSExtensionItem item = extensionContext.InputItems[0];
            NSItemProvider provider = item.Attachments[0];  //provider is null, no attachments

            enviado = false;

            string[] type  = outputItem[0].Attachments[0].RegisteredTypeIdentifiers;

      
          
            foreach (string x in type)
            {
                if ("public.zip-archive".Contains(x))
                {
                    if (!enviado)
                    {

                        enviado = true;

                       

                        var item2 = extensionContext.InputItems[0];

                        NSItemProvider prov = null;
                        if (item2 != null) prov = item2.Attachments[0];
                        if (prov != null)
                        {
                            prov.LoadItem(UTType.ZipArchive, null, (NSObject url, NSError error) =>
                            {
                                if (url == null) return;
                                NSUrl newUrl = (NSUrl)url;

                                InvokeOnMainThread(() =>
                                {
                                    //String encode1 = System.Web.HttpUtility.UrlEncode("thirdpartyappurlscheme://?param=" + rece);

                                    NSUrl request = new NSUrl("aclara-mtu-programmer://?script_path=" + newUrl);

                                    try
                                    {
                                        bool isOpened = UIApplication.SharedApplication.OpenUrl(request);

                                        if (isOpened == false)
                                            UIApplication.SharedApplication.OpenUrl(new NSUrl("aclara-mtu-programmer://?script_path=" + newUrl));
                                        extensionContext.CompleteRequest(new NSExtensionItem[0], null);
                                    }
                                    catch (Exception ex)
                                    {
                                        Debug.WriteLine("Cannot open url: {0}, Error: {1}", request.AbsoluteString, ex.Message);
                                        var alertView = new UIAlertView("Error", ex.Message, null, "OK", null);

                                        alertView.Show();
                                    }
                                });
                            });
                        }
                    }
                }


                if ("public.xml".Contains(x))
                {
                    if (!enviado)
                    {

                        enviado = true;



                        var item2 = extensionContext.InputItems[0];

                        NSItemProvider prov = null;
                        if (item2 != null) prov = item2.Attachments[0];
                        if (prov != null)
                        {
                            prov.LoadItem(UTType.XML, null, (NSObject url, NSError error) =>
                            {

                               


                                if (url == null) return;
                                NSUrl newUrl = (NSUrl)url;


                                byte[] uploadData = (NSData.FromUrl((NSUrl)newUrl).ToArray());


                                // From byte array to string
                                string s = System.Text.Encoding.UTF8.GetString(uploadData, 0, uploadData.Length);


                                s = WebUtility.UrlEncode(s);

                                InvokeOnMainThread(() =>
                                {
                                    //String encode1 = System.Web.HttpUtility.UrlEncode("thirdpartyappurlscheme://?param=" + rece);

                                    NSUrl request = new NSUrl("aclara-mtu-programmer://?script_path=" + s.ToString());

                                    try
                                    {
                                        bool isOpened = UIApplication.SharedApplication.OpenUrl(request);

                                        if (isOpened == false)
                                            UIApplication.SharedApplication.OpenUrl(new NSUrl("aclara-mtu-programmer://?script_path=" + s.ToString()));
                                        extensionContext.CompleteRequest(new NSExtensionItem[0], null);
                                    }
                                    catch (Exception ex)
                                    {
                                        Debug.WriteLine("Cannot open url: {0}, Error: {1}", request.AbsoluteString, ex.Message);
                                        var alertView = new UIAlertView("Error", ex.Message, null, "OK", null);

                                        alertView.Show();
                                    }
                                });
                            });
                        }
                    }
                }




                if ("public.data".Contains(x))
                {
                    if (!enviado)
                    {

                        enviado = true;

                       
                        var item2 = extensionContext.InputItems[0];

                        NSItemProvider prov = null;
                        if (item2 != null) prov = item2.Attachments[0];
                        if (prov != null)
                        {
                            prov.LoadItem(UTType.Data, null, (NSObject url, NSError error) =>
                            {
                                if (url == null) return;
                                NSUrl newUrl = (NSUrl)url;
                                 
                                InvokeOnMainThread(() =>
                                {
                                    //String encode1 = System.Web.HttpUtility.UrlEncode("thirdpartyappurlscheme://?param=" + rece);

                                    NSUrl request = new NSUrl("aclara-mtu-programmer://?script_path=" + newUrl.ToString());

                                    try
                                    {
                                        bool isOpened = UIApplication.SharedApplication.OpenUrl(request);

                                        if (isOpened == false)
                                            UIApplication.SharedApplication.OpenUrl(new NSUrl("aclara-mtu-programmer://?script_path=" + newUrl.ToString()));
                                        extensionContext.CompleteRequest(new NSExtensionItem[0], null);
                                    }
                                    catch (Exception ex)
                                    {
                                        Debug.WriteLine("Cannot open url: {0}, Error: {1}", request.AbsoluteString, ex.Message);
                                        var alertView = new UIAlertView("Error", ex.Message, null, "OK", null);

                                        alertView.Show();
                                    }
                                });
                            });
                        }
                    }
                }



                if ("public.file-url".Contains(x))
                {
                    if (!enviado)
                    {
                        enviado = true;

                        string value = outputItem[0].AttributedContentText.Value.ToString();

                        String encode1 = System.Net.WebUtility.UrlEncode(value);

                        extensionContext.CompleteRequest(new NSExtensionItem[0], null);

                        UIApplication.SharedApplication.OpenUrl(new NSUrl("aclara-mtu-programmer://?script_path=" + encode1));

                    }
                }
            }


        }

        //public override bool IsContentValid()
        //  {
            // Do validation of contentText and/or NSExtensionContext attachments here
        //      return true;
        //   }



        public override SLComposeSheetConfigurationItem[] GetConfigurationItems()
        {
            // To add configuration options via table cells at the bottom of the sheet, return an array of SLComposeSheetConfigurationItem here.
            return new SLComposeSheetConfigurationItem[0];
        }
    }
}
