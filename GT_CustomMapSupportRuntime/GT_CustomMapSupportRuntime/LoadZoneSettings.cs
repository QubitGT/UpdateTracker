using System.Collections.Generic;
using UnityEngine;

namespace GT_CustomMapSupportRuntime;

[RequireComponent(typeof(BoxCollider))]
[DisallowMultipleComponent]
public class LoadZoneSettings : MonoBehaviour
{
	public bool useDynamicLighting;

	public Color UberShaderAmbientDynamicLight = Color.black;

	public List<string> scenesToLoad = new List<string>();

	public List<string> scenesToUnload = new List<string>();
}
