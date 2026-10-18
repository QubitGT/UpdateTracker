using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction;

public interface IFingerAPI
{
	bool GetFingerIsGrabbing(HandFinger finger);

	bool GetFingerIsGrabbingChanged(HandFinger finger, bool targetPinchState);

	float GetFingerGrabScore(HandFinger finger);

	Vector3 GetWristOffsetLocal();

	void Update(IHand hand);
}
