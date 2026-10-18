using UnityEngine;
using UnityEngine.Networking;

namespace Fusion.Photon.Realtime;

internal class PingHttp : PhotonPing
{
	private UnityWebRequest webRequest;

	public override bool StartPing(string address)
	{
		Init();
		string arg = (Application.isEditor ? "http://" : "https://");
		address = $"{arg}{address}/photon/m/?ping&r={Random.Range(0, 10000)}";
		webRequest = UnityWebRequest.Get(address);
		webRequest.SendWebRequest();
		return true;
	}

	public override bool Done()
	{
		if (webRequest.isDone)
		{
			Successful = true;
			return true;
		}
		return false;
	}

	public override void Dispose()
	{
		webRequest.Dispose();
	}
}
