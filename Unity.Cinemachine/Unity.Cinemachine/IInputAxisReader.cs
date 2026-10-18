using UnityEngine;

namespace Unity.Cinemachine;

public interface IInputAxisReader
{
	float GetValue(Object context, IInputAxisOwner.AxisDescriptor.Hints hint);
}
