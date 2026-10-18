using System.Linq;
using Meta.XR.Util;
using UnityEngine;

namespace Oculus.Interaction.Input;

[Feature(Feature.Interaction)]
public class SetDisplayRefresh : MonoBehaviour
{
	[SerializeField]
	private float _desiredDisplayFrequency = 90f;

	public void SetDesiredDisplayFrequency(float desiredDisplayFrequency)
	{
		if (OVRPlugin.systemDisplayFrequenciesAvailable.Contains(_desiredDisplayFrequency))
		{
			Debug.Log("[Oculus.Interaction] Setting desired display frequency to " + _desiredDisplayFrequency);
			OVRPlugin.systemDisplayFrequency = _desiredDisplayFrequency;
		}
	}

	protected virtual void Awake()
	{
		SetDesiredDisplayFrequency(_desiredDisplayFrequency);
	}
}
