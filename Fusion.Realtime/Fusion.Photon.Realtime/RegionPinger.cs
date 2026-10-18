#define DEBUG
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using ExitGames.Client.Photon;
using UnityEngine;

namespace Fusion.Photon.Realtime;

internal class RegionPinger
{
	[CompilerGenerated]
	private sealed class _003CRegionPingCoroutine_003Ed__22 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public RegionPinger _003C_003E4__this;

		private int _003CrttSum_003E5__1;

		private int _003CreplyCount_003E5__2;

		private Stopwatch _003Csw_003E5__3;

		private string _003Caddress_003E5__4;

		private int _003CindexOfColon_003E5__5;

		private Exception _003Ce_003E5__6;

		private int _003Crtt_003E5__7;

		private int _003Ci_003E5__8;

		private Exception _003Ce_003E5__9;

		private int _003CbestRtt_003E5__10;

		private int _003CworstRtt_003E5__11;

		private int _003CweighedRttSum_003E5__12;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CRegionPingCoroutine_003Ed__22(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003Csw_003E5__3 = null;
			_003Caddress_003E5__4 = null;
			_003Ce_003E5__6 = null;
			_003Ce_003E5__9 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			switch (_003C_003E1__state)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E4__this.region.Ping = PingWhenFailed;
				_003CrttSum_003E5__1 = 0;
				_003CreplyCount_003E5__2 = 0;
				_003Csw_003E5__3 = new Stopwatch();
				try
				{
					_003Caddress_003E5__4 = _003C_003E4__this.region.HostAndPort;
					_003CindexOfColon_003E5__5 = _003Caddress_003E5__4.LastIndexOf(':');
					if (_003CindexOfColon_003E5__5 > 1)
					{
						_003Caddress_003E5__4 = _003Caddress_003E5__4.Substring(0, _003CindexOfColon_003E5__5);
					}
					_003Csw_003E5__3.Start();
					_003C_003E4__this.regionAddress = ResolveHost(_003Caddress_003E5__4);
					_003Csw_003E5__3.Stop();
					if (_003Csw_003E5__3.ElapsedMilliseconds > 100)
					{
						UnityEngine.Debug.Log($"RegionPingCoroutine.ResolveHost() took: {_003Csw_003E5__3.ElapsedMilliseconds}ms");
					}
					_003Caddress_003E5__4 = null;
				}
				catch (Exception ex)
				{
					_003Ce_003E5__6 = ex;
					UnityEngine.Debug.Log($"RegionPingCoroutine ResolveHost failed for {_003C_003E4__this.region}. Caught: {_003Ce_003E5__6}");
					_003C_003E4__this.Aborted = true;
				}
				_003C_003E4__this.CurrentAttempt = 0;
				goto IL_03d7;
			case 1:
				_003C_003E1__state = -1;
				goto IL_01b0;
			case 2:
				_003C_003E1__state = -1;
				goto IL_0293;
			case 3:
				_003C_003E1__state = -1;
				goto IL_037a;
			case 4:
				_003C_003E1__state = -1;
				_003C_003E4__this.CurrentAttempt++;
				goto IL_03d7;
			case 5:
				{
					_003C_003E1__state = -1;
					return false;
				}
				IL_03f2:
				_003C_003E4__this.Done = true;
				_003C_003E4__this.ping.Dispose();
				if (_003C_003E4__this.rttResults.Count > 1 && _003CreplyCount_003E5__2 > 0)
				{
					_003CbestRtt_003E5__10 = _003C_003E4__this.rttResults.Min();
					_003CworstRtt_003E5__11 = _003C_003E4__this.rttResults.Max();
					_003CweighedRttSum_003E5__12 = _003CrttSum_003E5__1 - _003CworstRtt_003E5__11 + _003CbestRtt_003E5__10;
					_003C_003E4__this.region.Ping = _003CweighedRttSum_003E5__12 / _003CreplyCount_003E5__2;
				}
				_003C_003E4__this.onDoneCall(_003C_003E4__this.region);
				_003C_003E2__current = null;
				_003C_003E1__state = 5;
				return true;
				IL_03d7:
				if (_003C_003E4__this.CurrentAttempt < Attempts)
				{
					if (_003C_003E4__this.Aborted)
					{
						_003C_003E2__current = null;
						_003C_003E1__state = 1;
						return true;
					}
					goto IL_01b0;
				}
				goto IL_03f2;
				IL_037a:
				if (!_003C_003E4__this.ping.Done() && _003Ci_003E5__8 > 0)
				{
					_003Ci_003E5__8--;
					_003C_003E2__current = new WaitForSeconds(0.1f);
					_003C_003E1__state = 3;
					return true;
				}
				_003C_003E2__current = new WaitForSeconds(0.1f);
				_003C_003E1__state = 4;
				return true;
				IL_01b0:
				_003Csw_003E5__3.Reset();
				_003Csw_003E5__3.Start();
				try
				{
					_003C_003E4__this.ping.StartPing(_003C_003E4__this.regionAddress);
				}
				catch (Exception ex)
				{
					_003Ce_003E5__9 = ex;
					UnityEngine.Debug.Log("RegionPinger.RegionPingCoroutine() caught exception for ping.StartPing(). Exception: " + _003Ce_003E5__9?.ToString() + " Source: " + _003Ce_003E5__9.Source + " Message: " + _003Ce_003E5__9.Message);
					goto IL_03f2;
				}
				goto IL_0293;
				IL_0293:
				if (!_003C_003E4__this.ping.Done() && _003Csw_003E5__3.ElapsedMilliseconds < MaxMillisecondsPerPing)
				{
					_003C_003E2__current = new WaitForSecondsRealtime(0.01f);
					_003C_003E1__state = 2;
					return true;
				}
				_003Csw_003E5__3.Stop();
				_003Crtt_003E5__7 = (int)(_003C_003E4__this.ping.Successful ? _003Csw_003E5__3.ElapsedMilliseconds : MaxMillisecondsPerPing);
				_003C_003E4__this.rttResults.Add(_003Crtt_003E5__7);
				_003CrttSum_003E5__1 += _003Crtt_003E5__7;
				_003CreplyCount_003E5__2++;
				_003C_003E4__this.region.Ping = _003CrttSum_003E5__1 / _003CreplyCount_003E5__2;
				_003Ci_003E5__8 = 4;
				goto IL_037a;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	public static int Attempts = 5;

	public static int MaxMillisecondsPerPing = 800;

	public static int PingWhenFailed = Attempts * MaxMillisecondsPerPing;

	public int CurrentAttempt = 0;

	private Action<Region> onDoneCall;

	private PhotonPing ping;

	private List<int> rttResults;

	private Region region;

	private string regionAddress;

	public bool Done { get; private set; }

	public bool Aborted { get; internal set; }

	public RegionPinger(Region region, Action<Region> onDoneCallback)
	{
		this.region = region;
		this.region.Ping = PingWhenFailed;
		Done = false;
		onDoneCall = onDoneCallback;
	}

	private PhotonPing GetPingImplementation()
	{
		PhotonPing photonPing = null;
		if (RuntimeUnityFlagsSetup.IsUNITY_WEBGL)
		{
			if (RegionHandler.PingImplementation == null || RegionHandler.PingImplementation == typeof(PingHttp))
			{
				photonPing = new PingHttp();
			}
		}
		else if (RegionHandler.PingImplementation == null || RegionHandler.PingImplementation == typeof(PingMono))
		{
			photonPing = new PingMono();
		}
		if (photonPing == null && RegionHandler.PingImplementation != null)
		{
			photonPing = (PhotonPing)Activator.CreateInstance(RegionHandler.PingImplementation);
		}
		return photonPing;
	}

	public bool Start()
	{
		ping = GetPingImplementation();
		Done = false;
		CurrentAttempt = 0;
		rttResults = new List<int>(Attempts);
		if (Aborted)
		{
			return false;
		}
		if (RuntimeUnityFlagsSetup.IsUNITY_WEBGL)
		{
			MonoBehaviourEmpty.BuildInstance("RegionPing_" + region.Code).StartCoroutineAndDestroy(RegionPingCoroutine());
		}
		else
		{
			bool flag = false;
			try
			{
				flag = ThreadPool.QueueUserWorkItem(delegate
				{
					RegionPingThreaded();
				});
			}
			catch
			{
				flag = false;
			}
			if (!flag)
			{
				SupportClass.StartBackgroundCalls(RegionPingThreaded, 0, "RegionPing_" + region.Code + "_" + region.Cluster);
			}
		}
		return true;
	}

	protected internal void Abort()
	{
		Aborted = true;
		if (ping != null)
		{
			ping.Dispose();
		}
	}

	protected internal bool RegionPingThreaded()
	{
		region.Ping = PingWhenFailed;
		int num = 0;
		int num2 = 0;
		Stopwatch stopwatch = new Stopwatch();
		try
		{
			string text = region.HostAndPort;
			int num3 = text.LastIndexOf(':');
			if (num3 > 1)
			{
				text = text.Substring(0, num3);
			}
			stopwatch.Start();
			regionAddress = ResolveHost(text);
			stopwatch.Stop();
			if (stopwatch.ElapsedMilliseconds > 100)
			{
				System.Diagnostics.Debug.WriteLine($"RegionPingThreaded.ResolveHost() took: {stopwatch.ElapsedMilliseconds}ms");
			}
		}
		catch (Exception arg)
		{
			System.Diagnostics.Debug.WriteLine($"RegionPingThreaded ResolveHost failed for {region}. Caught: {arg}");
			Aborted = true;
		}
		CurrentAttempt = 0;
		while (CurrentAttempt < Attempts && !Aborted)
		{
			stopwatch.Reset();
			stopwatch.Start();
			try
			{
				ping.StartPing(regionAddress);
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine("RegionPinger.RegionPingThreaded() caught exception for ping.StartPing(). Exception: " + ex?.ToString() + " Source: " + ex.Source + " Message: " + ex.Message);
				break;
			}
			while (!ping.Done() && stopwatch.ElapsedMilliseconds < MaxMillisecondsPerPing)
			{
				Thread.Sleep(1);
			}
			stopwatch.Stop();
			int num4 = (int)(ping.Successful ? stopwatch.ElapsedMilliseconds : MaxMillisecondsPerPing);
			rttResults.Add(num4);
			num += num4;
			num2++;
			region.Ping = num / num2;
			int num5 = 4;
			while (!ping.Done() && num5 > 0)
			{
				num5--;
				Thread.Sleep(100);
			}
			Thread.Sleep(10);
			CurrentAttempt++;
		}
		Done = true;
		ping.Dispose();
		if (rttResults.Count > 1 && num2 > 0)
		{
			int num6 = rttResults.Min();
			int num7 = rttResults.Max();
			int num8 = num - num7 + num6;
			region.Ping = num8 / num2;
		}
		onDoneCall(region);
		return false;
	}

	[IteratorStateMachine(typeof(_003CRegionPingCoroutine_003Ed__22))]
	protected internal IEnumerator RegionPingCoroutine()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CRegionPingCoroutine_003Ed__22(0)
		{
			_003C_003E4__this = this
		};
	}

	public string GetResults()
	{
		return $"{region.Code}: {region.Ping} ({rttResults.ToStringFull()})";
	}

	public static string ResolveHost(string hostName)
	{
		if (hostName.StartsWith("wss://"))
		{
			hostName = hostName.Substring(6);
		}
		if (hostName.StartsWith("ws://"))
		{
			hostName = hostName.Substring(5);
		}
		string text = string.Empty;
		try
		{
			if (RuntimeUnityFlagsSetup.IsUNITY_WEBGL)
			{
				return hostName;
			}
			IPAddress[] hostAddresses = Dns.GetHostAddresses(hostName);
			if (hostAddresses.Length == 1)
			{
				return hostAddresses[0].ToString();
			}
			foreach (IPAddress iPAddress in hostAddresses)
			{
				if (iPAddress != null)
				{
					if (iPAddress.ToString().Contains(":"))
					{
						return iPAddress.ToString();
					}
					if (string.IsNullOrEmpty(text))
					{
						text = hostAddresses.ToString();
					}
				}
			}
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine("RegionPinger.ResolveHost() caught an exception for Dns.GetHostAddresses(). Exception: " + ex?.ToString() + " Source: " + ex.Source + " Message: " + ex.Message);
		}
		return text;
	}
}
