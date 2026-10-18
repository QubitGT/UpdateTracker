using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Fusion;

[RequireComponent(typeof(FusionBootstrap))]
[AddComponentMenu("Fusion/Fusion Boostrap Debug GUI")]
[ScriptHelp(BackColor = ScriptHeaderBackColor.Steel)]
public class FusionBootstrapDebugGUI : Behaviour
{
	[InlineHelp]
	public bool EnableHotkeys;

	[InlineHelp]
	public GUISkin BaseSkin;

	private FusionBootstrap _networkDebugStart;

	private string _clientCount;

	private bool _isMultiplePeerMode;

	private Dictionary<FusionBootstrap.Stage, string> _nicifiedStageNames;

	protected virtual void OnValidate()
	{
		ValidateClientCount();
	}

	protected void ValidateClientCount()
	{
		if (_clientCount == null)
		{
			_clientCount = "1";
		}
		else
		{
			_clientCount = Regex.Replace(_clientCount, "[^0-9]", "");
		}
	}

	protected int GetClientCount()
	{
		try
		{
			return Convert.ToInt32(_clientCount);
		}
		catch
		{
			return 0;
		}
	}

	protected virtual void Awake()
	{
		_nicifiedStageNames = ConvertEnumToNicifiedNameLookup<FusionBootstrap.Stage>("Fusion Status: ");
		_networkDebugStart = EnsureNetworkDebugStartExists();
		_clientCount = _networkDebugStart.AutoClients.ToString();
		ValidateClientCount();
	}

	protected virtual void Start()
	{
		_isMultiplePeerMode = NetworkProjectConfig.Global.PeerMode == NetworkProjectConfig.PeerModes.Multiple;
	}

	protected FusionBootstrap EnsureNetworkDebugStartExists()
	{
		if ((bool)_networkDebugStart && _networkDebugStart.gameObject == base.gameObject)
		{
			return _networkDebugStart;
		}
		if (TryGetBehaviour<FusionBootstrap>(out var behaviour))
		{
			_networkDebugStart = behaviour;
			return behaviour;
		}
		_networkDebugStart = AddBehaviour<FusionBootstrap>();
		return _networkDebugStart;
	}

	private void Update()
	{
		FusionBootstrap fusionBootstrap = EnsureNetworkDebugStartExists();
		if (!fusionBootstrap.ShouldShowGUI || fusionBootstrap.CurrentStage != 0 || !EnableHotkeys)
		{
			return;
		}
		if (Input.GetKeyDown(KeyCode.I))
		{
			_networkDebugStart.StartSinglePlayer();
		}
		if (Input.GetKeyDown(KeyCode.H))
		{
			if (_isMultiplePeerMode)
			{
				StartHostWithClients(_networkDebugStart);
			}
			else
			{
				_networkDebugStart.StartHost();
			}
		}
		if (Input.GetKeyDown(KeyCode.S))
		{
			if (_isMultiplePeerMode)
			{
				StartServerWithClients(_networkDebugStart);
			}
			else
			{
				_networkDebugStart.StartServer();
			}
		}
		if (Input.GetKeyDown(KeyCode.C))
		{
			if (_isMultiplePeerMode)
			{
				StartMultipleClients(fusionBootstrap);
			}
			else
			{
				fusionBootstrap.StartClient();
			}
		}
		if (Input.GetKeyDown(KeyCode.A))
		{
			if (_isMultiplePeerMode)
			{
				StartMultipleAutoClients(fusionBootstrap);
			}
			else
			{
				fusionBootstrap.StartAutoClient();
			}
		}
		if (Input.GetKeyDown(KeyCode.P))
		{
			if (_isMultiplePeerMode)
			{
				StartMultipleSharedClients(fusionBootstrap);
			}
			else
			{
				fusionBootstrap.StartSharedClient();
			}
		}
	}

