using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using PubnubApi;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace IOTApp
{
    [Activity(Label = "IOT App", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        Pubnub pubnub;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            Button yellowButton = FindViewById<Button>(Resource.Id.YellowButton);
            Button redButton = FindViewById<Button>(Resource.Id.RedButton);
            Button blueButton = FindViewById<Button>(Resource.Id.BlueButton);

            yellowButton.Click += delegate { SendMessage(LedColor.Yellow); };
            redButton.Click += delegate { SendMessage(LedColor.Red); };
            blueButton.Click += delegate { SendMessage(LedColor.Blue); };

            // Set up the connection to PubNub for message publishing
            PNConfiguration config = new PNConfiguration();
            config.SubscribeKey = GetPubNubSubscribeKey();
            config.PublishKey = GetPubNubPublishKey();
            config.Secure = false;

            pubnub = new Pubnub(config);
        }

        protected void SendMessage(LedColor ledColor) {
            // Maps the colors to the appropriate GPIO pin number on the Raspberry Pi
            Dictionary<LedColor, int> pins = new Dictionary<LedColor, int>();
            pins.Add(LedColor.Yellow, 23);
            pins.Add(LedColor.Red, 24);
            pins.Add(LedColor.Blue, 25);

            // Creates the appropriate format for the PubNub message to the Raspberry Pi
            Dictionary<string, string> dictMessage = new Dictionary<string, string>();
            dictMessage.Add("pin", pins[ledColor].ToString());

            pubnub.Publish()
                .Channel("Channel-kz4mchwed") //The channel used can be anything, but it must match the channel on the receiver
                .Message(dictMessage)
                .Async(new PNPublishResultExt(
                    (result, status) =>
                    {
                        if (status.Error)
                        {
                            Toast.MakeText(this.ApplicationContext, "Error sending command", ToastLength.Short).Show();
                        }
                        else
                        {
                            Toast.MakeText(this.ApplicationContext, ledColor + " LED toggled", ToastLength.Short).Show();
                        }
                    }
                ));
        }

        public static string GetPubNubSubscribeKey()
        {
            return GetBase("pubnub-subscribe-key");
        }

        public static string GetPubNubPublishKey()
        {
            return GetBase("pubnub-publish-key");
        }

        static string GetBase(string configKey)
        {
            var type = typeof(AppDomainSetup);
            var resource = $"{type.Namespace}.config.xml";

            using (var stream = type.Assembly.GetManifestResourceStream(resource))
            using (var reader = new StreamReader(stream))
            {
                var doc = XDocument.Parse(reader.ReadToEnd());

                return doc.Element("config").Element(configKey).Value;
            }
        }

        public enum LedColor
        {
            Red,
            Yellow,
            Blue
        }
    }
}

