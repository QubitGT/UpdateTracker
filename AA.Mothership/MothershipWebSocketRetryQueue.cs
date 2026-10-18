using System;
using System.Collections.Generic;

internal class MothershipWebSocketRetryQueue
{
	private class RetryingWebSocket : IEquatable<ActiveWebSocket>
	{
		private readonly ActiveWebSocket _websocket;

		private float _lastSetTime = 5f;

		private float _timeLeft = 5f;

		private bool _retryEnabled;

		public RetryingWebSocket(ActiveWebSocket socket)
		{
			_websocket = socket;
		}

		public void Retry()
		{
			_retryEnabled = true;
		}

		public void Reset()
		{
			_timeLeft = 5f;
			_lastSetTime = _timeLeft;
		}

		public void Tick(float deltaSeconds)
		{
			if (_retryEnabled)
			{
				_timeLeft -= deltaSeconds;
				if (!(_timeLeft > 0f))
				{
					_timeLeft = MathF.Min(_lastSetTime * 2f, 120f);
					_lastSetTime = _timeLeft;
					_retryEnabled = false;
					_websocket.resetSocket?.Invoke(_websocket.requestData);
				}
			}
		}

		public bool Equals(ActiveWebSocket other)
		{
			return _websocket == other;
		}
	}

	private const float MAX_RETRY_SECONDS = 120f;

	private const float INITIAL_RETRY_SECONDS = 5f;

	private readonly List<RetryingWebSocket> _websockets = new List<RetryingWebSocket>();

	public void AddSocket(ActiveWebSocket socket)
	{
		if (GetRetryingSocket(socket) == null)
		{
			_websockets.Add(new RetryingWebSocket(socket));
		}
	}

	public void RemoveSocket(ActiveWebSocket socket)
	{
		for (int i = 0; i < _websockets.Count; i++)
		{
			if (_websockets[i].Equals(socket))
			{
				_websockets.RemoveAt(i);
				break;
			}
		}
	}

	public void ClearSockets()
	{
		_websockets.Clear();
	}

	public void RetrySocket(ActiveWebSocket socket)
	{
		GetRetryingSocket(socket)?.Retry();
	}

	public void ResetSocket(ActiveWebSocket socket)
	{
		GetRetryingSocket(socket)?.Reset();
	}

	public void Tick(float deltaSeconds)
	{
		foreach (RetryingWebSocket websocket in _websockets)
		{
			websocket.Tick(deltaSeconds);
		}
	}

	private RetryingWebSocket GetRetryingSocket(ActiveWebSocket socket)
	{
		foreach (RetryingWebSocket websocket in _websockets)
		{
			if (websocket.Equals(socket))
			{
				return websocket;
			}
		}
		return null;
	}
}
