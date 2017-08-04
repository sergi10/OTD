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
using Android.Opengl;
using System.Xml;
using System.Text.RegularExpressions;

namespace OTD
{
	[Activity(Label = "@string/Title", MainLauncher = true, Icon = "@drawable/remotewhite")]
	public class MainActivity : Activity
	{
		private DataConfig dataConfig = new DataConfig();
		WifiManager wifiManager;
		TextView _labelSsid;
		ImageButton _IconButton;
		Button _ConfigButton;
		Button _ExitButton;
		private string FileConfig = "otd_conf.json";


		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);

			// Set our view from the "main" layout resource
			SetContentView(Resource.Layout.Main);

			_labelSsid = FindViewById<TextView>(Resource.Id.LaBelSSID);
			_IconButton = FindViewById<ImageButton>(Resource.Id.OpenDoorButton);
			_ConfigButton = FindViewById<Button>(Resource.Id.btnConfig);
			_ExitButton = FindViewById<Button>(Resource.Id.btnExit);

			_ConfigButton.Visibility = ViewStates.Visible;
			_ExitButton.Visibility = ViewStates.Visible;

			wifiManager = GetSystemService(WifiService).JavaCast<WifiManager>();

			_IconButton.Click += _IconButton_Click;
			_ConfigButton.Click += _ConfigButton_Click;
			_ExitButton.Click += _ExitButton_Click;
			if (!this.ReadConfig())
			{
				_labelSsid.Text = "ERROR EN FICHERO DE CONFIGURACION \n CIERRE LA APLICACION.";
				_IconButton.SetImageResource(Resource.Drawable.remotered);
				_IconButton.Enabled = false;
				_ConfigButton.Enabled = false;
				_ExitButton.Enabled = true;
			}
			else
			{
				_ConfigButton.Enabled = true;
				_ExitButton.Enabled = false;
				_labelSsid.Text = "PREPARADO";
			}
		}

		private void _ExitButton_Click(object sender, EventArgs e)
		{
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

			if (AddNetwork())
			{
				_IconButton.SetImageResource(Resource.Drawable.remotegray);
				if (Connect2Network())
				{
					_IconButton.SetImageResource(Resource.Drawable.remoteorange);
					if (Hodoor())
					{
						_IconButton.SetImageResource(Resource.Drawable.remotegreen);
						_ConfigButton.Enabled = false;
						_ExitButton.Enabled = true;
					}
					else
					{
						_labelSsid.Text = "NO SE PUEDE ABRIR!!\n ERROR EN LA PETICION HTTP";
					}
				}
				else
				{
					_labelSsid.Text = "NO SE PUEDE CONECTAR!!\n ERROR EN LA CONFIGURACION WIFI";
					_IconButton.SetImageResource(Resource.Drawable.remotered);
				}
			}
			else
			{
				_labelSsid.Text = "ERROR\n NO SE PUEDE AÑADIR NETWORK!!";
				_IconButton.SetImageResource(Resource.Drawable.remotegray);
			}
		}

		private bool ReadConfig()
		{
			bool result = false;

			using (StreamReader sr = new StreamReader(Assets.Open(FileConfig)))
			{
				_labelSsid.Text = "LEYENDO FICHERO";
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

		#region WiFi MANGEMENT

		private bool AddNetwork()
		{
			bool result = false;
			if (dataConfig != null)
			{
				try
				{
					_labelSsid.Text = "AÑADIR NETWORK!!!";

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
				_labelSsid.Text = "CONECTANDO WIFI!!!";

				IList<WifiConfiguration> myWifiList = wifiManager.ConfiguredNetworks;
				wifiManager.Disconnect();
				WifiConfiguration myWiFi = myWifiList.Where(x => x.Ssid.Contains(dataConfig.SSID)).FirstOrDefault();
				wifiManager.EnableNetwork(myWiFi.NetworkId, true);
				wifiManager.Reconnect();

				Thread.Sleep(TimeSpan.FromSeconds(5));
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

		private bool Hodoor()
		{
			bool result = false;
			ConnectivityManager cm = (ConnectivityManager)BaseContext.GetSystemService(Context.ConnectivityService);
			NetworkInfo networkInfo = cm.ActiveNetworkInfo;
			if (networkInfo.IsConnected)
			{
				_labelSsid.Text = "PETICION HTTP!!!";
				if (dataConfig != null)
				{
					try
					{

						string URI = "http://" + dataConfig.baseURI + dataConfig.methodURI;
						string myParameters = "code=" + dataConfig.code + "&bac=ACCESO";

						using (WebClient wc = new WebClient())
						{
							wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
							string HtmlResult = wc.UploadString(URI, "POST", myParameters);
							Console.WriteLine(HtmlResult);

							if (Regex.IsMatch(HtmlResult, "ACEPTADO", RegexOptions.IgnoreCase))
							{
								result = true;
							}
						}
					}
					catch (Exception ex)
					{
						_labelSsid.Text = string.Format("ERROR!!!\n{0}", ex.Message);
					}

				}
			}
			return result;
		}
	}
}

