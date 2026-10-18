using System.Collections;
using Liv.Lck.Collections;
using UnityEngine;

namespace Liv.Lck;

internal class LckAudioCaptureWwise : MonoBehaviour, ILckAudioSource
{
	private bool _captureAudio;

	private AudioBuffer _audioBuffer = new AudioBuffer(96000);

	public bool IsCapturing()
	{
		return _captureAudio;
	}

	private IEnumerator Start()
	{
		yield return null;
	}

	public virtual void EnableCapture()
	{
		LckLog.Log("Wwise: enable capture", "EnableCapture", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckAudioCaptureWwise.cs", 81);
	}

	public virtual void DisableCapture()
	{
		LckLog.Log("Wwise: disable capture", "DisableCapture", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckAudioCaptureWwise.cs", 97);
	}

	private void OnDestroy()
	{
		LckLog.Log("Wwise destroyed", "OnDestroy", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckAudioCaptureWwise.cs", 120);
	}

	public void GetAudioData(ILckAudioSource.AudioDataCallbackDelegate callback)
	{
	}
}
