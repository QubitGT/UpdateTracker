using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace UnityEngine.VFX.Utility;

[AddComponentMenu("VFX/Property Binders/Input Mouse Binder")]
[VFXBinder("Input/Mouse")]
internal class VFXInputMouseBinder : VFXBinderBase
{
	[VFXPropertyBinding(new string[] { "System.Boolean" })]
	[SerializeField]
	[FormerlySerializedAs("m_MouseLeftClickParameter")]
	protected ExposedProperty m_MouseLeftClickProperty = "LeftClick";

	[VFXPropertyBinding(new string[] { "System.Boolean" })]
	[SerializeField]
	[FormerlySerializedAs("m_MouseRightClickParameter")]
	protected ExposedProperty m_MouseRightClickProperty = "RightClick";

	[VFXPropertyBinding(new string[] { "UnityEditor.VFX.Position", "UnityEngine.Vector3" })]
	[SerializeField]
	[FormerlySerializedAs("m_PositionParameter")]
	protected ExposedProperty m_PositionProperty = "Position";

	[VFXPropertyBinding(new string[] { "UnityEngine.Vector3" })]
	[SerializeField]
	[FormerlySerializedAs("m_VelocityParameter")]
	protected ExposedProperty m_VelocityProperty = "Velocity";

	public Camera Target;

	public float Distance = 10f;

	public bool SetVelocity;

	public bool CheckLeftClick = true;

	public bool CheckRightClick;

	private Vector3 m_PreviousPosition;

	public string MouseLeftClickProperty
	{
		get
		{
			return (string)m_MouseLeftClickProperty;
		}
		set
		{
			m_MouseLeftClickProperty = value;
		}
	}

	public string MouseRightClickProperty
	{
		get
		{
			return (string)m_MouseRightClickProperty;
		}
		set
		{
			m_MouseRightClickProperty = value;
		}
	}

	public string PositionProperty
	{
		get
		{
			return (string)m_PositionProperty;
		}
		set
		{
			m_PositionProperty = value;
		}
	}

	public string VelocityProperty
	{
		get
		{
			return (string)m_VelocityProperty;
		}
		set
		{
			m_VelocityProperty = value;
		}
	}

	public override bool IsValid(VisualEffect component)
	{
		if (component.HasVector3(m_PositionProperty) && (!CheckLeftClick || component.HasBool(m_MouseLeftClickProperty)) && (!CheckRightClick || component.HasBool(m_MouseRightClickProperty)))
		{
			if (!SetVelocity)
			{
				return true;
			}
			return component.HasVector3(m_VelocityProperty);
		}
		return false;
	}

	public override void UpdateBinding(VisualEffect component)
	{
		Vector3 zero = Vector3.zero;
		if (CheckLeftClick)
		{
			component.SetBool(MouseLeftClickProperty, IsLeftClickPressed());
		}
		if (CheckRightClick)
		{
			component.SetBool(MouseRightClickProperty, IsRightClickPressed());
		}
		if (Target != null)
		{
			Vector3 position = GetMousePosition();
			position.z = Distance;
			zero = Target.ScreenToWorldPoint(position);
		}
		else
		{
			zero = GetMousePosition();
		}
		component.SetVector3(m_PositionProperty, zero);
		if (SetVelocity)
		{
			component.SetVector3(m_VelocityProperty, (zero - m_PreviousPosition) / Time.deltaTime);
		}
		m_PreviousPosition = zero;
	}

	private bool IsRightClickPressed()
	{
		if (Mouse.current == null)
		{
			return false;
		}
		return Mouse.current.rightButton.isPressed;
	}

	private bool IsLeftClickPressed()
	{
		if (Mouse.current == null)
		{
			return false;
		}
		return Mouse.current.leftButton.isPressed;
	}

	private Vector2 GetMousePosition()
	{
		return Pointer.current.position.ReadValue();
	}

	public override string ToString()
	{
		return string.Format("Mouse: '{0}' -> {1}", m_PositionProperty, (Target == null) ? "(null)" : Target.name);
	}
}
