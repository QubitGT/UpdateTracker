using System;
using System.Diagnostics;
using UnityEngine;

namespace Sirenix.OdinInspector;

[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
[Conditional("UNITY_EDITOR")]
public abstract class PropertyGroupAttribute : Attribute
{
	[HideInInspector]
	public string GroupID;

	[Delayed]
	[ValidateInput("ValidateGroupName", null, InfoMessageType.Error)]
	public string GroupName;

	[HideInInspector]
	public float Order;

	[LabelWidth(200f)]
	public bool HideWhenChildrenAreInvisible = true;

	[LabelWidth(200f)]
	public bool AnimateVisibility = true;

	public string VisibleIf;

	public PropertyGroupAttribute(string groupId, float order)
	{
		GroupID = groupId;
		Order = order;
		if (groupId == null)
		{
			GroupName = string.Empty;
			return;
		}
		int num = groupId.LastIndexOf('/');
		GroupName = ((num >= 0 && num < groupId.Length) ? groupId.Substring(num + 1) : groupId);
	}

	public PropertyGroupAttribute(string groupId)
		: this(groupId, 0f)
	{
	}

	public PropertyGroupAttribute Combine(PropertyGroupAttribute other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		if (other.GetType() != GetType())
		{
			throw new ArgumentException("Attributes to combine are not of the same type.");
		}
		if (other.GroupID != GroupID)
		{
			throw new ArgumentException("PropertyGroupAttributes to combine must have the same group id.");
		}
		if (Order == 0f)
		{
			Order = other.Order;
		}
		else if (other.Order != 0f)
		{
			Order = Math.Min(Order, other.Order);
		}
		HideWhenChildrenAreInvisible &= other.HideWhenChildrenAreInvisible;
		if (VisibleIf == null)
		{
			VisibleIf = other.VisibleIf;
		}
		AnimateVisibility &= other.AnimateVisibility;
		CombineValuesWith(other);
		return this;
	}

	protected virtual void CombineValuesWith(PropertyGroupAttribute other)
	{
	}

	private static bool ValidateGroupName(string value, ref string errorMessage)
	{
		if (string.IsNullOrEmpty(value))
		{
			return true;
		}
		if (value.Contains("."))
		{
			errorMessage = "GroupName can't contain the '.' character";
			return false;
		}
		return true;
	}
}
