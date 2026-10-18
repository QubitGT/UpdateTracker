using System;
using UnityEngine;

namespace Photon.Voice.Unity.UtilityScripts;

[RequireComponent(typeof(Recorder))]
public class MicrophonePermission : VoiceComponent
{
	private Recorder recorder;

	private bool hasPermission;

	[SerializeField]
	private bool autoStart = true;

	public bool HasPermission
	{
		get
		{
			return hasPermission;
		}
		private set
		{
			base.Logger.LogInfo("Microphone Permission Granted: {0}", value);
			MicrophonePermission.MicrophonePermissionCallback?.Invoke(value);
			if (hasPermission != value)
			{
				hasPermission = value;
				if (hasPermission)
				{
					recorder.AutoStart = autoStart;
				}
			}
		}
	}

	public static event Action<bool> MicrophonePermissionCallback;

	protected override void Awake()
	{
		base.Awake();
		recorder = GetComponent<Recorder>();
		recorder.AutoStart = false;
		InitVoice();
	}

	public void InitVoice()
	{
		HasPermission = true;
	}
}
