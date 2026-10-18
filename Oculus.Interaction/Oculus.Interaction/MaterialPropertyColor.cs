using System;
using UnityEngine;

namespace Oculus.Interaction;

[Serializable]
public struct MaterialPropertyColor
{
	public string name;

	[ColorUsage(true, true)]
	public Color value;
}
