using System;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction;

public class HandPokeLimiterVisual : MonoBehaviour
{
	[SerializeField]
	[Interface(typeof(IHand), new Type[] { })]
	private UnityEngine.Object _hand;

	private IHand Hand;

	[SerializeField]
	private PokeInteractor _pokeInteractor;

	[SerializeField]
	private SyntheticHand _syntheticHand;

	private bool _isTouching;

	protected bool _started;

	protected virtual void Awake()
	{
		Hand = _hand as IHand;
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
		if (_started)
		{
			_pokeInteractor.WhenStateChanged += HandleStateChanged;
			PokeInteractor pokeInteractor = _pokeInteractor;
			pokeInteractor.WhenPassedSurfaceChanged = (Action<bool>)Delegate.Combine(pokeInteractor.WhenPassedSurfaceChanged, new Action<bool>(HandlePassedSurfaceChanged));
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			if (_isTouching)
			{
				UnlockWrist();
			}
			_pokeInteractor.WhenStateChanged -= HandleStateChanged;
			PokeInteractor pokeInteractor = _pokeInteractor;
			pokeInteractor.WhenPassedSurfaceChanged = (Action<bool>)Delegate.Remove(pokeInteractor.WhenPassedSurfaceChanged, new Action<bool>(HandlePassedSurfaceChanged));
		}
	}

	private void HandlePassedSurfaceChanged(bool passed)
	{
		CheckPassedSurface();
	}

	private void HandleStateChanged(InteractorStateChangeArgs args)
	{
		CheckPassedSurface();
	}

	private void CheckPassedSurface()
	{
		if (_pokeInteractor.IsPassedSurface)
		{
			LockWrist();
		}
		else
		{
			UnlockWrist();
		}
	}

	protected virtual void LateUpdate()
	{
		UpdateWrist();
	}

	private void LockWrist()
	{
		bool isTouching = _isTouching;
		_isTouching = true;
		if (!isTouching && _isTouching)
		{
			NativeMethods.isdk_NativeComponent_Activate(5795969328266964340uL);
		}
	}

	private void UnlockWrist()
	{
		_syntheticHand.FreeWrist();
		_isTouching = false;
	}

	private void UpdateWrist()
	{
		if (_isTouching && Hand.GetRootPose(out var pose))
		{
			Vector3 vector = pose.position - _pokeInteractor.Origin;
			Vector3 position = _pokeInteractor.TouchPoint + vector + _pokeInteractor.Radius * _pokeInteractor.TouchNormal;
			Pose wristPose = new Pose(position, pose.rotation);
			_syntheticHand.LockWristPose(wristPose, 1f, SyntheticHand.WristLockMode.Full, worldPose: true, skipAnimation: true);
			_syntheticHand.MarkInputDataRequiresUpdate();
		}
	}

	public void InjectAllHandPokeLimiterVisual(IHand hand, PokeInteractor pokeInteractor, SyntheticHand syntheticHand)
	{
		InjectHand(hand);
		InjectPokeInteractor(pokeInteractor);
		InjectSyntheticHand(syntheticHand);
	}

	public void InjectHand(IHand hand)
	{
		_hand = hand as UnityEngine.Object;
		Hand = hand;
	}

	public void InjectPokeInteractor(PokeInteractor pokeInteractor)
	{
		_pokeInteractor = pokeInteractor;
	}

	public void InjectSyntheticHand(SyntheticHand syntheticHand)
	{
		_syntheticHand = syntheticHand;
	}
}
