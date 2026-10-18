using System.Runtime.InteropServices;
using Liv.Lck.Collections;
using UnityEngine;

namespace Liv.Lck;

internal class LckAudioCaptureFMOD : MonoBehaviour, ILckAudioSource
{
	private GCHandle mObjHandle;

	private AudioBuffer _tmpDownmixBuffer = new AudioBuffer(98000);

	private AudioBuffer _tmpAudio = new AudioBuffer(98000);

	private AudioBuffer _audioBuffer = new AudioBuffer(98000);

	private const int channels = 2;

	private bool _isCapturing;

	private readonly object _audioThreadLock = new object();

	public bool IsCapturing()
	{
		return _isCapturing;
	}

	private void Start()
	{
	}

	private void OnDestroy()
	{
	}

	public void GetAudioData(ILckAudioSource.AudioDataCallbackDelegate callback)
	{
		lock (_audioThreadLock)
		{
			callback(_audioBuffer);
			_audioBuffer.Clear();
		}
	}

	public void EnableCapture()
	{
		_isCapturing = true;
		_audioBuffer.Clear();
	}

	public void DisableCapture()
	{
		_isCapturing = false;
		_audioBuffer.Clear();
	}
}
