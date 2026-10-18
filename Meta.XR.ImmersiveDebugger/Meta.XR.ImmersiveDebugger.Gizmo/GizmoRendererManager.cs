using System;
using System.Collections.Generic;
using System.Reflection;
using Meta.XR.ImmersiveDebugger.Utils;
using UnityEngine;

namespace Meta.XR.ImmersiveDebugger.Gizmo;

internal class GizmoRendererManager : MonoBehaviour
{
	private Type _classType;

	private MemberInfo _memberInfo;

	private bool _isStatic;

	private InstanceCache _instanceCache;

	private DebugGizmoType _gizmoType;

	private Color _gizmoColor;

	private List<GizmoRenderer> _renderers = new List<GizmoRenderer>();

	private HashSet<int> _enabledInstances = new HashSet<int>();

	public void Setup(Type classType, MemberInfo memberInfo, DebugGizmoType gizmoType, Color gizmoColor, InstanceCache instanceCache)
	{
		_classType = classType;
		_memberInfo = memberInfo;
		_isStatic = memberInfo.IsStatic();
		_instanceCache = instanceCache;
		_gizmoType = gizmoType;
		_gizmoColor = gizmoColor;
	}

	private void Start()
	{
		AddGizmoRenderer();
	}

	private void Update()
	{
		if (_isStatic && _renderers.Count != 0)
		{
			_renderers[0].UpdateDataSource(_memberInfo.GetValue(null));
			_renderers[0].enabled = _enabledInstances.Contains(0);
			return;
		}
		List<InstanceHandle> cacheDataForClass = _instanceCache.GetCacheDataForClass(_classType);
		if (cacheDataForClass.Count == 0)
		{
			return;
		}
		while (_renderers.Count < cacheDataForClass.Count)
		{
			AddGizmoRenderer();
		}
		int i;
		for (i = 0; i < cacheDataForClass.Count; i++)
		{
			InstanceHandle instanceHandle = cacheDataForClass[i];
			if (instanceHandle.Valid)
			{
				_renderers[i].UpdateDataSource(_memberInfo.GetValue(instanceHandle.Instance));
				_renderers[i].enabled = _enabledInstances.Contains(instanceHandle.InstanceId);
			}
			else
			{
				_renderers[i].enabled = false;
			}
		}
		for (; i < _renderers.Count && _renderers[i].enabled; i++)
		{
			_renderers[i].enabled = false;
		}
	}

	private void AddGizmoRenderer()
	{
		GizmoRenderer gizmoRenderer = base.gameObject.AddComponent<GizmoRenderer>();
		gizmoRenderer.SetUpGizmo(_gizmoType, _gizmoColor);
		gizmoRenderer.enabled = false;
		_renderers.Add(gizmoRenderer);
	}

	public bool GetState(UnityEngine.Object instance)
	{
		int item = ((instance != null) ? instance.GetInstanceID() : 0);
		return _enabledInstances.Contains(item);
	}

	public void SetState(UnityEngine.Object instance, bool state)
	{
		int item = ((instance != null) ? instance.GetInstanceID() : 0);
		if (state)
		{
			_enabledInstances.Add(item);
		}
		else
		{
			_enabledInstances.Remove(item);
		}
	}
}
