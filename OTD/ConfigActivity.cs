using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Android.App;
using Android.Content;
using Android.Net.Wifi;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using OTD.Models;

namespace OTD
{
    [Activity(Label = "ConfigActivity")]
    public class ConfigActivity : Activity
    {
        EditText _entryWiFiPass;
        EditText _entryCodeDoor;
        EditText _entryURLDoor;
        TextView _lblWiFiToUseSelected;
        ListView _listView;
        Button _btnSave;
        private ArrayAdapter<WiFiDetail> _adapter;
        private WiFiDetail selected = new WiFiDetail( );
        WifiManager wifiManager;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.Configuracion);
            //OpenDoorButton
            wifiManager = GetSystemService(WifiService).JavaCast<WifiManager>( );

            _entryWiFiPass = FindViewById<EditText>(Resource.Id.EntryPassWiFi);
            _entryCodeDoor = FindViewById<EditText>(Resource.Id.entryCodDoor);
            _entryURLDoor = FindViewById<EditText>(Resource.Id.entryUrlDoor);
            _lblWiFiToUseSelected = FindViewById<TextView>(Resource.Id.lblWiFiToUseSelected);
            _listView = FindViewById<ListView>(Resource.Id.WiFiListView2);
            _btnSave = FindViewById<Button>(Resource.Id.btnSave);
            

            _btnSave.Click += _btnSave_Click;
            _listView.ItemSelected += _listView_ItemSelected;
            _listView.ItemClick += ListViewOnItemClick;
            this.RefreshWifiList( );
        }

        private void ListViewOnItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            selected = _adapter.GetItem(e.Position);
            _lblWiFiToUseSelected.Text = selected.SSID;
        }

        private void _listView_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            selected = _adapter.GetItem(e.Position);
            _lblWiFiToUseSelected.Text = selected.SSID;
        }

        private void _btnSave_Click(object sender, EventArgs e)
        {
            this.Finish( );
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
                    _adapter = new ArrayAdapter<WiFiDetail>(this, Android.Resource.Layout.SimpleListItemChecked,
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