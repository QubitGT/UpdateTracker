using System;
using UnityEngine;

namespace Oculus.Interaction;

public class ActiveStateSelector : MonoBehaviour, ISelector
{
	[Tooltip("ISelector events will be raised based on state changes of this IActiveState.")]
	[SerializeField]
	[Interface(typeof(IActiveState), new Type[] { })]
	private UnityEngine.Object _activeState;

	private bool _selecting;

	protected IActiveState ActiveState { get; private set; }

	public event Action WhenSelected = delegate
	{
	};

	public event Action WhenUnselected = delegate
	{
	};

	protected virtual void Awake()
	{
		ActiveState = _activeState as IActiveState;
	}

	protected virtual void Start()
	{
	}

	protected virtual void Update()
	{
		if (_selecting != ActiveState.Active)
		{
			_selecting = ActiveState.Active;
			if (_selecting)
			{
				this.WhenSelected();
			}
			else
			{
				this.WhenUnselected();
			}
		}
	}

	public void InjectAllActiveStateSelector(IActiveState activeState)
	{
		InjectActiveState(activeState);
	}

	public void InjectActiveState(IActiveState activeState)
	{
		_activeState = activeState as UnityEngine.Object;
		ActiveState = activeState;
	}
}
