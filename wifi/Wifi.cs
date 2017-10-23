using System.Collections.Generic;
using System.Linq;
using System.Text;
using NativeWifi;
using Wox.Plugin;

namespace wifi
{
	public class Wifi : IPlugin, IContextMenu
	{
		private PluginInitContext _ctx;
		
		#region IPlugin implementation
		public List<Result> Query(Query query)
		{
			var results = new List<Result>();

			List<Wifissid> ssids;

			var arg1 = query.FirstSearch;
			
			switch (arg1)
			{
					case "search" :
						ssids = SearchName(query.SecondSearch);
						break;
					case "free" :
						ssids = GetFreeList();
						break;
					case "reconnect" :
						ssids = GetIsConnectingSsids();
						break;
					case "connect":
						ssids = GetIsConnectingSsids(false, query.SecondSearch);
						break;
					default:
						ssids = ScanSsid();
						break;
			}
			
			ssids.Sort((a, b) => a.WlanSignalQuality > b.WlanSignalQuality ? -1 : 1);

			switch (arg1)
			{
					case "reconnect":
						ssids.ForEach(wifi =>
						{
							results.Add(new Result()
							{
								Title = "[RE-CONNECT]" + wifi.Ssid,
								IcoPath = "images\\reconnect.png",
								Action = aCtx =>
								{
									_ctx.API.ShowMsg("reconnect " + wifi.Ssid);
									return true;
								}
							});
						});
						break;
					case "connect":
						ssids.ForEach(wifi =>
						{
							results.Add(new Result()
							{
								Title = "[CONNECT]" + wifi.Ssid,
								SubTitle = wifi.WlanSignalQuality + " (" + wifi.Dot11DefaultAuthAlgorithm + ")",
								IcoPath = "images\\" + (wifi.IsLock ? "l" : "n") + (wifi.WlanSignalQuality / 25 + 1).ToString() + ".png",
								Action = aCtx =>
								{
									_ctx.API.ShowMsg("connect" + wifi.Ssid);
									return true;
								}
							});
						});
						break;
					default:
						ssids.ForEach(wifi =>
						{
							results.Add(new Result
							{
								Title = wifi.Ssid + (wifi.IsCurrentConnect ? "[CONNECTING]" : ""),
								SubTitle = wifi.WlanSignalQuality + " (" + wifi.Dot11DefaultAuthAlgorithm + ")",
								IcoPath = "images\\" + (wifi.IsLock ? "l" : "n") + (wifi.WlanSignalQuality / 25 + 1).ToString() + ".png",
								Action = aCtx =>
								{
									_ctx.API.ChangeQuery("wifi " + (wifi.IsCurrentConnect ? "reconnect " : "connect ") + wifi.Ssid, true);
									return false;
								}
							});
						});
						break;
			}

			return results;
		}
		public void Init(PluginInitContext context)
		{
			_ctx= context;
		}
		#endregion

		private static List<Wifissid> GetIsConnectingSsids(bool isConnecting = true, string ssid = "")
		{
			var all = ScanSsid();
			var results = new List<Wifissid>();
			all.ForEach(wifi =>
			{
				if (wifi.IsCurrentConnect == isConnecting && (ssid == "" || (ssid != "" && wifi.Ssid == ssid)))
				{
					results.Add(wifi);
				}
			});
			return results;
		}

		private static List<Wifissid> SearchName(string key)
		{
			var all = ScanSsid();
			var results = new List<Wifissid>();
			all.ForEach(wifi =>
			{
				if (wifi.Ssid.Contains(key))
				{
					results.Add(wifi);
				}
			});

			return results;
		}

		private static List<Wifissid> GetFreeList()
		{
			var all = ScanSsid();
			var results = new List<Wifissid>();
			
			all.ForEach(wifi =>
			{
				if (!wifi.IsLock)
				{
					results.Add(wifi);
				}
			});

			return results;
		}
		
		private static List<Wifissid> ScanSsid()
		{
			var client = new WlanClient();
			foreach (var clientInterface in client.Interfaces)
			{
				// 2 refresh wifi list
				clientInterface.Scan();
			}
			return (from wlanIface in client.Interfaces
				let networks = wlanIface.GetAvailableNetworkList(0)
				from network in networks
				select new Wifissid
				{
					WlanInterface = wlanIface,
					IsCurrentConnect = wlanIface.CurrentConnection.profileName.Equals(GetStringForSsid(network.dot11Ssid)),
					IsLock = network.securityEnabled,
					WlanSignalQuality = (int) network.wlanSignalQuality,
					Ssid = GetStringForSsid(network.dot11Ssid),
					Dot11DefaultAuthAlgorithm = network.dot11DefaultAuthAlgorithm.ToString(),
					Dot11DefaultCipherAlgorithm = network.dot11DefaultCipherAlgorithm.ToString()
				}).ToList();
		}

		private static string GetStringForSsid(Wlan.Dot11Ssid ssid)  
        {  
            return Encoding.UTF8.GetString(ssid.SSID, 0, (int)ssid.SSIDLength);  
        }

		private class Wifissid
		{
			public string Ssid = "NONE";
			public bool IsCurrentConnect;
			public bool IsLock = true;
			public string Dot11DefaultAuthAlgorithm = "";
			public string Dot11DefaultCipherAlgorithm = "";
			public bool NetworkConnectable = true;
			public string WlanNotConnectableReason = "";
			public int WlanSignalQuality;
			public WlanClient.WlanInterface WlanInterface;
		}

		public List<Result> LoadContextMenus(Result selectedResult)
		{
			var results = new List<Result>();
			for (var i = 0; i < 3; i++)
			{
				results.Add(new Result
				{
					Title = i + selectedResult.Title
				});
			}
			return results;
		}
	}
}
