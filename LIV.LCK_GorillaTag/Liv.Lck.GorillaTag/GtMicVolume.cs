using Liv.Lck.DependencyInjection;
using UnityEngine;

namespace Liv.Lck.GorillaTag;

public class GtMicVolume : MonoBehaviour
{
	[InjectLck]
	private ILckService _lckService;

	[SerializeField]
	private float _incomingVolume;

	[SerializeField]
	private GtAudioButton _audioButton;

	private void Update()
	{
		_incomingVolume = Mathf.Clamp01(_lckService.GetMicrophoneOutputLevel().Result * 10f);
		_audioButton.SetProgress(_incomingVolume);
	}
}
