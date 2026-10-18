#define FUSION_LOGLEVEL_TRACE
using System;
using UnityEngine;

namespace Fusion;

[AddComponentMenu("")]
public sealed class RunnerVisibilityLink : MonoBehaviour
{
	public enum PreferredRunners
	{
		Auto,
		Server,
		Client,
		InputAuthority
	}

	private enum ComponentType
	{
		None,
		Renderer,
		Behaviour
	}

	[SerializeField]
	public PreferredRunners PreferredRunner;

	public Component Component;

	[SerializeField]
	[ReadOnly]
	internal string Guid;

	[SerializeField]
	[HideInInspector]
	internal bool _showAtRuntime;

	internal NetworkRunner _runner;

	private ComponentType _componentType;

	private NetworkObject _networkObject;

	private bool _originalState;

	public bool IsOnSingleRunner { get; private set; }

	public bool DefaultState
	{
		get
		{
			return _originalState;
		}
		set
		{
			_originalState = value;
		}
	}

	internal bool Enabled
	{
		get
		{
			if (_componentType != ComponentType.Renderer)
			{
				return (Component as UnityEngine.Behaviour).enabled;
			}
			return (Component as Renderer).enabled;
		}
		set
		{
			if (!(Component == null))
			{
				if (_componentType == ComponentType.Renderer)
				{
					(Component as Renderer).enabled = value;
				}
				else
				{
					(Component as UnityEngine.Behaviour).enabled = value;
				}
			}
		}
	}

	private void Reset()
	{
		_showAtRuntime = true;
		Guid = System.Guid.NewGuid().ToString();
	}

	private bool AssociateComponent(Component component)
	{
		Component = component;
		component.GetType();
		if (component as Renderer != null)
		{
			_componentType = ComponentType.Renderer;
			return true;
		}
		if (component as UnityEngine.Behaviour != null)
		{
			_componentType = ComponentType.Behaviour;
			return true;
		}
		return false;
	}

	private void OnValidate()
	{
		if (Component != null)
		{
			if (Component.transform != base.transform)
			{
				Debug.LogWarning("RunnerVisibilityLink can only be associated with components on the same GameObject.");
				Component = null;
			}
			else if (!AssociateComponent(Component))
			{
				Debug.LogWarning("RunnerVisibilityLink can only be associated with Components that can be enabled/disabled.");
				Component = null;
			}
		}
	}

	private void Awake()
	{
		if (!_showAtRuntime)
		{
			base.hideFlags = HideFlags.HideInInspector;
		}
	}

	private void OnDestroy()
	{
		this.UnregisterNode();
	}

	internal void Initialize(Component comp, NetworkRunner runner)
	{
		_runner = runner;
		_networkObject = GetComponentInChildren<NetworkObject>();
		if (!_networkObject)
		{
			_networkObject = GetComponentInParent<NetworkObject>();
		}
		if (!_networkObject && PreferredRunner == PreferredRunners.InputAuthority)
		{
			Log.Warn("No NetworkObject found for RunnerVisibilityLink on " + base.gameObject.name + " with preferred runner as Input Authority. EnableOnSingleRunner will always disable it.");
		}
		if (comp is Renderer renderer)
		{
			_componentType = ComponentType.Renderer;
			_originalState = renderer.enabled;
			renderer.enabled = runner.GetVisible() && _originalState;
			Component = comp;
		}
		else if (comp is UnityEngine.Behaviour behaviour)
		{
			_componentType = ComponentType.Behaviour;
			_originalState = behaviour.enabled;
			behaviour.enabled = runner.GetVisible() && _originalState;
			Component = comp;
		}
	}

	public void SetEnabled(bool enabled)
	{
		if (enabled)
		{
			if (!_originalState)
			{
				if (!Enabled)
				{
					return;
				}
				_originalState = true;
			}
			Enabled = true;
		}
		else
		{
			Enabled = false;
		}
	}

	internal bool IsInputAuth()
	{
		if ((bool)_networkObject && _networkObject.IsValid)
		{
			return _networkObject.HasInputAuthority;
		}
		return false;
	}

	internal void SetupOnSingleRunnerLink(PreferredRunners preferredRunner)
	{
		PreferredRunner = preferredRunner;
		IsOnSingleRunner = true;
	}

	internal void InvokeRefreshCommonObjectVisibilities(float time)
	{
		StopAllCoroutines();
		Invoke("RetryRefreshCommonLinks", time);
	}

	private void RetryRefreshCommonLinks()
	{
		NetworkRunnerVisibilityExtensions.RetryRefreshCommonLinks();
	}
}
