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
using System.Net;
using System.Collections.Specialized;
using static Android.Util.Xml;
using System.Reflection;
using System.IO;
using Android.Content.Res;
using Newtonsoft.Json;

namespace OTD
{
    [Activity(Label = "@string/Title", MainLauncher = true, Icon = "@drawable/remotewhite")]
    public class MainActivity : Activity
    {
        private WiFiDetail selected = new WiFiDetail();
        private DataConfig dataConfig = new DataConfig();
        WifiManager wifiManager;
        TextView _labelSsid;
        ImageButton _IconButton;
        Button _ConfigButton;
        Button _ExitButton;
        private string FileConfig;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            FileConfig= "otd_conf.json";
            //_listView = FindViewById<ListView>(Resource.Id.WiFiListView);
            _labelSsid = FindViewById<TextView>(Resource.Id.LaBelSSID);
            _IconButton = FindViewById<ImageButton>(Resource.Id.OpenDoorButton);
            _ConfigButton = FindViewById<Button>(Resource.Id.btnConfig);
            _ExitButton = FindViewById<Button>(Resource.Id.btnExit);

            wifiManager = GetSystemService(WifiService).JavaCast<WifiManager>();

            _IconButton.Click += _IconButton_Click;
            _ConfigButton.Click += _ConfigButton_Click;
            _ExitButton.Click += _ExitButton_Click;
            //_listView.ItemClick += ListViewOnItemClick;
            //if (!this.ReadConfig())
            //{
            //    _labelSsid.Text = "ERROR EN FICHERO DE CONFIGURACION";
            //    _IconButton.Enabled = false;
            //    _ConfigButton.Enabled = false;
            //    _ExitButton.Enabled = true;
            //}
        }

        private void _ExitButton_Click(object sender, EventArgs e)
        {
            //var activity = (Activity)this.BaseContext;
            //this.FinishAffinity();
            Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
        }

        private void _ConfigButton_Click(object sender, EventArgs e)
        {
            var Intent = new Android.Content.Intent(this, typeof(ConfigActivity));
            Intent.PutExtra("currentConfig", JsonConvert.SerializeObject(dataConfig));
            StartActivity(Intent);
        }

        private void _IconButton_Click(object sender, EventArgs e)
        {
            _IconButton.SetImageResource(Resource.Drawable.remotegray);
            //RefreshWifiList( );
            //_IconButton.Enabled = false;
            //_IconButton.Enabled = Hodoor( );
            //this.ReadConfig();
            if (!this.ReadConfig())
            {
                _labelSsid.Text = "ERROR EN FICHERO DE CONFIGURACION";
                _IconButton.Enabled = false;
                _ConfigButton.Enabled = false;
                _ExitButton.Enabled = true;
            }
        }

        private bool ReadConfig()
        {
            bool result = false;

            using (StreamReader sr = new StreamReader(Assets.Open(FileConfig)))
            {
                try
                {
                    var json = sr.ReadToEnd();
                    dataConfig = JsonConvert.DeserializeObject<DataConfig>(json);

                    if (dataConfig != null)
                    {
                        result = true;
                    }
                }
                catch (Exception e)
                {
                    _labelSsid.Text = e.Message;
                }
            }
            return result;
        }
        //private bool WriteConfig()
        //{
        //    bool result = false;

        //    dataConfig.baseURI = "baseUri Cambiada";
        //    string json = JsonConvert.SerializeObject(dataConfig);
        //    string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        //    string filePath = Path.Combine(path, FileConfig);
        //    using (var file = File.Open(filePath, FileMode.Create, FileAccess.Write))
        //    using (var strm = new StreamWriter(file))
        //    {
        //        strm.Write(json);
        //        result = true;
        //    }
        //    return result;
        //}


        #region WiFi MANGEMENT

        private bool AddNetwork()
        {
            bool result = false;
            if (dataConfig != null)
            {
                try
                {
                    var networkPass = dataConfig.passW;
                    var config = new WifiConfiguration();
                    // Solo para  WPA/WPA2, WEP es diferente
                    config.PreSharedKey = '"' + dataConfig.passW + '"';
                    config.Ssid = '"' + dataConfig.SSID + '"';

                    wifiManager.AddNetwork(config);
                    result = true;
                }
                catch (Exception e)
                {
                    _labelSsid.Text = e.Message;
                }
            }
            return result;
        }

