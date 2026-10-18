using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Serialization;

namespace UnityEngine.VFX.Utility;

[AddComponentMenu("VFX/Property Binders/Input Touch Binder")]
[VFXBinder("Input/Touch")]
internal class VFXInputTouchBinder : VFXBinderBase
{
	[VFXPropertyBinding(new string[] { "System.Boolean" })]
	[SerializeField]
	[FormerlySerializedAs("m_TouchEnabledParameter")]
	protected ExposedProperty m_TouchEnabledProperty = "TouchEnabled";

	[VFXPropertyBinding(new string[] { "UnityEditor.VFX.Position", "UnityEngine.Vector3" })]
	[SerializeField]
	protected ExposedProperty m_Parameter = "Position";

	[VFXPropertyBinding(new string[] { "UnityEngine.Vector3" })]
	[SerializeField]
	protected ExposedProperty m_VelocityParameter = "Velocity";

	public int TouchIndex;

	public Camera Target;

	public float Distance = 10f;

	public bool SetVelocity;

	private Vector3 m_PreviousPosition;

	private bool m_PreviousTouch;

	public string TouchEnabledProperty
	{
		get
		{
			return (string)m_TouchEnabledProperty;
		}
		set
		{
			m_TouchEnabledProperty = value;
		}
	}

	public string Parameter
	{
		get
		{
			return (string)m_Parameter;
		}
		set
		{
			m_Parameter = value;
		}
	}

	public string VelocityParameter
	{
		get
		{
			return (string)m_VelocityParameter;
		}
		set
		{
			m_VelocityParameter = value;
		}
	}

	public override bool IsValid(VisualEffect component)
	{
		if (Target != null && component.HasVector3(m_Parameter) && component.HasBool(m_TouchEnabledProperty))
		{
			if (!SetVelocity)
			{
				return true;
			}
			return component.HasVector3(m_VelocityParameter);
		}
		return false;
	}

	public override void UpdateBinding(VisualEffect component)
	{
		Vector3 vector = Vector3.zero;
		bool flag = false;
		if (GetTouchCount() > TouchIndex)
		{
			Vector2 touchPosition = GetTouchPosition(TouchIndex);
			flag = true;
			Vector3 position = touchPosition;
			position.z = Distance;
			vector = Target.ScreenToWorldPoint(position);
			component.SetBool(m_TouchEnabledProperty, b: true);
			component.SetVector3(m_Parameter, vector);
		}
		else
		{
			flag = false;
			component.SetBool(m_TouchEnabledProperty, b: false);
			component.SetVector3(m_Parameter, Vector3.zero);
		}
		if (SetVelocity)
		{
			if (m_PreviousTouch)
			{
				component.SetVector3(m_VelocityParameter, (vector - m_PreviousPosition) / Time.deltaTime);
			}
			else
			{
				component.SetVector3(m_VelocityParameter, Vector3.zero);
			}
		}
		m_PreviousTouch = flag;
		m_PreviousPosition = vector;
	}

	private int GetTouchCount()
	{
		if (Touchscreen.current == null)
		{
			return 0;
		}
		return Touchscreen.current.touches.Count((TouchControl t) => t.IsPressed());
	}

	private Vector2 GetTouchPosition(int touchIndex)
	{
		if (Touchscreen.current == null || touchIndex >= Touchscreen.current.touches.Count || touchIndex < 0)
		{
			return Vector2.zero;
		}
		return Touchscreen.current.touches[touchIndex].ReadValue().position;
	}

	public override string ToString()
	{
		return string.Format("Touch #{2} : '{0}' -> {1}", m_Parameter, (Target == null) ? "(null)" : Target.name, TouchIndex);
	}
}