	protected virtual void OnGUI()
	{
		FusionBootstrap fusionBootstrap = EnsureNetworkDebugStartExists();
		if (!fusionBootstrap.ShouldShowGUI)
		{
			return;
		}
		FusionBootstrap.Stage currentStage = fusionBootstrap.CurrentStage;
		if (fusionBootstrap.AutoHideGUI && currentStage == FusionBootstrap.Stage.AllConnected)
		{
			return;
		}
		GUISkin skin = GUI.skin;
		GUI.skin = FusionScalableIMGUI.GetScaledSkin(BaseSkin, out var height, out var width, out var _, out var margin, out var boxLeft);
		GUILayout.BeginArea(new Rect(boxLeft, margin, width, Screen.height));
		GUILayout.BeginVertical(GUI.skin.window);
		GUILayout.BeginHorizontal(GUILayout.Height(height));
		GUILayout.Label(_nicifiedStageNames.TryGetValue(fusionBootstrap.CurrentStage, out var value) ? value : "Unrecognized Stage", new GUIStyle(GUI.skin.label)
		{
			fontSize = (int)((float)GUI.skin.label.fontSize * 0.8f),
			alignment = TextAnchor.UpperLeft
		});
		if (!fusionBootstrap.AutoHideGUI && fusionBootstrap.CurrentStage == FusionBootstrap.Stage.AllConnected && GUILayout.Button("X", GUILayout.ExpandHeight(expand: true), GUILayout.Width(height)))
		{
			fusionBootstrap.AutoHideGUI = true;
		}
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();
		GUILayout.BeginVertical(GUI.skin.window);
		if (currentStage == FusionBootstrap.Stage.Disconnected)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label("Room:", GUILayout.Height(height), GUILayout.Width(width * 0.33f));
			fusionBootstrap.DefaultRoomName = GUILayout.TextField(fusionBootstrap.DefaultRoomName, 25, GUILayout.Height(height));
			GUILayout.EndHorizontal();
			if (GUILayout.Button(EnableHotkeys ? "Start Single Player (I)" : "Start Single Player", GUILayout.Height(height)))
			{
				fusionBootstrap.StartSinglePlayer();
			}
			if (GUILayout.Button(EnableHotkeys ? "Start Shared Client (P)" : "Start Shared Client", GUILayout.Height(height)))
			{
				if (_isMultiplePeerMode)
				{
					StartMultipleSharedClients(fusionBootstrap);
				}
				else
				{
					fusionBootstrap.StartSharedClient();
				}
			}
			if (GUILayout.Button(EnableHotkeys ? "Start Server (S)" : "Start Server", GUILayout.Height(height)))
			{
				if (_isMultiplePeerMode)
				{
					StartServerWithClients(fusionBootstrap);
				}
				else
				{
					fusionBootstrap.StartServer();
				}
			}
			if (GUILayout.Button(EnableHotkeys ? "Start Host (H)" : "Start Host", GUILayout.Height(height)))
			{
				if (_isMultiplePeerMode)
				{
					StartHostWithClients(fusionBootstrap);
				}
				else
				{
					fusionBootstrap.StartHost();
				}
			}
			if (GUILayout.Button(EnableHotkeys ? "Start Client (C)" : "Start Client", GUILayout.Height(height)))
			{
				if (_isMultiplePeerMode)
				{
					StartMultipleClients(fusionBootstrap);
				}
				else
				{
					fusionBootstrap.StartClient();
				}
			}
			if (GUILayout.Button(EnableHotkeys ? "Start Auto Host Or Client (A)" : "Start Auto Host Or Client", GUILayout.Height(height)))
			{
				if (_isMultiplePeerMode)
				{
					StartMultipleAutoClients(fusionBootstrap);
				}
				else
				{
					fusionBootstrap.StartAutoClient();
				}
			}
			if (_isMultiplePeerMode)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label("Client Count:", GUILayout.Height(height));
				GUILayout.Label("", GUILayout.Width(4f));
				string text = GUILayout.TextField(_clientCount, 10, GUILayout.Width(width * 0.25f), GUILayout.Height(height));
				if (_clientCount != text)
				{
					_clientCount = text;
					ValidateClientCount();
				}
				GUILayout.EndHorizontal();
			}
		}
		else if (GUILayout.Button("Shutdown", GUILayout.Height(height)))
		{
			_networkDebugStart.ShutdownAll();
		}
		GUILayout.EndVertical();
		GUILayout.EndArea();
		GUI.skin = skin;
	}

	private void StartHostWithClients(FusionBootstrap nds)
	{
		int clientCount;
		try
		{
			clientCount = Convert.ToInt32(_clientCount);
		}
		catch
		{
			clientCount = 0;
		}
		nds.StartHostPlusClients(clientCount);
	}

	private void StartServerWithClients(FusionBootstrap nds)
	{
		int clientCount;
		try
		{
			clientCount = Convert.ToInt32(_clientCount);
		}
		catch
		{
			clientCount = 0;
		}
		nds.StartServerPlusClients(clientCount);
	}

	private void StartMultipleClients(FusionBootstrap nds)
	{
		int clientCount;
		try
		{
			clientCount = Convert.ToInt32(_clientCount);
		}
		catch
		{
			clientCount = 0;
		}
		nds.StartMultipleClients(clientCount);
	}

	private void StartMultipleAutoClients(FusionBootstrap nds)
	{
		int.TryParse(_clientCount, out var result);
		nds.StartMultipleAutoClients(result);
	}

	private void StartMultipleSharedClients(FusionBootstrap nds)
	{
		int clientCount;
		try
		{
			clientCount = Convert.ToInt32(_clientCount);
		}
		catch
		{
			clientCount = 0;
		}
		nds.StartMultipleSharedClients(clientCount);
	}

	public static Dictionary<T, string> ConvertEnumToNicifiedNameLookup<T>(string prefix = null, Dictionary<T, string> nonalloc = null) where T : Enum
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (nonalloc == null)
		{
			nonalloc = new Dictionary<T, string>();
		}
		else
		{
			nonalloc.Clear();
		}
		string[] names = Enum.GetNames(typeof(T));
		Array values = Enum.GetValues(typeof(T));
		int i = 0;
		for (int num = names.Length; i < num; i++)
		{
			stringBuilder.Clear();
			if (prefix != null)
			{
				stringBuilder.Append(prefix);
			}
			string text = names[i];
			for (int j = 0; j < text.Length; j++)
			{
				if (char.IsUpper(text[j]) && j != 0)
				{
					stringBuilder.Append(" ");
				}
				stringBuilder.Append(text[j]);
			}
			nonalloc.Add((T)values.GetValue(i), stringBuilder.ToString());
		}
		return nonalloc;
	}
}
