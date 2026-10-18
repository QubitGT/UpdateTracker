using UnityEngine;

namespace Oculus.Interaction.GrabAPI;

public class FingerRawPinchInjector : MonoBehaviour
{
	[SerializeField]
	private HandGrabAPI _handGrabAPI;

	protected virtual void Awake()
	{
		_handGrabAPI.InjectOptionalFingerPinchAPI(new FingerRawPinchAPI());
		_handGrabAPI.InjectOptionalFingerGrabAPI(new FingerRawPinchAPI());
	}
}
