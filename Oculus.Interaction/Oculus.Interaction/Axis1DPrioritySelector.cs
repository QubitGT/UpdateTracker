using System;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction;

public class Axis1DPrioritySelector : MonoBehaviour, IAxis1D
{
	[Serializable]
	public class AxisData
	{
		[SerializeField]
		[Interface(typeof(IActiveState), new Type[] { })]
		private UnityEngine.Object _activeState;

		public IActiveState ActiveState;

		[SerializeField]
		[Interface(typeof(IAxis1D), new Type[] { })]
		private UnityEngine.Object _axis;

		public IAxis1D Axis;

		public void Initialize()
		{
			ActiveState = _activeState as IActiveState;
			Axis = _axis as IAxis1D;
		}

		public void Validate(Component context)
		{
		}
	}

	[SerializeField]
	private AxisData[] _axisData;

	[SerializeField]
	[Interface(typeof(IAxis1D), new Type[] { })]
	private UnityEngine.Object _fallbackIfNoMatchAxis;

	private IAxis1D FallbackIfNoMatchAxis;

	private AxisData ActiveAxis;

	protected IAxis1D Current => GetActiveAxis();

	protected virtual void Awake()
	{
		AxisData[] axisData = _axisData;
		for (int i = 0; i < axisData.Length; i++)
		{
			axisData[i].Initialize();
		}
		FallbackIfNoMatchAxis = _fallbackIfNoMatchAxis as IAxis1D;
	}

	protected virtual void Start()
	{
		AxisData[] axisData = _axisData;
		for (int i = 0; i < axisData.Length; i++)
		{
			axisData[i].Validate(this);
		}
	}

	public float Value()
	{
		return Current.Value();
	}

	private IAxis1D GetActiveAxis()
	{
		if (ActiveAxis != null && ActiveAxis.ActiveState.Active)
		{
			return ActiveAxis.Axis;
		}
		AxisData[] axisData = _axisData;
		foreach (AxisData axisData2 in axisData)
		{
			if (axisData2.ActiveState.Active)
			{
				ActiveAxis = axisData2;
				return ActiveAxis.Axis;
			}
		}
		return FallbackIfNoMatchAxis;
	}

	public void InjectAll(AxisData[] axisData, IAxis1D fallbackIfNoMatchAxis)
	{
		_axisData = axisData;
		for (int i = 0; i < axisData.Length; i++)
		{
			axisData[i].Validate(this);
		}
		FallbackIfNoMatchAxis = fallbackIfNoMatchAxis;
		_fallbackIfNoMatchAxis = fallbackIfNoMatchAxis as UnityEngine.Object;
	}
}
