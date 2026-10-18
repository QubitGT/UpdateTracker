namespace Photon.Realtime;

public class Region
{
	public string Code { get; private set; }

	public string Cluster { get; private set; }

	public string HostAndPort { get; protected internal set; }

	public int Ping { get; set; }

	public bool WasPinged => Ping != int.MaxValue;

	public Region(string code, string address)
	{
		SetCodeAndCluster(code);
		HostAndPort = address;
		Ping = int.MaxValue;
	}

	public Region(string code, int ping)
	{
		SetCodeAndCluster(code);
		Ping = ping;
	}

	private void SetCodeAndCluster(string codeAsString)
	{
		if (codeAsString == null)
		{
			Code = "";
			Cluster = "";
			return;
		}
		codeAsString = codeAsString.ToLower();
		int num = codeAsString.IndexOf('/');
		Code = ((num <= 0) ? codeAsString : codeAsString.Substring(0, num));
		Cluster = ((num <= 0) ? "" : codeAsString.Substring(num + 1, codeAsString.Length - num - 1));
	}

	public override string ToString()
	{
		return ToString();
	}

	public string ToString(bool compact = false)
	{
		string text = Code;
		if (!string.IsNullOrEmpty(Cluster))
		{
			text = text + "/" + Cluster;
		}
		if (compact)
		{
			return $"{text}:{Ping}";
		}
		return string.Format("{0}[{2}]: {1}ms", text, Ping, HostAndPort);
	}
}
