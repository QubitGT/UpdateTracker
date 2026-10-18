using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Modio.API;
using Modio.Errors;
using Modio.Extensions;
using Modio.Mods;
using Modio.Users;

namespace Modio.Metrics;

public class MetricsManager
{
	private const int HEARTBEAT_INTERVAL = 150;

	private readonly Dictionary<string, MetricsSession> _sessions = new Dictionary<string, MetricsSession>();

	private readonly MetricsSettings _settings;

	private string Secret
	{
		get
		{
			if (_settings != null)
			{
				return _settings.Secret;
			}
			return string.Empty;
		}
	}

	public MetricsManager()
	{
		ModioSettings modioSettings = ModioServices.Resolve<ModioSettings>();
		_settings = modioSettings.GetPlatformSettings<MetricsSettings>();
		if (string.IsNullOrEmpty(Secret))
		{
			ModioLog.Error?.Log("Metrics Secret has not been set.");
		}
	}

	public async Task<(string, Error)> StartSession()
	{
		string guid = Guid.NewGuid().ToString();
		return (guid, await StartSession(guid));
	}

	public async Task<Error> StartSession(string id)
	{
		return await StartSession(id, (from mod in User.Current.ModRepository.GetSubscribed()
			where mod.IsSubscribed && mod.IsEnabled
			select mod).Select((Func<Mod, long>)((Mod mod) => mod.Id)).ToArray());
	}

	public async Task<(string, Error)> StartSession(long[] mods)
	{
		string guid = Guid.NewGuid().ToString();
		return (guid, await StartSession(guid, mods));
	}

	public async Task<Error> StartSession(string id, long[] mods)
	{
		if (string.IsNullOrEmpty(Secret))
		{
			return new Error(ErrorCode.INVALID_METRICS_SECRET);
		}
		if (_sessions.ContainsKey(id))
		{
			ModioLog.Warning?.Log("Metric session '" + id + "' already active in session cache\nPlease start a session with a different ID");
			return Error.None;
		}
		MetricsSession session = new MetricsSession(id, mods);
		Error item = (await ModioAPI.Metrics.MetricsSessionStart(session.ToRequest(includeIds: true, Secret))).Item1;
		if ((bool)item)
		{
			ModioLog.Warning?.Log($"Metrics failed with: {item}");
			return item;
		}
		session.Active = true;
		_sessions.Add(session.SessionId, session);
		session.HeartbeatCancellationToken = new CancellationTokenSource();
		Heartbeat(session.SessionId).ForgetTaskSafely();
		return Error.None;
	}

	private async Task Heartbeat(string id)
	{
		if (string.IsNullOrEmpty(Secret) || !_sessions.TryGetValue(id, out var session))
		{
			return;
		}
		CancellationToken cancellationToken = session.HeartbeatCancellationToken.Token;
		try
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				session.SessionOrderId++;
				Error item = (await ModioAPI.Metrics.MetricsSessionHeartbeat(session.ToRequest(includeIds: false, Secret))).Item1;
				if ((bool)item)
				{
					ModioLog.Warning?.Log($"Metrics failed with: {item}");
				}
				try
				{
					await Task.Delay(TimeSpan.FromSeconds(150.0), cancellationToken);
				}
				catch (TaskCanceledException)
				{
					break;
				}
			}
		}
		finally
		{
			session.HeartbeatCompletionSource.SetResult(result: true);
		}
	}

	public async Task<Error> EndSession(string id)
	{
		if (string.IsNullOrEmpty(Secret))
		{
			return new Error(ErrorCode.INVALID_METRICS_SECRET);
		}
		if (!_sessions.TryGetValue(id, out var session))
		{
			ModioLog.Warning?.Log("Metric session '" + id + "' not in session cache\nPlease make sure to start a session.");
			return Error.None;
		}
		if (!session.HeartbeatCancellationToken.Token.IsCancellationRequested)
		{
			session.HeartbeatCancellationToken.Cancel();
		}
		await session.HeartbeatCompletionSource.Task;
		Error item = (await ModioAPI.Metrics.MetricsSessionEnd(session.ToRequest(includeIds: false, Secret))).Item1;
		session.Active = false;
		if ((bool)item)
		{
			return Error.None;
		}
		ModioLog.Warning?.Log($"Metrics failed with: {item}");
		return item;
	}

	public void EndAllSessions()
	{
		foreach (MetricsSession item in _sessions.Values.Where((MetricsSession session) => session.Active))
		{
			EndSession(item.SessionId).ForgetTaskSafely();
		}
	}
}
