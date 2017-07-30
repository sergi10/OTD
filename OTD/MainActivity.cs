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
using Android.Net;

namespace OTD
{
    [Activity(Label = "@string/Title", MainLauncher = true, Icon = "@drawable/remoteorange")]
    public class MainActivity : Activity
    {
        private ListView _listView;
        private ArrayAdapter<WiFiDetail> _adapter;
        private WiFiDetail selected = new WiFiDetail( );
        WifiManager wifiManager;
        TextView _labelSsid;
        TextView _labelCrypt;
        ImageButton _IconButton;
        Button _ConfigButton;
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
            _IconButton = FindViewById<ImageButton>(Resource.Id.OpenDoorButton);
            _ConfigButton = FindViewById<Button>(Resource.Id.btnConfig);

            //OpenDoorButton
            wifiManager = GetSystemService(WifiService).JavaCast<WifiManager>( );

            _IconButton.Click += _IconButton_Click;
            _ConfigButton.Click += _ConfigButton_Click;
            _listView.ItemClick += ListViewOnItemClick;

        }

        private void _ConfigButton_Click(object sender, EventArgs e)
        {
            var Intent = new Android.Content.Intent(this, typeof(ConfigActivity));
            StartActivity(Intent);
        }

        private void _IconButton_Click(object sender, EventArgs e)
        {
            _IconButton.SetImageResource(Resource.Drawable.remotegray);
            RefreshWifiList( );
            _IconButton.Enabled = false;
        }
        #region WiFi MANGEMENT
        private bool AddNetwork( )
        {
            bool result = false;
            try
            {
                var networkPass = "detrasdelapuertaestaelsaber";
                var config = new WifiConfiguration( );
                // Solo para  WPA/WPA2, WEP es diferente
                config.PreSharedKey = '"' + networkPass + '"';
                config.Ssid = '"' + selected.SSID + '"';

                wifiManager.AddNetwork(config);

                result = true;
            }
            catch (Exception)
            {
                throw;
            }
            return result;
        }

        private bool Connect2Network( )
        {
            var context = this.BaseContext;
            bool result = false;
            try
            {
                IList<WifiConfiguration> myWifiList = wifiManager.ConfiguredNetworks;
                wifiManager.Disconnect( );
                WifiConfiguration myWiFi = myWifiList.Where(x => x.Ssid.Contains(selected.SSID)).FirstOrDefault( );
                wifiManager.EnableNetwork(myWiFi.NetworkId, true);
                wifiManager.Reconnect( );

                //ConnectivityManager cm = (ConnectivityManager)context.GetSystemService(Context.ConnectivityService);
                //NetworkInfo networkInfo = cm.ActiveNetworkInfo;

                //if (getCurrentSsid(context) == selected.SSID)
                //    var currentb = networkInfo.ExtraInfo.ToString( );

                WifiInfo info = wifiManager.ConnectionInfo;
                var current = info.SSID;
                var uno = info.SupplicantState;
                var dos = info.NetworkId;
                var tres = info.DescribeContents( );
                var cuatro = getCurrentSsid(context);

                if (cuatro == selected.SSID)
                {
                    result = true;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return result;
        }

        private static String getCurrentSsid(Context context)
        {
            String ssid = null;
            ConnectivityManager cm = (ConnectivityManager)context.GetSystemService(Context.ConnectivityService);
            NetworkInfo networkInfo = cm.ActiveNetworkInfo;
            if (networkInfo == null)
            {
                return null;
            }

            if (networkInfo.IsConnected)
            {
                WifiManager wifiManager = (WifiManager)context.GetSystemService(Context.WifiService);
                WifiInfo connectionInfo = wifiManager.ConnectionInfo;
                if (connectionInfo != null && !String.IsNullOrEmpty(connectionInfo.SSID))
                {
                    ssid = connectionInfo.SSID;
                    var a = ssid.Trim('\"');
                    var b = ssid.Trim('"');
                    var c = a.ToString( );
                    var d = b.ToString( );
                }
            }

            return ssid;
        }
        #endregion
        private void ListViewOnItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            selected = _adapter.GetItem(e.Position);
            _labelSsid.Text = selected.SSID;
            _labelCrypt.Text = selected.Encryption;
            //ThreadPool.QueueUserWorkItem(lol2 =>
            //{
            if (AddNetwork( ))
            {
                if (Connect2Network( ))
                {
                    _labelSsid.Text += "  --> CONECTADO";
                    RunOnUiThread(( ) => _IconButton.SetImageResource(Resource.Drawable.remotegreen));
                    //Thread.Sleep(TimeSpan.FromSeconds(3));
                    //var activity = (Activity)this.BaseContext;
                    this.FinishAffinity( );
                    // Android.OS.Process.KillProcess(Android.OS.Process.MyPid( ));

                }
                else
                {
                    _labelSsid.Text = " ERROR!!! \n SIN CONEXION";
                    _IconButton.SetImageResource(Resource.Drawable.remotered);
                    //RunOnUiThread(( ) => ));
                    //RunOnUiThread(( ) => _IconButton.Notify( ));

                    Thread.Sleep(TimeSpan.FromSeconds(3));
                    selected = null;
                    //RunOnUiThread(( ) => ));
                    //RunOnUiThread(( ) => _IconButton.Notify( ));

                    //_IconButton.SetImageResource(Resource.Drawable.remotewhite);
                    _IconButton.Enabled = true;
                    _IconButton.SetImageResource(Resource.Drawable.remotewhite);

                    //this.FinishAffinity( );
                }
            }
            //});
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

