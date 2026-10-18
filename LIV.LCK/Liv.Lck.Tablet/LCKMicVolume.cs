using Liv.Lck.DependencyInjection;
using UnityEngine;
using UnityEngine.UI;

namespace Liv.Lck.Tablet;

[DefaultExecutionOrder(1000)]
public class LCKMicVolume : MonoBehaviour
{
	[InjectLck]
	private ILckService _lckService;

	[SerializeField]
	private float _incomingVolume;

	[SerializeField]
	private Image _micVolumeImage;

	private void Awake()
	{
		if ((bool)_micVolumeImage)
		{
			_micVolumeImage.transform.SetSiblingIndex(0);
		}
	}

	private void Update()
	{
		if (_lckService != null)
		{
			_incomingVolume = Mathf.Clamp01(_lckService.GetMicrophoneOutputLevel().Result * 10f);
			if ((bool)_micVolumeImage)
			{
				_micVolumeImage.fillAmount = _incomingVolume;
			}
		}
	}
}
