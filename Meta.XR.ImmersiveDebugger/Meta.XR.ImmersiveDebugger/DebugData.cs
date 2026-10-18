using System;
using System.Collections.Generic;
using UnityEngine;

namespace Meta.XR.ImmersiveDebugger;

[Serializable]
public class DebugData
{
	[SerializeField]
	public string AssemblyName;

	[SerializeField]
	public List<string> DebugTypes;

	public DebugData(string assemblyName, List<string> types)
	{
		AssemblyName = assemblyName;
		DebugTypes = types;
	}
}
