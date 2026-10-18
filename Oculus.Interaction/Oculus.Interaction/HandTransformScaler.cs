using System;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction;

public class HandTransformScaler : MonoBehaviour
{
	[SerializeField]
	[Interface(typeof(IHand), new Type[] { })]
	private UnityEngine.Object _hand;

	protected bool _started;

	private Vector3 _originalScale = Vector3.one;

	public IHand Hand { get; private set; }

	protected virtual void Awake()
	{
		Hand = _hand as IHand;
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		_originalScale = base.transform.localScale;
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
		if (_started)
		{
			Hand.WhenHandUpdated += HandleHandUpdated;
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			Hand.WhenHandUpdated -= HandleHandUpdated;
		}
	}

	private void HandleHandUpdated()
	{
		float num = 1f;
		if (base.transform.parent != null)
		{
			num = base.transform.parent.lossyScale.x;
		}
		base.transform.localScale = _originalScale * Hand.Scale / num;
	}
}
