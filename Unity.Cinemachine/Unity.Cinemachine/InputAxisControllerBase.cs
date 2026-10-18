using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Unity.Cinemachine;

[ExecuteAlways]
[SaveDuringPlay]
public abstract class InputAxisControllerBase<T> : MonoBehaviour, IInputAxisController where T : IInputAxisReader, new()
{
	[Serializable]
	public class Controller
	{
		[HideInInspector]
		public string Name;

		[HideInInspector]
		public UnityEngine.Object Owner;

		[Tooltip("When enabled, this controller will drive the input axis")]
		public bool Enabled = true;

		[HideFoldout]
		public T Input;

		public float InputValue;

		[HideFoldout]
		public DefaultInputAxisDriver Driver;
	}

	[Tooltip("If set, a recursive search for IInputAxisOwners behaviours will be performed.  Otherwise, only behaviours attached directly to this GameObject will be considered, and child objects will be ignored")]
	public bool ScanRecursively = true;

	[HideIfNoComponent(typeof(CinemachineVirtualCameraBase))]
	[Tooltip("If set, input will not be processed while the Cinemachine Camera is participating in a blend.")]
	public bool SuppressInputWhileBlending = true;

	public bool IgnoreTimeScale;

	[Header("Driven Axes")]
	[InputAxisControllerManager]
	[SerializeField]
	[NoSaveDuringPlay]
	internal InputAxisControllerManager<T> m_ControllerManager = new InputAxisControllerManager<T>();

	public List<Controller> Controllers
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_ControllerManager.Controllers;
		}
	}

	protected virtual void OnValidate()
	{
		m_ControllerManager.Validate();
	}

	protected virtual void Reset()
	{
		ScanRecursively = true;
		SuppressInputWhileBlending = true;
		m_ControllerManager.Reset();
		SynchronizeControllers();
	}

	protected virtual void OnEnable()
	{
		SynchronizeControllers();
	}

	protected virtual void OnDisable()
	{
		m_ControllerManager.OnDisable();
	}

	public void SynchronizeControllers()
	{
		m_ControllerManager.CreateControllers(base.gameObject, ScanRecursively, base.enabled, InitializeControllerDefaultsForAxis);
	}

	protected virtual void InitializeControllerDefaultsForAxis(in IInputAxisOwner.AxisDescriptor axis, Controller controller)
	{
	}

	protected void UpdateControllers()
	{
		UpdateControllers(IgnoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime);
	}

	protected void UpdateControllers(float deltaTime)
	{
		if (!SuppressInputWhileBlending || !TryGetComponent<CinemachineVirtualCameraBase>(out var component) || !component.IsParticipatingInBlend())
		{
			m_ControllerManager.UpdateControllers(this, deltaTime);
		}
	}
}
