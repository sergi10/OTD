using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Net.Wifi;
using System.Threading;
using OTD.Models;
using System.Linq;
using System.Collections.Generic;

namespace OTD
{
    [Activity(Label = "@string/Title", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private ListView _listView;
        private ArrayAdapter<WiFiDetail> _adapter;
        private WiFiDetail selected = new WiFiDetail( );
        WifiManager wifiManager;
        TextView _labelSsid;
        TextView _labelCrypt;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            _listView = FindViewById<ListView>(Resource.Id.WiFiListView);
            _labelSsid = FindViewById<TextView>(Resource.Id.LaBelSSID);
            _labelCrypt = FindViewById<TextView>(Resource.Id.LabelCrypt);
            wifiManager = GetSystemService(WifiService).JavaCast<WifiManager>( );
            RefreshWifiList( );

            _listView.ItemClick += ListViewOnItemClick;

        }

        private void AddNetwork( )
        {
            var networkSSID = selected.SSID;
            var networkPass = "Overflow";
            var config = new WifiConfiguration( );
            config.Ssid = '"' + networkSSID + '"';

            // For WPA/WPA2, WEP is different (still using WEP? shame on you ;-)
            config.PreSharedKey = '"' + networkPass + '"';
            wifiManager.AddNetwork(config);
        }

        //private void Connect2Network()
        //{
        //    IList<WifiConfiguration> myWifi = wifiManager.ConfiguredNetworks;
        //    wifiManager.Disconnect( );
        //    //wifiManager.EnableNetwork(myWifi.FindFirst(x => x.Ssid.Contains(networkSSID)), true);
        //    //wifiManager.EnableNetwork(myWifi.Where(x => x.Ssid.Contains(selected.SSID)).FirstOrDefault(), true);
        //    wifiManager.Reconnect( );
        //}


        private void ListViewOnItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            selected = _adapter.GetItem(e.Position);
            _labelSsid.Text = selected.SSID;
            _labelCrypt.Text = selected.Encryption;
        }

        private void RefreshWifiList( )
        {

            if (!wifiManager.IsWifiEnabled)
                wifiManager.SetWifiEnabled(true);
            wifiManager.StartScan( );

            ThreadPool.QueueUserWorkItem(lol =>
            {
                Thread.Sleep(TimeSpan.FromSeconds(3));

                var wifiList = wifiManager.ScanResults;

                if (null == _adapter)
                {
                    _adapter = new ArrayAdapter<WiFiDetail>(this, Android.Resource.Layout.SimpleListItemSingleChoice,
                                                       Android.Resource.Id.Text1);
                    RunOnUiThread(( ) => _listView.Adapter = _adapter);
                }

                if (_adapter.Count > 0)
                {
                    RunOnUiThread(( ) => _adapter.Clear( ));
                }


                foreach (var wifi in wifiList)
                {
                    var wifi1 = wifi;
                    RunOnUiThread(( ) => _adapter.Add(new WiFiDetail { SSID = wifi1.Ssid, Encryption = wifi1.Capabilities }));
                }

                RunOnUiThread(( ) => _adapter.NotifyDataSetChanged( ));
            });
        }

    }
}

