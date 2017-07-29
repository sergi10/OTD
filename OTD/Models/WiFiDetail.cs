using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace OTD.Models
{
    public class WiFiDetail
    {

        public string SSID { get; set; }
        public string Encryption { get; set; }

        public override string ToString( )
        {
            return SSID + " : " + Encryption;
        }

    }
}