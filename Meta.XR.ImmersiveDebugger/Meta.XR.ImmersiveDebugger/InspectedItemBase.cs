using System;
using UnityEngine;

namespace Meta.XR.ImmersiveDebugger;

[Serializable]
internal class InspectedItemBase
{
	[SerializeField]
	public bool enabled;

	[SerializeField]
	protected string typeName;

	public bool Valid { get; protected set; }

	public bool Visible
	{
		get
		{
			if (Valid)
			{
				return enabled;
			}
			return false;
		}
	}
}
