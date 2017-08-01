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
    [Activity(Label = "ConfigActivity")]
    public class ConfigActivity : Activity
    {
        EditText _entryWiFiPass;
        EditText _entryCodeDoor;
        EditText _entryURLDoor;
        TextView _lblWiFiToUseSelected;
        ListView _listView;
        Button _btnSave;
        private DataConfig dataConfig = new DataConfig();
        public string FileConfig = "otd_conf.json";
        private ArrayAdapter<WiFiDetail> _adapter;
        private WiFiDetail selected = new WiFiDetail();
        WifiManager wifiManager;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            wifiManager = GetSystemService(WifiService).JavaCast<WifiManager>();
            string confit_txt = Intent.GetStringExtra("currentConfig");
            if (!String.IsNullOrEmpty(confit_txt))
            {
                dataConfig = JsonConvert.DeserializeObject<DataConfig>(confit_txt);
            }
            else
            {
                this.Finish();
            }
            // Create your application here
            SetContentView(Resource.Layout.Configuracion);


            _entryWiFiPass = FindViewById<EditText>(Resource.Id.EntryPassWiFi);
            _entryCodeDoor = FindViewById<EditText>(Resource.Id.entryCodDoor);
            _entryURLDoor = FindViewById<EditText>(Resource.Id.entryUrlDoor);
            _lblWiFiToUseSelected = FindViewById<TextView>(Resource.Id.lblWiFiToUseSelected);
            _listView = FindViewById<ListView>(Resource.Id.WiFiListView2);
            _btnSave = FindViewById<Button>(Resource.Id.btnSave);


            _btnSave.Click += _btnSave_Click;
            //_listView.ItemSelected += _listView_ItemSelected;
            _listView.ItemClick += ListViewOnItemClick;

            if (dataConfig != null)
            {
                _entryWiFiPass.Text = dataConfig.passW;
                _entryCodeDoor.Text = dataConfig.code;
                _entryURLDoor.Text = dataConfig.baseURI;
                _lblWiFiToUseSelected.Text = dataConfig.SSID;
            }
            this.RefreshWifiList();
        }

        private void ListViewOnItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            selected = _adapter.GetItem(e.Position);
            _lblWiFiToUseSelected.Text = selected.SSID;
        }

        //private void _listView_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        //{
        //    selected = _adapter.GetItem(e.Position);
        //    _lblWiFiToUseSelected.Text = selected.SSID;
        //}

        private void _btnSave_Click(object sender, EventArgs e)
        {
            bool result = false;

            dataConfig.baseURI = "baseUri Cambiada";
            string json = JsonConvert.SerializeObject(dataConfig);
            string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            string filePath = Path.Combine(path, FileConfig);
            using (var file = File.Open(filePath, FileMode.Create, FileAccess.Write))
            using (var strm = new StreamWriter(file))
            {
                strm.Write(json);
                result = true;
            }
            if (result)
            {
                this.Finish();

            }
        }

        private void RefreshWifiList()
        {

            if (!wifiManager.IsWifiEnabled)
                wifiManager.SetWifiEnabled(true);
            wifiManager.StartScan();

            ThreadPool.QueueUserWorkItem(lol =>
            {
                Thread.Sleep(TimeSpan.FromSeconds(3));

                var wifiList = wifiManager.ScanResults;

                if (null == _adapter)
                {
                    _adapter = new ArrayAdapter<WiFiDetail>(this, Android.Resource.Layout.SimpleListItemChecked,
                                                       Android.Resource.Id.Text1);
                    RunOnUiThread(() => _listView.Adapter = _adapter);
                }

                if (_adapter.Count > 0)
                {
                    RunOnUiThread(() => _adapter.Clear());
                }


                foreach (var wifi in wifiList)
                {
                    var wifi1 = wifi;
                    RunOnUiThread(() => _adapter.Add(new WiFiDetail { SSID = wifi1.Ssid, Encryption = wifi1.Capabilities }));
                }

                RunOnUiThread(() => _adapter.NotifyDataSetChanged());
            });
        }
    }
}