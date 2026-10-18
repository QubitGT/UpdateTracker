using Fusion;
using Meta.XR.MultiplayerBlocks.Shared;
using UnityEngine;

namespace Meta.XR.MultiplayerBlocks.Fusion;

public class PlayerNameTagSpawnerFusion : MonoBehaviour, INameTagSpawner
{
	[SerializeField]
	private GameObject playerNameTagPrefab;

	private NetworkRunner _networkRunner;

	private bool _sceneLoaded;

	public bool IsConnected
	{
		get
		{
			if (_networkRunner != null)
			{
				return _sceneLoaded;
			}
			return false;
		}
	}

	private void OnEnable()
	{
		FusionBBEvents.OnSceneLoadDone += OnLoaded;
	}

	private void OnDisable()
	{
		FusionBBEvents.OnSceneLoadDone -= OnLoaded;
	}

	private void OnLoaded(NetworkRunner networkRunner)
	{
		_sceneLoaded = true;
		_networkRunner = networkRunner;
	}

	public void Spawn(string playerName)
	{
		_networkRunner.Spawn(playerNameTagPrefab, Vector3.zero, Quaternion.identity, _networkRunner.LocalPlayer).GetComponent<PlayerNameTagFusion>().OculusName = playerName;
	}
}
