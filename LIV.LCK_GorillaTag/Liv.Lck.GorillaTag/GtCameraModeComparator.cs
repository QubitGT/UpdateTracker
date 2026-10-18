using UnityEngine;
using UnityEngine.Events;

namespace Liv.Lck.GorillaTag;

public class GtCameraModeComparator : MonoBehaviour
{
	[SerializeField]
	private GtSelectorsGroup _gtSelectorsGroup;

	[SerializeField]
	[Tooltip("Compares the target mode with the current one")]
	private CameraMode _targetMode;

	public UnityEvent<bool> onTargetModeSelected;

	private void OnEnable()
	{
		_gtSelectorsGroup.onCameraModeChanged.AddListener(EvaluateTargetModeSelection);
	}

	private void OnDisable()
	{
		_gtSelectorsGroup.onCameraModeChanged.RemoveListener(EvaluateTargetModeSelection);
	}

	private void EvaluateTargetModeSelection(CameraMode mode)
	{
		bool arg = mode == _targetMode;
		onTargetModeSelected.Invoke(arg);
	}
}
