using System.Collections;
using Meta.XR.BuildingBlocks;
using UnityEngine;

namespace Meta.XR.MultiplayerBlocks.Shared;

public class PlayerNameTagSpawner : MonoBehaviour
{
	[Header("Randomized name for non-entitled folks eg. 'HappyHippo'", order = 1)]
	[SerializeField]
	private string[] namePrefix = new string[4] { "Happy", "Running", "Laughing", "Smiling" };

	[SerializeField]
	private string[] namePostfix = new string[4] { "Cat", "Dog", "Hippo", "Bird" };

	private INameTagSpawner _nameTagSpawner;

	private void Start()
	{
		_nameTagSpawner = this.GetInterfaceComponent<INameTagSpawner>();
		PlatformInit.GetEntitlementInformation(OnEntitlementFinished);
	}

	private IEnumerator SpawnCoroutine(string playerName)
	{
		if (_nameTagSpawner != null)
		{
			while (!_nameTagSpawner.IsConnected)
			{
				yield return null;
			}
			_nameTagSpawner.Spawn(playerName);
		}
	}

	private void OnEntitlementFinished(PlatformInfo info)
	{
		Debug.Log($"Entitlement callback: isEntitled: {info.IsEntitled} Name: {info.OculusUser?.OculusID} UserID: {info.OculusUser?.ID}");
		string playerName = ((!info.IsEntitled) ? GetRandomName() : info.OculusUser?.OculusID);
		StartCoroutine(SpawnCoroutine(playerName));
	}

	private string GetRandomName()
	{
		if (namePrefix.Length == 0 || namePostfix.Length == 0)
		{
			return null;
		}
		string obj = namePrefix[Random.Range(0, namePrefix.Length - 1)];
		string text = namePostfix[Random.Range(0, namePostfix.Length - 1)];
		return obj + " " + text;
	}
}
