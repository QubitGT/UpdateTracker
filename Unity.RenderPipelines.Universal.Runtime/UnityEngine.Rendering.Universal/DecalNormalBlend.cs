namespace UnityEngine.Rendering.Universal;

internal enum DecalNormalBlend
{
	[Tooltip("Low quality of normal reconstruction (Uses 1 sample).")]
	Low,
	[Tooltip("Medium quality of normal reconstruction (Uses 5 samples).")]
	Medium,
	[Tooltip("High quality of normal reconstruction (Uses 9 samples).")]
	High
}
