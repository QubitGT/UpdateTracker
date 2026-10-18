using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Modio.API.SchemaDefinitions;

namespace Modio.Metrics;

public class MetricsSession
{
	private readonly long[] _ids;

	internal readonly string SessionId;

	internal long SessionOrderId;

	internal bool Active;

	public CancellationTokenSource HeartbeatCancellationToken;

	public TaskCompletionSource<bool> HeartbeatCompletionSource;

	public MetricsSession(string id, long[] mods)
	{
		SessionId = id;
		_ids = mods;
		SessionOrderId = 2L;
	}

	private string GetSessionHash(bool includeIds, string sessionTs, string nonce, string secret)
	{
		string text = null;
		if (includeIds)
		{
			text = string.Join(",", _ids);
		}
		text = text + sessionTs + SessionId + nonce;
		byte[] bytes = Encoding.UTF8.GetBytes(secret);
		byte[] bytes2 = Encoding.UTF8.GetBytes(text);
		using HMACSHA256 hMACSHA = new HMACSHA256(bytes);
		return BitConverter.ToString(hMACSHA.ComputeHash(bytes2)).Replace("-", "").ToLower();
	}

	internal MetricsSessionRequest ToRequest(bool includeIds, string secret)
	{
		string text = Guid.NewGuid().ToString();
		long sessionTs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
		string sessionHash = GetSessionHash(includeIds, sessionTs.ToString(), text, secret);
		return new MetricsSessionRequest(SessionId, sessionTs, sessionHash, text, SessionOrderId, includeIds ? _ids : null);
	}
}
