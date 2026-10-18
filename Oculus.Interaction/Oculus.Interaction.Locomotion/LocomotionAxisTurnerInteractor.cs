using System;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.Locomotion;

public class LocomotionAxisTurnerInteractor : Interactor<LocomotionAxisTurnerInteractor, LocomotionAxisTurnerInteractable>, IAxis1D
{
	[SerializeField]
	[Interface(typeof(IAxis2D), new Type[] { })]
	[Tooltip("Input 2D Axis from which the Horizontal axis will be extracted")]
	private UnityEngine.Object _axis2D;

	private IAxis2D Axis2D;

	[SerializeField]
	[Range(0f, 1f)]
	[Tooltip("The Axis.x absolute value must be bigger than this to go into Hover and Select states")]
	private float _deadZone = 0.5f;

	private float _horizontalAxisValue;

	public float DeadZone
	{
		get
		{
			return _deadZone;
		}
		set
		{
			_deadZone = value;
		}
	}

	public override bool ShouldHover => Mathf.Abs(_horizontalAxisValue) > _deadZone;

	public override bool ShouldUnhover => !ShouldHover;

	protected override bool ComputeShouldSelect()
	{
		return ShouldHover;
	}

	protected override bool ComputeShouldUnselect()
	{
		return ShouldUnhover;
	}

	protected override void Awake()
	{
		base.Awake();
		Axis2D = _axis2D as IAxis2D;
	}

	protected override void OnDisable()
	{
		if (_started)
		{
			_horizontalAxisValue = 0f;
		}
		base.OnDisable();
	}

	protected override void Start()
	{
		this.BeginStart(ref _started, delegate
		{
			base.Start();
		});
		this.EndStart(ref _started);
	}

	protected override void DoPreprocess()
	{
		base.DoPreprocess();
		_horizontalAxisValue = Axis2D.Value().x;
	}

	protected override LocomotionAxisTurnerInteractable ComputeCandidate()
	{
		return null;
	}

	public float Value()
	{
		return _horizontalAxisValue;
	}

	public void InjectAllLocomotionAxisTurner(IAxis2D axis2D)
	{
		InjectAxis2D(axis2D);
	}

	public void InjectAxis2D(IAxis2D axis2D)
	{
		_axis2D = axis2D as UnityEngine.Object;
		Axis2D = axis2D;
	}
}
