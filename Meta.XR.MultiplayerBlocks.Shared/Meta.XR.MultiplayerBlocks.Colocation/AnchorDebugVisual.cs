using System;
using UnityEngine;

namespace Meta.XR.MultiplayerBlocks.Colocation;

internal class AnchorDebugVisual : MonoBehaviour
{
	private static bool _debugVisualsVisible = true;

	public static bool DebugVisualsVisible
	{
		get
		{
			return _debugVisualsVisible;
		}
		set
		{
			if (value != _debugVisualsVisible)
			{
				_debugVisualsVisible = value;
				AnchorDebugVisual._debugVisibilityChanged();
			}
		}
	}

	private static event Action _debugVisibilityChanged;

	private void Awake()
	{
		_debugVisibilityChanged += OnDebugVisibilityChanged;
		OnDebugVisibilityChanged();
	}

	private void OnDebugVisibilityChanged()
	{
		base.gameObject.SetActive(_debugVisualsVisible);
	}
}
