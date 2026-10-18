using System;
using UnityEngine;

namespace Photon.Voice.Unity;

public class AudioOutCapture : MonoBehaviour
{
	public event Action<float[], int> OnAudioFrame;

	private void OnAudioFilterRead(float[] frame, int channels)
	{
		if (this.OnAudioFrame != null)
		{
			this.OnAudioFrame(frame, channels);
		}
	}
}
