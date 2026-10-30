using System.Collections;
using System.Collections.Generic;
using Photon.Voice.Unity;
using UnityEngine;

namespace GorillaTag.Audio;

public class LoudSpeakerNetwork : MonoBehaviour
{
	[SerializeField]
	private AudioSource[] _speakerSources;

	[SerializeField]
	private List<Speaker> _currentSpeakers;

	[SerializeField]
	private int _currentSpeakerActor = -1;

	public bool ReparentLocalSpeaker = true;

	private RigContainer _rigContainer;

	private GTRecorder _localRecorder;

	private bool _localEchoActive;

	public AudioSource[] SpeakerSources => _speakerSources;

	private void Awake()
	{
		if (_speakerSources == null || _speakerSources.Length == 0)
		{
			_speakerSources = base.transform.GetComponentsInChildren<AudioSource>();
		}
		_currentSpeakers = new List<Speaker>();
	}

	private void Start()
	{
		if (GetParentRigContainer(out var rigContainer) && rigContainer.Voice != null)
		{
			GTSpeaker gTSpeaker = (GTSpeaker)rigContainer.Voice.SpeakerInUse;
			if (gTSpeaker != null)
			{
				gTSpeaker.AddExternalAudioSources(_speakerSources);
			}
		}
	}

	private bool GetParentRigContainer(out RigContainer rigContainer)
	{
		if (_rigContainer == null)
		{
			_rigContainer = base.transform.GetComponentInParent<RigContainer>();
		}
		rigContainer = _rigContainer;
		return rigContainer != null;
	}

	private void OnEnable()
	{
		if (GetParentRigContainer(out var rigContainer))
		{
			rigContainer.AddLoudSpeakerNetwork(this);
		}
	}

	private void OnDisable()
	{
		if (GetParentRigContainer(out var rigContainer))
		{
			rigContainer.RemoveLoudSpeakerNetwork(this);
		}
		ResetBroadcastState();
	}

	private void ResetBroadcastState()
	{
		if (GetParentRigContainer(out var rigContainer) && rigContainer.isActiveAndEnabled && rigContainer.Voice != null && rigContainer.Voice.SpeakerInUse != null && rigContainer.Voice.SpeakerInUse.transform.IsChildOf(base.transform))
		{
			rigContainer.StartCoroutine(RestoreSpeakerParentNextFrame(rigContainer, base.transform));
		}
		if (_localEchoActive && _localRecorder != null)
		{
			_localRecorder.DebugEchoMode = false;
		}
		_localEchoActive = false;
		if (_currentSpeakerActor == -1)
		{
			return;
		}
		foreach (Speaker currentSpeaker in _currentSpeakers)
		{
			if (currentSpeaker is GTSpeaker gTSpeaker && gTSpeaker != null)
			{
				gTSpeaker.ToggleAudioSource(toggle: false);
				gTSpeaker.BroadcastExternal = false;
			}
		}
		_currentSpeakerActor = -1;
	}

	private static IEnumerator RestoreSpeakerParentNextFrame(RigContainer rigContainer, Transform loudSpeaker)
	{
		yield return null;
		if (!(rigContainer == null) && !(rigContainer.Voice == null) && !(rigContainer.Voice.SpeakerInUse == null) && (!(loudSpeaker != null) || !loudSpeaker.gameObject.activeInHierarchy))
		{
			Transform transform = rigContainer.Voice.SpeakerInUse.transform;
			if (!(loudSpeaker == null) && transform.IsChildOf(loudSpeaker))
			{
				transform.SetParent(rigContainer.SpeakerHead, worldPositionStays: false);
				transform.localPosition = Vector3.zero;
			}
		}
	}

	public void AddSpeaker(Speaker speaker)
	{
		if (!_currentSpeakers.Contains(speaker))
		{
			_currentSpeakers.Add(speaker);
		}
	}

	public void RemoveSpeaker(Speaker speaker)
	{
		_currentSpeakers.Remove(speaker);
	}

	public void StartBroadcastSpeakerOutput(VRRig player)
	{
		if (!(player == null) && player.Creator != null)
		{
			GorillaTagger.Instance.rigSerializer.BroadcastLoudSpeakerNetwork(toggleBroadcast: true, player.Creator.ActorNumber);
		}
	}

	public void BroadcastLoudSpeakerNetwork(int actorNumber, bool isLocal = false)
	{
		if (isLocal)
		{
			if (_localRecorder == null)
			{
				_localRecorder = (GTRecorder)NetworkSystem.Instance.LocalRecorder;
			}
			if (_localRecorder != null)
			{
				_localRecorder.DebugEchoMode = true;
				_localEchoActive = true;
				if (ReparentLocalSpeaker)
				{
					Transform obj = _rigContainer.Voice.SpeakerInUse.transform;
					obj.transform.SetParent(base.transform, worldPositionStays: false);
					obj.localPosition = Vector3.zero;
				}
			}
			return;
		}
		using (List<Speaker>.Enumerator enumerator = _currentSpeakers.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				GTSpeaker obj2 = (GTSpeaker)enumerator.Current;
				obj2.ToggleAudioSource(toggle: true);
				obj2.BroadcastExternal = true;
				if (VRRigCache.Instance.TryGetVrrig(NetworkSystem.Instance.GetPlayer(actorNumber), out var playerRig))
				{
					Transform obj3 = playerRig.Voice.SpeakerInUse.transform;
					obj3.SetParent(base.transform, worldPositionStays: false);
					obj3.localPosition = Vector3.zero;
				}
			}
		}
		_currentSpeakerActor = actorNumber;
	}

	public void StopBroadcastSpeakerOutput(VRRig player)
	{
		if (!(player == null) && player.Creator != null)
		{
			GorillaTagger.Instance.rigSerializer.BroadcastLoudSpeakerNetwork(toggleBroadcast: false, player.Creator.ActorNumber);
		}
	}

	public void StopBroadcastLoudSpeakerNetwork(int actorNumber, bool isLocal = false)
	{
		if (isLocal)
		{
			if (_localRecorder == null)
			{
				_localRecorder = (GTRecorder)NetworkSystem.Instance.LocalRecorder;
			}
			if (_localRecorder != null)
			{
				_localRecorder.DebugEchoMode = false;
				_localEchoActive = false;
				if (ReparentLocalSpeaker && GetParentRigContainer(out var rigContainer))
				{
					Transform obj = rigContainer.Voice.SpeakerInUse.transform;
					obj.SetParent(rigContainer.SpeakerHead, worldPositionStays: false);
					obj.localPosition = Vector3.zero;
				}
			}
		}
		else
		{
			if (actorNumber != _currentSpeakerActor)
			{
				return;
			}
			using (List<Speaker>.Enumerator enumerator = _currentSpeakers.GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					GTSpeaker obj2 = (GTSpeaker)enumerator.Current;
					obj2.ToggleAudioSource(toggle: false);
					obj2.BroadcastExternal = false;
					if (VRRigCache.Instance.TryGetVrrig(NetworkSystem.Instance.GetPlayer(actorNumber), out var playerRig))
					{
						Transform obj3 = playerRig.Voice.SpeakerInUse.transform;
						obj3.SetParent(playerRig.SpeakerHead, worldPositionStays: false);
						obj3.localPosition = Vector3.zero;
					}
				}
			}
			_currentSpeakerActor = -1;
		}
	}
}
