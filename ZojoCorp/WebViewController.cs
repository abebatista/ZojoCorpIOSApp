using System;
using Foundation;
using UIKit;
using WebKit;
using CoreLocation;
using System.Threading;
using System.Threading.Tasks;
using ObjCRuntime;

namespace ZojoCorp
{
	public class WebNavigationDelegate : WKNavigationDelegate
	{

    	public override void DecidePolicy(WKWebView webView, WKNavigationAction navigationAction, Action<WKNavigationActionPolicy> decisionHandler)
		{
			var url = navigationAction.Request.Url;

			if (url.Scheme == "tel")
			{
				var preUrl = "telprompt:";
				var postUrl = url.ResourceSpecifier;
				var newUrl = preUrl + postUrl;
				if (UIApplication.SharedApplication.CanOpenUrl(new NSUrl(newUrl)))
				{
					UIApplication.SharedApplication.OpenUrl(new NSUrl(newUrl));
				}
			}
			decisionHandler(WKNavigationActionPolicy.Allow);
			/*
			return;
			if (navigationAction.NavigationType == WKNavigationType.LinkActivated)
			{
				if (UIApplication.SharedApplication.CanOpenUrl(new NSUrl(navigationAction.Request.Url.ToString())))
				{
					UIApplication.SharedApplication.OpenUrl(new NSUrl(navigationAction.Request.Url.ToString()));
				}
			}
			decisionHandler(WKNavigationActionPolicy.Allow);

			/*
			if (true) //Whatever your test happens to be
			{
				decisionHandler(WKNavigationActionPolicy.Allow);
			}
			else
			{
				decisionHandler(WKNavigationActionPolicy.Cancel);
			}
			*/
		}

	}


	public partial class WebViewController : UIViewController
	{
		protected WKWebView webView;

		static bool UserInterfaceIdiomIsPhone
		{
			get { return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone; }
		}

		protected WebViewController(IntPtr handle) : base(handle)
		{
			// Note: this .ctor should not contain any initialization logic.
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			webView = new WKWebView(View.Frame, new WKWebViewConfiguration());
			var url = new NSUrl("https://zojocorp.com");
			//var url = new NSUrl("https://maps.google.com");
			var request = new NSUrlRequest(url);
			webView.NavigationDelegate = new WebNavigationDelegate();
			webView.LoadRequest(request);
			View.AddSubview(webView);
		}

		public override void DidReceiveMemoryWarning()
		{
			base.DidReceiveMemoryWarning();
			// Release any cached data, images, etc that aren't in use.
		}

		bool HandleShouldStartLoad(UIWebView webView, NSUrlRequest request, UIWebViewNavigationType navigationType)
		{
			// If the URL is not our own custom scheme, just let the webView load the URL as usual
			const string scheme = "hybrid:";

			if (request.Url.Scheme != scheme.Replace(":", ""))
				return true;

			// This handler will treat everything between the protocol and "?"
			// as the method name.  The querystring has all of the parameters.
			var resources = request.Url.ResourceSpecifier.Split('?');
			var method = resources[0];
			var parameters = System.Web.HttpUtility.ParseQueryString(resources[1]);

			if (method == "UpdateLabel")
			{
				var textbox = parameters["textbox"];

				// Add some text to our string here so that we know something
				// happened on the native part of the round trip.
				var prepended = string.Format("C# says: {0}", textbox);

				// Build some javascript using the C#-modified result
				var js = string.Format("SetLabelText('{0}');", prepended);

				webView.EvaluateJavascript(js);
			}

			return false;
		}
	}
}

