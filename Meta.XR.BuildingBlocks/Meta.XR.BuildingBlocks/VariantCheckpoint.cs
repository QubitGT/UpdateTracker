using System;
using UnityEngine;

namespace Meta.XR.BuildingBlocks;

[Serializable]
public class VariantCheckpoint
{
	[SerializeField]
	protected string _memberName;

	[SerializeField]
	protected string _value;

	public string MemberName => _memberName;

	public string Value => _value;

	public VariantCheckpoint(string memberName, string value)
	{
		_memberName = memberName;
		_value = value;
	}
}
