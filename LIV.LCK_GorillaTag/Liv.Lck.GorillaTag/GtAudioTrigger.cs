using UnityEngine;

namespace Liv.Lck.GorillaTag;

public class GtAudioTrigger : MonoBehaviour
{
	[SerializeField]
	private LckDiscreetAudioController _audioController;

	public void PlayTapStartedSound()
	{
		_audioController.PlayDiscreetAudioClip(LckDiscreetAudioController.AudioClip.ClickDown);
	}

	public void PlayTapEndedSound()
	{
		_audioController.PlayDiscreetAudioClip(LckDiscreetAudioController.AudioClip.ClickUp);
	}
}
