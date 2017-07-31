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
    class DataConfig
    {
        //baseURI  "http://192.168.0.5:88/SCB";
        //methodURIn "/CODE2.PHP";
        public string baseURI { get; set; }
        public string methodURI { get; set; }
        public string SSID { get; set; }
        public string passW { get; set; }
        public string code { get; set; }
    }
}