        private bool Connect2Network()
        {
            var context = this.BaseContext;
            bool result = false;
            try
            {
                IList<WifiConfiguration> myWifiList = wifiManager.ConfiguredNetworks;
                wifiManager.Disconnect();
                WifiConfiguration myWiFi = myWifiList.Where(x => x.Ssid.Contains(dataConfig.SSID)).FirstOrDefault();
                wifiManager.EnableNetwork(myWiFi.NetworkId, true);
                wifiManager.Reconnect();

                Thread.Sleep(TimeSpan.FromSeconds(3));
                var current = getCurrentSsid(context);
                WifiInfo info = wifiManager.ConnectionInfo;
                var currentWI = info.SSID.Trim('"');

                if (current == dataConfig.SSID || currentWI == dataConfig.SSID)
                {
                    result = true;
                }
            }
            catch (Exception e)
            {
                _labelSsid.Text = e.Message;
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
                    ssid = ssid.Trim('"');
                }
            }
            return ssid;
        }
        #endregion

        //private void ListViewOnItemClick(object sender, AdapterView.ItemClickEventArgs e)
        //{
        //    selected = _adapter.GetItem(e.Position);
        //    _labelSsid.Text = selected.SSID;
        //    //ThreadPool.QueueUserWorkItem(lol2 =>
        //    //{
        //    if (AddNetwork( ))
        //    {
        //        if (Connect2Network( ))
        //        {
        //            _labelSsid.Text += "  --> CONECTADO";
        //            //RunOnUiThread(( ) => _IconButton.SetImageResource(Resource.Drawable.remotegreen));
        //            _IconButton.SetImageResource(Resource.Drawable.remotegreen);
        //            _ConfigButton.Visibility = Android.Views.ViewStates.Gone;
        //            _ExitButton.Visibility = Android.Views.ViewStates.Visible;
        //            Thread.Sleep(TimeSpan.FromSeconds(2));
        //        }
        //        else
        //        {
        //            _labelSsid.Text = " ERROR!!! \n SIN CONEXION";
        //            _IconButton.SetImageResource(Resource.Drawable.remotered);
        //            selected = null;
        //            _IconButton.Enabled = true;
        //        }
        //    }
        //}

        //private void RefreshWifiList( )
        //{

        //    if (!wifiManager.IsWifiEnabled)
        //        wifiManager.SetWifiEnabled(true);
        //    wifiManager.StartScan( );

        //    ThreadPool.QueueUserWorkItem(lol =>
        //    {
        //        Thread.Sleep(TimeSpan.FromSeconds(3));

        //        var wifiList = wifiManager.ScanResults;

        //        if (null == _adapter)
        //        {
        //            _adapter = new ArrayAdapter<WiFiDetail>(this, Android.Resource.Layout.SimpleListItemSingleChoice,
        //                                               Android.Resource.Id.Text1);
        //            RunOnUiThread(( ) => _listView.Adapter = _adapter);
        //        }

        //        if (_adapter.Count > 0)
        //        {
        //            RunOnUiThread(( ) => _adapter.Clear( ));
        //        }


        //        foreach (var wifi in wifiList)
        //        {
        //            var wifi1 = wifi;
        //            RunOnUiThread(( ) => _adapter.Add(new WiFiDetail { SSID = wifi1.Ssid, Encryption = wifi1.Capabilities }));
        //        }

        //        RunOnUiThread(( ) => _adapter.NotifyDataSetChanged( ));
        //    });
        //}


        private bool Hodoor()
        {
            bool result = false;
            using (var client = new WebClient())
            {
                var a = 50;
                var b = 100;
                //var response = client.DownloadString(string.Format("http://example.com/add.php?a={0}&b={1}", a, b));
                var response2 = client.DownloadString(string.Format("http://httpbin.org/get"));

                //var respuesta =  client.PostAsync(string.Format("http://httpbin.org/get");
                //System.Diagnostics.Debug.WriteLine("\n\nO_Transaction Request for " + request.@params.model.ToUpper( ));
                //if (respuesta.StatusCode == HttpStatusCode.OK)
                //{
                //var response3= client.DownloadString(string.Format("http://httpbin.org/post"));
                //Console.WriteLine(response);
                Console.WriteLine(response2);
                //Console.WriteLine(response3);
            }

            //string URI = "http://www.myurl.com/post.php";
            //string myParameters = "param1=value1&param2=value2&param3=value3";
            string URI = " https://httpbin.org/get";
            string myParameters = "?show_env=1";

            using (WebClient wc = new WebClient())
            {
                //wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                string HtmlResult = wc.UploadString(URI, myParameters);
                Console.WriteLine(HtmlResult);

                //var reqparm = new System.Collections.Specialized.NameValueCollection( );
                //reqparm.Add("param1", "<any> kinds & of = ? strings");
                //reqparm.Add("param2", "escaping is already handled");
                //byte[ ] responsebytes = wc.UploadValues("http://localhost", "POST", reqparm);
                //Console.WriteLine(responsebytes.ToString());
                //string responsebody = Encoding.UTF8.GetString(responsebytes);
            }


            return result;
        }
    }
}

