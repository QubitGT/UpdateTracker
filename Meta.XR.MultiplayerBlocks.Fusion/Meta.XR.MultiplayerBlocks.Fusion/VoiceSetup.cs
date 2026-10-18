using System.Collections;
using System.Reflection;
using Fusion;
using POpusCodec.Enums;
using Photon.Voice.Fusion;
using Photon.Voice.Unity;
using Photon.Voice.Unity.UtilityScripts;
using UnityEngine;

namespace Meta.XR.MultiplayerBlocks.Fusion;

public class VoiceSetup : MonoBehaviour
{
	public Transform centerEyeAnchor;

	private const uint CustomSpeakerPrefabID = 100000u;

	public GameObject Speaker { get; private set; }

	private void Awake()
	{
		CustomNetworkObjectProvider.RegisterCustomNetworkObject(100000u, delegate
		{
			GameObject obj = new GameObject("Voice");
			AudioSource audioSource = obj.AddComponent<AudioSource>();
			audioSource.bypassReverbZones = true;
			audioSource.spatialBlend = 1f;
			Recorder recorder = obj.AddComponent<Recorder>();
			recorder.StopRecordingWhenPaused = true;
			recorder.SamplingRate = SamplingRate.Sampling48000;
			obj.AddComponent<Speaker>();
			obj.AddComponent<LipSyncPhotonFix>();
			obj.AddComponent<MicAmplifier>().AmplificationFactor = 2f;
			obj.AddComponent<VoiceNetworkObject>();
			obj.AddComponent<NetworkTransform>();
			NetworkObject obj2 = obj.AddComponent<NetworkObject>();
			FieldInfo field = typeof(NetworkObject).GetField("ObjectInterest", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			FieldInfo field2 = typeof(NetworkObject).GetNestedType("ObjectInterestModes", BindingFlags.NonPublic).GetField("AreaOfInterest");
			if (field != null && field2 != null)
			{
				field.SetValue(obj2, (int)field2.GetValue(null));
			}
			return obj;
		});
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
		StartCoroutine(SpawnSpeaker(networkRunner));
	}

	private IEnumerator SpawnSpeaker(NetworkRunner networkRunner)
	{
		while (networkRunner == null)
		{
			yield return null;
		}
		NetworkObject networkObject = networkRunner.Spawn(NetworkPrefabId.FromRaw(100000u), centerEyeAnchor.position, centerEyeAnchor.rotation, networkRunner.LocalPlayer);
		networkObject.transform.SetParent(centerEyeAnchor.transform);
		Speaker = networkObject.gameObject;
	}
}
