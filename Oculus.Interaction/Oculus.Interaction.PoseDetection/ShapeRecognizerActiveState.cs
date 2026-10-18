using System;
using System.Collections.Generic;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.PoseDetection;

public class ShapeRecognizerActiveState : MonoBehaviour, IActiveState
{
	private struct FingerFeatureStateUsage
	{
		public HandFinger handFinger;

		public ShapeRecognizer.FingerFeatureConfig config;
	}

	[SerializeField]
	[Interface(typeof(IHand), new Type[] { })]
	private UnityEngine.Object _hand;

	[SerializeField]
	[Interface(typeof(IFingerFeatureStateProvider), new Type[] { })]
	private UnityEngine.Object _fingerFeatureStateProvider;

	protected IFingerFeatureStateProvider FingerFeatureStateProvider;

	[SerializeField]
	private ShapeRecognizer[] _shapes;

	private List<FingerFeatureStateUsage> _allFingerStates = new List<FingerFeatureStateUsage>();

	private bool _nativeActive;

	public IHand Hand { get; private set; }

	public IReadOnlyList<ShapeRecognizer> Shapes => _shapes;

	public Handedness Handedness => Hand.Handedness;

	public bool Active
	{
		get
		{
			if (!base.isActiveAndEnabled || _allFingerStates.Count == 0)
			{
				return _nativeActive = false;
			}
			foreach (FingerFeatureStateUsage allFingerState in _allFingerStates)
			{
				if (!FingerFeatureStateProvider.IsStateActive(allFingerState.handFinger, allFingerState.config.Feature, allFingerState.config.Mode, allFingerState.config.State))
				{
					return _nativeActive = false;
				}
			}
			if (!_nativeActive)
			{
				NativeMethods.isdk_NativeComponent_Activate(5210787310278567284uL);
			}
			return _nativeActive = true;
		}
	}

	protected virtual void Awake()
	{
		Hand = _hand as IHand;
		FingerFeatureStateProvider = _fingerFeatureStateProvider as IFingerFeatureStateProvider;
	}

	protected virtual void Start()
	{
		_allFingerStates = FlattenUsedFeatures();
		InitStateProvider();
	}

	private void InitStateProvider()
	{
		foreach (FingerFeatureStateUsage allFingerState in _allFingerStates)
		{
			FingerFeatureStateProvider.GetCurrentState(allFingerState.handFinger, allFingerState.config.Feature, out var _);
		}
	}

	private List<FingerFeatureStateUsage> FlattenUsedFeatures()
	{
		List<FingerFeatureStateUsage> list = new List<FingerFeatureStateUsage>();
		ShapeRecognizer[] shapes = _shapes;
		foreach (ShapeRecognizer shapeRecognizer in shapes)
		{
			int num = 0;
			for (int j = 0; j < 5; j++)
			{
				HandFinger handFinger = (HandFinger)j;
				foreach (ShapeRecognizer.FingerFeatureConfig fingerFeatureConfig in shapeRecognizer.GetFingerFeatureConfigs(handFinger))
				{
					num++;
					list.Add(new FingerFeatureStateUsage
					{
						handFinger = handFinger,
						config = fingerFeatureConfig
					});
				}
			}
		}
		return list;
	}

	public void InjectAllShapeRecognizerActiveState(IHand hand, IFingerFeatureStateProvider fingerFeatureStateProvider, ShapeRecognizer[] shapes)
	{
		InjectHand(hand);
		InjectFingerFeatureStateProvider(fingerFeatureStateProvider);
		InjectShapes(shapes);
	}

	public void InjectHand(IHand hand)
	{
		_hand = hand as UnityEngine.Object;
		Hand = hand;
	}

	public void InjectFingerFeatureStateProvider(IFingerFeatureStateProvider fingerFeatureStateProvider)
	{
		_fingerFeatureStateProvider = fingerFeatureStateProvider as UnityEngine.Object;
		FingerFeatureStateProvider = fingerFeatureStateProvider;
	}

	public void InjectShapes(ShapeRecognizer[] shapes)
	{
		_shapes = shapes;
	}
}
