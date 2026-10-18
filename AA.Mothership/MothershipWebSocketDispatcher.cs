using UnityEngine;

public class MothershipWebSocketDispatcher : MonoBehaviour
{
	private static MothershipWebSocketDispatcher _instance;

	private static bool _isApplicationQuitting;

	public static MothershipWebSocketDispatcher instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = Object.FindFirstObjectByType<MothershipWebSocketDispatcher>();
				if (_instance == null)
				{
					_instance = new GameObject("MothershipWebSocketDispatcher").AddComponent<MothershipWebSocketDispatcher>();
				}
			}
			return _instance;
		}
	}

	public void Awake()
	{
		if (_instance != null && _instance != this)
		{
			Debug.LogWarning("WebSocket: Duplicate MothershipWebSocketDispatcher found. Destroying.");
			Object.DestroyImmediate(base.gameObject);
		}
		else
		{
			_instance = this;
			Object.DontDestroyOnLoad(base.gameObject);
		}
	}

	public void Update()
	{
		if (!_isApplicationQuitting)
		{
			MothershipClientApiUnity.TickWebSockets(Time.deltaTime);
		}
	}

	private void OnApplicationQuit()
	{
		_isApplicationQuitting = true;
		MothershipClientApiUnity.CloseWebSockets();
	}

	private void OnDestroy()
	{
		if (_instance == this)
		{
			MothershipClientApiUnity.CloseWebSockets();
			_instance = null;
		}
	}
}
