using UnityEngine;

namespace Unity.AI.Navigation;

public enum CollectObjects
{
	[InspectorName("All Game Objects")]
	All,
	[InspectorName("Volume")]
	Volume,
	[InspectorName("Current Object Hierarchy")]
	Children,
	[InspectorName("NavMeshModifier Component Only")]
	MarkedWithModifier
}
